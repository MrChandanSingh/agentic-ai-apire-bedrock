param(
    [string]$ChangeType,
    [string]$DocPath
)

$version = Get-Content ".claude/agents/doc-version.json" | ConvertFrom-Json
$current = $version.version_control.current -split "\."

switch ($ChangeType) {
    "major" { $current[0] = [int]$current[0] + 1; $current[1] = 0; $current[2] = 0 }
    "minor" { $current[1] = [int]$current[1] + 1; $current[2] = 0 }
    "patch" { $current[2] = [int]$current[2] + 1 }
}

$newVersion = $current -join "."
$version.version_control.current = $newVersion
$version.version_control.history += @{
    "version" = $newVersion
    "date" = (Get-Date).ToString("yyyy-MM-dd")
    "path" = $DocPath
}

$version | ConvertTo-Json -Depth 10 | Set-Content ".claude/agents/doc-version.json"