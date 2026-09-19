#!/bin/bash
set -euo pipefail
rid="${1:-osx-arm64}"
version="${2:-0.1.5}"
case "$rid" in osx-x64|osx-arm64) ;; *) echo "Expected osx-x64 or osx-arm64" >&2; exit 1 ;; esac
[[ "$(uname -s)" == Darwin ]] || { echo 'DMG creation and signing require macOS.' >&2; exit 1; }
root="$(cd "$(dirname "$0")/.." && pwd)"
cd "$root"
command -v pwsh >/dev/null || { echo 'PowerShell 7 is required for shared publishing and metadata.' >&2; exit 1; }
pwsh -NoProfile -File scripts/Publish.ps1 -Runtime "$rid" -Version "$version"
stage="$(mktemp -d "$root/artifacts/margin-dmg.XXXXXX")"
app="$stage/Margin.app"
mkdir -p "$app/Contents/MacOS" "$app/Contents/Resources" "$root/artifacts/installers"
cp -R "$root/artifacts/publish/margin/$rid/." "$app/Contents/MacOS/"
chmod +x "$app/Contents/MacOS/Margin"
sed "s/VERSION_PLACEHOLDER/$version/g" packaging/macos/Info.plist > "$app/Contents/Info.plist"
plutil -lint "$app/Contents/Info.plist"
iconutil -c icns src/MDPlayer.Desktop/Assets/Margin.iconset -o "$app/Contents/Resources/Margin.icns"
iconutil -c icns src/MDPlayer.Desktop/Assets/Margin.Document.iconset -o "$app/Contents/Resources/Margin.Document.icns"
while IFS= read -r -d '' binary; do
  if file "$binary" | grep -q 'Mach-O'; then codesign --force --sign - "$binary"; fi
done < <(find "$app/Contents/MacOS" -type f -print0)
codesign --force --sign - "$app"
codesign --verify --deep --strict --verbose=2 "$app"
ln -s /Applications "$stage/Applications"
dmg="$root/artifacts/installers/Margin-$version-$rid.dmg"
[[ ! -e "$dmg" ]] || { echo "Artifact already exists: $dmg" >&2; exit 1; }
mkdir -p "$stage/.background"
cp packaging/assets/dmg-background.png "$stage/.background/background.png"
writable="$stage-layout.dmg"
mount="$stage-mount"
mkdir "$mount"
hdiutil create -volname "Margin $version" -srcfolder "$stage" -format UDRW "$writable"
hdiutil attach "$writable" -mountpoint "$mount" -nobrowse
trap 'hdiutil detach "$mount" >/dev/null 2>&1 || true' EXIT
osascript - "$mount" <<'APPLESCRIPT'
on run argv
  set mountedFolder to POSIX file (item 1 of argv) as alias
  tell application "Finder"
    open mountedFolder
    set diskWindow to container window of mountedFolder
    set current view of diskWindow to icon view
    set toolbar visible of diskWindow to false
    set statusbar visible of diskWindow to false
    set bounds of diskWindow to {100, 100, 820, 540}
    set options to icon view options of diskWindow
    set arrangement of options to not arranged
    set icon size of options to 96
    set background picture of options to file ".background:background.png" of mountedFolder
    set position of item "Margin.app" of mountedFolder to {190, 220}
    set position of item "Applications" of mountedFolder to {530, 220}
    update mountedFolder without registering applications
    delay 2
    close diskWindow
  end tell
end run
APPLESCRIPT
sync
hdiutil detach "$mount"
trap - EXIT
hdiutil convert "$writable" -format UDZO -o "$dmg"
hdiutil verify "$dmg"
shasum -a 256 "$dmg" > "$dmg.sha256"
echo "Built $dmg. Install and native testing remain separate steps. Staging retained at $stage."
