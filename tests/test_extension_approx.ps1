# --- Configuration ---
$GeneratorPath = ".\file-generator.exe"
$SolverPath = ".\siep.exe"
$DataDir = ".\data\ext_approx"
$ResultsFile = "results\extension_approx.csv"
$Seeds = 10, 50

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
$GraphSizes = 10, 20, 30, 50, 100, 200, 300, 400, 500, 600
$Density = 0.4

Write-Host "Starting Approx. Extension Heuristics Tests..." -ForegroundColor Cyan

foreach ($n in $GraphSizes) {
    $totalTap = 0
    $totalFV2 = 0
    $filename = "ext_n${n}.txt"
    $filepath = Join-Path $DataDir $filename

    # Calculate estimated edges: 0.4 * N * (N-1) / 2
    $estEdges = [Math]::Floor($Density * ($n * ($n - 1)) / 2)

    Write-Host "  Processing Size N=$n (E~$estEdges)" -NoNewline

    foreach ($seed in $Seeds) {
        # Generate RANDOM graphs
        & $GeneratorPath $n $n --output $filepath --density $Density --seed $seed | Out-Null

        # 1. TAP Strategy
        $t1 = Measure-Command {
            & $SolverPath --file $filepath --extend --extalgo tap --subalgo deg | Out-Null
        }
        $totalTap += $t1.TotalMilliseconds

        # 2. FV2 Strategy
        $t2 = Measure-Command {
            & $SolverPath --file $filepath --extend --extalgo fv2 --subalgo deg | Out-Null
        }
        $totalFV2 += $t2.TotalMilliseconds
        
        Write-Host "." -NoNewline
    }

    # Calculate averages
    $avgTap = $totalTap / $Seeds.Count
    $avgFV2 = $totalFV2 / $Seeds.Count

    # Log results with Edge Count
    "TAP,$n,$estEdges,$avgTap" | Out-File $ResultsFile -Append -Encoding utf8
    "FV2,$n,$estEdges,$avgFV2" | Out-File $ResultsFile -Append -Encoding utf8

    Write-Host " Done. TAP: $("{0:N0}" -f $avgTap)ms | FV2: $("{0:N0}" -f $avgFV2)ms" -ForegroundColor Green
}

Write-Host "`nTest complete." -ForegroundColor Cyan