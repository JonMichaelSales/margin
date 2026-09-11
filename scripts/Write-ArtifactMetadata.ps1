param([Parameter(Mandatory)][string]$Directory, [Parameter(Mandatory)][string]$Runtime, [string]$Version = '0.1.2')
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$commit = git -c "safe.directory=$root" -C $root rev-parse --verify HEAD 2>$null
if ($LASTEXITCODE) { $commit = 'uncommitted' }
$sourceFiles = Get-ChildItem (Join-Path $root 'src') -Recurse -File | Where-Object { $_.FullName -notmatch '[\\/](bin|obj)[\\/]' } | Sort-Object FullName | ForEach-Object {
  @{ path = [IO.Path]::GetRelativePath($root, $_.FullName); sha256 = (Get-FileHash $_.FullName -Algorithm SHA256).Hash }
}
$dirty = [bool](git -c "safe.directory=$root" -C $root status --porcelain)
$metadata = @{ dirtyWorkingTree=$dirty; application='Margin'; version=$Version; runtime=$Runtime; commit=$commit; builtUtc=[DateTime]::UtcNow.ToString('O'); sdk=(dotnet --version); source=$sourceFiles }
$metadata | ConvertTo-Json -Depth 8 | Set-Content (Join-Path $Directory 'build-info.json') -Encoding utf8
$assets = Get-Content (Join-Path $root 'src/MDPlayer.Desktop/obj/project.assets.json') -Raw | ConvertFrom-Json -AsHashtable
$cache = @($assets.packageFolders.Keys)[0]
$notices = @('# Margin third-party notices', '', 'This personal test build includes the following NuGet packages. License expressions and package license files are recorded below.', '')
$licenseDir = Join-Path $Directory 'licenses'
New-Item -ItemType Directory -Force $licenseDir | Out-Null
foreach ($key in ($assets.libraries.Keys | Sort-Object)) {
  if ($assets.libraries[$key].type -ne 'package') { continue }
  $package = Join-Path $cache $assets.libraries[$key].path
  $nuspecPath = Get-ChildItem $package -Filter '*.nuspec' | Select-Object -First 1
  [xml]$nuspec = Get-Content $nuspecPath.FullName -Raw
  $node = $nuspec.SelectSingleNode('//*[local-name()="metadata"]')
  $license = $node.SelectSingleNode('*[local-name()="license"]')
  $notices += "- $key — $($license.InnerText) — https://www.nuget.org/packages/$key"
  foreach ($file in (Get-ChildItem $package -File -Recurse | Where-Object { $_.Name -match '^(license|licence|notice|copying)' -or ($license.type -eq 'file' -and [IO.Path]::GetRelativePath($package, $_.FullName).Replace('\','/') -eq $license.InnerText) })) {
    $name = $key.Replace('/', '-') + '-' + $file.Name
    Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $licenseDir $name)
  }
}
$runtimeVersion = (Get-Content (Join-Path $Directory 'Margin.runtimeconfig.json') -Raw | ConvertFrom-Json).runtimeOptions.includedFrameworks[0].version
$runtimePack = Join-Path $cache "microsoft.netcore.app.runtime.$Runtime/$runtimeVersion"
foreach ($name in @('LICENSE.TXT','THIRD-PARTY-NOTICES.TXT')) {
  $runtimeNotice = Join-Path $runtimePack $name
  if (!(Test-Path -LiteralPath $runtimeNotice)) { throw "Missing runtime notice: $runtimeNotice" }
  Copy-Item -LiteralPath $runtimeNotice -Destination (Join-Path $licenseDir "dotnet-runtime-$name")
}
if (Test-Path (Join-Path $root 'packaging/licenses')) { Copy-Item (Join-Path $root 'packaging/licenses/*') -Destination $licenseDir -Force }
$notices += "- Microsoft.NETCore.App $runtimeVersion — bundled runtime license and third-party notices are included."
$notices | Set-Content (Join-Path $Directory 'THIRD-PARTY-NOTICES.md') -Encoding utf8
Get-ChildItem $Directory -Recurse -File | Where-Object Name -ne 'SHA256SUMS.txt' | Sort-Object FullName | ForEach-Object {
  (Get-FileHash $_.FullName -Algorithm SHA256).Hash.ToLowerInvariant() + '  ' + [IO.Path]::GetRelativePath($Directory, $_.FullName).Replace('\','/')
} | Set-Content (Join-Path $Directory 'SHA256SUMS.txt') -Encoding utf8
