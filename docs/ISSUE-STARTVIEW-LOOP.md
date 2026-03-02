# ISSUE: Infinite GET /api/business/public loop in WPF

Severity: High

Description:
- The WPF application triggered an infinite loop repeatedly calling `GET /api/business/public` when starting.
- This caused UI freeze and excessive load on the local API.

Root cause:
- Legacy `StartView` was being used by `MainWindow.OnContentRendered`, which executed `StartView.Initialize(...)` and triggered repeated calls to `ApiClient.GetBusinessesPublicAsync()`.
- Default API base URL was set to `https://localhost:5001/` in parts of the code while the API was running on `http://localhost:5000/`.
- `MainWindow` overwrote the content previously set by `App.GoToLoginAsync()` causing the legacy view to load after navigation to `LoginView`.

Reproduction steps:
1. Run API: `dotnet run --launch-profile http` (http://localhost:5000)
2. Run WPF: `dotnet run` in `BMTECHRD.Pos.App`
3. Observe hundreds of `Start processing GET /api/business/public` logs and UI freeze.

Fix applied (temporary):
- Changed default base URL in `App.xaml.cs` and `MainWindow.xaml.cs` to `http://localhost:5000/`.
- Commented out the `Content = _navigationCoordinator.BuildStartContent(...)` line in `MainWindow.OnContentRendered` to avoid overriding `LoginView`.
- Reverted a prior experimental change to `MainWindowNavigationCoordinator`.

Suggested next steps (follow-ups):
- Remove or migrate legacy `StartView` entirely and consolidate to a single `LoginView` implementation.
- Centralize base URL configuration (single source) and document it (appsettings or device config file).
- Add a guard or debounce in `StartView` if it remains to prevent tight polling loops.
- Add integration test that starts API + WPF harness to detect regressions.

Files changed:
- `BMTECHRD.Pos.App/App.xaml.cs` - baseUrl default changed
- `BMTECHRD.Pos.App/MainWindow.xaml.cs` - baseUrl default changed, content overwrite commented
- `BMTECHRD.Pos.App/Services/MainWindowNavigationCoordinator.cs` - reverted experimental edits

Author: GitHub Copilot (assistant)
Date: (auto)
