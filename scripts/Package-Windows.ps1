param([ValidateSet('x64','arm64')][string]$Architecture = 'x64', [string]$Version = '0.1.5', [string]$Compiler = 'C:/Program Files (x86)/Inno Setup 6/ISCC.exe')
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$toolchain = Get-Content (Join-Path $root 'packaging/windows/toolchain.json') -Raw | ConvertFrom-Json
if (!(Test-Path -LiteralPath $Compiler)) { throw "Install the pinned Inno Setup compiler listed in packaging/windows/toolchain.json." }
if ((Get-FileHash $Compiler -Algorithm SHA256).Hash -ne $toolchain.isccSha256) { throw 'The Inno Setup compiler does not match the pinned toolchain.' }
& "$PSScriptRoot/Publish.ps1" -Runtime "win-$Architecture" -Version $Version
$output = Join-Path $root 'artifacts/installers'
New-Item -ItemType Directory -Force $output | Out-Null
& $Compiler /Qp "/DAppVersion=$Version" "/DTargetArch=$Architecture" "/DPublishDir=$root/artifacts/publish/margin/win-$Architecture" "/DArtifactDir=$output" "$root/packaging/windows/MDPlayer.iss"
if ($LASTEXITCODE) { throw 'Inno Setup compilation failed.' }
$installer = Join-Path $output "Margin-$Version-win-$Architecture-setup.exe"
(Get-FileHash $installer -Algorithm SHA256).Hash.ToLowerInvariant() + '  ' + [IO.Path]::GetFileName($installer) | Set-Content "$installer.sha256" -Encoding utf8
Write-Output $installer
