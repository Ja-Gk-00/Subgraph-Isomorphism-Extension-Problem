# --- Configuration ---
$GeneratorPath = ".\file-generator.exe"
$SolverPath = ".\siep.exe"
$DataDir = ".\data\correctness"
$ResultsFile = "results\correctness_exact.txt"

# --- Directory Setup ---
if (!(Test-Path $DataDir)) { New-Item -ItemType Directory -Path $DataDir -Force | Out-Null }
$ResultsDir = [System.IO.Path]::GetDirectoryName($ResultsFile)
if (!(Test-Path $ResultsDir)) { New-Item -ItemType Directory -Path $ResultsDir -Force | Out-Null }

# Clear log
"" | Out-File $ResultsFile -Encoding utf8

function Run-Test {
    param ([string]$name, [string]$file, [string]$algo, [string]$expected)

    # Run solver
    $output = & $SolverPath --file $file --check --subalgo $algo 2>&1 | Out-String

    $result = "Error"
    if ($output -match "is NOT a subgraph") {
        $result = "False"
    }
    elseif ($output -match "is a subgraph") {
        $result = "True"
    }

    # Verify against expected
    $status = "FAIL"
    $color = "Red"
    
    if ($expected -eq "Any") {
        $status = "INFO"
        $color = "Yellow"
    }
    elseif ($result -eq $expected) {
        $status = "PASS"
        $color = "Green"
    }

    Write-Host "  [$algo] $name : $result (Expected: $expected) -> $status" -ForegroundColor $color
    "$name,$algo,$result,$expected,$status" | Out-File $ResultsFile -Append
    return $result
}

Write-Host "Starting Correctness Tests (Exact Algorithms)..." -ForegroundColor Cyan

# --- TEST 1: Exact Isomorphism (Positive) ---
# N=8, Isomorphic flag -> Should be TRUE
$fileIso = Join-Path $DataDir "iso_check.txt"
& $GeneratorPath 8 8 --isomorphic --output $fileIso --seed 123 | Out-Null

Write-Host "`n1. Checking Isomorphism (N=8, Must be True)" -ForegroundColor Magenta
Run-Test "Iso_Test" $fileIso "naive" "True"
Run-Test "Iso_Test" $fileIso "ullmann" "True"

# --- TEST 2: Subgraph Isomorphism (Positive) ---
# N1=5, N2=10, Isomorphic flag (embeds smaller in larger) -> Should be TRUE
$fileSub = Join-Path $DataDir "sub_check.txt"
& $GeneratorPath 5 10 --isomorphic --output $fileSub --seed 456 | Out-Null

Write-Host "`n2. Checking Subgraph (N=5 in N=10, Must be True)" -ForegroundColor Magenta
Run-Test "Sub_Test" $fileSub "naive" "True"
Run-Test "Sub_Test" $fileSub "ullmann" "True"

# --- TEST 3: Consistency Check (Random Graphs) ---
# Naive and Ullmann MUST give the same answer.
Write-Host "`n3. Consistency Check (Naive vs Ullmann)" -ForegroundColor Magenta

for ($i=1; $i -le 3; $i++) {
    $fileRand = Join-Path $DataDir "rand_$i.txt"
    & $GeneratorPath 6 6 --output $fileRand --density 0.5 --seed $i | Out-Null
    
    $resNaive = Run-Test "Rand_$i" $fileRand "naive" "Any"
    $resUllmann = Run-Test "Rand_$i" $fileRand "ullmann" "Any"

    if ($resNaive -eq "Error" -or $resUllmann -eq "Error") {
         Write-Host "  -> ERROR: Could not parse output" -ForegroundColor Red
    }
    elseif ($resNaive -eq $resUllmann) {
        Write-Host "  -> CONSISTENCY PASS: Both returned $resNaive" -ForegroundColor Green
    } else {
        Write-Host "  -> CONSISTENCY FAIL: Naive=$resNaive, Ullmann=$resUllmann" -ForegroundColor Red
    }
}

Write-Host "`nDone. Check $ResultsFile" -ForegroundColor Cyan