# --- Configuration ---
$GeneratorPath = ".\file-generator.exe"
$SolverPath = ".\siep.exe"
$DataDir = ".\data\metric_quality"
$ResultsFile = "results\metric_values.txt"

# --- Directory Setup ---
if (!(Test-Path $DataDir)) { New-Item -ItemType Directory -Path $DataDir -Force | Out-Null }
$ResultsDir = [System.IO.Path]::GetDirectoryName($ResultsFile)
if (!(Test-Path $ResultsDir)) { New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null }

# --- Clear previous results ---
"" | Out-File $ResultsFile -Encoding utf8

Write-Host "Starting Metric Quality Check..." -ForegroundColor Cyan
"--- Metric Quality Test Results ---" | Out-File $ResultsFile -Append

# --- TEST 1: Isomorphic Graphs (Expected Distance ~ 0) ---
Write-Host "1. Testing Isomorphic Pair (Expect ~0)..." -NoNewline
$isoFile = Join-Path $DataDir "iso_check.txt"
# Generate isomorphic pair
& $GeneratorPath 20 20 --isomorphic --output $isoFile --density 0.5 --seed 123 | Out-Null

# Run solver and capture output
$outputIso = & $SolverPath --file $isoFile --distance 2>&1

"TYPE: Isomorphic Pair (N=20)" | Out-File $ResultsFile -Append
"OUTPUT: $outputIso" | Out-File $ResultsFile -Append
"-----------------------------" | Out-File $ResultsFile -Append
Write-Host " Done." -ForegroundColor Green

# --- TEST 2: Random Different Graphs (Expected Distance > 0) ---
Write-Host "2. Testing Random Pair (Expect >0)..." -NoNewline
$randFile = Join-Path $DataDir "rand_check.txt"
# Generate random pair (likely different)
& $GeneratorPath 20 20 --output $randFile --density 0.5 --seed 124 | Out-Null

$outputRand = & $SolverPath --file $randFile --distance 2>&1

"TYPE: Random Pair (N=20)" | Out-File $ResultsFile -Append
"OUTPUT: $outputRand" | Out-File $ResultsFile -Append
"-----------------------------" | Out-File $ResultsFile -Append
Write-Host " Done." -ForegroundColor Green

Write-Host "`nCheck values in $ResultsFile" -ForegroundColor Cyan