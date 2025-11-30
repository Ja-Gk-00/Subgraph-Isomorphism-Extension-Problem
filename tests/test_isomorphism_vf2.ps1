# --- Configuration ---
$GeneratorPath = ".\file-generator.exe"
$SolverPath = ".\siep.exe"
$DataDir = ".\data\iso_vf2"
$ResultsFile = "results\isomorphism_vf2.csv"
$Seeds = 10

# --- Directory Setup ---
if (!(Test-Path $DataDir)) { 
    New-Item -ItemType Directory -Path $DataDir -Force | Out-Null 
}
$ResultsDir = [System.IO.Path]::GetDirectoryName($ResultsFile)
if (!(Test-Path $ResultsDir)) { 
    New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null 
}

# --- Initialize CSV ---
"Algorithm,Size_N,Est_Edges,Avg_Time_ms" | Out-File $ResultsFile -Encoding utf8

# --- Test Parameters ---
$GraphSizes = 100, 200, 500, 1000, 2000, 5000, 6000
$Density = 0.6
$MaxWeight = 5

Write-Host "Starting Exact Isomorphism VF2 Tests..." -ForegroundColor Cyan

foreach ($n in $GraphSizes) {
    $totalNaive = 0
    $totalUllmann = 0
    $totalFV2 = 0
    $filename = "iso_n${n}.txt"
    $filepath = Join-Path $DataDir $filename

    # Calculate estimated edges: 0.6 * N * (N-1) / 2
    $estEdges = [Math]::Floor($Density * $MaxWeight * ($n * ($n - 1)) / 2)

    Write-Host "  Processing Size N=$n (E~$estEdges)" -NoNewline

    foreach ($seed in $Seeds) {
        # Generate ISOMORPHIC graphs (worst-case scenario)
        & $GeneratorPath $n $n --isomorphic --allow-loops --max-weight $MaxWeight --output $filepath --density $Density --seed $seed | Out-Null

        # 3. VF2
        $t3 = Measure-Command {
            & $SolverPath --file $filepath --check --subalgo vf2 | Out-Null
        }
        $totalVF2 += $t3.TotalMilliseconds
        
        Write-Host "." -NoNewline
    }

    $avgNaive = $totalNaive / $Seeds.Count
    $avgUllmann = $totalUllmann / $Seeds.Count
    $avgVF2 = $totalVF2 / $Seeds.Count

    # Log results with Edge Count
    "FV2,$n,$estEdges,$avgVF2" | Out-File $ResultsFile -Append -Encoding utf8

    Write-Host " Done. VF2: $("{0:N0}" -f $avgVF2)ms" -ForegroundColor Green
}

Write-Host "`nTest complete." -ForegroundColor Cyan