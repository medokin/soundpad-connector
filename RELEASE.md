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

Thanks to @itsameshaw for investigating large responses in [#15](https://github.com/medokin/soundpad-connector/pull/15). The implementation uses message boundaries rather than reading until EOF; that external PR was closed as superseded without merging it.

## Compatibility and remaining checks

Publication proceeded with isolated-test coverage under the owner's authorization. The earlier installation-absence conclusion was incorrect: registry association absence did not imply installation absence. The installed Steam executable was then 4.0.30; help-dialog inspection was not native API verification. The two existing opt-in integration tests replace a soundlist and are not a complete smoke matrix.

After publication, an owner-authorized disposable session verified Steam Soundpad 4.0.35 (remote control API 1.1.2) on Windows. Native pipe flags confirmed message mode. Read-only version, title, count, soundlist and category requests passed. Loading spaced and Unicode paths preserved the expected counts, URL sets and Unicode title prefixes. A 1,200-entry list returned 735,757 UTF-8 bytes with its count, exact URL set and title prefixes checked; 1,200 distinct indices were recorded. Fifteen sequential large-list/count/version requests and three disconnect/reconnect cycles passed. The starting four-entry soundlist was restored and its content signature compared. Native soundlist XML confirmed four tag attributes; color encoding, seek and category playback remain untested. These results cover this Steam version, not all distributions or versions. Only use a disposable Soundpad session when testing mutating commands.

- [#2](https://github.com/medokin/soundpad-connector/issues/2): closed as obsolete after Travis was retired and replaced by verified GitHub Actions; this does not claim to fix the upstream Travis defect.
- [#10](https://github.com/medokin/soundpad-connector/issues/10): closed after real spaced and Unicode soundlist loading passed on Steam 4.0.35.
- [#12](https://github.com/medokin/soundpad-connector/issues/12): direct soundlist reads passed on both Steam and standalone trial 4.0.35. The original Stream Deck integration is untested; closed as not reproducible in the connector, with reopening requested for a direct reproducer.
- [#13](https://github.com/medokin/soundpad-connector/issues/13): closed after the large native message-mode response passed on Steam 4.0.35. Other versions remain unconfirmed; byte-mode retains a single-read limitation.
- [#15](https://github.com/medokin/soundpad-connector/pull/15): closed as superseded by the reviewed message-boundary implementation; contributor credited.

Standalone trial 4.0.35 was tested with Steam Soundpad stopped, using the vendor-signed executable and dependencies extracted from the official installer without running an installation. The standalone and Steam executables had different SHA256 hashes and the same valid signing certificate. Read-only version, title, count, soundlist and category requests passed, including nine sequential requests per connection on two fresh connections. The four sounds and their tags were returned over a native message-mode pipe. A device-configuration warning appeared, but no device settings or drivers were changed. Audio playback, licensed standalone behavior and the original Stream Deck integration were not tested. UWP remains outside the verified matrix.

`GetVersion` reads the remote control API version; `GetSoundpadVersion` reads the product version. Text replies matching the reserved protocol status form (`R-` plus three digits, optionally followed by a colon) are reported as errors, not values. Caller `WaitAsync` cancellation does not cancel a command by itself: disconnect/dispose when abandoning it.

## Publishing setup

The **NuGet publishing** workflow (`.github/workflows/nuget-publish.yml`) supports the 1.5.0 release. A published, non-prerelease GitHub release for `v1.5.0` triggers publishing; other releases are ignored. Manual dispatch is a read-only dry run: it verifies/uploads the candidate but never requests NuGet credentials or publishes. Push/PR CI and the Release candidate workflow remain read-only. No workflow creates releases/tags or deploys documentation.

One-time setup in the owning NuGet account, [Trusted Publishing](https://www.nuget.org/account/trustedpublishing):

- Policy name: `soundpad-connector` (a descriptive label).
- Package owner: `medokin`; provider: GitHub Actions.
- Repository owner: `medokin`; repository: `soundpad-connector`.
- Workflow file: `nuget-publish.yml` (filename only); environment: `nuget`.
- Allow **Push only new package versions**, with exact package pattern `SoundpadConnector`. Leave unlist/relist disabled.

When creating or editing the policy in NuGet's form, enter the package pattern and move focus away with Tab before clicking the bottom **Create** button. During this setup, visible text that had not been committed to the form resulted in "At least one valid glob pattern or package must be specified." Commit all fields before submission, then verify the saved policy is **Active** with the exact identity and scope above. Reuse that policy for later versions when the repository, workflow filename and environment stay unchanged; do not create a new long-lived API key for each release.

The protected GitHub `nuget` environment requires approval from `medokin`, permits only tag `v1.5.0`, and disallows administrator bypass. Self-approval is allowed because this repository has one designated release approver, but approval is still an explicit manual step. Do not remove these protections to make a failed deployment pass. For later release versions, update and review the workflow, release validation, candidate checks and allowed environment tag together.

Authentication uses pinned [NuGet/login](https://github.com/NuGet/login) and [Trusted Publishing](https://learn.microsoft.com/en-us/nuget/nuget-org/trusted-publishing), not a stored long-lived API key. Only the publishing job has `id-token: write`. It downloads the immutable artifact from its own successful verification job, requires an exact package/checksum manifest, and obtains the short-lived NuGet key immediately before pushing the one package. No rebuild, wildcard push, duplicate skipping, or credentials in command-line arguments.

The release preflight rejects any tag other than `v1.5.0`, a project version other than stable `1.5.0`, a README still referencing the development version or missing the `dotnet add package SoundpadConnector --version 1.5.0` installation command, and a commit outside `origin/master` history. Source and guidance now describe stable `1.5.0`; publication still requires all verification and approval gates below. The safe dry run verifies the build path only; it does not prove NuGet authentication or an actual successful push. The owner created the scoped NuGet policy and its Active status was confirmed during setup in [#30](https://github.com/medokin/soundpad-connector/issues/30).

## Controlled publication

Publication requires a separate maintainer decision, even after publishing infrastructure is merged.

1. Review the candidate result for the exact intended commit, compatibility notes and real-Soundpad test decision. Record tested product versions/distributions or explicit acceptance of the unavailable checks. Complete the account/environment setup above.
2. Approve the version and release notes. Update source version to stable `1.5.0` and README installation guidance to describe that approved release. The banner URL is absolute and commit-pinned on an [allowed NuGet image domain](https://learn.microsoft.com/en-us/nuget/nuget-org/package-readme-on-nuget-org#allowed-domains-for-images), but URL compliance alone does not prove rendering. Review and merge the stable guidance/version change only with green CI. Preview the package README before publication; do not submit a NuGet upload merely to preview it.
3. Rebuild and inspect the candidate from the final approved commit. Verify its recorded SHA256 and package contents. Do not publish an earlier development-source artifact.
4. Only after explicit publication approval, create tag `v1.5.0` and publish a non-prerelease GitHub release for that same approved master commit. This triggers the publishing workflow. A draft does not publish. Do not reuse or move a tag to a different commit.
5. Inspect that run's verification result and exact package artifact, then approve the `nuget` environment deployment. After the push, confirm the version/content and indexing status on NuGet.org. NuGet authentication and push are only verified by that approved deployment, not by a dry run. Failed or duplicate-version pushes fail visibly; inspect the existing NuGet version before any retry.

Documentation deployment is a separate decision; local/CI documentation generation does not refresh GitHub Pages. README examples and lifecycle guarantees describe `1.5.0`, not older NuGet 1.3.1.

## Next release checklist

The 1.5.0 publication exposed a README verification gap: GitHub and DocFX rendered the HTML banner, but NuGet displayed the `<p>`/`<img>` markup as text. The downloaded package payload matched the verified release artifact, so signing or upload did not introduce this defect. The repository now uses Markdown image syntax with the same allowed absolute URL. A regression test and local rendered preview use Markdig with raw HTML disabled, matching NuGet Gallery's rendering boundary; this is not a NuGet upload or production preview. Verify the actual NuGet banner on the next publication. [NuGet README guidance](https://learn.microsoft.com/en-us/nuget/nuget-org/package-readme-on-nuget-org#preview-your-readme) requires a new package version to correct an already published README; do not overwrite 1.5.0 or move its tag for a cosmetic fix.

1. Check the current vendor remote-control documentation/help for API changes and compare both the latest GitHub release and published NuGet versions. Choose the next version explicitly; those histories have diverged before. Resolve or clearly retain the live-test limitations above.
2. Update the approved version as one reviewed change across `src/SoundpadConnector/SoundpadConnector.csproj`, README installation/build guidance, this document, `verify-release.ps1` (candidate and expected assembly version), `validate-nuget-release.ps1` (tag, stable version, README/development checks), and `src/SoundpadConnector.Tests/NuGetReleaseTests.cs` (accepted version and rejection fixtures). Do not silently move the fixed API baseline; justify any baseline change and its compatibility implications.
3. Update both `.github/workflows/release-candidate.yml` and `.github/workflows/nuget-publish.yml`: names, exact tags/versions/artifact paths, concurrency group, checksum filename/regex, and exact push target. Review the protected GitHub `nuget` environment's allowed tag policy for the new approved tag, retaining its reviewer/no-bypass protections. Recheck the active NuGet policy; its workflow/environment identity is independent of package version.
4. Run full verification, review Standards and Spec, and merge only the reviewed commit on green CI. On final master, rerun preflight/verification and inspect the package README, metadata and checksum. A manual publishing-workflow dispatch is a safe build dry run, not an authentication or push test.
5. With explicit publication approval, create the new tag and published non-prerelease GitHub release for that exact verified master commit. Never move an existing tag. Inspect the release run's successful verification and download its exact package/checksum before approving the protected deployment as the designated reviewer. Checksums belong to a particular build; a separately rebuilt local package is not a substitute for the run's artifact.
6. Confirm checksum validation, NuGet login, and push steps succeeded. If attaching GitHub release assets, use the same verified package/checksum without overwriting existing assets. Inspect remote state before retrying a failed or uncertain push; duplicate-version errors are not success.
7. A successful push means NuGet accepted the upload, not that install/restore is ready. Its package page can show validation/indexing pending for up to an hour. Confirm the version in the public package index, download/inspect the published README and library, and verify the rendered banner. NuGet repository signing can change the archive bytes, so compare package payload rather than assuming the public archive must have the upload's SHA256. Record outcomes in the release issue, leave master clean, and keep any documentation deployment separate.
