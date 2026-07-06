# Branch Strategy

- **`main`** — stable, production-ready code
- **`feature/<name>`** — short-lived branches for new work (e.g. `feature/dashboard-shell`)

## Workflow

1. Branch from `main`: `git checkout -b feature/my-feature`
2. Commit and push your changes on the feature branch
3. Open a PR and **squash-merge** into `main` when ready

## Example

```bash
git checkout main
git pull
git checkout -b feature/settings-page
# ... work, commit ...
git push -u origin feature/settings-page
```
