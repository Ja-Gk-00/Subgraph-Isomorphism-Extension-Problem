# --- Configuration ---
$GeneratorPath = ".\file-generator.exe"
$SolverPath = ".\siep.exe"
$DataDir = ".\data\iso"
$ResultsFile = "results\isomorphism.csv"
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
# Added Est_Edges column
"Algorithm,Size_N,Est_Edges,Avg_Time_ms" | Out-File $ResultsFile -Encoding utf8

# --- Test Parameters ---
$GraphSizes = 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 18, 20
$Density = 0.6

Write-Host "Starting Exact Isomorphism Tests..." -ForegroundColor Cyan

foreach ($n in $GraphSizes) {
    $totalNaive = 0
    $totalUllmann = 0
    $filename = "iso_n${n}.txt"
    $filepath = Join-Path $DataDir $filename

    # Calculate estimated edges: 0.6 * N * (N-1) / 2
    $estEdges = [Math]::Floor($Density * ($n * ($n - 1)) / 2)

    Write-Host "  Processing Size N=$n (E~$estEdges)" -NoNewline

    foreach ($seed in $Seeds) {
        # Generate ISOMORPHIC graphs (worst-case scenario)
        & $GeneratorPath $n $n --isomorphic --output $filepath --density $Density --seed $seed | Out-Null
        
        # 1. Naive (Brute-force)
        if ($n -le 12) {
            $t1 = Measure-Command { & $SolverPath --file $filepath --check --subalgo naive | Out-Null }
            $totalNaive += $t1.TotalMilliseconds
        } else {
            # naive only for n <= 12 due to factorial time complexity
            $totalNaive += 0 
        }

        # 2. Ullmann
        $t2 = Measure-Command {
            & $SolverPath --file $filepath --check --subalgo ullmann | Out-Null
        }
        $totalUllmann += $t2.TotalMilliseconds
        
        Write-Host "." -NoNewline
    }

    $avgNaive = $totalNaive / $Seeds.Count
    $avgUllmann = $totalUllmann / $Seeds.Count

    # Log results with Edge Count
    "Naive,$n,$estEdges,$avgNaive" | Out-File $ResultsFile -Append -Encoding utf8
    "Ullmann,$n,$estEdges,$avgUllmann" | Out-File $ResultsFile -Append -Encoding utf8

    Write-Host " Done. Naive: $("{0:N0}" -f $avgNaive)ms | Ullmann: $("{0:N0}" -f $avgUllmann)ms" -ForegroundColor Green
}

Write-Host "`nTest complete." -ForegroundColor Cyan