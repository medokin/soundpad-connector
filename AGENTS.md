# Repository agent instructions

## Layout and tooling

- `src/SoundpadConnector`: the public library, targeting .NET Standard 2.0. Preserve existing public APIs unless a breaking change is explicitly approved.
- `src/SoundpadConnector.Tests`: safe parser, transport, connection, and release-preflight tests.
- `src/SoundpadConnector.IntegrationTests`: opt-in tests that can launch Soundpad and replace its soundlist.
- `examples/Examples`: read-only console example and bounded soundlist polling helper.
- `docfx`: documentation sources and assets. Generated packages/docs/baselines belong in ignored `artifacts`, not source control.
- Use Windows and PowerShell 7 for repository verification. Install the SDK selected by `global.json`; use the repository NuGet sources and pinned local tools.

## Implementation and verification

- Keep changes small and directly tied to the issue. Match existing C# conventions, use English, and avoid speculative abstractions or comments for obvious logic.
- Inspect the working tree first and preserve unrelated user changes. Prefer `rg` for search and non-interactive Git commands.
- Add focused regression tests for behavior changes. Prefer isolated real behavior over mocks; use unique pipes/temporary fixtures, bounded waits, and cleanup restricted to owned resources.
- Run `./build.ps1` for Release builds, safe tests, dependency audits, and packing. Run `./build-docs.ps1` for documentation verification. Run `./verify-release.ps1` for the full release candidate checks.
- Investigate failures before claiming success. Report unrelated failures with evidence instead of silently expanding scope.
- Never run opt-in integration tests or mutating Soundpad commands without explicit authorization for a disposable session. `build.ps1` disables live tests even when the environment opts in.
- Distinguish isolated Windows test coverage from real Soundpad verification. Keep framing, path-loading, distribution, and metadata limitations accurate in README/release notes.

## Git and review

- The default branch is `master`. Use `codex/` branches, conventional lowercase commit descriptions, and scoped issues/PRs.
- Review each implementation against both repository standards and its issue/spec before merging. Merge only the reviewed commit with green CI; never bypass checks to finish a task.
- Update README only when the change makes it factually wrong. Avoid adding attribution to source, tests, configuration, or repository documentation; keep any required disclosure in change metadata.

## Releases and documentation

- Read `RELEASE.md` before release work. Its checklist is the source of truth for version updates, fixed API baseline, package inspection, account setup, and remaining live-test gaps.
- A request to prepare infrastructure or a draft is not publication approval. Obtain explicit authorization before creating a release/tag or pushing a package. Documentation deployment is a separate decision.
- The publishing workflow is intentionally pinned to one approved version. Update its version gates, candidate checks, preflight/tests, stable README guidance, and the protected environment's allowed tag together for the next release.
- Keep NuGet Trusted Publishing package-scoped, with the designated reviewer and no administrator bypass. Only the publishing job may request OIDC credentials; manual dispatch must remain non-publishing.
- Publish the exact verified artifact from the release run after checksum inspection and protected approval. Do not rebuild a substitute, move an existing tag, skip duplicates, expose credentials, or retry an uncertain push without inspecting remote state.
- A successful push does not prove install availability. Confirm NuGet validation/indexing and downloaded package contents before reporting the release fully available.
