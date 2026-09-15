param([string]$Version = '0.1.4')
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$installer = Join-Path $root "artifacts/installers/Margin-$Version-win-x64-setup.exe"
$installed = Join-Path $env:LOCALAPPDATA 'Programs/MDPlayer'
$settings = Join-Path $env:LOCALAPPDATA 'MDPlayer/preferences.json'
$fixture = Join-Path $root 'artifacts/qualification/Reader sample.md'
$evidence = Join-Path $root 'artifacts/qualification'
if (Get-Process Margin,MDPlayer.Desktop -ErrorAction SilentlyContinue) { throw 'Close Margin and MDPlayer before lifecycle testing.' }
if (!(Test-Path -LiteralPath $fixture)) { throw 'The qualification fixture must exist before testing.' }
if (!(Test-Path -LiteralPath (Join-Path $installed 'unins000.exe'))) { throw 'Install the prior version before running upgrade qualification.' }
New-Item -ItemType Directory -Force $evidence | Out-Null
function Hash-OrMissing([string]$path) { if (Test-Path -LiteralPath $path) { return (Get-FileHash -LiteralPath $path -Algorithm SHA256).Hash }; return 'missing' }
function Default-Associations {
  $result = [ordered]@{}
  foreach ($extension in @('.md','.markdown')) {
    foreach ($keyPath in @("Software\Classes\$extension", "Software\Microsoft\Windows\CurrentVersion\Explorer\FileExts\$extension\UserChoice")) {
      $key = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey($keyPath)
      $result[$keyPath] = if ($null -eq $key) { $null } else { @($key.GetValue(''), $key.GetValue('ProgId'), $key.GetValue('Hash')); $key.Dispose() }
    }
  }
  return ConvertTo-Json -InputObject $result -Compress
}
$before = @{ settings = Hash-OrMissing $settings; document = Hash-OrMissing $fixture; defaults = Default-Associations; version = (Get-Content (Join-Path $installed 'build-info.json') -Raw | ConvertFrom-Json).version }
function Assert-Preserved {
  if ((Hash-OrMissing $settings) -ne $before.settings) { throw 'Preferences changed during installation or uninstall.' }
  if ((Hash-OrMissing $fixture) -ne $before.document) { throw 'The Markdown fixture changed during installation or uninstall.' }
  if ((Default-Associations) -ne $before.defaults) { throw 'Default Markdown associations changed.' }
}
function Install([string]$phase) {
  $log = Join-Path $evidence "margin-$Version-$phase.log"
  $process = Start-Process -FilePath $installer -ArgumentList @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',"/LOG=`"$log`"") -WindowStyle Hidden -PassThru -Wait
  if ($process.ExitCode -ne 0) { throw "$phase returned $($process.ExitCode). See $log" }
  $expected = Join-Path $root 'artifacts/publish/margin/win-x64/Margin.exe'
  if ((Hash-OrMissing (Join-Path $installed 'Margin.exe')) -ne (Hash-OrMissing $expected)) { throw 'Installed executable differs from the qualified publish output.' }
  Assert-Preserved
}
Install 'upgrade'
if (Test-Path -LiteralPath (Join-Path $installed 'MDPlayer.Desktop.exe')) { throw 'Upgrade left the legacy application launcher installed.' }
$legacyKey = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\Classes\Applications\MDPlayer.Desktop.exe\shell\open\command')
if ($null -eq $legacyKey) { throw 'The compatibility handler for existing Windows choices is missing.' }
$legacyCommand = $legacyKey.GetValue(''); $legacyKey.Dispose()
if ($legacyCommand -ne ('"' + (Join-Path $installed 'Margin.exe') + '" "%1"')) { throw 'The old application handler does not resolve to Margin.' }
if ((Get-Item (Join-Path $installed 'Margin.exe')).VersionInfo.ProductName -ne 'Margin') { throw 'Installed product metadata still uses the old brand.' }
$commandKey = [Microsoft.Win32.Registry]::CurrentUser.OpenSubKey('Software\Classes\MDPlayer.Markdown\shell\open\command')
$command = $commandKey.GetValue(''); $commandKey.Dispose()
if ($command -ne ('"' + (Join-Path $installed 'Margin.exe') + '" "%1"')) { throw 'Markdown handler command is not correctly quoted.' }
$uninstallLog = Join-Path $evidence "margin-$Version-uninstall.log"
$process = Start-Process -FilePath (Join-Path $installed 'unins000.exe') -ArgumentList @('/VERYSILENT','/SUPPRESSMSGBOXES','/NORESTART',"/LOG=`"$uninstallLog`"") -WindowStyle Hidden -PassThru -Wait
if ($process.ExitCode -ne 0) { throw "Uninstall returned $($process.ExitCode)." }
if (Test-Path -LiteralPath (Join-Path $installed 'Margin.exe')) { throw 'Uninstall left the application executable installed.' }
Assert-Preserved
Install 'reinstall'
$result = @{ testedUtc = [DateTime]::UtcNow.ToString('O'); architecture = 'win-x64'; upgradedFrom = $before.version; upgradedTo = $Version; upgrade = 'passed'; uninstall = 'passed'; reinstall = 'passed'; settingsPreserved = $true; documentPreserved = $true; defaultsPreserved = $true; quotedHandler = $command; installerSha256 = (Get-FileHash $installer).Hash; cleanMachineWithoutDotnet = 'not tested'; nativeArm64 = 'not tested' }
$result | ConvertTo-Json | Set-Content (Join-Path $evidence "margin-$Version-windows-lifecycle.json") -Encoding utf8
$result | ConvertTo-Json
