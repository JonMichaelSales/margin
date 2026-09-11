#ifndef AppVersion
  #define AppVersion "0.1.2"
#endif
#ifndef TargetArch
  #define TargetArch "x64"
#endif
#ifndef PublishDir
  #error PublishDir must point at the self-contained publish directory.
#endif
#ifndef ArtifactDir
  #error ArtifactDir must be provided.
#endif
#if TargetArch == "arm64"
  #define ArchAllowed "arm64"
#elif TargetArch == "x64"
  #define ArchAllowed "x64os"
#else
  #error Unsupported architecture.
#endif

[Setup]
AppId={{E03534B1-0C22-4228-9537-788B9997C661}
AppName=Margin
AppVerName=Margin
UninstallDisplayName=Margin
AppVersion={#AppVersion}
AppPublisher=Jon Sales
DefaultDirName={localappdata}\Programs\MDPlayer
DefaultGroupName=Margin
UsePreviousGroup=no
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed={#ArchAllowed}
ArchitecturesInstallIn64BitMode={#ArchAllowed}
MinVersion=10.0.22000
OutputDir={#ArtifactDir}
OutputBaseFilename=Margin-{#AppVersion}-win-{#TargetArch}-setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
WizardImageFile=..\assets\wizard.bmp
WizardSmallImageFile=..\assets\header.bmp
WizardImageStretch=yes
UninstallDisplayIcon={app}\Margin.exe
SetupIconFile=..\..\src\MDPlayer.Desktop\Assets\Margin.ico
ChangesAssociations=yes
CloseApplications=no
RestartApplications=no
AppMutex=MDPlayer.Running

[Tasks]
Name: desktopicon; Description: "Create a desktop shortcut"; Flags: unchecked

[Files]
Source: "{#PublishDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Margin"; Filename: "{app}\Margin.exe"
Name: "{autodesktop}\Margin"; Filename: "{app}\Margin.exe"; Tasks: desktopicon

[InstallDelete]
Type: files; Name: "{app}\MDPlayer.Desktop.exe"
Type: files; Name: "{app}\MDPlayer.Desktop.dll"
Type: files; Name: "{app}\MDPlayer.Desktop.deps.json"
Type: files; Name: "{app}\MDPlayer.Desktop.runtimeconfig.json"
Type: files; Name: "{app}\MDPlayer.Desktop.pdb"
Type: files; Name: "{userprograms}\MDPlayer\MDPlayer.lnk"
Type: files; Name: "{autodesktop}\MDPlayer.lnk"

[Registry]
Root: HKCU; Subkey: "Software\Classes\MDPlayer.Markdown"; ValueType: string; ValueData: "Markdown document"; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\MDPlayer.Markdown\DefaultIcon"; ValueType: string; ValueData: """{app}\Assets\Margin.Document.ico"",0"
Root: HKCU; Subkey: "Software\Classes\MDPlayer.Markdown\shell\open\command"; ValueType: string; ValueData: """{app}\Margin.exe"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\.md\OpenWithProgids"; ValueType: none; ValueName: "MDPlayer.Markdown"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\.markdown\OpenWithProgids"; ValueType: none; ValueName: "MDPlayer.Markdown"; Flags: uninsdeletevalue
Root: HKCU; Subkey: "Software\Classes\Applications\Margin.exe"; ValueType: string; ValueName: FriendlyAppName; ValueData: Margin; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Applications\Margin.exe\shell\open\command"; ValueType: string; ValueData: """{app}\Margin.exe"" ""%1"""
Root: HKCU; Subkey: "Software\Classes\Applications\Margin.exe\SupportedTypes"; ValueType: string; ValueName: ".md"; ValueData: ""
Root: HKCU; Subkey: "Software\Classes\Applications\Margin.exe\SupportedTypes"; ValueType: string; ValueName: ".markdown"; ValueData: ""

Root: HKCU; Subkey: "Software\Classes\Applications\MDPlayer.Desktop.exe"; ValueType: string; ValueName: FriendlyAppName; ValueData: Margin; Flags: uninsdeletekey
Root: HKCU; Subkey: "Software\Classes\Applications\MDPlayer.Desktop.exe\shell\open\command"; ValueType: string; ValueData: """{app}\Margin.exe"" ""%1"""

[Run]
Filename: "{app}\Margin.exe"; Description: "Open Margin"; Flags: nowait postinstall skipifsilent unchecked

[Code]
function InitializeUninstall(): Boolean;
begin
  Result := not CheckForMutexes('MDPlayer.Running');
  if not Result then
    MsgBox('Margin or an earlier MDPlayer version is running. Save or discard your edits and close its windows before uninstalling.', mbInformation, MB_OK);
end;
