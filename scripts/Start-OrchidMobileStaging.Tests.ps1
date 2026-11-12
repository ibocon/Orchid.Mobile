#requires -Modules @{ ModuleName = 'Pester'; ModuleVersion = '5.0' }

Describe 'Start-OrchidMobileStaging' {
    BeforeAll {
        $script:ScriptPath = Join-Path -Path $PSScriptRoot -ChildPath 'Start-OrchidMobileStaging.ps1'
    }

    It 'Serial 과 Staging RelayUrl 을 MSBuild 프로퍼티로 전달한다' {
        # Given
        $FakeRepo = Join-Path -Path $TestDrive -ChildPath 'mobile-repo'
        $SrcDir = Join-Path -Path $FakeRepo -ChildPath 'src'
        $null = New-Item -ItemType Directory -Path $SrcDir -Force
        Set-Content -LiteralPath (Join-Path -Path $SrcDir -ChildPath 'Orchid.Mobile.csproj') -Value '<Project />' -Encoding utf8NoBOM

        $ArgsCapturePath = Join-Path -Path $TestDrive -ChildPath 'mobile-dotnet-args.json'
        $FakeDotnet = Join-Path -Path $TestDrive -ChildPath 'fake-dotnet.ps1'
        @"
@(`$args) | ConvertTo-Json -Compress | Set-Content -LiteralPath '$ArgsCapturePath' -Encoding utf8NoBOM
exit 0
"@ | Set-Content -LiteralPath $FakeDotnet -Encoding utf8NoBOM

        # When
        & $script:ScriptPath `
            -RepositoryRoot $FakeRepo `
            -DotnetCommand $FakeDotnet `
            -Serial 'R5CT319N57B' `
            -SkipPreflight

        # Then
        $CapturedArgs = @(Get-Content -LiteralPath $ArgsCapturePath -Raw | ConvertFrom-Json)
        $CapturedArgs | Should -Contain '-t:Run'
        $CapturedArgs | Should -Contain '-p:OrchidRelayUrl=https://stag.botanicsoft.com/orchid'
        $CapturedArgs | Should -Contain '-p:AdbTarget=-s R5CT319N57B'
    }
}
