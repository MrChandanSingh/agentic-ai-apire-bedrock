# Documentation Review Script
param(
    [string[]]$Files
)

$docsNeeded = @()
$configPath = Join-Path $PSScriptRoot "doc-sync.json"
$config = Get-Content $configPath | ConvertFrom-Json

# Analyze changed files
foreach ($file in $Files) {
    # Check file type and content
    $ext = [System.IO.Path]::GetExtension($file)
    
    switch ($ext) {
        ".cs" {
            # Check for API changes
            if (Select-String -Path $file -Pattern "public class|public interface|public enum") {
                $docsNeeded += @{
                    "type" = "api"
                    "file" = $file
                    "reason" = "Public API change detected"
                }
            }
        }
        ".js" {
            # Check for component changes
            if (Select-String -Path $file -Pattern "export|class|function") {
                $docsNeeded += @{
                    "type" = "component"
                    "file" = $file
                    "reason" = "Component/Function change detected"
                }
            }
        }
    }
}

# Create documentation tasks
if ($docsNeeded.Count -gt 0) {
    $taskFile = Join-Path $PSScriptRoot "doc-tasks.json"
    
    # Load existing tasks or create new array
    $tasks = if (Test-Path $taskFile) {
        Get-Content $taskFile | ConvertFrom-Json
    } else {
        @()
    }

    # Add new tasks
    foreach ($doc in $docsNeeded) {
        $tasks += @{
            "id" = [guid]::NewGuid().ToString()
            "type" = $doc.type
            "file" = $doc.file
            "reason" = $doc.reason
            "status" = "pending"
            "created" = Get-Date -Format "yyyy-MM-dd"
        }
    }

    # Save tasks
    $tasks | ConvertTo-Json | Set-Content $taskFile

    # Trigger doc-sync agent
    Write-Output "Documentation updates needed for $($docsNeeded.Count) files"
    claude-code run "Task(description='Update docs', prompt='Review and update documentation for changes', subagent_type='doc-sync')"
}