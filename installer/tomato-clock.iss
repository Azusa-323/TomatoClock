; 番茄时钟 Inno Setup 安装脚本
; 编译:iscc installer\tomato-clock.iss
; 前置:先执行 dotnet publish -c Release -r win-x64 --self-contained true

#define MyAppName "番茄时钟"
#define MyAppNameEn "TomatoClock"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "TomatoClock"
#define MyAppExeName "TomatoClock.exe"

[Setup]
AppId={{8B7A6C5D-4E2F-4A9B-9C1D-3F5E7A9B2D41}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppNameEn}
DefaultGroupName={#MyAppName}
UninstallDisplayName={#MyAppName}
OutputDir=Output
OutputBaseFilename=TomatoClock-Setup-{#MyAppVersion}
SetupIconFile=..\src\TomatoClock\Resources\app.ico
Compression=lzma2/max
SolidCompression=yes
WizardStyle=modern
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
; 允许每用户安装,避免需要管理员权限

[Languages]
Name: "chinesesimplified"; MessagesFile: "ChineseSimplified.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "..\src\TomatoClock\bin\Release\net8.0-windows10.0.17763.0\win-x64\publish\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\{cm:UninstallProgram,{#MyAppName}}"; Filename: "{uninstallexe}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#MyAppName}}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; 卸载时清理程序文件(用户数据 %APPDATA%\TomatoClock 保留)
Type: filesandordirs; Name: "{app}"
