param([string]$Pass = '2026-09-08-pass-01')
$ErrorActionPreference = 'Stop'
$root = [IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..'))
if ($Pass -notmatch '^\d{4}-\d{2}-\d{2}-pass-\d{2}$') { throw 'Unexpected pass name.' }
$directory = Join-Path $root "design/generated/$Pass"
$manifest = Get-Content (Join-Path $directory 'manifest.json') -Raw | ConvertFrom-Json
$pack = Get-Content (Join-Path $root 'design/prompts/margin-asset-prompts.json') -Raw | ConvertFrom-Json
Add-Type -AssemblyName System.Drawing
$items = foreach ($prompt in $pack.prompts) {
    $record = $manifest.records | Where-Object id -EQ $prompt.id | Select-Object -Last 1
    $item = [ordered]@{ id=$prompt.id; title=$prompt.title; uses=$prompt.uses; prompt=$prompt.positive_prompt; generated=$false; group=$(if ([int]$prompt.id.Substring(0,2) -le 12) {'Brand'} elseif ([int]$prompt.id.Substring(0,2) -ge 39) {'Status'} else {'Controls'}) }
    if ($record) {
        if ($record.file -notmatch '^\d{2}-[a-z-]+\.png$') { throw "Unexpected output name: $($record.file)" }
        $destination = Join-Path $directory $record.file
        if (!(Test-Path -LiteralPath $destination)) { Copy-Item -LiteralPath $record.source_path -Destination $destination }
        if ((Get-FileHash -LiteralPath $destination).Hash -ne (Get-FileHash -LiteralPath $record.source_path).Hash) { throw "Copied image differs from original: $($record.file)" }
        $bitmap = [Drawing.Bitmap]::new($destination)
        $item.generated=$true; $item.file=$record.file; $item.width=$bitmap.Width; $item.height=$bitmap.Height
        $item.cornerAlpha=$bitmap.GetPixel(0,0).A
        $item.sha256=(Get-FileHash -LiteralPath $destination).Hash.ToLowerInvariant()
        $item.review=if ($record.review) {$record.review} else {'First-pass concept; production cleanup and approval pending.'}
        $bitmap.Dispose()
    }
    [pscustomobject]$item
}
$items | ConvertTo-Json -Depth 10 | Set-Content (Join-Path $directory 'inventory.json') -Encoding utf8
$data = ($items | ConvertTo-Json -Depth 10 -Compress).Replace('<','\u003c')
$html = @'
<!doctype html><html lang="en"><meta charset="utf-8"><meta name="viewport" content="width=device-width,initial-scale=1"><title>Margin — asset review</title>
<style>
*{box-sizing:border-box}body{margin:0;background:#f3f2ee;color:#222b2b;font:15px/1.5 system-ui,sans-serif}main{max-width:1450px;margin:auto;padding:32px}h1{font:42px Georgia,serif;margin:0}p{max-width:900px}header{margin-bottom:20px}nav{display:flex;gap:12px;align-items:center;flex-wrap:wrap;margin:18px 0}select,button{font:inherit;padding:8px 14px;border:1px solid #bdc7c3;border-radius:6px;background:white;color:#222b2b}button:focus-visible,select:focus-visible,a:focus-visible{outline:3px solid #5261b5;outline-offset:3px}button:disabled{opacity:.45}.grid{display:grid;grid-template-columns:repeat(auto-fit,minmax(240px,1fr));gap:18px}.card{background:white;border:1px solid #d6ddd7;border-radius:10px;overflow:hidden}.well{height:230px;display:flex;align-items:center;justify-content:center;background:var(--well,#fff);padding:12px}.well img{display:block;max-width:100%;max-height:100%;object-fit:contain}.meta{padding:14px}.meta h2{font-size:16px;margin:0 0 6px}.muted{font-size:12px;color:#596761}.pending{font-size:14px;color:#667}.note{font-size:13px}a{color:#5261b5}details{font-size:12px;margin-top:10px}pre{white-space:pre-wrap;overflow-wrap:anywhere}#count{font-weight:600}.samples{display:flex;gap:12px;align-items:center;height:52px;background:#fff;padding:8px;border-top:1px solid #eee}.samples img{object-fit:contain}footer{margin-top:20px;color:#596761;font-size:13px}
</style><main><header><h1>Margin</h1><p>A little room to read. — First asset pass</p><p>Generated concepts for comparison, not approved production artwork. Transparent originals are displayed against the selected surface without changing their files. The application still uses its existing assets.</p></header>
<nav><label>Group <select id="group"><option>All</option><option>Brand</option><option>Controls</option><option>Status</option></select></label><label>Preview surface <select id="surface"><option value="#fff">White</option><option value="#fcfbf7">Paper</option><option value="#242d2d">Dark</option></select></label><button id="previous">Previous page</button><span id="page"></span><button id="next">Next page</button><span id="count"></span></nav>
<div id="grid" class="grid"></div><footer>Use the full-size links to inspect edges and lettering. Small samples show the untrimmed source at 16, 24, and 32 pixels; production versions may need tighter bounds and optical adjustments.</footer></main>
<script id="data" type="application/json">__DATA__</script><script>
const items=JSON.parse(document.getElementById('data').textContent);let page=0;const size=8;const group=document.getElementById('group');
function element(tag,text,cls){const e=document.createElement(tag);if(text!==undefined)e.textContent=text;if(cls)e.className=cls;return e}
function render(){const selected=items.filter(i=>group.value==='All'||i.group===group.value);const pages=Math.max(1,Math.ceil(selected.length/size));page=Math.min(page,pages-1);const grid=document.getElementById('grid');grid.replaceChildren();for(const item of selected.slice(page*size,(page+1)*size)){const card=element('article',undefined,'card');const well=element('div',undefined,'well');if(item.generated){const a=element('a');a.href=item.file;a.target='_blank';a.rel='noopener';a.style='display:contents';const img=element('img');img.src=item.file;img.alt=item.title;a.append(img);well.append(a)}else well.append(element('span','Generation pending','pending'));card.append(well);const meta=element('div',undefined,'meta');meta.append(element('h2',item.id+' · '+item.title));meta.append(element('div',item.uses.join(' · '),'muted'));if(item.generated){const a=element('a','Open full-size PNG');a.href=item.file;a.target='_blank';a.rel='noopener';meta.append(a,element('div',item.width+' × '+item.height+' · corner alpha '+item.cornerAlpha,'muted'),element('p',item.review,'note'))}const details=element('details');details.append(element('summary','Source prompt'),element('pre',item.prompt));meta.append(details);card.append(meta);if(item.generated&&item.group!=='Brand'){const samples=element('div',undefined,'samples');for(const px of [16,24,32]){const img=element('img');img.src=item.file;img.width=px;img.height=px;img.alt=item.title+' at '+px+' pixels';samples.append(img)}samples.append(element('span','16 / 24 / 32 px','muted'));card.append(samples)}grid.append(card)}document.getElementById('page').textContent=(page+1)+' / '+pages;document.getElementById('previous').disabled=page===0;document.getElementById('next').disabled=page===pages-1;document.getElementById('count').textContent=items.filter(i=>i.generated).length+' / '+items.length+' generated'}
group.onchange=()=>{page=0;render()};document.getElementById('surface').onchange=e=>document.documentElement.style.setProperty('--well',e.target.value);document.getElementById('previous').onclick=()=>{page--;render()};document.getElementById('next').onclick=()=>{page++;render()};render();
</script></html>
'@
[IO.File]::WriteAllText((Join-Path $directory 'index.html'), $html.Replace('__DATA__',$data))
Write-Output "$(@($items | Where-Object generated).Count) / $($items.Count) images copied, hashed, and indexed."
