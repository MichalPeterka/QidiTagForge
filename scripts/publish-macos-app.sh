#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
RUNTIME="${1:-osx-arm64}"
CONFIGURATION="${CONFIGURATION:-Release}"

PROJECT="$ROOT/src/QidiTagForge/QidiTagForge.fsproj"
ICON="$ROOT/src/QidiTagForge/Assets/QidiTagForge.icns"
PUBLISH_DIR="$ROOT/artifacts/publish/$RUNTIME"
APP_DIR="$ROOT/artifacts/QidiTagForge.app"
CONTENTS_DIR="$APP_DIR/Contents"
MACOS_DIR="$CONTENTS_DIR/MacOS"
RESOURCES_DIR="$CONTENTS_DIR/Resources"

if [[ ! -f "$ICON" ]]; then
  echo "Missing icon: $ICON" >&2
  exit 1
fi

dotnet publish "$PROJECT" \
  -c "$CONFIGURATION" \
  -r "$RUNTIME" \
  --self-contained true \
  -o "$PUBLISH_DIR"

rm -rf "$APP_DIR"
mkdir -p "$MACOS_DIR" "$RESOURCES_DIR"

cp -R "$PUBLISH_DIR"/. "$MACOS_DIR"/
cp "$ICON" "$RESOURCES_DIR/QidiTagForge.icns"
chmod +x "$MACOS_DIR/QidiTagForge"

cat > "$CONTENTS_DIR/Info.plist" <<'PLIST'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
  <key>CFBundleDevelopmentRegion</key>
  <string>en</string>
  <key>CFBundleDisplayName</key>
  <string>QidiTagForge</string>
  <key>CFBundleExecutable</key>
  <string>QidiTagForge</string>
  <key>CFBundleIconFile</key>
  <string>QidiTagForge</string>
  <key>CFBundleIdentifier</key>
  <string>com.qiditagforge.app</string>
  <key>CFBundleInfoDictionaryVersion</key>
  <string>6.0</string>
  <key>CFBundleName</key>
  <string>QidiTagForge</string>
  <key>CFBundlePackageType</key>
  <string>APPL</string>
  <key>CFBundleShortVersionString</key>
  <string>1.0.0</string>
  <key>CFBundleVersion</key>
  <string>1</string>
  <key>LSMinimumSystemVersion</key>
  <string>10.15</string>
  <key>NSHighResolutionCapable</key>
  <true/>
</dict>
</plist>
PLIST

echo "Created $APP_DIR"
echo "Run it with: open \"$APP_DIR\""
