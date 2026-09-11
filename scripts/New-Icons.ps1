$ErrorActionPreference='Stop'
Add-Type -AssemblyName System.Drawing
$root=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
$palette=Get-Content (Join-Path $root 'design/brand-palette.json') -Raw | ConvertFrom-Json
$assets=Join-Path $root 'src/MDPlayer.Desktop/Assets'
$accent=[Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml($palette.accent))
$paper=[Drawing.SolidBrush]::new([Drawing.ColorTranslator]::FromHtml($palette.paper))
function Draw-Symbol($g,$brush) {
 $pen=[Drawing.Pen]::new($brush,1.5);$pen.StartCap=$pen.EndCap=[Drawing.Drawing2D.LineCap]::Round;$pen.LineJoin=[Drawing.Drawing2D.LineJoin]::Round
 $p=[Drawing.Drawing2D.GraphicsPath]::new()
 $p.AddLine(17,3,7,3);$p.AddBezier(7,3,5,3,5,3,5,5);$p.AddLine(5,5,5,19);$p.AddBezier(5,19,5,21,5,21,7,21)
 $p.AddLine(7,21,17,21);$p.AddBezier(17,21,19,21,19,21,19,19);$p.AddLine(19,19,19,7);$p.AddLine(19,7,18,6)
 $g.DrawPath($pen,$p);$g.DrawLine($pen,8,6,8,18);$p.Dispose();$pen.Dispose()
}
function Draw-Icon([int]$size,[string]$output,[bool]$document) {
 $bitmap=[Drawing.Bitmap]::new($size,$size);$g=[Drawing.Graphics]::FromImage($bitmap)
 $g.SmoothingMode=[Drawing.Drawing2D.SmoothingMode]::AntiAlias;$g.ScaleTransform($size/24.0,$size/24.0)
 $p=[Drawing.Drawing2D.GraphicsPath]::new()
 if(!$document){
  $p.AddArc(1,1,6,6,180,90);$p.AddArc(17,1,6,6,270,90);$p.AddArc(17,17,6,6,0,90);$p.AddArc(1,17,6,6,90,90);$p.CloseFigure();$g.FillPath($accent,$p)
  $g.TranslateTransform(2.4,2.4);$g.ScaleTransform(.8,.8);Draw-Symbol $g $paper
 }else{
  $p.AddLines([Drawing.PointF[]]@([Drawing.PointF]::new(5,2),[Drawing.PointF]::new(15,2),[Drawing.PointF]::new(20,7),[Drawing.PointF]::new(20,22),[Drawing.PointF]::new(5,22)));$p.CloseFigure();$g.FillPath($paper,$p)
  $pen=[Drawing.Pen]::new($accent,1.3);$pen.LineJoin=[Drawing.Drawing2D.LineJoin]::Round;$g.DrawPath($pen,$p);$g.DrawLine($pen,15,2,15,7);$g.DrawLine($pen,15,7,20,7);$g.DrawLine($pen,8,5,8,19)
  if($size -ge 32){$g.TranslateTransform(13,15);$g.ScaleTransform(.25,.25);Draw-Symbol $g $accent};$pen.Dispose()
 }
 $p.Dispose();$bitmap.Save($output,[Drawing.Imaging.ImageFormat]::Png);$g.Dispose();$bitmap.Dispose()
}
foreach($name in @('Margin','Margin.Document')){
 $document=$name -eq 'Margin.Document';$iconset=Join-Path $assets "$name.iconset";New-Item -ItemType Directory -Force $iconset | Out-Null
 foreach($size in @(16,32,128,256,512)){Draw-Icon $size (Join-Path $iconset "icon_${size}x${size}.png") $document;Draw-Icon ($size*2) (Join-Path $iconset "icon_${size}x${size}@2x.png") $document}
 $images=@(foreach($size in @(16,24,32,48,64,128,256)){$path=Join-Path $assets "$name-$size.png";Draw-Icon $size $path $document;[pscustomobject]@{Size=$size;Bytes=[IO.File]::ReadAllBytes($path)}})
 $writer=[IO.BinaryWriter]::new([IO.File]::Create((Join-Path $assets "$name.ico")))
 $writer.Write([uint16]0);$writer.Write([uint16]1);$writer.Write([uint16]$images.Count);$offset=6+16*$images.Count
 foreach($img in $images){$dimension=if($img.Size -eq 256){0}else{$img.Size};$writer.Write([byte]$dimension);$writer.Write([byte]$dimension);$writer.Write([byte]0);$writer.Write([byte]0);$writer.Write([uint16]1);$writer.Write([uint16]32);$writer.Write([uint32]$img.Bytes.Length);$writer.Write([uint32]$offset);$offset+=$img.Bytes.Length}
 foreach($img in $images){$writer.Write([byte[]]$img.Bytes)};$writer.Dispose();Draw-Icon 256 (Join-Path $assets "$name.png") $document
}
# Flat installer plates share the production symbol, typography and palette.
$packaging=Join-Path $root 'packaging/assets';New-Item -ItemType Directory -Force $packaging | Out-Null
foreach($layout in @(@{Name='wizard';W=164;H=314},@{Name='header';W=55;H=55},@{Name='dmg-background';W=720;H=440})){
 $bitmap=[Drawing.Bitmap]::new($layout.W,$layout.H);$g=[Drawing.Graphics]::FromImage($bitmap);$g.SmoothingMode=[Drawing.Drawing2D.SmoothingMode]::AntiAlias;$g.Clear($paper.Color)
 if($layout.Name -eq 'header'){$g.TranslateTransform(3,3);$g.ScaleTransform(2,2);Draw-Symbol $g $accent}
 elseif($layout.Name -eq 'wizard'){
  $g.FillRectangle($accent,0,0,5,$layout.H);$state=$g.Save();$g.TranslateTransform(34,38);$g.ScaleTransform(4,4);Draw-Symbol $g $accent;$g.Restore($state)
  $font=[Drawing.Font]::new('Georgia',23,[Drawing.FontStyle]::Bold,[Drawing.GraphicsUnit]::Pixel);$g.DrawString('Margin',$font,$accent,23,160);$font.Dispose()
  $font=[Drawing.Font]::new('Segoe UI',12,[Drawing.GraphicsUnit]::Pixel);$g.DrawString("A little room`nto read.",$font,$accent,24,200);$font.Dispose()
 }else{
  $g.FillRectangle($accent,0,0,6,$layout.H);$font=[Drawing.Font]::new('Georgia',32,[Drawing.FontStyle]::Bold,[Drawing.GraphicsUnit]::Pixel);$g.DrawString('Margin',$font,$accent,40,32);$font.Dispose()
  $pen=[Drawing.Pen]::new($accent,3);$g.DrawLine($pen,324,220,396,220);$g.DrawLine($pen,382,206,396,220);$g.DrawLine($pen,382,234,396,220);$pen.Dispose()
  $font=[Drawing.Font]::new('Segoe UI',16,[Drawing.GraphicsUnit]::Pixel);$g.DrawString('Drag Margin to Applications to install.',$font,$accent,200,360);$font.Dispose()
 }
 $bitmap.Save((Join-Path $packaging ($layout.Name+'.png')),[Drawing.Imaging.ImageFormat]::Png)
 if($layout.Name -ne 'dmg-background'){$bitmap.Save((Join-Path $packaging ($layout.Name+'.bmp')),[Drawing.Imaging.ImageFormat]::Bmp)};$g.Dispose();$bitmap.Dispose()
}
$accent.Dispose();$paper.Dispose();Write-Output 'Generated Margin application/document ICOs, Mac iconsets, and installer artwork.'
