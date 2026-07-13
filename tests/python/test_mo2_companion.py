import hashlib
import json
import os
import sys
import unittest
from datetime import datetime, timedelta, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "integrations" / "mo2"))

from wastelandforge_bridge.core import CompanionError, EffectiveProviderPreflight, Mo2EffectiveProviderAdapter, Mo2LaunchCompanion


class Organizer:
    def __init__(self, handle=4824):
        self.handle = handle
        self.calls = []
    def instanceName(self): return "Synthetic FNV"
    def profileName(self): return "Default"
    def profileNames(self): return ["Testing", "Default", "Testing"]
    def startApplication(self, *args): self.calls.append(args); return self.handle


class VirtualEntry:
    def __init__(self, name, children=None):
        self._name = name
        self._children = children
    def name(self): return self._name
    def isFile(self): return self._children is None
    def isDir(self): return self._children is not None


class VirtualTree(VirtualEntry):
    CONTINUE = object()
    STOP = object()

    def __init__(self, name="", children=None):
        super().__init__(name, children or [])

    def find(self, path):
        current = self
        for part in path.replace("\\", "/").split("/"):
            match = next((entry for entry in current._children if entry.name().casefold() == part.casefold()), None)
            if match is None or not match.isDir(): return None
            current = match
        return current

    def walk(self, callback, sep="\\"):
        def visit(tree, folder):
            for entry in tree._children:
                result = callback(folder, entry)
                if result is self.STOP: return self.STOP
                if entry.isDir() and result is self.CONTINUE:
                    if visit(entry, folder + entry.name() + sep) is self.STOP: return self.STOP
            return self.CONTINUE
        visit(self, "")


class VirtualTreeOrganizer:
    def __init__(self, tree, resolutions, profiles=None):
        self.tree = tree
        self.resolutions = {path.replace("\\", "/").casefold(): value for path, value in resolutions.items()}
        self.profiles = profiles or ["Probe Test"]
        self.calls = []
        self.profile_values = ["Probe Test"]
    def instanceName(self): self.calls.append("instanceName"); return "Synthetic FNV"
    def profileName(self):
        self.calls.append("profileName")
        return self.profile_values.pop(0) if len(self.profile_values) > 1 else self.profile_values[0]
    def profileNames(self): self.calls.append("profileNames"); return list(self.profiles)
    def virtualFileTree(self): self.calls.append("virtualFileTree"); return self.tree
    def resolvePath(self, path): self.calls.append(("resolvePath", path)); return self.resolutions.get(path.replace("\\", "/").casefold(), "")


class Native:
    def __init__(self): self.pids = []; self.closed = []
    def process_id(self, handle): self.pids.append(handle); return 9001
    def close_handle(self, handle): self.closed.append(handle)


class MemoryFileSystem:
    def __init__(self): self.files = {}; self.directories = set(); self.reparse = set(); self.reads = []
    def normalize(self, path): return os.path.normcase(os.path.abspath(str(path)))
    def add_directory(self, path): self.directories.add(self.normalize(path))
    def add_file(self, path, data): self.files[self.normalize(path)] = data
    def resolve(self, path, strict=False):
        value = self.normalize(path)
        if strict and value not in self.files and value not in self.directories: raise FileNotFoundError(value)
        return value
    def parent(self, path): return self.normalize(os.path.dirname(path))
    def name(self, path): return os.path.basename(path)
    def read_bytes(self, path): self.reads.append(self.normalize(path)); return self.files[self.normalize(path)]
    def exists(self, path): return self.normalize(path) in self.files or self.normalize(path) in self.directories
    def is_reparse(self, path): return self.normalize(path) in self.reparse
    def size(self, path): return len(self.read_bytes(path))
    def write_receipt(self, path, receipt):
        target = self.normalize(path)
        if target in self.files: raise CompanionError("Receipt already exists.")
        self.files[target] = (json.dumps(receipt, indent=2) + "\n").encode()


