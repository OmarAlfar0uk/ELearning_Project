<!-- ## What
&lt;!-- What does this PR change? --&gt;

## Why
&lt;!-- Why is this change needed? --&gt;

## Type
- [ ] Bug fix
- [ ] Feature
- [ ] Refactor
- [ ] Infrastructure
- [ ] EF Core Migration

## Checklist
- [ ] Builds locally: `dotnet build`
- [ ] Dockerfile builds: `docker build ./ELearningProject`
- [ ] No secrets committed (checked with `git diff --cached --name-only | xargs grep -i "password\|secret\|token\|key" || true`)
- [ ] If migration added: it is backward-compatible or includes backup plan
- [ ] Correlation ID middleware handles new endpoints (if applicable) -->