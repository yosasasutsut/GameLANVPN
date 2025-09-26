[Setup]
; Basic Information
AppName=GameLANVPN Client
AppVersion=1.0.0
AppPublisher=GameLANVPN Development Team
AppPublisherURL=https://github.com/yosasasutsut/GameLANVPN
AppSupportURL=https://github.com/yosasasutsut/GameLANVPN/issues
AppUpdatesURL=https://github.com/yosasasutsut/GameLANVPN/releases
AppCopyright=Copyright © 2024 GameLANVPN
VersionInfoVersion=1.0.0

; Installation Settings
DefaultDirName={autopf}\GameLANVPN
DefaultGroupName=GameLANVPN
UninstallDisplayName=GameLANVPN Client
UninstallDisplayIcon={app}\GameLANVPN.Client.exe
AllowNoIcons=yes
OutputDir=installer
OutputBaseFilename=GameLANVPN-Setup-v1.0.0
Compression=lzma
SolidCompression=yes
SetupIconFile=.\assets\app.ico
WizardImageFile=.\assets\wizard-large.bmp
WizardSmallImageFile=.\assets\wizard-small.bmp

; System Requirements
MinVersion=10.0.17763
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64

; Privileges and Security
PrivilegesRequired=admin
PrivilegesRequiredOverridesAllowed=dialog

; UI Settings
WizardStyle=modern
DisableProgramGroupPage=no
DisableReadyPage=no
DisableStartupPrompt=yes
DisableFinishedPage=no
ShowLanguageDialog=no
LicenseFile=.\assets\LICENSE.txt

; Directory and Registry Settings
ChangesAssociations=no
ChangesEnvironment=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: checked
Name: "quicklaunchicon"; Description: "{cm:CreateQuickLaunchIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked; OnlyBelowVersion: 6.1
Name: "startupicon"; Description: "Start GameLANVPN with Windows"; GroupDescription: "Startup Options"; Flags: unchecked

[Files]
; Main application files
Source: ".\publish\client\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: ".\publish\core\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

; Configuration and documentation
Source: ".\assets\README.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\assets\LICENSE.txt"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\assets\appsettings.json"; DestDir: "{app}"; Flags: ignoreversion

; Icons and assets
Source: ".\assets\app.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
; Start Menu
Name: "{group}\GameLANVPN Client"; Filename: "{app}\GameLANVPN.Client.exe"; IconFilename: "{app}\app.ico"
Name: "{group}\GameLANVPN README"; Filename: "{app}\README.txt"
Name: "{group}\{cm:UninstallProgram,GameLANVPN Client}"; Filename: "{uninstallexe}"

; Desktop Icon
Name: "{autodesktop}\GameLANVPN Client"; Filename: "{app}\GameLANVPN.Client.exe"; IconFilename: "{app}\app.ico"; Tasks: desktopicon

; Quick Launch (Windows 7 and below)
Name: "{userappdata}\Microsoft\Internet Explorer\Quick Launch\GameLANVPN Client"; Filename: "{app}\GameLANVPN.Client.exe"; IconFilename: "{app}\app.ico"; Tasks: quicklaunchicon

[Registry]
; Startup entry (optional)
Root: HKCU; Subkey: "SOFTWARE\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "GameLANVPN"; ValueData: """{app}\GameLANVPN.Client.exe"" --minimized"; Flags: uninsdeletevalue; Tasks: startupicon

; Application registration
Root: HKLM; Subkey: "SOFTWARE\GameLANVPN"; ValueType: string; ValueName: "InstallPath"; ValueData: "{app}"; Flags: uninsdeletekey
Root: HKLM; Subkey: "SOFTWARE\GameLANVPN"; ValueType: string; ValueName: "Version"; ValueData: "1.0.0"; Flags: uninsdeletekey

[Run]
; Post-installation actions
Filename: "{app}\GameLANVPN.Client.exe"; Description: "{cm:LaunchProgram,GameLANVPN Client}"; Flags: nowait postinstall skipifsilent

[UninstallRun]
; Cleanup tasks
Filename: "taskkill"; Parameters: "/F /IM GameLANVPN.Client.exe"; Flags: runhidden; RunOnceId: "KillGameLANVPN"

[UninstallDelete]
; Remove user data (optional)
Type: files; Name: "{app}\logs\*"
Type: dirifempty; Name: "{app}\logs"

[Code]
// Check for .NET 8 Runtime
function InitializeSetup(): Boolean;
var
  ResultCode: Integer;
  DotNetInstalled: Boolean;
begin
  Result := True;

  // Check if .NET 8 runtime is installed
  if Exec(ExpandConstant('{cmd}'), '/C dotnet --list-runtimes | findstr "Microsoft.WindowsDesktop.App 8."', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) then
  begin
    DotNetInstalled := (ResultCode = 0);
  end
  else
  begin
    DotNetInstalled := False;
  end;

  if not DotNetInstalled then
  begin
    if MsgBox('.NET 8 Desktop Runtime is required but not installed.' + #13#13 +
              'Would you like to download and install it now?' + #13#13 +
              'Note: Internet connection required.',
              mbConfirmation, MB_YESNO) = IDYES then
    begin
      ShellExec('open', 'https://dotnet.microsoft.com/en-us/download/dotnet/8.0', '', '', SW_SHOWNORMAL, ewNoWait, ResultCode);
      MsgBox('Please install .NET 8 Desktop Runtime and run this installer again.', mbInformation, MB_OK);
      Result := False;
    end
    else
    begin
      MsgBox('GameLANVPN requires .NET 8 Desktop Runtime to function properly.' + #13#13 +
             'The installation will continue, but the application may not work correctly.',
             mbInformation, MB_OK);
    end;
  end;
end;

// Check for administrator privileges for network features
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    if not IsAdminLoggedOn then
    begin
      MsgBox('GameLANVPN may require administrator privileges for virtual network features.' + #13#13 +
             'If you experience network connectivity issues, try running as administrator.',
             mbInformation, MB_OK);
    end;
  end;
end;

// Custom welcome message
function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;

  if CurPageID = wpWelcome then
  begin
    MsgBox('Welcome to GameLANVPN Client Setup!' + #13#13 +
           'This installer will install GameLANVPN Client on your computer.' + #13#13 +
           'GameLANVPN allows you to play LAN games over the internet by creating virtual network connections.' + #13#13 +
           'System Requirements:' + #13 +
           '• Windows 10/11 (64-bit)' + #13 +
           '• .NET 8 Desktop Runtime' + #13 +
           '• Administrator privileges for network features',
           mbInformation, MB_OK);
  end;
end;