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
"Algorithm,Size_N,Avg_Time_ms" | Out-File $ResultsFile -Encoding utf8

# --- Test Parameters ---
$GraphSizes = 10, 20, 30, 50, 100, 200, 500, 1000

Write-Host "Starting Approx. Extension Heuristics Tests..." -ForegroundColor Cyan

foreach ($n in $GraphSizes) {
    $totalSimple = 0
    $totalReuse = 0
    $filename = "ext_n${n}.txt"
    $filepath = Join-Path $DataDir $filename

    Write-Host "  Processing Size N=$n" -NoNewline

    foreach ($seed in $Seeds) {
        # Generate RANDOM graphs to ensure extension is required
        & $GeneratorPath $n $n --output $filepath --density 0.4 --seed $seed | Out-Null

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

    # Log results
    "TAP,$n,$avgTap" | Out-File $ResultsFile -Append -Encoding utf8
    "FV2,$n,$avgFV2" | Out-File $ResultsFile -Append -Encoding utf8

    Write-Host " Done. TAP: $("{0:N0}" -f $avgTap)ms | FV2: $("{0:N0}" -f $avgFV2)ms" -ForegroundColor Green
}

Write-Host "`nTest complete." -ForegroundColor Cyan