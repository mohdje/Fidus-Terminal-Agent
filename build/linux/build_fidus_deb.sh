#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd -- "$SCRIPT_DIR/../.." && pwd)"
PROJECT="$REPO_ROOT/Fidus/Fidus.csproj"
PUBLISH_DIR="$SCRIPT_DIR/publish"
VERSION="$(dotnet msbuild "$PROJECT" -nologo -getProperty:Version)"

if [[ -z "$VERSION" ]]; then
    echo "Unable to determine the project version from $PROJECT" >&2
    exit 1
fi

dotnet publish "$PROJECT" -c Release -r linux-x64 --self-contained true /p:Version="$VERSION"

rm -rf "$PUBLISH_DIR/"
mkdir -p "$PUBLISH_DIR/usr/bin/fidusCLI"
mkdir -p "$PUBLISH_DIR/DEBIAN"

cp -r "$REPO_ROOT/Fidus/bin/Release/net10.0/linux-x64/publish/." "$PUBLISH_DIR/usr/bin/fidusCLI/"

mv "$PUBLISH_DIR/usr/bin/fidusCLI/Fidus" "$PUBLISH_DIR/usr/bin/fidusCLI/fidus"

cat > "$PUBLISH_DIR/DEBIAN/control" <<EOF
Package: fidus
Version: ${VERSION}
Section: utils
Priority: optional
Architecture: all
Maintainer: mohdje <djellebm@hotmail.fr>
Description: CLI AI assistant
EOF

dpkg-deb --build "$PUBLISH_DIR" "$SCRIPT_DIR/fidus.deb"

zip -j "$SCRIPT_DIR/fidus.deb.zip" "$SCRIPT_DIR/fidus.deb"

echo "Build and packaging complete. Output: $SCRIPT_DIR/fidus.deb, $SCRIPT_DIR/fidus.deb.zip"