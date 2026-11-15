#requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0' }

Describe 'Deploy-S22StagingOrchidMobile' {
    BeforeAll {
        $script:ScriptPath = Join-Path -Path $PSScriptRoot -ChildPath 'Deploy-S22StagingOrchidMobile.ps1'
    }

    It 'adb devices -l 에서 model:SM_S908N 기기를 자동 감지해 Start-OrchidMobileStaging 에 위임하고 pidof 로 기동을 확인한다' {
        # Given
        $StateDir = Join-Path -Path $TestDrive -ChildPath 's22-auto'
        $null = New-Item -ItemType Directory -Path $StateDir -Force
        $FakeAdb = Join-Path -Path $StateDir -ChildPath 'fake-adb.ps1'
        $FakeStaging = Join-Path -Path $StateDir -ChildPath 'fake-staging.ps1'
        $StagingCapturePath = Join-Path -Path $StateDir -ChildPath 'staging-capture.json'

        @"
`$joined = `$args -join ' '
if (`$joined -eq 'devices -l') {
    Write-Output 'List of devices attached'
    Write-Output '10.79.96.2:5555        device product:b0qksx model:SM_S908N device:b0q transport_id:1'
    exit 0
}
if (`$joined -eq '-s 10.79.96.2:5555 shell pidof com.botanicsoft.orchid') {
    Write-Output '24680'
    exit 0
}
exit 0
"@ | Set-Content -LiteralPath $FakeAdb -Encoding utf8NoBOM

        @"
param(
    [string] `$Serial,
    [string] `$Configuration,
    [string] `$RelayUrl,
    [switch] `$SkipPreflight
)
@{
    Serial = `$Serial
    Configuration = `$Configuration
    RelayUrl = `$RelayUrl
    SkipPreflight = [bool] `$SkipPreflight
} | ConvertTo-Json -Compress | Set-Content -LiteralPath '$StagingCapturePath' -Encoding utf8NoBOM
exit 0
"@ | Set-Content -LiteralPath $FakeStaging -Encoding utf8NoBOM

        # When
        & $script:ScriptPath `
            -AdbCommand $FakeAdb `
            -StagingScriptPath $FakeStaging `
            -PollIntervalSeconds 0

        # Then
        $Captured = Get-Content -LiteralPath $StagingCapturePath -Raw | ConvertFrom-Json
        $Captured.Serial | Should -Be '10.79.96.2:5555'
        $Captured.Configuration | Should -Be 'Debug'
        $Captured.RelayUrl | Should -Be 'https://stag.botanicsoft.com/orchid'
    }

    It 'adb devices 에 SM-S908N 이 없을 때 기본 주소(10.79.96.2:5555)로 adb connect 폴백을 수행하고 pidof 미응답 시 monkey 런처를 실행한다' {
        # Given
        $StateDir = Join-Path -Path $TestDrive -ChildPath 's22-fallback'
        $null = New-Item -ItemType Directory -Path $StateDir -Force
        $FakeAdb = Join-Path -Path $StateDir -ChildPath 'fake-adb.ps1'
        $FakeStaging = Join-Path -Path $StateDir -ChildPath 'fake-staging.ps1'
        $ConnectedMarker = Join-Path -Path $StateDir -ChildPath 'connected.flag'
        $MonkeyMarker = Join-Path -Path $StateDir -ChildPath 'monkey.flag'

        @"
`$joined = `$args -join ' '
if (`$joined -eq 'devices -l') {
    Write-Output 'List of devices attached'
    if (Test-Path -LiteralPath '$ConnectedMarker') {
        Write-Output '10.79.96.2:5555        device product:b0qksx model:SM_S908N device:b0q transport_id:1'
    }
    exit 0
}
if (`$joined -eq 'connect 10.79.96.2:5555') {
    Set-Content -LiteralPath '$ConnectedMarker' -Value '1' -Encoding utf8NoBOM
    Write-Output 'connected to 10.79.96.2:5555'
    exit 0
}
if (`$joined -eq '-s 10.79.96.2:5555 shell pidof com.botanicsoft.orchid') {
    if (Test-Path -LiteralPath '$MonkeyMarker') {
        Write-Output '13579'
        exit 0
    }
    exit 1
}
if (`$joined -eq '-s 10.79.96.2:5555 shell monkey -p com.botanicsoft.orchid -c android.intent.category.LAUNCHER 1') {
    Set-Content -LiteralPath '$MonkeyMarker' -Value '1' -Encoding utf8NoBOM
    Write-Output 'Events injected: 1'
    exit 0
}
exit 0
"@ | Set-Content -LiteralPath $FakeAdb -Encoding utf8NoBOM

        @"
param(
    [string] `$Serial,
    [string] `$Configuration,
    [string] `$RelayUrl,
    [switch] `$SkipPreflight
)
exit 0
"@ | Set-Content -LiteralPath $FakeStaging -Encoding utf8NoBOM

        # When
        & $script:ScriptPath `
            -AdbCommand $FakeAdb `
            -StagingScriptPath $FakeStaging `
            -PollIntervalSeconds 0 `
            -SkipPreflight

        # Then
        Test-Path -LiteralPath $ConnectedMarker | Should -BeTrue
        Test-Path -LiteralPath $MonkeyMarker | Should -BeTrue
    }
}
