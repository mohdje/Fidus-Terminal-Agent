#!/bin/bash
set -e

# Build Release self-contained version for linux-x64
dotnet publish -c Release -r linux-x64 --self-contained true

# Copy the built files to Fidus/publish
cp Fidus/bin/Release/net10.0/linux-x64/publish/* Fidus/publish/usr/bin/

# Build the deb package
dpkg-deb --build Fidus/publish  Fidus/fidus.deb

# Create a zip archive of the deb package
zip -j Fidus/fidus.zip Fidus/fidus.deb

echo "Build and packaging complete. Output: fidus.deb"