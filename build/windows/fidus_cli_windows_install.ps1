$ErrorActionPreference = 'Stop'
$ProgressPreference = 'SilentlyContinue'

$architecture = $env:PROCESSOR_ARCHITEW6432
if ([string]::IsNullOrWhiteSpace($architecture)) {
    $architecture = $env:PROCESSOR_ARCHITECTURE
}
if ([string]::IsNullOrWhiteSpace($architecture)) {
    throw 'Could not determine the Windows processor architecture.'
}

switch ($architecture.ToUpperInvariant()) {
    'AMD64' { $runtime = 'win-x64' }
    'ARM64' { $runtime = 'win-arm64' }
    default {
        throw "Unsupported Windows architecture: $architecture"
    }
}

if ([string]::IsNullOrWhiteSpace($env:LOCALAPPDATA)) {
    throw 'LOCALAPPDATA is not set; run this script from a normal Windows user session.'
}

$releaseUrl = "https://github.com/mohdje/Fidus-Terminal-Agent/releases/latest/download/fidus-$runtime.zip"
$installDirectory = Join-Path $env:LOCALAPPDATA 'Programs\FidusCLI'
$installParent = Split-Path -Parent $installDirectory
$temporaryDirectory = Join-Path ([System.IO.Path]::GetTempPath()) "fidus-install-$([guid]::NewGuid().ToString('N'))"
$stagingDirectory = "$installDirectory.install-$([guid]::NewGuid().ToString('N'))"
$backupDirectory = "$installDirectory.backup-$([guid]::NewGuid().ToString('N'))"
$archivePath = Join-Path $temporaryDirectory 'fidus.zip'
$backupCreated = $false

try {
    New-Item -ItemType Directory -Path $temporaryDirectory | Out-Null
    New-Item -ItemType Directory -Path $installParent -Force | Out-Null

    Invoke-WebRequest -Uri $releaseUrl -OutFile $archivePath -UseBasicParsing
    Expand-Archive -LiteralPath $archivePath -DestinationPath $stagingDirectory

    if (-not (Test-Path -LiteralPath (Join-Path $stagingDirectory 'Fidus.exe') -PathType Leaf)) {
        throw 'The downloaded archive does not contain Fidus.exe.'
    }
    if (-not (Test-Path -LiteralPath (Join-Path $stagingDirectory 'Resources\agents.json') -PathType Leaf)) {
        throw 'The downloaded archive does not contain the Fidus resources.'
    }

    if (Test-Path -LiteralPath $installDirectory) {
        if (-not (Test-Path -LiteralPath $installDirectory -PathType Container)) {
            throw "Cannot install because the target exists and is not a directory: $installDirectory"
        }
        Move-Item -LiteralPath $installDirectory -Destination $backupDirectory
        $backupCreated = $true
    }

    try {
        Move-Item -LiteralPath $stagingDirectory -Destination $installDirectory
    }
    catch {
        if ($backupCreated -and -not (Test-Path -LiteralPath $installDirectory)) {
            Move-Item -LiteralPath $backupDirectory -Destination $installDirectory
            $backupCreated = $false
        }
        throw
    }

    if ($backupCreated) {
        Remove-Item -LiteralPath $backupDirectory -Recurse -Force
        $backupCreated = $false
    }

    $userPath = [Environment]::GetEnvironmentVariable('Path', 'User')
    $pathEntries = @($userPath -split ';' | ForEach-Object { $_.Trim().Trim('"').TrimEnd('\', '/') })
    $normalizedInstallDirectory = $installDirectory.TrimEnd('\', '/')
    if ($pathEntries -notcontains $normalizedInstallDirectory) {
        $newUserPath = if ([string]::IsNullOrWhiteSpace($userPath)) {
            $installDirectory
        }
        else {
            "$($userPath.TrimEnd(';'));$installDirectory"
        }
        [Environment]::SetEnvironmentVariable('Path', $newUserPath, 'User')
    }

    if (($env:Path -split ';' | ForEach-Object { $_.Trim().Trim('"').TrimEnd('\', '/') }) -notcontains $normalizedInstallDirectory) {
        $env:Path = "$env:Path;$installDirectory"
    }

    Write-Output "Fidus CLI installed at $(Join-Path $installDirectory 'Fidus.exe')"
    Write-Output 'Open a new terminal to use the fidus command.'
}
finally {
    if (Test-Path -LiteralPath $temporaryDirectory) {
        Remove-Item -LiteralPath $temporaryDirectory -Recurse -Force
    }
    if (Test-Path -LiteralPath $stagingDirectory) {
        Remove-Item -LiteralPath $stagingDirectory -Recurse -Force
    }
}
