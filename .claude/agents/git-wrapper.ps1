# Git command wrapper for Windows environments
param(
    [Parameter(Mandatory=$true)]
    [string]$Command,
    
    [Parameter(Mandatory=$false)]
    [string]$Path = ".",
    
    [Parameter(Mandatory=$false)]
    [string]$Message = "",
    
    [Parameter(Mandatory=$false)]
    [string[]]$Files = @()
)

# Set working directory
$repoPath = (Get-Location).Path
if ($Path -ne ".") {
    $repoPath = $Path
}

# Command handlers
function Invoke-GitAdd {
    param($files)
    if ($files.Count -eq 0) {
        git -C $repoPath add .
    } else {
        foreach ($file in $files) {
            $filePath = $file -replace '/', '\'
            git -C $repoPath add "$filePath"
        }
    }
}

function Invoke-GitCommit {
    param($msg)
    if ([string]::IsNullOrEmpty($msg)) {
        throw "Commit message is required"
    }
    git -C $repoPath commit -m "$msg"
}

function Invoke-GitPush {
    git -C $repoPath push
}

function Invoke-GitStatus {
    git -C $repoPath status
}

# Execute requested command
try {
    switch ($Command.ToLower()) {
        "add" { Invoke-GitAdd -files $Files }
        "commit" { Invoke-GitCommit -msg $Message }
        "push" { Invoke-GitPush }
        "status" { Invoke-GitStatus }
        default { throw "Unknown command: $Command" }
    }
} catch {
    Write-Error "Error executing git command: $_"
    exit 1
}