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

"Algorithm,Size_N,Avg_Time_ms" | Out-File $ResultsFile -Encoding utf8

# --- Test Parameters ---
$GraphSizes = 10, 50, 100, 500, 1000, 2000, 5000

Write-Host "Starting Metric Tests (Averaged over $($Seeds.Count) seeds)..." -ForegroundColor Cyan

foreach ($n in $GraphSizes) {
    $totalTime = 0
    $filename = "graph_${n}.txt"
    $filepath = Join-Path $DataDir $filename

    Write-Host "  Processing Size N=$n" -NoNewline

    foreach ($seed in $Seeds) {
        # 1. Generate with specific SEED
        # Arguments: N1 N2 --output path --density 0.3 --seed X
        & $GeneratorPath $n $n --output $filepath --density 0.3 --seed $seed | Out-Null
        
        # 2. Measure time
        $time = Measure-Command {
            & $SolverPath --file $filepath --distance | Out-Null
        }
        $totalTime += $time.TotalMilliseconds
        Write-Host "." -NoNewline # Progress dot
    }

    # 3. Calculate Average
    $avgTime = $totalTime / $Seeds.Count
    
    # 4. Log Result
    "WL_Kernel,$n,$avgTime" | Out-File $ResultsFile -Append -Encoding utf8
    
    Write-Host " Avg Time: $("{0:N2}" -f $avgTime) ms" -ForegroundColor Yellow
}

Write-Host "`nTest complete. Results saved to $ResultsFile" -ForegroundColor Cyan