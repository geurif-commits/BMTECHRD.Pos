# ISSUE: Centralize API base URL configuration

Severity: High

Description:
- API base URL is currently read from `DeviceConfig` in some places, but several files contained hardcoded defaults (e.g., `https://localhost:5001/`).
- This caused environment mismatch and runtime failures.

Acceptance criteria:
- Introduce a single configuration source for `ApiBaseUrl` (e.g., `appsettings.json` or `DeviceConfig`) and ensure all components read from it.
- Add validation and normalization logic.
- Document configuration in README.

Suggested steps:
1. Add `ConfigurationService` or extend `LocalDeviceConfigService` to expose `ApiBaseUrl` reliably.
2. Replace hardcoded defaults with `NormalizeBaseUrl(config.ApiBaseUrl) ?? default` only in one location.
3. Add integration test that ensures baseUrl is used consistently.

Author: GitHub Copilot
