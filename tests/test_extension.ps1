# --- Configuration ---
$GeneratorPath = ".\file-generator.exe"
$SolverPath = ".\siep.exe"
$DataDir = ".\data\ext_exact"
$ResultsFile = "results\extension_exact.csv"
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
# Zmieniono nagłówek - dodano Est_Edges
"Algorithm,Size_N,Est_Edges,Avg_Time_ms" | Out-File $ResultsFile -Encoding utf8

# --- Test Parameters ---
$GraphSizes = 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18
$Density = 0.4

Write-Host "Starting Exact Extension Tests (Simple/Reuse + Ullmann)..." -ForegroundColor Cyan

foreach ($n in $GraphSizes) {
    $totalSimple = 0
    $totalReuse = 0
    $filename = "ext_exact_n${n}.txt"
    $filepath = Join-Path $DataDir $filename

    # Obliczanie szacowanej liczby krawędzi (E)
    $estEdges = [Math]::Floor($Density * ($n * ($n - 1)) / 2)

    Write-Host "  Processing Size N=$n (E~$estEdges)" -NoNewline

    foreach ($seed in $Seeds) {
        # Generate RANDOM graphs
        & $GeneratorPath $n $n --output $filepath --density $Density --seed $seed | Out-Null

        # 1. Simple Strategy (with Exact Check)
        $t1 = Measure-Command {
            & $SolverPath --file $filepath --extend --extalgo simple --subalgo ullmann | Out-Null
        }
        $totalSimple += $t1.TotalMilliseconds

        # 2. Reuse Strategy (with Exact Check)
        $t2 = Measure-Command {
            & $SolverPath --file $filepath --extend --extalgo reuse --subalgo ullmann | Out-Null
        }
        $totalReuse += $t2.TotalMilliseconds
        
        Write-Host "." -NoNewline
    }

    # Calculate averages
    $avgSimple = $totalSimple / $Seeds.Count
    $avgReuse = $totalReuse / $Seeds.Count

    # Log results (Dodano $estEdges)
    "Simple+Ullmann,$n,$estEdges,$avgSimple" | Out-File $ResultsFile -Append -Encoding utf8
    "Reuse+Ullmann,$n,$estEdges,$avgReuse" | Out-File $ResultsFile -Append -Encoding utf8

    Write-Host " Done. Simple: $("{0:N0}" -f $avgSimple)ms | Reuse: $("{0:N0}" -f $avgReuse)ms" -ForegroundColor Green
}

Write-Host "`nTest complete." -ForegroundColor Cyan