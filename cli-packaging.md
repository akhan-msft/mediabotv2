# Cross-Platform Teams App Package Builder

## For PowerShell (Windows/Linux/macOS)
```powershell
# Create Teams app package using PowerShell
$files = @("manifest.json", "outline.png", "color.png")
$packageName = "MediaBot-TeamsApp.zip"

# Verify files exist
$missingFiles = $files | Where-Object { -not (Test-Path $_) }
if ($missingFiles) {
    Write-Host "Missing files: $($missingFiles -join ', ')" -ForegroundColor Red
    exit 1
}

# Create package
Compress-Archive -Path $files -DestinationPath $packageName -Force
Write-Host "✓ Created: $packageName" -ForegroundColor Green
```

## For Linux/macOS (using zip)
```bash
#!/bin/bash
# Create Teams app package using zip

echo "Building Teams App Package..."

# Verify required files
required_files=("manifest.json" "outline.png" "color.png")
for file in "${required_files[@]}"; do
    if [[ ! -f "$file" ]]; then
        echo "❌ Missing file: $file"
        exit 1
    fi
done

# Create package
zip -r MediaBot-TeamsApp.zip manifest.json outline.png color.png

echo "✓ Created: MediaBot-TeamsApp.zip"
echo "📦 Package contents:"
unzip -l MediaBot-TeamsApp.zip
```

## For Python (Cross-platform)
```python
#!/usr/bin/env python3
# Create Teams app package using Python

import zipfile
import os
import sys

required_files = ["manifest.json", "outline.png", "color.png"]
package_name = "MediaBot-TeamsApp.zip"

# Verify files exist
missing_files = [f for f in required_files if not os.path.exists(f)]
if missing_files:
    print(f"❌ Missing files: {', '.join(missing_files)}")
    sys.exit(1)

# Create package
with zipfile.ZipFile(package_name, 'w', zipfile.ZIP_DEFLATED) as zf:
    for file in required_files:
        zf.write(file)

print(f"✓ Created: {package_name}")
print("📦 Package contents:")
with zipfile.ZipFile(package_name, 'r') as zf:
    for info in zf.infolist():
        print(f"  - {info.filename} ({info.file_size} bytes)")
```

Save as `build-teams-app.py` and run with: `python3 build-teams-app.py`