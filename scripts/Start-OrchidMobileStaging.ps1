#!/usr/bin/env pwsh
#requires -Version 7.0

<#
.SYNOPSIS
    Orchid.Mobile을 Hanul Staging(/orchid) 환경 설정으로 빌드하고 연결된 Android 기기/에뮬레이터에 실행한다.

.PARAMETER Serial
    대상 Android 기기 시리얼(adb devices 결과). 생략 시 단일 연결 기기를 사용한다.

.PARAMETER Configuration
    빌드 구성(Debug 또는 Release). 기본값은 Debug다.

.PARAMETER RelayUrl
    모바일 클라이언트가 연결할 릴레이 베이스 URL. 기본값은 https://stag.botanicsoft.com/orchid 다.

.PARAMETER BuildOnly
    지정 시 기기 설치/실행(`-t:Run`) 없이 Staging Android 빌드만 수행한다.

.PARAMETER SkipPreflight
    지정 시 Staging 릴레이 엔드포인트 사전 연결 점검을 건너뛴다.
#>

[CmdletBinding()]
param(
    [string] $Serial,

    [ValidateSet('Debug', 'Release')]
    [string] $Configuration = 'Debug',

    [string] $RelayUrl = 'https://stag.botanicsoft.com/orchid',

    [switch] $BuildOnly,

    [switch] $SkipPreflight,

    [Parameter(DontShow)]
    [string] $RepositoryRoot = (Resolve-Path -Path (Join-Path -Path $PSScriptRoot -ChildPath '..')).Path,

    [Parameter(DontShow)]
    [string] $DotnetCommand = 'dotnet'
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$NormalizedRelayUrl = $RelayUrl.TrimEnd('/')
$NegotiateUri = "$NormalizedRelayUrl/hubs/clipboard/negotiate?negotiateVersion=1"
$ProjectPath = Join-Path -Path $RepositoryRoot -ChildPath 'src\Orchid.Mobile.csproj'

if (-not (Test-Path -LiteralPath $ProjectPath -PathType Leaf)) {
    throw "[orchid-mobile-staging] project-not-found: $ProjectPath"
}

if (-not $SkipPreflight) {
    Write-Output "[orchid-mobile-staging] preflight -> POST $NegotiateUri"
    $NegotiateResponse = Invoke-RestMethod -Method Post -Uri $NegotiateUri -Body '' -TimeoutSec 15
    if ($null -eq $NegotiateResponse -or [string]::IsNullOrWhiteSpace([string] $NegotiateResponse.connectionId)) {
        throw "[orchid-mobile-staging] preflight-failed: connectionId missing from $NegotiateUri"
    }

    Write-Output "[orchid-mobile-staging] preflight OK (connectionId=$($NegotiateResponse.connectionId))"
}

$BuildArgs = [System.Collections.Generic.List[string]]::new()
$BuildArgs.Add('build')
$BuildArgs.Add($ProjectPath)
$BuildArgs.Add('-f')
$BuildArgs.Add('net10.0-android')
$BuildArgs.Add('-c')
$BuildArgs.Add($Configuration)
$BuildArgs.Add('-p:DOTNET_ENVIRONMENT=Staging')
$BuildArgs.Add("-p:OrchidRelayUrl=$NormalizedRelayUrl")
$BuildArgs.Add('-p:EnforceCodeStyleInBuild=false')

if (-not $BuildOnly) {
    $BuildArgs.Add('-t:Run')
    if (-not [string]::IsNullOrWhiteSpace($Serial)) {
        $BuildArgs.Add("-p:AdbTarget=-s $Serial")
    }
}

Write-Output "[orchid-mobile-staging] running dotnet $($BuildArgs -join ' ')"
& $DotnetCommand @BuildArgs
if ($LASTEXITCODE -ne 0) {
    throw "[orchid-mobile-staging] build-or-run-failed: exit code $LASTEXITCODE"
}
