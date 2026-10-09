#!/bin/bash
set -e

#fetch the latest version of the release from github 
curl -sL https://github.com/mohdje/Fidus-Terminal-Agent/releases/latest/download/fidus.deb.zip -o fidus.deb.zip

# Unzip the downloaded file and install the deb package
unzip -o fidus.deb.zip
dpkg -i fidus.deb

# Create a symbolic link for easier access
ln -s /usr/bin/fidusCLI/fidus /usr/bin/fidus

# Cleanup
rm fidus.deb
rm fidus.deb.zip