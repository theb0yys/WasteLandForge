import hashlib
import json
import os
import re
import stat
import threading
from datetime import datetime, timedelta, timezone
from pathlib import Path


class CompanionError(RuntimeError):
    pass


class LocalFileSystem:
    def resolve(self, path, strict=False): return str(Path(path).resolve(strict=strict))
    def parent(self, path): return str(Path(path).parent)
    def name(self, path): return Path(path).name
    def read_bytes(self, path): return Path(path).read_bytes()
    def exists(self, path): return Path(path).exists()
    def is_reparse(self, path): return _is_reparse(path)
    def size(self, path): return Path(path).stat().st_size
    def write_receipt(self, path, receipt):
        target = Path(path)
        if target.exists(): raise CompanionError("Receipt already exists.")
        temporary = target.with_name(target.name + ".tmp-" + os.urandom(8).hex())
        try:
            temporary.write_text(json.dumps(receipt, indent=2) + "\n", encoding="utf-8", newline="\n")
            os.rename(temporary, target)
        except FileExistsError as exc:
            raise CompanionError("Receipt collision refused.") from exc
        finally:
            if temporary.exists(): temporary.unlink()


def _duplicates_refused(pairs):
    result = {}
    for key, value in pairs:
        if key in result:
            raise CompanionError(f"Duplicate JSON key: {key}")
        result[key] = value
    return result


def _exact_keys(value, expected, label):
    if not isinstance(value, dict) or set(value) != set(expected):
        raise CompanionError(f"{label} shape is invalid.")


def _sha(data):
    return hashlib.sha256(data).hexdigest()


def _is_reparse(path):
    info = os.lstat(path)
    attributes = getattr(info, "st_file_attributes", 0)
    return os.path.islink(path) or bool(attributes & getattr(stat, "FILE_ATTRIBUTE_REPARSE_POINT", 0))


def _parse_time(value):
    if not isinstance(value, str):
        raise CompanionError("Request time is invalid.")
    try:
        parsed = datetime.fromisoformat(value.replace("Z", "+00:00"))
    except ValueError as exc:
        raise CompanionError("Request time is invalid.") from exc
    if parsed.tzinfo is None:
        raise CompanionError("Request time must include an offset.")
    return parsed.astimezone(timezone.utc)


class EffectiveProviderPreflight:
    PROBE_PATH = "Data/NVSE/Plugins/WastelandForge.GeckProbe.dll"

    @staticmethod
    def inspect(instance, profile, entries, expected_probe_sha256, inspection_complete=True):
        if not inspection_complete:
            raise CompanionError("Effective virtual-tree inspection is incomplete.")
        if not isinstance(instance, str) or not instance.strip() or not isinstance(profile, str) or not profile.strip():
            raise CompanionError("An explicit MO2 instance and selected profile are required.")
        if not isinstance(expected_probe_sha256, str) or not re.fullmatch(r"[0-9a-f]{64}", expected_probe_sha256):
            raise CompanionError("Expected probe digest is invalid.")
        if not isinstance(entries, list):
            raise CompanionError("Effective virtual-tree entries are invalid.")

        normalized = []
        seen = set()
        for entry in entries:
            _exact_keys(entry, ["path", "length", "sha256"], "Effective virtual-tree entry")
            path = entry["path"].replace("\\", "/") if isinstance(entry["path"], str) else ""
            folded = path.casefold()
            if not path or not folded.startswith("data/nvse/plugins/") or "/../" in f"/{path}/":
                raise CompanionError("Effective provider path is outside Data/NVSE/Plugins.")
            if folded in seen:
                raise CompanionError("Duplicate effective provider path was reported.")
            seen.add(folded)
            if not isinstance(entry["length"], int) or entry["length"] < 1 or not isinstance(entry["sha256"], str) or not re.fullmatch(r"[0-9a-f]{64}", entry["sha256"]):
                raise CompanionError("Effective provider identity is invalid.")
            normalized.append({"path": path, "length": entry["length"], "sha256": entry["sha256"]})

        markers = [entry["path"] for entry in normalized if "garyhax" in entry["path"].casefold() or "geckextender" in entry["path"].casefold()]
        if markers:
            raise CompanionError("GECK Extender/GaryHax is effectively visible during the xNVSE-only smoke.")
        native = [entry for entry in normalized if entry["path"].casefold().endswith(".dll")]
        probe = [entry for entry in native if entry["path"].casefold() == EffectiveProviderPreflight.PROBE_PATH.casefold()]
        if len(probe) != 1:
            raise CompanionError("Exactly one effective WastelandForge GECK probe DLL is required.")
        if probe[0]["sha256"] != expected_probe_sha256:
            raise CompanionError("The effective GECK probe digest does not match the approved build.")
        if len(native) != 1:
            raise CompanionError("Unexpected effective native NVSE provider DLLs are visible.")

        providers = sorted(native, key=lambda item: item["path"])
        snapshot = {
            "formatVersion": "0.1",
            "kind": "wastelandforge.geck-host-probe-effective-providers",
            "status": "passed",
            "instance": instance,
            "profile": profile,
            "providers": providers,
            "inspectionComplete": True,
            "readOnly": True,
            "profileMutation": False,
            "filesWritten": False,
            "externalToolExecuted": False,
        }
        canonical = json.dumps(snapshot, sort_keys=True, separators=(",", ":")).encode("utf-8")
        snapshot["virtualTreeSha256"] = _sha(canonical)
        return snapshot


