#!/usr/bin/env bash
set -euo pipefail

REPO="https://github.com/mohdje/Fidus-Terminal-Agent/releases/latest/download"

case "$(uname -m)" in
    arm64|aarch64) rid="osx-arm64" ;;
    x86_64) rid="osx-x64" ;;
    *)
        echo "Unsupported macOS architecture: $(uname -m)" >&2
        exit 1
        ;;
esac

archive="fidus-$rid.tar.gz"
install_dir="$HOME/.local/lib/fidusCLI"
bin_dir="$HOME/.local/bin"
binary_link="$bin_dir/fidus"
temp_dir="$(mktemp -d)"
trap 'rm -rf "$temp_dir"' EXIT

curl --fail --location --silent --show-error \
    "$REPO/$archive" \
    --output "$temp_dir/$archive"
mkdir "$temp_dir/extracted"
tar -xzf "$temp_dir/$archive" -C "$temp_dir/extracted"

if [[ ! -f "$temp_dir/extracted/fidus" ]]; then
    echo "The downloaded archive does not contain the fidus executable." >&2
    exit 1
fi

if [[ -e "$binary_link" && ! -L "$binary_link" ]]; then
    echo "Refusing to replace the existing file: $binary_link" >&2
    exit 1
fi

mkdir -p "$install_dir" "$bin_dir"
cp -R "$temp_dir/extracted/." "$install_dir/"
chmod +x "$install_dir/fidus"
ln -sfn "$install_dir/fidus" "$binary_link"

echo "Fidus CLI installed at $install_dir/fidus"
if [[ ":$PATH:" != *":$bin_dir:"* ]]; then
    echo "Add it to your PATH with: export PATH=\"$bin_dir:\$PATH\""
fi
