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
