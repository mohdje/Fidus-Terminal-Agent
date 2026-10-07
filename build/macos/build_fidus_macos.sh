#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd -- "$SCRIPT_DIR/../.." && pwd)"
PROJECT="$REPO_ROOT/Fidus/Fidus.csproj"
PUBLISH_ROOT="$SCRIPT_DIR/publish"

for rid in osx-arm64 osx-x64; do
    publish_dir="$PUBLISH_ROOT/$rid"
    archive="$SCRIPT_DIR/fidus-$rid.tar.gz"

    rm -rf "$publish_dir"
    mkdir -p "$publish_dir"

    dotnet publish "$PROJECT" \
        --configuration Release \
        --runtime "$rid" \
        --self-contained true \
        --output "$publish_dir"

    mv "$publish_dir/Fidus" "$publish_dir/fidus"
    tar -czf "$archive" -C "$publish_dir" .

    echo "Created $archive"
done

echo "macOS release builds complete."