class CompanionTests(unittest.TestCase):
    def setUp(self):
        self.fs = MemoryFileSystem()
        self.root = r"C:\Synthetic\WastelandForge"
        self.tools = os.path.join(self.root, "tools")
        self.requests = os.path.join(self.root, "requests")
        for path in (self.root, self.tools, self.requests): self.fs.add_directory(path)
        self.exe = os.path.join(self.tools, "xEdit.exe")
        self.fs.add_file(self.exe, b"synthetic tool")
        self.now = datetime(2026, 7, 12, 12, 0, tzinfo=timezone.utc)
        self.path = self.write_request()

    def write_request(self, mutate=None):
        request_id = "0123456789abcdef0123456789abcdef"
        data = {
            "formatVersion": "0.1", "kind": "wastelandforge.mo2-launch-request", "requestId": request_id,
            "createdUtc": self.now.isoformat(), "expiresUtc": (self.now + timedelta(minutes=15)).isoformat(),
            "project": {"root": self.root, "contextKind": "pending-plugin-review", "contextId": "io.test.plugin", "contextSha256": "a" * 64},
            "tool": {"kind": "xedit", "executablePath": self.exe, "workingDirectory": self.tools, "length": len(self.fs.read_bytes(self.exe)), "sha256": hashlib.sha256(self.fs.read_bytes(self.exe)).hexdigest(), "arguments": []},
            "safety": {"shellExecution": False, "elevation": False, "profileMutation": False, "executableRegistration": False, "automaticLaunch": False},
        }
        if mutate: mutate(data)
        path = os.path.join(self.requests, f"{request_id}.json")
        self.fs.add_file(path, (json.dumps(data, indent=2) + "\n").encode())
        return path

    def companion(self, organizer=None, native=None):
        return Mo2LaunchCompanion(organizer or Organizer(), native or Native(), self.requests, -1, lambda: self.now, self.fs)

    def test_inspect_and_launch_exact_profile_contract_and_receipt(self):
        organizer, native = Organizer(), Native()
        companion = self.companion(organizer, native)
        view = companion.inspect(self.path)
        self.assertEqual(["Default", "Testing"], view["profiles"])
        receipt = companion.launch(self.path, "Testing", view["requestSha256"], True)
        self.assertEqual((self.exe, [], self.tools, "Testing", "", False), organizer.calls[0])
        self.assertEqual([4824], native.pids); self.assertEqual([4824], native.closed)
        self.assertTrue(receipt["processCreated"])
        self.assertTrue(self.fs.exists(os.path.join(self.requests, "0123456789abcdef0123456789abcdef.receipt.json")))
        with self.assertRaises(CompanionError): companion.inspect(self.path)

    def test_refuses_expiry_arguments_flags_digest_profile_and_invalid_handle(self):
        cases = [
            lambda d: d.__setitem__("expiresUtc", (self.now + timedelta(minutes=16)).isoformat()),
            lambda d: d["tool"].__setitem__("arguments", ["plugin.esp"]),
            lambda d: d["safety"].__setitem__("automaticLaunch", True),
            lambda d: d["tool"].__setitem__("sha256", "b" * 64),
        ]
        for mutate in cases:
            self.fs.files.pop(self.fs.normalize(self.path)); self.path = self.write_request(mutate)
            with self.assertRaises(CompanionError): self.companion().inspect(self.path)
        self.fs.files.pop(self.fs.normalize(self.path)); self.path = self.write_request()
        view = self.companion().inspect(self.path)
        with self.assertRaises(CompanionError): self.companion().launch(self.path, "Missing", view["requestSha256"], True)
        native = Native()
        with self.assertRaises(CompanionError): self.companion(Organizer(-1), native).launch(self.path, "Default", view["requestSha256"], True)
        self.assertEqual([], native.closed)

    def test_refuses_duplicate_keys_and_request_drift(self):
        duplicate = self.fs.read_bytes(self.path).decode().replace('"formatVersion": "0.1",', '"formatVersion": "0.1", "formatVersion": "0.1",')
        self.fs.add_file(self.path, duplicate.encode())
        with self.assertRaises(CompanionError): self.companion().inspect(self.path)
        self.fs.files.pop(self.fs.normalize(self.path)); self.path = self.write_request()
        companion = self.companion(); view = companion.inspect(self.path)
        self.fs.add_file(self.path, self.fs.read_bytes(self.path) + b" ")
        with self.assertRaises(CompanionError): companion.launch(self.path, "Default", view["requestSha256"], True)

    def test_synthetic_effective_provider_preflight_is_read_only_and_exact(self):
        digest = "a" * 64
        entries = [{
            "path": r"Data\NVSE\Plugins\WastelandForge.GeckProbe.dll",
            "length": 123904,
            "sha256": digest,
        }]
        original = json.loads(json.dumps(entries))
        result = EffectiveProviderPreflight.inspect("Synthetic FNV", "Probe Test", entries, digest)
        self.assertEqual("passed", result["status"])
        self.assertEqual("Data/NVSE/Plugins/WastelandForge.GeckProbe.dll", result["providers"][0]["path"])
        self.assertEqual(64, len(result["virtualTreeSha256"]))
        self.assertTrue(result["readOnly"])
        self.assertFalse(result["profileMutation"])
        self.assertFalse(result["filesWritten"])
        self.assertFalse(result["externalToolExecuted"])
        self.assertEqual(original, entries)

    def test_synthetic_effective_provider_preflight_refuses_unresolved_extra_duplicate_and_marker(self):
        digest = "a" * 64
        probe = {"path": EffectiveProviderPreflight.PROBE_PATH, "length": 123904, "sha256": digest}
        cases = [
            ("", "Probe Test", [probe], digest, True),
            ("Synthetic FNV", "Probe Test", [probe], digest, False),
            ("Synthetic FNV", "Probe Test", [probe, {"path": "Data/NVSE/Plugins/Other.dll", "length": 1, "sha256": "b" * 64}], digest, True),
            ("Synthetic FNV", "Probe Test", [probe, dict(probe)], digest, True),
            ("Synthetic FNV", "Probe Test", [probe, {"path": "Data/NVSE/Plugins/GeckExtender.dll", "length": 1, "sha256": "b" * 64}], digest, True),
            ("Synthetic FNV", "Probe Test", [dict(probe, sha256="b" * 64)], digest, True),
        ]
        for instance, profile, entries, expected, complete in cases:
            with self.subTest(entries=entries, complete=complete):
                with self.assertRaises(CompanionError):
                    EffectiveProviderPreflight.inspect(instance, profile, entries, expected, complete)

    def test_mo2_virtual_tree_adapter_hashes_the_effective_winner_read_only(self):
        probe = b"synthetic approved probe"
        digest = hashlib.sha256(probe).hexdigest()
        physical = os.path.join(self.root, "mods", "Probe", "NVSE", "Plugins", "WastelandForge.GeckProbe.dll")
        self.fs.add_file(physical, probe)
        tree = VirtualTree(children=[
            VirtualTree("NVSE", [
                VirtualTree("Plugins", [VirtualEntry("WastelandForge.GeckProbe.dll")])
            ])
        ])
        organizer = VirtualTreeOrganizer(tree, {"NVSE/Plugins/WastelandForge.GeckProbe.dll": physical})
        self.fs.reads.clear()

        result = Mo2EffectiveProviderAdapter(organizer, self.fs).inspect("Probe Test", digest)

        self.assertEqual("passed", result["status"])
        self.assertEqual("Synthetic FNV", result["instance"])
        self.assertEqual("Probe Test", result["profile"])
        self.assertEqual(len(probe), result["providers"][0]["length"])
        self.assertEqual([self.fs.normalize(physical)], self.fs.reads)
        self.assertEqual(1, sum(1 for call in organizer.calls if isinstance(call, tuple) and call[0] == "resolvePath"))
        self.assertNotIn("startApplication", organizer.calls)

    def test_mo2_virtual_tree_adapter_refuses_profile_drift_unresolved_paths_and_extra_native_providers(self):
        probe = b"synthetic approved probe"
        digest = hashlib.sha256(probe).hexdigest()
        physical = os.path.join(self.root, "mods", "Probe", "NVSE", "Plugins", "WastelandForge.GeckProbe.dll")
        extra = os.path.join(self.root, "mods", "Other", "NVSE", "Plugins", "Other.dll")
        self.fs.add_file(physical, probe)
        self.fs.add_file(extra, b"other")

        def tree(*names):
            return VirtualTree(children=[VirtualTree("NVSE", [VirtualTree("Plugins", [VirtualEntry(name) for name in names])])])

        mismatch = VirtualTreeOrganizer(tree("WastelandForge.GeckProbe.dll"), {"NVSE/Plugins/WastelandForge.GeckProbe.dll": physical}, ["Default", "Probe Test"])
        mismatch.profile_values = ["Default"]
        with self.assertRaisesRegex(CompanionError, "already be the current"):
            Mo2EffectiveProviderAdapter(mismatch, self.fs).inspect("Probe Test", digest)
        self.assertNotIn("virtualFileTree", mismatch.calls)

        invalid_digest = VirtualTreeOrganizer(tree("WastelandForge.GeckProbe.dll"), {"NVSE/Plugins/WastelandForge.GeckProbe.dll": physical})
        with self.assertRaisesRegex(CompanionError, "digest is invalid"):
            Mo2EffectiveProviderAdapter(invalid_digest, self.fs).inspect("Probe Test", "A" * 64)
        self.assertEqual([], invalid_digest.calls)

        unresolved = VirtualTreeOrganizer(tree("WastelandForge.GeckProbe.dll"), {})
        with self.assertRaisesRegex(CompanionError, "could not resolve"):
            Mo2EffectiveProviderAdapter(unresolved, self.fs).inspect("Probe Test", digest)

        additional = VirtualTreeOrganizer(tree("WastelandForge.GeckProbe.dll", "Other.dll"), {
            "NVSE/Plugins/WastelandForge.GeckProbe.dll": physical,
            "NVSE/Plugins/Other.dll": extra,
        })
        with self.assertRaisesRegex(CompanionError, "Unexpected effective native"):
            Mo2EffectiveProviderAdapter(additional, self.fs).inspect("Probe Test", digest)

        drift = VirtualTreeOrganizer(tree("WastelandForge.GeckProbe.dll"), {"NVSE/Plugins/WastelandForge.GeckProbe.dll": physical}, ["Probe Test", "Changed"])
        drift.profile_values = ["Probe Test", "Changed"]
        with self.assertRaisesRegex(CompanionError, "changed during"):
            Mo2EffectiveProviderAdapter(drift, self.fs).inspect("Probe Test", digest)


if __name__ == "__main__":
    unittest.main()
