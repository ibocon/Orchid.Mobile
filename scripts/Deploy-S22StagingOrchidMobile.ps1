#!/usr/bin/env pwsh
#requires -Version 7.0

<#
.SYNOPSIS
    Galaxy S22 Ultra(SM-S908N)에 Orchid.Mobile Staging 빌드를 배포하고 기동을 검증한다.

.DESCRIPTION
    Peony.Mobile의 Deploy-S22StagingPeonyMobile.ps1과 동일한 진입점 패턴으로,
    adb devices -l에서 SM-S908N(model:SM_S908N 또는 ro.product.model == SM-S908N)을 자동 탐색하거나
    기본 무선 ADB 주소(10.79.96.2:5555)로 connect 폴백을 수행한 뒤 Start-OrchidMobileStaging.ps1에 위임한다.
    배포 완료 후 pidof com.botanicsoft.orchid(필요 시 monkey 런처 실행 폴백)로 프로세스 실행을 확인한다.

.PARAMETER Serial
    대상 Android 기기 시리얼. 생략 시 연결된 SM-S908N을 자동 탐색하거나 10.79.96.2:5555로 연결한다.

.PARAMETER Configuration
    빌드 구성(Debug 또는 Release). 기본값은 Debug다.

.PARAMETER RelayUrl
    모바일 클라이언트가 연결할 릴레이 베이스 URL. 기본값은 https://stag.botanicsoft.com/orchid 다.

.PARAMETER SkipPreflight
    지정 시 Staging 릴레이 엔드포인트 사전 연결 점검을 건너뛴다.

.EXAMPLE
    pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/Deploy-S22StagingOrchidMobile.ps1
#>

[CmdletBinding()]
param(
    [string] $Serial,

    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug',

    [string] $RelayUrl = 'https://stag.botanicsoft.com/orchid',

    [switch] $SkipPreflight,

    [Parameter(DontShow)]
    [string] $AdbCommand = 'adb',

    [Parameter(DontShow)]
    [string] $StagingScriptPath = (Join-Path -Path $PSScriptRoot -ChildPath 'Start-OrchidMobileStaging.ps1'),

    [Parameter(DontShow)]
    [string] $PackageId = 'com.botanicsoft.orchid',

    [Parameter(DontShow)]
    [string] $FallbackSerial = '10.79.96.2:5555',

    [Parameter(DontShow)]
    [int] $VerifyTimeoutSeconds = 60,

    [Parameter(DontShow)]
    [int] $PollIntervalSeconds = 2
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

function Resolve-S22Serial {
    if (-not [string]::IsNullOrWhiteSpace($Serial)) {
        [Console]::WriteLine("[s22-orchid-staging] using explicit serial: $Serial")
        return $Serial.Trim()
    }

    $deviceLines = @(& $AdbCommand devices -l 2>&1)
    foreach ($line in $deviceLines) {
        if ([string] $line -match '^\s*(?<serial>\S+)\s+device\b.*\bmodel:SM[-_]S908N\b') {
            $detected = [string] $Matches['serial']
            [Console]::WriteLine("[s22-orchid-staging] detected SM-S908N serial from adb devices -l: $detected")
            return $detected
        }
    }

    foreach ($line in $deviceLines) {
        if ([string] $line -match '^\s*(?<serial>\S+)\s+device\b') {
            $candidate = [string] $Matches['serial']
            $model = (& $AdbCommand -s $candidate shell getprop ro.product.model 2>&1 | Out-String).Trim()
            if ($model -match '^SM[-_]S908N$') {
                [Console]::WriteLine("[s22-orchid-staging] detected SM-S908N serial via getprop: $candidate")
                return $candidate
            }
        }
    }

    [Console]::WriteLine("[s22-orchid-staging] SM-S908N not found in adb devices; attempting adb connect $FallbackSerial...")
    $connectOutput = (& $AdbCommand connect $FallbackSerial 2>&1 | Out-String).Trim()
    if ($connectOutput -notmatch 'connected to|already connected') {
        throw "[s22-orchid-staging] sm-s908n-not-found: failed to connect to $FallbackSerial ($connectOutput)"
    }

    $postConnectLines = @(& $AdbCommand devices -l 2>&1)
    foreach ($line in $postConnectLines) {
        if ([string] $line -match '^\s*(?<serial>\S+)\s+device\b') {
            $candidate = [string] $Matches['serial']
            if ([string] $line -match '\bmodel:SM[-_]S908N\b' -or $candidate -eq $FallbackSerial) {
                [Console]::WriteLine("[s22-orchid-staging] connected SM-S908N serial: $candidate")
                return $candidate
            }
        }
    }

    throw "[s22-orchid-staging] sm-s908n-not-ready: $FallbackSerial did not enter device state after adb connect."
}

if (-not (Test-Path -LiteralPath $StagingScriptPath -PathType Leaf)) {
    throw "[s22-orchid-staging] staging-script-not-found: $StagingScriptPath"
}

$ResolvedSerial = Resolve-S22Serial

$StagingParams = @{
    Serial        = $ResolvedSerial
    Configuration = $Configuration
    RelayUrl      = $RelayUrl
}
if ($SkipPreflight) {
    $StagingParams['SkipPreflight'] = $true
}

Write-Output "[s22-orchid-staging] delegating to $StagingScriptPath (Serial=$ResolvedSerial, Configuration=$Configuration)"
& $StagingScriptPath @StagingParams

$Deadline = [DateTime]::UtcNow.AddSeconds([Math]::Max(5, $VerifyTimeoutSeconds))
$PollCount = 0
$LaunchedFallback = $false

while ([DateTime]::UtcNow -lt $Deadline) {
    $PollCount++
    $PidOutput = (& $AdbCommand -s $ResolvedSerial shell pidof $PackageId 2>$null | Out-String).Trim()
    if (-not [string]::IsNullOrWhiteSpace($PidOutput)) {
        Write-Output "[s22-orchid-staging] verified $PackageId running on $ResolvedSerial (PID=$PidOutput)"
        return
    }

    if (-not $LaunchedFallback -and $PollCount -ge 2) {
        $LaunchedFallback = $true
        Write-Output "[s22-orchid-staging] launching $PackageId via monkey on $ResolvedSerial..."
        $null = & $AdbCommand -s $ResolvedSerial shell monkey -p $PackageId -c android.intent.category.LAUNCHER 1 2>&1
    }

    if ($PollIntervalSeconds -gt 0) {
        Start-Sleep -Seconds $PollIntervalSeconds
    }
}

throw "[s22-orchid-staging] app-not-running: $PackageId did not report a running PID on $ResolvedSerial within ${VerifyTimeoutSeconds}s."
