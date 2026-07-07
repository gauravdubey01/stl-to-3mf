; Inno Setup script for STL to 3MF Converter
; Requires Inno Setup 6+ (https://jrsoftware.org/isdl.php)

#define MyAppName "STL to 3MF Converter"
#define MyAppVersion "2.0"
#define MyAppPublisher "Your Company"
#define MyAppExeName "StlTo3mfConverter.exe"

[Setup]
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
DefaultDirName={autopf}\{#MyAppName}
DefaultGroupName={#MyAppName}
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma2
SolidCompression=yes
OutputDir=.
OutputBaseFilename=StlTo3mfConverter_Setup
SetupIconFile=E:\MY COMPANY\WINDOWS PROGRAM\STL TO 3MF CONVERTER\3MF.ico
WizardStyle=modern
DisableProgramGroupPage=yes

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"

[Files]
Source: "StlTo3mfConverter.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "StlTo3mfConverter.cs"; DestDir: "{app}\Source"; Flags: ignoreversion
Source: "StlParser.cs"; DestDir: "{app}\Source"; Flags: ignoreversion
Source: "ThreeMfWriter.cs"; DestDir: "{app}\Source"; Flags: ignoreversion
Source: "E:\MY COMPANY\WINDOWS PROGRAM\STL TO 3MF CONVERTER\3MF.ico"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\3MF.ico"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; WorkingDir: "{app}"; IconFilename: "{app}\3MF.ico"

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "Launch STL to 3MF Converter"; Flags: postinstall nowait skipifsilent

[UninstallRun]
