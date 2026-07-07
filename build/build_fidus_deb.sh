#!/bin/bash
set -e

# Build Release self-contained version for linux-x64
dotnet publish ../Fidus/Fidus.csproj -c Release -r linux-x64 --self-contained true

rm -rf ./publish/usr/

mkdir -p ./publish/usr/bin/fidusCLI

# Copy the built files to build/publish
cp -r ../Fidus/bin/Release/net10.0/linux-x64/publish/* ./publish/usr/bin/fidusCLI/

# Rename the main executable to "fidus"
mv ./publish/usr/bin/fidusCLI/Fidus ./publish/usr/bin/fidusCLI/fidus

# Build the deb package
dpkg-deb --build ./publish  ./fidus.deb

# Create a zip archive of the deb package
zip -j ./fidus.deb.zip ./fidus.deb

echo "Build and packaging complete. Output: ./fidus.deb, ./fidus.deb.zip"