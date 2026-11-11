; ==============================================================================
; MultiScan-X Studio Official Windows Installer Script (Inno Setup 6)
; TeMaat Automation Research Institute
; ==============================================================================

#define MyAppName "MultiScan-X Studio"
#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif
#define MyAppPublisher "TeMaat Automation Research Institute"
#define MyAppExeName "MultiScanX.App.exe"

[Setup]
AppId={{8F2A91B4-3C1D-4B5E-9A2F-7E1D4C3B2A10}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName=C:\MultiScanX
DefaultGroupName={#MyAppName}
DisableProgramGroupPage=yes
OutputDir=..\MultiScanX_deploy\04_App_Release
OutputBaseFilename=MultiScanX_Setup_v{#MyAppVersion}
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=admin

[Languages]
Name: "korean"; MessagesFile: "compiler:Languages\Korean.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"

[Dirs]
Name: "{app}\bin"
Name: "{app}\config"
Name: "{app}\versions"

[Files]
Source: "Release_Bin\{#MyAppExeName}"; DestDir: "{app}\bin"; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\bin\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\bin\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\bin\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

