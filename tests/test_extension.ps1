# --- Configuration ---
$GeneratorPath = ".\file-generator.exe"
$SolverPath = ".\siep.exe"
$DataDir = ".\data\ext"
$ResultsFile = "results\extension.csv"
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
"Algorithm,Size_N,Avg_Time_ms" | Out-File $ResultsFile -Encoding utf8

# --- Test Parameters ---
$GraphSizes = 10, 20, 30, 50, 100

Write-Host "Starting Extension Heuristics Tests..." -ForegroundColor Cyan

foreach ($n in $GraphSizes) {
    $totalSimple = 0
    $totalReuse = 0
    $filename = "ext_n${n}.txt"
    $filepath = Join-Path $DataDir $filename

    Write-Host "  Processing Size N=$n" -NoNewline

    foreach ($seed in $Seeds) {
        # Generate RANDOM graphs to ensure extension is required
        & $GeneratorPath $n $n --output $filepath --density 0.4 --seed $seed | Out-Null
        
        # 1. Simple Extension Strategy
        $t1 = Measure-Command {
            & $SolverPath --file $filepath --extend --extalgo simple | Out-Null
        }
        $totalSimple += $t1.TotalMilliseconds

        # 2. Reuse Extension Strategy
        $t2 = Measure-Command {
            & $SolverPath --file $filepath --extend --extalgo reuse | Out-Null
        }
        $totalReuse += $t2.TotalMilliseconds
        
        Write-Host "." -NoNewline
    }

    # Calculate averages
    $avgSimple = $totalSimple / $Seeds.Count
    $avgReuse = $totalReuse / $Seeds.Count

    # Log results
    "Simple,$n,$avgSimple" | Out-File $ResultsFile -Append -Encoding utf8
    "Reuse,$n,$avgReuse" | Out-File $ResultsFile -Append -Encoding utf8

    Write-Host " Done. Simple: $("{0:N0}" -f $avgSimple)ms | Reuse: $("{0:N0}" -f $avgReuse)ms" -ForegroundColor Green
}

Write-Host "`nTest complete." -ForegroundColor Cyan