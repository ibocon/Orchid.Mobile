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
