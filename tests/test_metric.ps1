# --- Configuration ---
$GeneratorPath = ".\file-generator.exe"
$SolverPath = ".\siep.exe"
$DataDir = ".\data\metric"
$ResultsFile = ".\results\metric.csv"
$Seeds = 10, 20, 30, 40, 50 

if (!(Test-Path $DataDir)) { New-Item -ItemType Directory -Path $DataDir | Out-Null }

$ResultsDir = [System.IO.Path]::GetDirectoryName($ResultsFile)
if (!(Test-Path $ResultsDir)) { 
    New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null 
}

# --- Initialize CSV ---
# Added Est_Edges column
"Algorithm,Size_N,Est_Edges,Avg_Time_ms" | Out-File $ResultsFile -Encoding utf8

# --- Test Parameters ---
$GraphSizes = 10, 50, 100, 500, 1000, 2000, 5000
$Density = 0.3

Write-Host "Starting Metric Tests (Averaged over $($Seeds.Count) seeds)..." -ForegroundColor Cyan

foreach ($n in $GraphSizes) {
    $totalTime = 0
    $filename = "graph_${n}.txt"
    $filepath = Join-Path $DataDir $filename

    # Calculate estimated edges: 0.3 * N * (N-1) / 2
    $estEdges = [Math]::Floor($Density * ($n * ($n - 1)) / 2)

    Write-Host "  Processing Size N=$n (E~$estEdges)" -NoNewline

    foreach ($seed in $Seeds) {
        # 1. Generate with specific SEED
        # Arguments: N1 N2 --output path --density 0.3 --seed X
        & $GeneratorPath $n $n --output $filepath --density $Density --seed $seed | Out-Null
        
        # 2. Measure time
        $time = Measure-Command {
            & $SolverPath --file $filepath --distance | Out-Null
        }
        $totalTime += $time.TotalMilliseconds
        Write-Host "." -NoNewline # Progress dot
    }

    # 3. Calculate Average
    $avgTime = $totalTime / $Seeds.Count
    
    # 4. Log Result (Added $estEdges)
    "WL_Kernel,$n,$estEdges,$avgTime" | Out-File $ResultsFile -Append -Encoding utf8
    
    Write-Host " Avg Time: $("{0:N2}" -f $avgTime) ms" -ForegroundColor Yellow
}

Write-Host "`nTest complete. Results saved to $ResultsFile" -ForegroundColor Cyan