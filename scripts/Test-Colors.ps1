$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$violations = @()
Get-ChildItem (Join-Path $root 'src') -Recurse -File -Include *.cs,*.axaml | Where-Object { $_.FullName -notmatch '[\\/](obj|bin)[\\/]' } | ForEach-Object {
  $line = 0
  foreach ($text in [IO.File]::ReadLines($_.FullName)) {
    $line++
    if ($text -match '#[0-9a-fA-F]{6}([0-9a-fA-F]{2})?\b|\b(Colors|Brushes)\.\w+|\bColor\.(FromRgb|FromArgb|Parse)\s*\(') {
      $violations += "$($_.FullName):${line}: $text"
    }
    if ($text -match '(Background|Foreground|BorderBrush|Fill|Stroke|CaretBrush|SelectionBrush)="(?!\{)[A-Za-z#]') {
      $violations += "$($_.FullName):${line}: fixed AXAML brush"
    }
  }
}
if ($violations.Count) { $violations | Write-Output; throw "$($violations.Count) fixed application colors found." }
Write-Output 'Color audit passed: application source uses named skin resources.'
