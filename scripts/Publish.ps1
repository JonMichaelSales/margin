param([Parameter(Mandatory)][ValidateSet('win-x64','win-arm64','osx-x64','osx-arm64')][string]$Runtime, [string]$Version = '0.1.2')
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$destination = Join-Path $root "artifacts/publish/margin/$Runtime"
Push-Location $root
try {
  if ($Version -notmatch '^\d+\.\d+\.\d+$') { throw 'Version must be three numeric components.' }
  dotnet publish src/MDPlayer.Desktop/MDPlayer.Desktop.csproj -c Release -r $Runtime --self-contained true -p:PublishReadyToRun=true -p:PublishTrimmed=false -p:PublishSingleFile=false -p:Version=$Version -p:RestoreLockedMode=true -o $destination
  if ($LASTEXITCODE) { throw "Publish failed for $Runtime." }
  & "$PSScriptRoot/Write-ArtifactMetadata.ps1" -Directory $destination -Runtime $Runtime -Version $Version
  Write-Output $destination
} finally { Pop-Location }
