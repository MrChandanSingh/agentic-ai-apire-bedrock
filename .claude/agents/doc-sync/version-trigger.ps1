# Documentation Version Manager
param(
    [string]$ChangeType = "patch",
    [string]$DocPath = "",
    [string]$Message = ""
)

$configPath = Join-Path $PSScriptRoot "doc-version.json"
$version = Get-Content $configPath | ConvertFrom-Json
$current = $version.version_control.current -split "\."

# Update version numbers
switch ($ChangeType) {
    "major" { $current[0] = [int]$current[0] + 1; $current[1] = 0; $current[2] = 0 }
    "minor" { $current[1] = [int]$current[1] + 1; $current[2] = 0 }
    "patch" { $current[2] = [int]$current[2] + 1 }
}

$newVersion = $current -join "."

# Update document
if ($DocPath -and (Test-Path $DocPath)) {
    $content = Get-Content $DocPath
    $content = $content -replace "Version: \d+\.\d+\.\d+", "Version: $newVersion"
    $content = $content -replace "Last updated: .*$", "Last updated: $(Get-Date -Format 'yyyy-MM-dd')"
    Set-Content -Path $DocPath -Value $content
}

# Update version control
$version.version_control.current = $newVersion
$version.version_control.history += @{
    "version" = $newVersion
    "date" = (Get-Date -Format "yyyy-MM-dd")
    "path" = $DocPath
    "message" = if ($Message) { $Message } else { "Auto-update to version $newVersion" }
}

$version | ConvertTo-Json -Depth 10 | Set-Content $configPath

Write-Output "Updated to version $newVersion"