class Mo2EffectiveProviderAdapter:
    PROVIDER_TREE_PATH = "NVSE/Plugins"

    def __init__(self, organizer, filesystem=None):
        self._organizer = organizer
        self._fs = filesystem or LocalFileSystem()

    def inspect(self, selected_profile, expected_probe_sha256):
        if not isinstance(selected_profile, str) or not selected_profile.strip():
            raise CompanionError("An explicit MO2 profile is required for effective-provider inspection.")
        if not isinstance(expected_probe_sha256, str) or not re.fullmatch(r"[0-9a-f]{64}", expected_probe_sha256):
            raise CompanionError("Expected probe digest is invalid.")
        try:
            instance = self._organizer.instanceName()
            current_profile = self._organizer.profileName()
            profiles = set(self._organizer.profileNames())
        except Exception as exc:
            raise CompanionError("MO2 instance or profile inspection failed.") from exc
        if not isinstance(instance, str) or not instance.strip():
            raise CompanionError("MO2 did not report an explicit instance.")
        if selected_profile != current_profile or selected_profile not in profiles:
            raise CompanionError("The requested MO2 profile must already be the current reported profile.")

        try:
            root = self._organizer.virtualFileTree()
            provider_tree = root.find(self.PROVIDER_TREE_PATH) if root is not None else None
            if provider_tree is None or not provider_tree.isDir():
                raise CompanionError("The effective virtual tree does not contain NVSE/Plugins.")

            entries = []

            def collect(folder, entry):
                if entry.isFile():
                    relative_path = self._virtual_path(folder, entry.name())
                    resolved = self._organizer.resolvePath(relative_path)
                    if not isinstance(resolved, str) or not resolved.strip():
                        raise CompanionError(f"MO2 could not resolve the effective provider path: Data/{relative_path}")
                    if self._fs.is_reparse(resolved):
                        raise CompanionError(f"Effective provider path is a reparse point: Data/{relative_path}")
                    physical = self._fs.resolve(resolved, strict=True)
                    if self._fs.is_reparse(physical):
                        raise CompanionError(f"Effective provider path is a reparse point: Data/{relative_path}")
                    content = self._fs.read_bytes(physical)
                    entries.append({
                        "path": f"Data/{relative_path}",
                        "length": len(content),
                        "sha256": _sha(content),
                    })
                return provider_tree.CONTINUE

            provider_tree.walk(collect, "/")
        except CompanionError:
            raise
        except Exception as exc:
            raise CompanionError("MO2 effective virtual-tree inspection failed.") from exc

        try:
            context_changed = self._organizer.instanceName() != instance or self._organizer.profileName() != current_profile
        except Exception as exc:
            raise CompanionError("MO2 instance or profile revalidation failed.") from exc
        if context_changed:
            raise CompanionError("MO2 instance or profile changed during effective-provider inspection.")
        return EffectiveProviderPreflight.inspect(
            instance,
            selected_profile,
            entries,
            expected_probe_sha256,
            inspection_complete=True,
        )

    @classmethod
    def _virtual_path(cls, folder, name):
        if not isinstance(folder, str) or not isinstance(name, str) or not name:
            raise CompanionError("MO2 virtual-tree entry identity is invalid.")
        candidate = f"{cls.PROVIDER_TREE_PATH}/{folder}{name}".replace("\\", "/")
        parts = candidate.split("/")
        if any(part in ("", ".", "..") for part in parts):
            raise CompanionError("MO2 virtual-tree entry path is invalid.")
        normalized = "/".join(parts)
        if not normalized.casefold().startswith(cls.PROVIDER_TREE_PATH.casefold() + "/"):
            raise CompanionError("MO2 virtual-tree entry escaped NVSE/Plugins.")
        return normalized


