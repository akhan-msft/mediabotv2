# Create Teams App Icons PowerShell Script

Add-Type -AssemblyName System.Drawing

Write-Host "Creating Teams app icons..." -ForegroundColor Green

# Create 32x32 outline icon
$outline = New-Object System.Drawing.Bitmap(32, 32)
$g = [System.Drawing.Graphics]::FromImage($outline)
$g.Clear([System.Drawing.Color]::Transparent)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias

# Draw simple bot outline
$pen = New-Object System.Drawing.Pen([System.Drawing.Color]::Black, 2)
$g.DrawRectangle($pen, 6, 8, 20, 16)  # Bot body
$g.DrawEllipse($pen, 10, 12, 4, 4)    # Left eye
$g.DrawEllipse($pen, 18, 12, 4, 4)    # Right eye
$g.DrawLine($pen, 12, 18, 20, 18)     # Mouth

$outline.Save("outline.png", [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose()
$outline.Dispose()
$pen.Dispose()
Write-Host "✓ outline.png (32x32) created successfully" -ForegroundColor Green

# Create 192x192 color icon
$color = New-Object System.Drawing.Bitmap(192, 192)
$g2 = [System.Drawing.Graphics]::FromImage($color)
$g2.Clear([System.Drawing.Color]::White)
$g2.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias

# Draw colorful bot
$blueBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::DodgerBlue)
$g2.FillRectangle($blueBrush, 36, 48, 120, 96)  # Bot body

$yellowBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::Gold)
$g2.FillEllipse($yellowBrush, 60, 72, 24, 24)   # Left eye
$g2.FillEllipse($yellowBrush, 108, 72, 24, 24)  # Right eye

$redBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::Red)
$g2.FillRectangle($redBrush, 72, 108, 48, 8)    # Mouth

# Add "MB" text
$font = New-Object System.Drawing.Font("Arial", 24, [System.Drawing.FontStyle]::Bold)
$whiteBrush = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
$g2.DrawString("MB", $font, $whiteBrush, 75, 120)

$color.Save("color.png", [System.Drawing.Imaging.ImageFormat]::Png)
$g2.Dispose()
$color.Dispose()
$blueBrush.Dispose()
$yellowBrush.Dispose()
$redBrush.Dispose()
$whiteBrush.Dispose()
$font.Dispose()
Write-Host "✓ color.png (192x192) created successfully" -ForegroundColor Green

Write-Host "Both icons created! Ready for Teams app package." -ForegroundColor Cyan