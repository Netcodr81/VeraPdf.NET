param(
    [Parameter(Mandatory = $true)]
    [string]$JavaResultsRoot,

    [Parameter(Mandatory = $true)]
    [string]$PdfCorpusRoot,

    [Parameter(Mandatory = $true)]
    [string]$OutputRoot,

    [Parameter(Mandatory = $false)]
    [string]$JavaValidatorVersion = "unknown",

    [Parameter(Mandatory = $false)]
    [string]$DefaultProfileId = "pdfa-1b"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-FullPath {
    param([string]$PathValue)
    return [System.IO.Path]::GetFullPath($PathValue)
}

function Ensure-Directory {
    param([string]$PathValue)
    if (-not (Test-Path -LiteralPath $PathValue)) {
        New-Item -ItemType Directory -Path $PathValue -Force | Out-Null
    }
}

function To-ManifestRelativePath {
    param(
        [string]$Root,
        [string]$FullPath
    )

    $rootPath = Resolve-FullPath $Root
    if (-not $rootPath.EndsWith("\")) {
        $rootPath = $rootPath + "\"
    }

    $rootUri = [System.Uri]$rootPath
    $fileUri = [System.Uri](Resolve-FullPath $FullPath)
    return [System.Uri]::UnescapeDataString($rootUri.MakeRelativeUri($fileUri).ToString())
}

$javaRoot = Resolve-FullPath $JavaResultsRoot
$pdfRoot = Resolve-FullPath $PdfCorpusRoot
$outRoot = Resolve-FullPath $OutputRoot

if (-not (Test-Path -LiteralPath $javaRoot)) {
    throw "Java results root does not exist: $javaRoot"
}

if (-not (Test-Path -LiteralPath $pdfRoot)) {
    throw "PDF corpus root does not exist: $pdfRoot"
}

Ensure-Directory -PathValue $outRoot

$baselinePdfRoot = Join-Path $outRoot "pdf"
$baselineResultsRoot = Join-Path $outRoot "java-results"

Ensure-Directory -PathValue $baselinePdfRoot
Ensure-Directory -PathValue $baselineResultsRoot

$javaJsonFiles = Get-ChildItem -Path $javaRoot -Recurse -File -Filter *.json |
    Sort-Object FullName

$cases = New-Object System.Collections.Generic.List[object]

foreach ($jsonFile in $javaJsonFiles) {
    $caseId = [System.IO.Path]::GetFileNameWithoutExtension($jsonFile.Name)

    $matchingPdf = Get-ChildItem -Path $pdfRoot -Recurse -File -Filter "$caseId.pdf" |
        Select-Object -First 1

    if ($null -eq $matchingPdf) {
        Write-Warning "Skipping case '$caseId': matching PDF not found in corpus root."
        continue
    }

    $copiedPdfPath = Join-Path $baselinePdfRoot ($matchingPdf.Name)
    $copiedJsonPath = Join-Path $baselineResultsRoot ($jsonFile.Name)

    Copy-Item -LiteralPath $matchingPdf.FullName -Destination $copiedPdfPath -Force
    Copy-Item -LiteralPath $jsonFile.FullName -Destination $copiedJsonPath -Force

    $cases.Add([ordered]@{
        id = $caseId
        profileId = $DefaultProfileId
        pdfPath = To-ManifestRelativePath -Root $outRoot -FullPath $copiedPdfPath
        javaResultPath = To-ManifestRelativePath -Root $outRoot -FullPath $copiedJsonPath
    })
}

$manifest = [ordered]@{
    version = 1
    javaValidatorVersion = $JavaValidatorVersion
    generatedUtc = (Get-Date).ToUniversalTime().ToString("o")
    cases = $cases
}

$manifestPath = Join-Path $outRoot "parity-baseline.manifest.json"
$manifestJson = $manifest | ConvertTo-Json -Depth 8
[System.IO.File]::WriteAllText(
    $manifestPath,
    $manifestJson,
    [System.Text.UTF8Encoding]::new($false))

Write-Host "Imported $($cases.Count) case(s)."
Write-Host "Manifest written to: $manifestPath"