class Mo2LaunchCompanion:
    def __init__(self, organizer, native, request_root, invalid_handle=-1, now=None, filesystem=None):
        self._organizer = organizer
        self._native = native
        self._fs = filesystem or LocalFileSystem()
        self._root = self._fs.resolve(request_root)
        self._invalid_handle = invalid_handle
        self._now = now or (lambda: datetime.now(timezone.utc))
        self._launch_lock = threading.Lock()

    def inspect(self, request_path):
        path = self._fs.resolve(request_path, strict=True)
        if self._fs.parent(path) != self._root or self._fs.is_reparse(path) or self._fs.is_reparse(self._root):
            raise CompanionError("Request must be a regular file directly under the private request root.")
        raw = self._fs.read_bytes(path)
        if raw.startswith(b"\xef\xbb\xbf"):
            raise CompanionError("Request must be UTF-8 without BOM.")
        try:
            request = json.loads(raw.decode("utf-8"), object_pairs_hook=_duplicates_refused)
        except (UnicodeDecodeError, json.JSONDecodeError) as exc:
            raise CompanionError("Request JSON is invalid.") from exc
        self._validate_shape(request)
        request_id = request["requestId"]
        if self._fs.name(path) != f"{request_id}.json" or not re.fullmatch(r"[0-9a-f]{32}", request_id):
            raise CompanionError("Request filename or ID is invalid.")
        created = _parse_time(request["createdUtc"])
        expires = _parse_time(request["expiresUtc"])
        if expires - created != timedelta(minutes=15):
            raise CompanionError("Request validity interval must be exactly 15 minutes.")
        now = self._now().astimezone(timezone.utc)
        if now < created or now > expires:
            raise CompanionError("Request is not currently valid.")
        tool = request["tool"]
        executable = self._fs.resolve(tool["executablePath"], strict=True)
        working = self._fs.resolve(tool["workingDirectory"], strict=True)
        if self._fs.parent(executable) != working or self._fs.is_reparse(executable) or self._fs.is_reparse(working):
            raise CompanionError("Executable or working directory is unsafe.")
        accepted = {"geck": {"geck.exe"}, "xedit": {"fnvedit.exe", "xedit.exe"}}
        if self._fs.name(executable).lower() not in accepted[tool["kind"]]:
            raise CompanionError("Executable filename does not match the requested tool.")
        executable_bytes = self._fs.read_bytes(executable)
        if len(executable_bytes) != tool["length"] or _sha(executable_bytes) != tool["sha256"]:
            raise CompanionError("Executable bytes do not match the request.")
        receipt = os.path.join(self._root, f"{request_id}.receipt.json")
        if self._fs.exists(receipt):
            raise CompanionError("Request already has a process-created receipt and cannot be replayed.")
        profiles = sorted(set(self._organizer.profileNames()))
        return {
            "path": path, "request": request, "requestSha256": _sha(raw),
            "instance": self._organizer.instanceName(), "currentProfile": self._organizer.profileName(),
            "profiles": profiles, "receiptPath": receipt,
            "summary": f"Instance: {self._organizer.instanceName()}\nTool: {tool['kind']}\nExecutable: {executable}\nArguments: none\nContext: {request['project']['contextKind']} / {request['project']['contextId']}\nRequest SHA-256: {_sha(raw)}\n\nSelect one reported profile explicitly. Process creation does not prove VFS contents, editor readiness, review, save, or correctness."
        }

    def launch(self, request_path, selected_profile, approved_request_sha, approved):
        if not approved:
            raise CompanionError("Explicit launch approval is required.")
        if not self._launch_lock.acquire(blocking=False):
            raise CompanionError("An MO2 launch is already being submitted.")
        try:
            view = self.inspect(request_path)
            if view["requestSha256"] != approved_request_sha:
                raise CompanionError("Request approval is stale.")
            if selected_profile not in view["profiles"]:
                raise CompanionError("Selected profile is not currently reported by MO2.")
            request = view["request"]
            tool = request["tool"]
            handle = self._organizer.startApplication(tool["executablePath"], [], tool["workingDirectory"], selected_profile, "", False)
            if handle in (None, 0, self._invalid_handle):
                raise CompanionError("MO2 returned an invalid process handle.")
            pid = None
            close_error = None
            try:
                pid = self._native.process_id(handle)
            finally:
                try:
                    self._native.close_handle(handle)
                except Exception as exc:  # process may still be running
                    close_error = str(exc)
            receipt = {
                "formatVersion": "0.1", "kind": "wastelandforge.mo2-launch-receipt",
                "requestId": request["requestId"], "requestSha256": view["requestSha256"],
                "instance": view["instance"], "profile": selected_profile,
                "toolKind": tool["kind"], "executableSha256": tool["sha256"],
                "processId": pid, "processCreated": True,
                "createdUtc": self._now().astimezone(timezone.utc).isoformat(),
                "handleCloseError": close_error,
            }
            self._fs.write_receipt(view["receiptPath"], receipt)
            return receipt
        finally:
            self._launch_lock.release()

    @staticmethod
    def _validate_shape(request):
        _exact_keys(request, ["formatVersion", "kind", "requestId", "createdUtc", "expiresUtc", "project", "tool", "safety"], "Request")
        if request["formatVersion"] != "0.1" or request["kind"] != "wastelandforge.mo2-launch-request":
            raise CompanionError("Request identity is invalid.")
        _exact_keys(request["project"], ["root", "contextKind", "contextId", "contextSha256"], "Project")
        _exact_keys(request["tool"], ["kind", "executablePath", "workingDirectory", "length", "sha256", "arguments"], "Tool")
        _exact_keys(request["safety"], ["shellExecution", "elevation", "profileMutation", "executableRegistration", "automaticLaunch"], "Safety")
        project, tool, safety = request["project"], request["tool"], request["safety"]
        if project["contextKind"] not in ("geck-handoff", "pending-plugin-review") or not isinstance(project["root"], str) or not isinstance(project["contextId"], str) or not re.fullmatch(r"[0-9a-f]{64}", project["contextSha256"]):
            raise CompanionError("Project context is invalid.")
        if tool["kind"] not in ("geck", "xedit") or tool["arguments"] != [] or not isinstance(tool["length"], int) or tool["length"] < 0 or not re.fullmatch(r"[0-9a-f]{64}", tool["sha256"]):
            raise CompanionError("Tool contract is invalid.")
        if any(value is not False for value in safety.values()):
            raise CompanionError("Safety contract is invalid.")
