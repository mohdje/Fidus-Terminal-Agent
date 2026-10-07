#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd -- "$SCRIPT_DIR/../.." && pwd)"
PROJECT="$REPO_ROOT/Fidus/Fidus.csproj"
PUBLISH_ROOT="$SCRIPT_DIR/publish"

for rid in win-x64 win-arm64; do
    publish_dir="$PUBLISH_ROOT/$rid"
    archive="$SCRIPT_DIR/fidus-$rid.zip"

    rm -rf "$publish_dir"
    mkdir -p "$publish_dir"

    dotnet publish "$PROJECT" \
        --configuration Release \
        --runtime "$rid" \
        --self-contained true \
        --output "$publish_dir"

    (
        cd "$publish_dir"
        zip -qr "$archive" .
    )

    echo "Created $archive"
done

echo "Windows release builds complete."
