# --- Configuration ---
$GeneratorPath = ".\file-generator.exe"
$SolverPath = ".\siep.exe"
$DataDir = ".\data\quality"
$ResultsFile = "results\quality_extension.csv"

# --- Directory Setup ---
if (!(Test-Path $DataDir)) { New-Item -ItemType Directory -Path $DataDir -Force | Out-Null }
$ResultsDir = [System.IO.Path]::GetDirectoryName($ResultsFile)
if (!(Test-Path $ResultsDir)) { New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null }

# --- Initialize CSV ---
"Case,Algorithm,Added_Vertices,Added_Edges" | Out-File $ResultsFile -Encoding utf8

Write-Host "Starting Extension Quality Check (Simple vs Reuse)..." -ForegroundColor Cyan

# Small N to ensure readability and speed
$N = 10
$Seeds = 10, 20, 30, 40, 50, 60, 70, 80, 90, 100

foreach ($seed in $Seeds) {
    $file = Join-Path $DataDir "qual_n${N}_s${seed}.txt"
    # Generate random graphs
    & $GeneratorPath $N $N --output $file --density 0.5 --seed $seed | Out-Null

    Write-Host "Case Seed=${seed}: " -NoNewline

    # --- Define Strategies to Test ---
    $strategies = "simple", "reuse", "greedy-reuse", "disjoint", "tap", "fv2-disjoint", "vf2-reuse"

    foreach ($algo in $strategies) {
        # Use 'deg' for speed, we care about the extension result quality
        $output = & $SolverPath --file $file --extend --extalgo $algo --subalgo deg 2>&1 | Out-String

        $v = "Unknown"
        $e = "Unknown"

        # Regex for "Added vertices: x"
        if ($output -match "Added vertices:\s*(\d+)") { 
            $v = $matches[1] 
        }
        # Regex for "Added edges (multiplicity units): y"
        if ($output -match "Added edges \(multiplicity units\):\s*(\d+)") { 
            $e = $matches[1] 
        }

        # Save to CSV
        "Seed_${seed},$algo,$v,$e" | Out-File $ResultsFile -Append -Encoding utf8
        
        # Print to console
        $color = if ($algo -eq "simple") { "Yellow" } else { "Green" }
        Write-Host "[${algo}: +$v V, +$e E] " -NoNewline -ForegroundColor $color
    }
    Write-Host ""
}

Write-Host "`nDone. Check $ResultsFile" -ForegroundColor Cyan