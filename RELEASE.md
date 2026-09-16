# 1.5.0 release

Release version: `1.5.0`. The owner authorized publication on 2026-09-16; source is stable-versioned. Verification and publication results are recorded in [#32](https://github.com/medokin/soundpad-connector/issues/32). Additive public APIs make this a minor release relative to GitHub v1.4.0 rather than the earlier 1.4.1 patch proposal. The library keeps `netstandard2.0` and existing public methods. NuGet's previous published version was 1.3.1; this release also delivers the V4 APIs already present in GitHub v1.4.0.

## Candidate verification

On Windows with the repository SDK, from a Git checkout containing the v1.4.0 commit:

```powershell
./verify-release.ps1
```

This builds both solutions with candidate version 1.5.0, runs safe tests, generates documentation, audits transitive dependencies, repeats isolated transport/connection tests five times, and compares the public API with fixed v1.4.0 commit `fbc5b8c32a2519370b1688abbdf5321d5d810162`. Pinned APICompat checks backwards compatibility from baseline to candidate, including parameter names and attributes. Strict bidirectional equality is no longer appropriate because the candidate adds APIs. Only CP0003 (assembly version equality) is excluded for the intentional version change.

Package checks confirm MIT/readme/version metadata, the single `netstandard2.0` library, matching packaged/built DLL bytes and the included README. The script writes the candidate and SHA256 checksum to `artifacts`; it neither publishes nor changes the source version. Generated baseline directories are retained under `artifacts` for inspection. The **Release candidate** workflow runs these checks on PRs or manual dispatch and uploads these files plus this document. It has read-only repository permissions, no publishing credentials, and no tag trigger.

Regular CI separately verifies the default package and documentation. Known NuGet vulnerabilities fail restore. Safe tests do not contact Soundpad.

## Release notes

- Preserve complete message-mode pipe responses, including large XML and split UTF-8 characters, without waiting for server EOF or consuming a following message.
- Correct successful numeric results and XML error handling.
- Correct the title-text wire command and report text-response protocol errors instead of treating them as successful values.
- Add absolute `Seek`, category-relative `PlaySoundFromCategory`, and `GetSoundpadVersion`. Preserve existing `GetVersion` remote control version semantics.
- Preserve optional soundlist `color` and `tag` attributes as raw strings without interpreting color encoding.
- Quote absolute soundlist paths when launching the registered executable.
- Report failed connections, make automatic retry opt-in, and cancel/recreate connection and polling work safely on disconnect/dispose.
- Update build tooling, safe test coverage, dependency auditing, CI, package metadata and reproducible library-only API documentation.
- Replace fire-and-forget examples and unbounded count polling with awaited, bounded examples.

Thanks to @itsameshaw for investigating large responses in [#15](https://github.com/medokin/soundpad-connector/pull/15). The implementation uses message boundaries rather than reading until EOF; that external PR has not been merged.

## Compatibility and remaining checks

The maintenance environment has a signed Steam Soundpad 4.0.30 executable at `C:/Program Files (x86)/Steam/steamapps/common/Soundpad/Soundpad.exe`. The earlier installation-absence conclusion was incorrect: registry association absence did not imply installation absence. The help dialog confirms seek and category playback, but no real pipe call, large soundlist, spaced-path load, sequential/reconnect call, or V4 command was smoke-tested. Metadata parsing tests use synthetic XML, not captured native responses; color encoding and category-response metadata remain unconfirmed. The two existing opt-in integration tests replace a soundlist and are not a complete smoke matrix. This release proceeds with the documented isolated-test coverage under the owner's publication authorization; it does not claim real-instance verification. Only use a disposable Soundpad session when testing mutating commands.

- [#2](https://github.com/medokin/soundpad-connector/issues/2): the obsolete Travis pipeline was retired and replaced by verified GitHub Actions. This supersedes the old CI setup; it does not claim to fix the upstream Travis defect. The historical issue remains open.
- [#10](https://github.com/medokin/soundpad-connector/issues/10): quoted path encoding is tested at the launch boundary; real soundlist loading remains unconfirmed.
- [#12](https://github.com/medokin/soundpad-connector/issues/12): no Steam-versus-standalone compatibility conclusion; remains open.
- [#13](https://github.com/medokin/soundpad-connector/issues/13): large message-mode responses pass isolated tests; actual Soundpad framing across versions remains unconfirmed. Byte-mode retains a single-read limitation; remains open.
- [#15](https://github.com/medokin/soundpad-connector/pull/15): contributor credited; remains open pending real-instance confirmation.

UWP and demo/trial editions are outside the verified matrix. `GetVersion` reads the remote control API version; `GetSoundpadVersion` reads the product version. Text replies matching the reserved protocol status form (`R-` plus three digits, optionally followed by a colon) are reported as errors, not values. Caller `WaitAsync` cancellation does not cancel a command by itself: disconnect/dispose when abandoning it.

## Publishing setup

The **NuGet publishing** workflow (`.github/workflows/nuget-publish.yml`) supports the 1.5.0 release. A published, non-prerelease GitHub release for `v1.5.0` triggers publishing; other releases are ignored. Manual dispatch is a read-only dry run: it verifies/uploads the candidate but never requests NuGet credentials or publishes. Push/PR CI and the Release candidate workflow remain read-only. No workflow creates releases/tags or deploys documentation.

One-time setup in the owning NuGet account, [Trusted Publishing](https://www.nuget.org/account/trustedpublishing):

- Policy name: `soundpad-connector` (a descriptive label).
- Package owner: `medokin`; provider: GitHub Actions.
- Repository owner: `medokin`; repository: `soundpad-connector`.
- Workflow file: `nuget-publish.yml` (filename only); environment: `nuget`.
- Allow **Push only new package versions**, with exact package pattern `SoundpadConnector`. Leave unlist/relist disabled.

The protected GitHub `nuget` environment requires approval from `medokin`, permits only tag `v1.5.0`, and disallows administrator bypass. Self-approval is allowed because this repository has one designated release approver, but approval is still an explicit manual step. Do not remove these protections to make a failed deployment pass. For later release versions, update and review the workflow, release validation, candidate checks and allowed environment tag together.

Authentication uses pinned [NuGet/login](https://github.com/NuGet/login) and [Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing), not a stored long-lived API key. Only the publishing job has `id-token: write`. It downloads the immutable artifact from its own successful verification job, requires an exact package/checksum manifest, and obtains the short-lived NuGet key immediately before pushing the one package. No rebuild, wildcard push, duplicate skipping, or credentials in command-line arguments.

The release preflight rejects any tag other than `v1.5.0`, a project version other than stable `1.5.0`, a README still referencing the development version or missing the `dotnet add package SoundpadConnector --version 1.5.0` installation command, and a commit outside `origin/master` history. Source and guidance now describe stable `1.5.0`; publication still requires all verification and approval gates below. The safe dry run verifies the build path only; it does not prove NuGet authentication or an actual successful push. The owner created the scoped NuGet policy and its Active status was confirmed during setup in [#30](https://github.com/medokin/soundpad-connector/issues/30).

## Controlled publication

Publication requires a separate maintainer decision, even after publishing infrastructure is merged.

1. Review the candidate result for the exact intended commit, compatibility notes and real-Soundpad test decision. Record tested product versions/distributions or explicit acceptance of the unavailable checks. Complete the account/environment setup above.
2. Approve the version and release notes. Update source version to stable `1.5.0` and README installation guidance to describe that approved release. The banner now uses an absolute, commit-pinned URL from an [allowed NuGet image domain](https://learn.microsoft.com/en-us/nuget/nuget-org/package-readme-on-nuget-org#allowed-domains-for-images). Review and merge the stable guidance/version change only with green CI. Preview the package README before publication; do not submit a NuGet upload merely to preview it.
3. Rebuild and inspect the candidate from the final approved commit. Verify its recorded SHA256 and package contents. Do not publish an earlier development-source artifact.
4. Only after explicit publication approval, create tag `v1.5.0` and publish a non-prerelease GitHub release for that same approved master commit. This triggers the publishing workflow. A draft does not publish. Do not reuse or move a tag to a different commit.
5. Inspect that run's verification result and exact package artifact, then approve the `nuget` environment deployment. After the push, confirm the version/content and indexing status on NuGet.org. NuGet authentication and push are only verified by that approved deployment, not by a dry run. Failed or duplicate-version pushes fail visibly; inspect the existing NuGet version before any retry.

Documentation deployment is a separate decision; local/CI documentation generation does not refresh GitHub Pages. README examples and lifecycle guarantees describe `1.5.0`, not older NuGet 1.3.1.
