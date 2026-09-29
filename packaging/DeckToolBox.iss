; Windows 安装程序（Inno Setup 6），由 GitHub Actions 调用：
;   ISCC /DAppVersion=0.1.0 /DSourceDir=<publish 目录> /DOutputDir=<输出目录> packaging\DeckToolBox.iss
; 按用户安装到 %LOCALAPPDATA%\Programs，不需要管理员权限。

[Setup]
AppId={{6B0C4C1E-3D52-4B7A-9E0F-DEC7B0C51A01}
AppName=DeckToolBox
AppVersion={#AppVersion}
AppPublisher=cxy251
AppPublisherURL=https://github.com/cxy-251/DeckToolBox
DefaultDirName={autopf}\DeckToolBox
DefaultGroupName=DeckToolBox
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputDir={#OutputDir}
OutputBaseFilename=DeckToolBox-{#AppVersion}-win-x64-setup
SetupIconFile=..\src\DeckToolBox.App\Assets\icon.ico
UninstallDisplayIcon={app}\DeckToolBox.exe
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\DeckToolBox.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\DeckToolBox"; Filename: "{app}\DeckToolBox.exe"
Name: "{autodesktop}\DeckToolBox"; Filename: "{app}\DeckToolBox.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\DeckToolBox.exe"; Description: "{cm:LaunchProgram,DeckToolBox}"; Flags: nowait postinstall skipifsilent
