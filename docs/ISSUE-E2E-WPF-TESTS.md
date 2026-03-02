# ISSUE: Add E2E WPF → API tests

Severity: Medium

Description:
- No automated E2E tests currently exercise the WPF UI against a running API instance. Manual testing validated login, but automated regression tests are needed.

Acceptance criteria:
- Implement automated smoke/e2e tests that run the API and a headless/automation-capable WPF harness or use UI automation tooling.
- Tests should cover: API health, login flow, basic navigation, and SignalR connect flow (mocked if needed).

Suggested tools:
- WinAppDriver or Appium for Windows UI automation
- Selenium-based approach with a test harness
- Custom test harness that exercises API endpoints and validates expected session state

Author: GitHub Copilot
