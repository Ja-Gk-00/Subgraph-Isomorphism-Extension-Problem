# --- Configuration ---
$GeneratorPath = ".\file-generator.exe"
$SolverPath = ".\siep.exe"
$DataDir = ".\data\multi_metric"
$ResultsFile = "results\multi_metric.csv"
$Seeds = 10, 20, 30, 40, 50

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
$GraphSizes = 100, 500, 1000, 2000
$Weights = 1, 5, 10 # 1 = simple graph, >1 = multigraph
$Density = 0.4

Write-Host "Starting Multigraph Metric Tests..." -ForegroundColor Cyan

foreach ($w in $Weights) {
    Write-Host "Testing Max Weight: $w" -ForegroundColor Magenta
    
    foreach ($n in $GraphSizes) {
        $totalTime = 0
        $filename = "multi_n${n}_w${w}.txt"
        $filepath = Join-Path $DataDir $filename

        # Calculate estimated edges: 0.4 * N * (N-1) / 2
        $estEdges = [Math]::Floor($Density * ($n * ($n - 1)) / 2)

        Write-Host "  Processing Size N=$n (E~$estEdges)" -NoNewline

        foreach ($seed in $Seeds) {
            # Generate multigraph
            & $GeneratorPath $n $n --output $filepath --density $Density --max-weight $w --allow-loops --seed $seed | Out-Null
            
            # Measure time
            $time = Measure-Command {
                & $SolverPath --file $filepath --distance | Out-Null
            }
            $totalTime += $time.TotalMilliseconds
            Write-Host "." -NoNewline
        }

        $avgTime = $totalTime / $Seeds.Count
        
        # Log result with Est_Edges
        "WL_Kernel,$n,$estEdges,$w,$avgTime" | Out-File $ResultsFile -Append -Encoding utf8
        
        Write-Host " Avg: $("{0:N2}" -f $avgTime) ms" -ForegroundColor Green
    }
}

Write-Host "`nTest complete." -ForegroundColor Cyan