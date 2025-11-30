# --- Configuration ---
$GeneratorPath = ".\file-generator.exe"
$SolverPath = ".\siep.exe"
$DataDir = ".\data\ext_multi_approx"
$ResultsFile = "results\extension_multigraph_approx.csv"
$Seeds = 50

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
"Algorithm,Size_N,Est_Edges,Max_Weight,Avg_Time_ms" | Out-File $ResultsFile -Encoding utf8

# --- Test Parameters ---
$GraphSizes = 10, 20, 30, 50, 100, 200, 300, 350
$MaxWeight = 5
$Density = 0.4

Write-Host "Starting Multigraph Extension Approx Tests (TAP/FV2)..." -ForegroundColor Cyan

foreach ($n in $GraphSizes) {
    $totalTap = 0
    $totalFv2 = 0
    $filename = "ext_multi_n${n}.txt"
    $filepath = Join-Path $DataDir $filename

    # Calculate estimated edges based on density for undirected graph
    # Formula: Density * (N * (N-1)) / 2
    $estEdges = [Math]::Floor($Density * ($n * ($n - 1)) / 2)

    Write-Host "  Processing Size N=$n (E~$estEdges, W=$MaxWeight)" -NoNewline

    foreach ($seed in $Seeds) {
        # Generate multigraphs (random)
        & $GeneratorPath $n $n --output $filepath --density $Density --max-weight $MaxWeight --allow-loops --seed $seed | Out-Null
        
        # 1. TAP Strategy (Approximation)
        $t1 = Measure-Command {
            & $SolverPath --file $filepath --extend --extalgo tap --subalgo deg | Out-Null
        }
        $totalTap += $t1.TotalMilliseconds

        # 2. FV2 Strategy (Approximation)
        $t2 = Measure-Command {
            & $SolverPath --file $filepath --extend --extalgo fv2 --subalgo deg | Out-Null
        }
        $totalFv2 += $t2.TotalMilliseconds
        
        Write-Host "." -NoNewline
    }

    $avgTap = $totalTap / $Seeds.Count
    $avgFv2 = $totalFv2 / $Seeds.Count

    # Log results with Edge Count
    "TAP,$n,$estEdges,$MaxWeight,$avgTap" | Out-File $ResultsFile -Append -Encoding utf8
    "FV2,$n,$estEdges,$MaxWeight,$avgFv2" | Out-File $ResultsFile -Append -Encoding utf8

    Write-Host " Done. TAP: $("{0:N0}" -f $avgTap)ms | FV2: $("{0:N0}" -f $avgFv2)ms" -ForegroundColor Green
}

Write-Host "`nTest complete." -ForegroundColor Cyan