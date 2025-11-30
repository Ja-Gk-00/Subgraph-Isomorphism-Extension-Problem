# --- Configuration ---
$GeneratorPath = ".\file-generator.exe"
$SolverPath = ".\siep.exe"
$DataDir = ".\data\ext_multi_time"
$RawResultsFile = "results\extension_multigraph_time_raw.csv"
$SummaryResultsFile = "results\extension_multigraph_time_summary.csv"

# --- Directory Setup ---
if (!(Test-Path $DataDir)) { New-Item -ItemType Directory -Path $DataDir -Force | Out-Null }
$ResultsDir = [System.IO.Path]::GetDirectoryName($RawResultsFile)
if (!(Test-Path $ResultsDir)) { New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null }

# --- Initialize CSVs ---
"Size_N,Est_Edges,Max_Weight,Seed,Algorithm,Time_ms" | Out-File $RawResultsFile -Encoding utf8
"Size_N,Est_Edges,Max_Weight,Algorithm,Avg_Time_ms" | Out-File $SummaryResultsFile -Encoding utf8

# --- Test Parameters ---
$GraphSizes = 50, 100, 200
$Weights = 1, 5, 10
$Seeds = 10, 20, 30, 40, 50
$Strategies = "tap", "lerp"
$Density = 0.4

Write-Host "Starting Full Extension Time Check (TAP vs LERP)..." -ForegroundColor Cyan

foreach ($n in $GraphSizes) {
    # Calculate estimated edges
    $estEdges = [Math]::Floor($Density * ($n * ($n - 1)) / 2)

    foreach ($w in $Weights) {
        
        Write-Host "`n--- Processing N=$n (E~$estEdges), MaxWeight=$w ---" -ForegroundColor Magenta
        
        # Inicjalizacja sumatorów do średniej
        $statsTime = @{}
        foreach ($algo in $Strategies) { 
            $statsTime[$algo] = 0
        }

        foreach ($seed in $Seeds) {
            $file = Join-Path $DataDir "ext_n${n}_w${w}_s${seed}.txt"
            
            # Generate random graphs
            & $GeneratorPath $n $n --output $file --density $Density --allow-loops --max-weight $w --seed $seed | Out-Null

            Write-Host "  Seed=${seed}: " -NoNewline

            foreach ($algo in $Strategies) {
                # Run solver AND measure time
                # Using --subalgo deg to verify it's polynomial/approx time we are measuring
                $time = Measure-Command {
                    & $SolverPath --file $file --extend --extalgo $algo --subalgo deg | Out-Null
                }
                
                $ms = $time.TotalMilliseconds

                # Save RAW result
                "$n,$estEdges,$w,$seed,$algo,$ms" | Out-File $RawResultsFile -Append -Encoding utf8
                
                # Accumulate for average
                $statsTime[$algo] += $ms

                # Print to console
                $color = if ($algo -eq "tap") { "Yellow" } else { "Green" }
                Write-Host "[${algo}: $(" {0:N0}" -f $ms)ms] " -NoNewline -ForegroundColor $color
            }
            Write-Host ""
        }

        # --- Calculate and Save Averages ---
        foreach ($algo in $Strategies) {
            $avgTime = $statsTime[$algo] / $Seeds.Count
            
            # Save Summary
            "$n,$estEdges,$w,$algo,$avgTime" | Out-File $SummaryResultsFile -Append -Encoding utf8
            
            Write-Host "  >> AVG [$algo]: Time $("{0:N1}" -f $avgTime) ms" -ForegroundColor Cyan
        }
    }
}

Write-Host "`nTest complete." -ForegroundColor Cyan
Write-Host "Raw data: $RawResultsFile"
Write-Host "Averages: $SummaryResultsFile"