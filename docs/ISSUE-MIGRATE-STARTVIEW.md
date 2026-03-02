# ISSUE: Migrate or remove legacy `StartView`

Severity: Medium

Description:
- `StartView` is a legacy login flow that automatically loads a list of businesses and caused an infinite polling loop in earlier runs.
- Currently mitigated by commenting out the `Content = BuildStartContent(...)` line in `MainWindow.OnContentRendered`, but the legacy view remains in the codebase and may be reintroduced.

Acceptance criteria:
- Remove `StartView` from the repository or migrate its functionality into the single `LoginView` implementation.
- Ensure no codepath loads `StartView` by default.
- Add unit / integration tests to guarantee no regressions.

Files to inspect:
- `BMTECHRD.Pos.App/Views/StartView.xaml` and `.xaml.cs`
- `BMTECHRD.Pos.App/ViewModels/StartViewModel.cs`
- `BMTECHRD.Pos.App/Services/MainWindowNavigationCoordinator.cs`

Suggested steps:
1. Assess whether StartView provides unique functionality needed by specialized device modes.
2. If not required, remove the view and viewmodel and related references.
3. If required, refactor to a modernized `LoginView` adapter and add feature flags if necessary.
4. Add tests and update docs.

Author: GitHub Copilot
