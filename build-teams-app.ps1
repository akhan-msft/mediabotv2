# Teams App Package Builder
# Run this script on your Windows VM after creating icons and updating manifest.json

Write-Host "Building Teams App Package..." -ForegroundColor Green

# Create icons if they don't exist
if (-not (Test-Path "outline.png") -or -not (Test-Path "color.png")) {
    Write-Host "Creating icons..." -ForegroundColor Yellow
    .\create-icons.ps1
}

# Verify required files exist
$requiredFiles = @("manifest.json", "outline.png", "color.png")
$missingFiles = @()

foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file)) {
        $missingFiles += $file
    }
}

if ($missingFiles.Count -gt 0) {
    Write-Host "Missing required files:" -ForegroundColor Red
    $missingFiles | ForEach-Object { Write-Host "  - $_" -ForegroundColor Red }
    exit 1
}

# Create Teams app package
$packageName = "MediaBot-TeamsApp.zip"

# Remove existing package if it exists
if (Test-Path $packageName) {
    Remove-Item $packageName -Force
    Write-Host "Removed existing package" -ForegroundColor Yellow
}

# Create zip package
try {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    [System.IO.Compression.ZipFile]::CreateFromDirectory((Get-Location), $packageName, [System.IO.Compression.CompressionLevel]::Optimal, $false)
    
    # Remove the zip from itself if it got included
    $zip = [System.IO.Compression.ZipFile]::Open($packageName, [System.IO.Compression.ZipArchiveMode]::Update)
    $entryToRemove = $zip.Entries | Where-Object { $_.Name -eq $packageName }
    if ($entryToRemove) {
        $entryToRemove.Delete()
    }
    $zip.Dispose()
    
    Write-Host "✓ Teams app package created: $packageName" -ForegroundColor Green
    
    # Show package contents
    Write-Host "`nPackage contents:" -ForegroundColor Cyan
    $zip = [System.IO.Compression.ZipFile]::OpenRead($packageName)
    $zip.Entries | ForEach-Object { 
        Write-Host "  - $($_.Name) ($($_.Length) bytes)" -ForegroundColor White
    }
    $zip.Dispose()
    
} catch {
    Write-Host "Error creating package: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

Write-Host "`n🎉 Teams app package ready for upload to Microsoft Teams!" -ForegroundColor Green
Write-Host "Upload path: Teams → Apps → Manage your apps → Upload custom app → Upload $packageName" -ForegroundColor Cyan