# Simple Teams App Icons

## outline.png (32x32) - Simple SVG to PNG conversion
This creates a simple outline icon for Teams app.

You can create this icon using any of these methods:

### Method 1: PowerShell (Windows VM)
```powershell
# Create a simple outline icon using PowerShell and .NET Graphics
Add-Type -AssemblyName System.Drawing

# Create 32x32 outline icon
$outline = New-Object System.Drawing.Bitmap(32, 32)
$g = [System.Drawing.Graphics]::FromImage($outline)
$g.Clear([System.Drawing.Color]::Transparent)

# Draw simple bot outline
$pen = New-Object System.Drawing.Pen([System.Drawing.Color]::Black, 2)
$g.DrawRectangle($pen, 6, 8, 20, 16)  # Bot body
$g.DrawEllipse($pen, 10, 12, 4, 4)    # Left eye
$g.DrawEllipse($pen, 18, 12, 4, 4)    # Right eye
$g.DrawLine($pen, 12, 18, 20, 18)     # Mouth

$outline.Save("outline.png", [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose()
$outline.Dispose()
Write-Host "outline.png created successfully"
```

### Method 2: Online Icon Generator
1. Go to https://favicon.io/favicon-generator/
2. Create a simple "MB" text icon (MediaBot)
3. Download 32x32 PNG as outline.png

### Method 3: Simple ASCII Art to Image
Create a text file and convert using online tools or create manually with any image editor.

## color.png (192x192) - Full color version
```powershell
# Create 192x192 color icon
$color = New-Object System.Drawing.Bitmap(192, 192)
$g = [System.Drawing.Graphics]::FromImage($color)
$g.Clear([System.Drawing.Color]::White)

# Draw colorful bot
$blueBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::DodgerBlue)
$g.FillRectangle($blueBrush, 36, 48, 120, 96)  # Bot body

$yellowBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::Gold)
$g.FillEllipse($yellowBrush, 60, 72, 24, 24)   # Left eye
$g.FillEllipse($yellowBrush, 108, 72, 24, 24)  # Right eye

$redBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::Red)
$g.FillRectangle($redBrush, 72, 108, 48, 8)    # Mouth

$color.Save("color.png", [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose()
$color.Dispose()
Write-Host "color.png created successfully"
```

Save this file as `create-icons.ps1` and run it on your Windows VM to generate both icons.