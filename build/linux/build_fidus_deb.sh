#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd -- "$SCRIPT_DIR/../.." && pwd)"
PROJECT="$REPO_ROOT/Fidus/Fidus.csproj"
PUBLISH_DIR="$SCRIPT_DIR/publish"

dotnet publish "$PROJECT" -c Release -r linux-x64 --self-contained true

rm -rf "$PUBLISH_DIR/usr/"
mkdir -p "$PUBLISH_DIR/usr/bin/fidusCLI"

cp -r "$REPO_ROOT/Fidus/bin/Release/net10.0/linux-x64/publish/." "$PUBLISH_DIR/usr/bin/fidusCLI/"

mv "$PUBLISH_DIR/usr/bin/fidusCLI/Fidus" "$PUBLISH_DIR/usr/bin/fidusCLI/fidus"

dpkg-deb --build "$PUBLISH_DIR" "$SCRIPT_DIR/fidus.deb"

zip -j "$SCRIPT_DIR/fidus.deb.zip" "$SCRIPT_DIR/fidus.deb"

echo "Build and packaging complete. Output: $SCRIPT_DIR/fidus.deb, $SCRIPT_DIR/fidus.deb.zip"