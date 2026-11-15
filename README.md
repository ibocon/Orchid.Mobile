# Orchid.Mobile (Universal Clipboard Bridge)

Android 및 iOS 모바일 환경을 지원하는 .NET MAUI Blazor Hybrid E2EE 클립보드 브리지 클라이언트입니다.

## 연결된 Android 기기/에뮬레이터에 Staging 환경 빌드 및 실행

Hanul Staging 백엔드(`https://stag.botanicsoft.com/orchid`)를 대상으로 연결된 Android 기기에 앱을 빌드·설치·실행하려면 아래 PowerShell 스크립트를 사용합니다.

### 1. 단일 연결 기기에 즉시 실행

```pwsh
pwsh -NoProfile -ExecutionPolicy Bypass -File D:\Source\BotanicSoft\Orchid.Mobile\scripts\Start-OrchidMobileStaging.ps1
```

### 2. 특정 기기 시리얼(`adb devices`) 지정 실행

```pwsh
adb devices
pwsh -NoProfile -ExecutionPolicy Bypass -File D:\Source\BotanicSoft\Orchid.Mobile\scripts\Start-OrchidMobileStaging.ps1 -Serial <device-id>
```

### 3. 기기 배포 없이 Staging Android 빌드만 검증

```pwsh
pwsh -NoProfile -ExecutionPolicy Bypass -File D:\Source\BotanicSoft\Orchid.Mobile\scripts\Start-OrchidMobileStaging.ps1 -BuildOnly
```

### 4. Galaxy S22 Ultra(`SM-S908N`) 고정 Staging 배포 및 실행

`Peony.Mobile`의 `Deploy-S22StagingPeonyMobile.ps1`과 동일하게 `adb devices -l`에서 `SM-S908N` 단말을 자동 감지(또는 기본 무선 ADB 주소 `10.79.96.2:5555`로 연결)하여 Staging 빌드를 배포하고 `com.botanicsoft.orchid` 프로세스 기동까지 검증합니다.

```pwsh
pwsh -NoProfile -ExecutionPolicy Bypass -File D:\Source\BotanicSoft\Orchid.Mobile\scripts\Deploy-S22StagingOrchidMobile.ps1
```
