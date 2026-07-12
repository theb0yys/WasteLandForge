import hashlib
import json
import os
import sys
import unittest
from datetime import datetime, timedelta, timezone
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
sys.path.insert(0, str(ROOT / "integrations" / "mo2"))

from wastelandforge_bridge.core import CompanionError, Mo2LaunchCompanion


class Organizer:
    def __init__(self, handle=4824):
        self.handle = handle
        self.calls = []
    def instanceName(self): return "Synthetic FNV"
    def profileName(self): return "Default"
    def profileNames(self): return ["Testing", "Default", "Testing"]
    def startApplication(self, *args): self.calls.append(args); return self.handle


class Native:
    def __init__(self): self.pids = []; self.closed = []
    def process_id(self, handle): self.pids.append(handle); return 9001
    def close_handle(self, handle): self.closed.append(handle)


class MemoryFileSystem:
    def __init__(self): self.files = {}; self.directories = set(); self.reparse = set()
    def normalize(self, path): return os.path.normcase(os.path.abspath(str(path)))
    def add_directory(self, path): self.directories.add(self.normalize(path))
    def add_file(self, path, data): self.files[self.normalize(path)] = data
    def resolve(self, path, strict=False):
        value = self.normalize(path)
        if strict and value not in self.files and value not in self.directories: raise FileNotFoundError(value)
        return value
    def parent(self, path): return self.normalize(os.path.dirname(path))
    def name(self, path): return os.path.basename(path)
    def read_bytes(self, path): return self.files[self.normalize(path)]
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


if __name__ == "__main__":
    unittest.main()
