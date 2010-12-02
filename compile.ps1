param(
  [switch]$run, 
  [switch]$skipcompilation
)

if (test-path 'C:\prgs\dev\FSharp-2.0.0.0\bin\fsc.exe') { $compilator = 'C:\prgs\dev\FSharp-2.0.0.0\bin\fsc.exe' }
else                                                    { $compilator = 'd:\prgs\dev\FSharp-2.0.0.0\bin\fsc.exe' }

$root = Split-Path -Path $MyInvocation.MyCommand.Definition
$bin  = join-path $root bin\byscript
if (!(test-path $bin)) {
  mkdir $bin
}
gci $root\lib | copy-item -dest $bin
$exe = join-path $bin GeoMap.exe
if (!$skipcompilation) {
  & $compilator `
    "$root\Utilities.fs" `
    "$root\CachesModel.fs" `
    "$root\CacheMapLayers.fs" `
    "$root\CachesSqlLite.fs" `
    "$root\Controls.fs" `
    "$root\Program.fs" `
    --platform:x86 `
    --out:$exe `
    --reference:$root\lib\BSE.Windows.Forms.dll `
    --reference:$root\lib\GMap.NET.Core.dll `
    --reference:$root\lib\GMap.NET.WindowsForms.dll  `
    --reference:$root\lib\Mono.Data.SqliteClient.dll  `
    --reference:$root\lib\System.Data.SQLite.dll `
    --reference:$root\lib\CachesImages.dll
}
if ($? -and $run) { write-host Running $exe; & $exe $root\caches.db  }