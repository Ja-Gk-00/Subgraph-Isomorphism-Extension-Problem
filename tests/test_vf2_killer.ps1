# --- Configuration ---
$GeneratorPath = ".\file-generator.exe"
$SolverPath = ".\siep.exe"
$DataDir = ".\data\iso_vf2_killer"
$ResultsFile = "results\isomorphism_vf2_killer.csv"
$Seeds = 10 

if (!(Test-Path $DataDir)) { New-Item -ItemType Directory -Path $DataDir -Force | Out-Null }
$ResultsDir = [System.IO.Path]::GetDirectoryName($ResultsFile)
if (!(Test-Path $ResultsDir)) { New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null }

# Added Est_Edges to CSV header
"Algorithm,Size_N,Est_Edges,Avg_Time_ms" | Out-File $ResultsFile -Encoding utf8

# Testing small N due to exponential explosion (N=13 hangs)
$GraphSizes = 10, 11, 12, 13

Write-Host "Starting VF2 Ultimate Killer Tests (K_N vs K_N-edge)..." -ForegroundColor Cyan

foreach ($n in $GraphSizes) {
    $totalVF2 = 0
    $filename = "iso_killer_n${n}.txt"
    $filepath = Join-Path $DataDir $filename

    # Edges for full graph K_N
    $estEdges = [Math]::Floor(($n * ($n - 1)) / 2)

    Write-Host "  Processing Size N=$n (E=$estEdges)" -NoNewline

    foreach ($seed in $Seeds) {
        # 1. Generate Full Graph (G1, G2 are K_N)
        & $GeneratorPath $n $n --output $filepath --density 1.0 --max-weight 1 --seed $seed | Out-Null
        
        # 2. Modify file: Remove one edge from G2 to break isomorphism
        $content = Get-Content $filepath
        $targetLineIdx = $n + 2
        $row = $content[$targetLineIdx]
        
        # Replace first '1' with '0' in G2's first row
        $regex = new-object System.Text.RegularExpressions.Regex "1"
        $newRow = $regex.Replace($row, "0", 1)
        
        $content[$targetLineIdx] = $newRow
        $content | Set-Content $filepath
        
        # 3. Run VF2
        $t = Measure-Command { 
            & $SolverPath --file $filepath --check --subalgo vf2 | Out-Null 
        }
        $totalVF2 += $t.TotalMilliseconds
        
        Write-Host "." -NoNewline
    }

    $avgVF2 = $totalVF2 / $Seeds.Count
    
    # Save result
    "VF2_Killer,$n,$estEdges,$avgVF2" | Out-File $ResultsFile -Append -Encoding utf8

    Write-Host " Done. Time: $("{0:N0}" -f $avgVF2)ms" -ForegroundColor Green
}

Write-Host "`nTest complete." -ForegroundColor Cyan