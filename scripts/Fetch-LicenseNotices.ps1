$ErrorActionPreference = 'Stop'
$destination = Join-Path $PSScriptRoot '../packaging/licenses'
New-Item -ItemType Directory -Path $destination -Force | Out-Null
$sources = [ordered]@{
  'Avalonia-11.3.13-LICENSE.txt' = 'https://raw.githubusercontent.com/AvaloniaUI/Avalonia/11.3.13/licence.md'
  'Markdig-1.3.2-LICENSE.txt' = 'https://raw.githubusercontent.com/xoofx/markdig/1.3.2/license.txt'
  'AvaloniaEdit-11.4.1-LICENSE.txt' = 'https://raw.githubusercontent.com/AvaloniaUI/AvaloniaEdit/11.4.1/LICENSE'
  'Inter-OFL.txt' = 'https://raw.githubusercontent.com/rsms/inter/v4.1/LICENSE.txt'
}
foreach ($item in $sources.GetEnumerator()) { Invoke-WebRequest $item.Value -OutFile (Join-Path $destination $item.Key) }
$sources | ConvertTo-Json | Set-Content (Join-Path $destination 'sources.json') -Encoding utf8
