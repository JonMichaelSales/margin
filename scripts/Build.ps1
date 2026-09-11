param([ValidateSet('Debug','Release')][string]$Configuration = 'Release')
$ErrorActionPreference = 'Stop'
Push-Location (Join-Path $PSScriptRoot '..')
try {
  & "$PSScriptRoot/Test-Colors.ps1"
  dotnet restore MDPlayer.slnx --locked-mode
  if ($LASTEXITCODE) { throw 'Locked restore failed.' }
  dotnet build MDPlayer.slnx -c $Configuration --no-restore
  if ($LASTEXITCODE) { throw 'Build failed.' }
  dotnet test MDPlayer.slnx -c $Configuration --no-build --no-restore --logger 'trx;LogFileName=MDPlayer.trx'
  if ($LASTEXITCODE) { throw 'Tests failed.' }
} finally { Pop-Location }
