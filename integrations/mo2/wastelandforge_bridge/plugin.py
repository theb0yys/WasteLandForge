import ctypes
import os
from pathlib import Path

import mobase
from PyQt6.QtGui import QIcon
from PyQt6.QtWidgets import QFileDialog, QInputDialog, QMessageBox

try:
    from .core import CompanionError, Mo2LaunchCompanion
except ImportError:  # MO2 may load plugin.py directly from its plugin folder.
    from core import CompanionError, Mo2LaunchCompanion


class NativeHandles:
    def process_id(self, handle):
        return int(ctypes.windll.kernel32.GetProcessId(ctypes.c_void_p(handle))) or None

    def close_handle(self, handle):
        if not ctypes.windll.kernel32.CloseHandle(ctypes.c_void_p(handle)):
            raise OSError("CloseHandle failed")


class WastelandForgeBridge(mobase.IPluginTool):
    def __init__(self):
        super().__init__()
        self._organizer = None
        self._parent = None

    def init(self, organizer):
        self._organizer = organizer
        return True

    def name(self): return "WastelandForge MO2 Bridge"
    def author(self): return "WastelandForge"
    def description(self): return "Explicitly imports and launches approved WastelandForge tool requests through the selected MO2 profile."
    def version(self): return mobase.VersionInfo(0, 1, 0, mobase.ReleaseType.FINAL)
    def isActive(self): return True
    def settings(self): return []
    def displayName(self): return "WastelandForge Launch Request"
    def tooltip(self): return "Import a short-lived WastelandForge GECK/xEdit launch request"
    def icon(self): return QIcon()
    def setParentWidget(self, widget): self._parent = widget

    def display(self):
        root = Path(os.environ["LOCALAPPDATA"]) / "WastelandForge" / "Mo2LaunchRequests"
        selected, _ = QFileDialog.getOpenFileName(self._parent, "Select WastelandForge MO2 launch request", str(root), "WastelandForge request (*.json)")
        if not selected:
            return
        companion = Mo2LaunchCompanion(self._organizer, NativeHandles(), root, mobase.INVALID_HANDLE_VALUE)
        try:
            view = companion.inspect(selected)
            profile, accepted = QInputDialog.getItem(self._parent, "Select MO2 profile", view["summary"], view["profiles"], 0, False)
            if not accepted:
                return
            approval = QMessageBox.question(self._parent, "Approve MO2 tool launch", view["summary"] + f"\n\nSelected profile: {profile}")
            if approval != QMessageBox.StandardButton.Yes:
                return
            receipt = companion.launch(selected, profile, view["requestSha256"], True)
            QMessageBox.information(self._parent, "WastelandForge", f"MO2 process created for profile {profile}. PID: {receipt['processId']}")
        except CompanionError as exc:
            QMessageBox.critical(self._parent, "WastelandForge request refused", str(exc))


def createPlugin():
    return WastelandForgeBridge()
