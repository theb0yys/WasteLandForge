; WastelandForge Gate 343 Inno Setup source scaffold.
; This source file is not a published installer, signed artifact, update
; channel, or release package. Build execution remains gated separately.

#define AppName "WastelandForge"
#define AppVersion "0.1.0"
#define AppPublisher "WastelandForge"

#ifndef AppShellSource
#define AppShellSource "..\..\dist\app\WastelandForge.Desktop"
#endif

#ifndef InstallerOutputDir
#define InstallerOutputDir "..\..\artifacts\installer\inno"
#endif

#ifndef InstallerOutputBaseFilename
#define InstallerOutputBaseFilename "WastelandForge-Setup-local"
#endif

[Setup]
AppId={{40EAD54A-7A5F-48D3-9461-AB031754AC2F}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
DefaultDirName={autopf}\WastelandForge
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir={#InstallerOutputDir}
OutputBaseFilename={#InstallerOutputBaseFilename}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
UninstallDisplayIcon={app}\WastelandForge.exe
ChangesEnvironment=no
VersionInfoVersion=0.1.0.0
VersionInfoCompany={#AppPublisher}
VersionInfoDescription=WastelandForge Windows app shell installer

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#AppShellSource}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\WastelandForge"; Filename: "{app}\WastelandForge.exe"; WorkingDir: "{app}"
Name: "{group}\Forge Backend Help"; Filename: "{app}\ForgeBackend\forge.exe"; Parameters: "help"; WorkingDir: "{app}\ForgeBackend"
Name: "{autodesktop}\WastelandForge"; Filename: "{app}\WastelandForge.exe"; WorkingDir: "{app}"; Tasks: desktopicon
