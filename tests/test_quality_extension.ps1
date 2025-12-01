# --- Configuration ---
$GeneratorPath = ".\file-generator.exe"
$SolverPath = ".\siep.exe"
$DataDir = ".\data\quality_full"
$RawResultsFile = "results\quality_extension_raw.csv"
$SummaryResultsFile = "results\quality_extension_summary.csv"

# --- Directory Setup ---
if (!(Test-Path $DataDir)) { New-Item -ItemType Directory -Path $DataDir -Force | Out-Null }
$ResultsDir = [System.IO.Path]::GetDirectoryName($RawResultsFile)
if (!(Test-Path $ResultsDir)) { New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null }

# --- Initialize CSVs ---
"Size_N,Est_Edges,Max_Weight,Seed,Algorithm,Added_Vertices,Added_Edges" | Out-File $RawResultsFile -Encoding utf8
"Size_N,Est_Edges,Max_Weight,Algorithm,Avg_Added_Vertices,Avg_Added_Edges" | Out-File $SummaryResultsFile -Encoding utf8

# --- Test Parameters ---
$GraphSizes = 50, 100, 200
$Weights = 1, 5, 10
$Seeds = 10, 20, 30, 40, 50
$Strategies = "tap", "lerp"
$Density = 0.4

Write-Host "Starting Full Extension Quality Check (TAP vs LERP)..." -ForegroundColor Cyan

foreach ($n in $GraphSizes) {

    $estEdges = [Math]::Floor($Density * ($n * ($n - 1)) / 2)

    foreach ($w in $Weights) {
        
        Write-Host "`n--- Processing N=$n (E~$estEdges), MaxWeight=$w ---" -ForegroundColor Magenta
        
        $statsV = @{}
        $statsE = @{}
        foreach ($algo in $Strategies) { 
            $statsV[$algo] = 0
            $statsE[$algo] = 0
        }

        foreach ($seed in $Seeds) {
            $file = Join-Path $DataDir "qual_n${n}_w${w}_s${seed}.txt"
            
            # Generate random graphs
            & $GeneratorPath $n $n --output $file --density $Density --allow-loops --max-weight $w --seed $seed | Out-Null

            Write-Host "  Seed=${seed}: " -NoNewline

            foreach ($algo in $Strategies) {
                # Run solver
                $output = & $SolverPath --file $file --extend --extalgo $algo 2>&1 | Out-String

                $v = 0
                $e = 0

                # Regex parsing
                if ($output -match "Added vertices:\s*(\d+)") { 
                    $v = [int]$matches[1] 
                }
                if ($output -match "Added edges \(multiplicity units\):\s*(\d+)") { 
                    $e = [int]$matches[1] 
                }

                # Save RAW result
                "$n,$estEdges,$w,$seed,$algo,$v,$e" | Out-File $RawResultsFile -Append -Encoding utf8
                
                # Accumulate for average
                $statsV[$algo] += $v
                $statsE[$algo] += $e

                # Print to console
                $color = if ($algo -eq "tap") { "Yellow" } else { "Green" }
                Write-Host "[${algo}: +$v V, +$e E] " -NoNewline -ForegroundColor $color
            }
            Write-Host ""
        }

        # --- Calculate and Save Averages ---
        foreach ($algo in $Strategies) {
            $avgV = $statsV[$algo] / $Seeds.Count
            $avgE = $statsE[$algo] / $Seeds.Count
            
            # Save Summary (with Est_Edges)
            "$n,$estEdges,$w,$algo,$avgV,$avgE" | Out-File $SummaryResultsFile -Append -Encoding utf8
            
            Write-Host "  >> AVG [$algo]: Vertices +$("{0:N1}" -f $avgV), Edges +$("{0:N1}" -f $avgE)" -ForegroundColor Cyan
        }
    }
}

Write-Host "`nTest complete." -ForegroundColor Cyan
Write-Host "Raw data: $RawResultsFile"
Write-Host "Averages: $SummaryResultsFile"