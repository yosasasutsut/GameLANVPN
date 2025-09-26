# GameLAN VPN - GitHub Repository Setup Script
# Run this script to configure your GitHub repository

param(
    [Parameter(Mandatory=$false)]
    [string]$GitHubUsername = "yosasasutsut",

    [Parameter(Mandatory=$false)]
    [string]$RepositoryName = "GameLANVPN",

    [Parameter(Mandatory=$false)]
    [switch]$CreateNew = $false,

    [Parameter(Mandatory=$false)]
    [switch]$Private = $false
)

Write-Host "GameLAN VPN - GitHub Setup" -ForegroundColor Cyan
Write-Host "===========================" -ForegroundColor Cyan

# Check if Git is installed
if (!(Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Host "Git is not installed. Please install Git first." -ForegroundColor Red
    exit 1
}

# Check if GitHub CLI is installed
$hasGitHubCLI = Get-Command gh -ErrorAction SilentlyContinue

if ($hasGitHubCLI) {
    Write-Host "GitHub CLI detected. Using gh commands..." -ForegroundColor Green

    # Check if authenticated
    $authStatus = gh auth status 2>&1
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Please authenticate with GitHub first:" -ForegroundColor Yellow
        gh auth login
    }

    if ($CreateNew) {
        # Create new repository
        Write-Host "Creating new repository on GitHub..." -ForegroundColor Yellow

        $visibility = if ($Private) { "--private" } else { "--public" }

        gh repo create "$GitHubUsername/$RepositoryName" `
            $visibility `
            --description "Virtual LAN Gaming Solution - Play LAN games over the Internet" `
            --add-readme `
            --clone=false

        if ($LASTEXITCODE -eq 0) {
            Write-Host "Repository created successfully!" -ForegroundColor Green
        } else {
            Write-Host "Failed to create repository. It may already exist." -ForegroundColor Red
        }
    }

    # Set up repository settings
    Write-Host "Configuring repository settings..." -ForegroundColor Yellow

    # Enable issues
    gh api repos/$GitHubUsername/$RepositoryName --method PATCH `
        -f has_issues=true `
        -f has_projects=true `
        -f has_wiki=true | Out-Null

    # Add topics
    gh api repos/$GitHubUsername/$RepositoryName/topics --method PUT `
        -f names='["vpn","gaming","lan","windows","dotnet","csharp","warcraft","networking"]' | Out-Null

    Write-Host "Repository settings configured!" -ForegroundColor Green

    # Create labels
    Write-Host "Creating issue labels..." -ForegroundColor Yellow

    $labels = @(
        @{name="enhancement"; color="a2eeef"; description="New feature or request"},
        @{name="bug"; color="d73a4a"; description="Something isn't working"},
        @{name="documentation"; color="0075ca"; description="Improvements or additions to documentation"},
        @{name="good first issue"; color="7057ff"; description="Good for newcomers"},
        @{name="help wanted"; color="008672"; description="Extra attention is needed"},
        @{name="client"; color="ffd700"; description="Client application related"},
        @{name="server"; color="ff6347"; description="Server component related"},
        @{name="networking"; color="4b0082"; description="Network layer related"}
    )

    foreach ($label in $labels) {
        gh label create $label.name `
            --color $label.color `
            --description $label.description `
            --repo $GitHubUsername/$RepositoryName 2>$null
    }

    Write-Host "Labels created!" -ForegroundColor Green

} else {
    Write-Host "GitHub CLI not found. Using standard git commands..." -ForegroundColor Yellow
    Write-Host "For full features, install GitHub CLI: https://cli.github.com/" -ForegroundColor Yellow
}

# Configure Git remote
Write-Host "`nConfiguring Git remote..." -ForegroundColor Yellow

$remoteUrl = "https://github.com/$GitHubUsername/$RepositoryName.git"
$existingRemote = git remote get-url origin 2>$null

if ($existingRemote) {
    Write-Host "Remote 'origin' already exists: $existingRemote" -ForegroundColor Yellow
    $response = Read-Host "Do you want to update it? (y/n)"
    if ($response -eq 'y') {
        git remote set-url origin $remoteUrl
        Write-Host "Remote URL updated to: $remoteUrl" -ForegroundColor Green
    }
} else {
    git remote add origin $remoteUrl
    Write-Host "Remote 'origin' added: $remoteUrl" -ForegroundColor Green
}

# Create and switch to main branch
$currentBranch = git branch --show-current
if ($currentBranch -ne "main") {
    git branch -M main
    Write-Host "Switched to 'main' branch" -ForegroundColor Green
}

# Push to GitHub
Write-Host "`nPushing to GitHub..." -ForegroundColor Yellow
$response = Read-Host "Do you want to push the current commits to GitHub? (y/n)"

if ($response -eq 'y') {
    git push -u origin main

    if ($LASTEXITCODE -eq 0) {
        Write-Host "`nSuccessfully pushed to GitHub!" -ForegroundColor Green
        Write-Host "Repository URL: https://github.com/$GitHubUsername/$RepositoryName" -ForegroundColor Cyan

        # Open in browser
        $openBrowser = Read-Host "`nDo you want to open the repository in your browser? (y/n)"
        if ($openBrowser -eq 'y') {
            Start-Process "https://github.com/$GitHubUsername/$RepositoryName"
        }
    } else {
        Write-Host "Push failed. Please check your credentials and try again." -ForegroundColor Red
    }
}

# Set up branch protection (requires GitHub CLI)
if ($hasGitHubCLI) {
    Write-Host "`nSetting up branch protection rules..." -ForegroundColor Yellow

    gh api repos/$GitHubUsername/$RepositoryName/branches/main/protection --method PUT `
        -f required_status_checks='{"strict":true,"contexts":["Build and Test"]}' `
        -f enforce_admins=false `
        -f required_pull_request_reviews='{"dismiss_stale_reviews":true,"require_code_owner_reviews":false,"required_approving_review_count":1}' `
        -f restrictions=null 2>$null

    if ($LASTEXITCODE -eq 0) {
        Write-Host "Branch protection configured!" -ForegroundColor Green
    }
}

# Create initial issues
if ($hasGitHubCLI) {
    Write-Host "`nCreating initial issues..." -ForegroundColor Yellow

    $issues = @(
        @{
            title = "Implement TAP adapter integration"
            body = "Integrate with Windows TAP adapter for virtual network interface creation"
            labels = "enhancement,networking"
        },
        @{
            title = "Add Warcraft III game support"
            body = "Implement protocol support for Warcraft III LAN games"
            labels = "enhancement,client"
        },
        @{
            title = "Implement packet encryption"
            body = "Add AES-256 encryption for all game packets"
            labels = "enhancement,security"
        },
        @{
            title = "Create game detection system"
            body = "Automatically detect installed games and their network requirements"
            labels = "enhancement,client"
        }
    )

    foreach ($issue in $issues) {
        gh issue create `
            --title $issue.title `
            --body $issue.body `
            --label $issue.labels `
            --repo $GitHubUsername/$RepositoryName 2>$null
    }

    Write-Host "Initial issues created!" -ForegroundColor Green
}

Write-Host "`n======================================" -ForegroundColor Green
Write-Host "GitHub setup complete!" -ForegroundColor Green
Write-Host "======================================" -ForegroundColor Green

Write-Host "`nNext steps:" -ForegroundColor Cyan
Write-Host "1. Visit: https://github.com/$GitHubUsername/$RepositoryName" -ForegroundColor White
Write-Host "2. Enable GitHub Actions in repository settings" -ForegroundColor White
Write-Host "3. Configure secrets for CI/CD (Settings > Secrets)" -ForegroundColor White
Write-Host "4. Start development and push your changes!" -ForegroundColor White

Write-Host "`nUseful commands:" -ForegroundColor Cyan
Write-Host "  git status                    # Check current status" -ForegroundColor Gray
Write-Host "  git add .                     # Stage all changes" -ForegroundColor Gray
Write-Host "  git commit -m 'message'       # Commit changes" -ForegroundColor Gray
Write-Host "  git push                      # Push to GitHub" -ForegroundColor Gray
Write-Host "  git pull                      # Pull latest changes" -ForegroundColor Gray