# --- Configuration ---
$GeneratorPath = ".\file-generator.exe"
$SolverPath = ".\siep.exe"
$DataDir = ".\data\subgraph"
$ResultsFile = "results\subgraph_fixed_pattern.csv"
$Seeds = 10, 20

# --- Directory Setup ---
if (!(Test-Path $DataDir)) { New-Item -ItemType Directory -Path $DataDir -Force | Out-Null }
$ResultsDir = [System.IO.Path]::GetDirectoryName($ResultsFile)
if (!(Test-Path $ResultsDir)) { New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null }

# --- Initialize CSV ---
# Added Est_Edges_Target column
"Algorithm,Pattern_N1,Target_N2,Est_Edges_Target,Avg_Time_ms" | Out-File $ResultsFile -Encoding utf8

# --- Test Parameters ---
$N1 = 5
$TargetSizes = 10, 20, 30, 50, 100
$Density = 0.5

Write-Host "Starting Subgraph Isomorphism Spot Check (Pattern N=$N1)..." -ForegroundColor Cyan

foreach ($n2 in $TargetSizes) {
    $totalUllmann = 0
    $filename = "subgraph_n${N1}_in_n${n2}.txt"
    $filepath = Join-Path $DataDir $filename

    # Calculate estimated edges for the TARGET graph (n2)
    $estEdges = [Math]::Floor($Density * ($n2 * ($n2 - 1)) / 2)

    Write-Host "  Search N1=$N1 inside N2=$n2 (Target E~$estEdges)" -NoNewline

    foreach ($seed in $Seeds) {
        # When N1 != n2, --isomorphic creates a subgraph relationship.
        # N1 (small) is embedded into n2 (large).
        & $GeneratorPath $N1 $n2 --output $filepath --density $Density --seed $seed --isomorphic | Out-Null
        
        # Test Ullmann
        $t = Measure-Command {
            & $SolverPath --file $filepath --check --subalgo ullmann | Out-Null
        }
        $totalUllmann += $t.TotalMilliseconds
        
        Write-Host "." -NoNewline
    }

    $avg = $totalUllmann / $Seeds.Count
    
    # Log result with Edge Count
    "Ullmann,$N1,$n2,$estEdges,$avg" | Out-File $ResultsFile -Append -Encoding utf8
    
    Write-Host " Done. Time: $("{0:N0}" -f $avg)ms" -ForegroundColor Green
}

Write-Host "`nTest complete." -ForegroundColor Cyan