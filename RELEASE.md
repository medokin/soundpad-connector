# 1.5.0 release preparation

Status: provisional candidate, not published or approved for publication. Source remains `1.5.0-dev`. Additive public APIs supersede the earlier 1.4.1 patch proposal with a 1.5.0 minor candidate relative to GitHub v1.4.0. The library keeps `netstandard2.0` and existing public methods. The latest published NuGet package observed during maintenance is 1.3.1; publishing this candidate would also deliver the V4 APIs already present in GitHub v1.4.0.

## Candidate verification

On Windows with the repository SDK, from a Git checkout containing the v1.4.0 commit:

```powershell
./verify-release.ps1
```

This builds both solutions with candidate version 1.5.0, runs safe tests, generates documentation, audits transitive dependencies, repeats isolated transport/connection tests five times, and compares the public API with fixed v1.4.0 commit `fbc5b8c32a2519370b1688abbdf5321d5d810162`. Pinned APICompat checks backwards compatibility from baseline to candidate, including parameter names and attributes. Strict bidirectional equality is no longer appropriate because the candidate adds APIs. Only CP0003 (assembly version equality) is excluded for the intentional version change.

Package checks confirm MIT/readme/version metadata, the single `netstandard2.0` library, matching packaged/built DLL bytes and the included README. The script writes the candidate and SHA256 checksum to `artifacts`; it neither publishes nor changes the source version. Generated baseline directories are retained under `artifacts` for inspection. The **Release candidate** workflow runs these checks on PRs or manual dispatch and uploads these files plus this document. It has read-only repository permissions, no publishing credentials, and no tag trigger.

Regular CI separately verifies the default development package and documentation. Known NuGet vulnerabilities fail restore. Safe tests do not contact Soundpad.

## Draft release notes

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

The maintenance environment has a signed Steam Soundpad 4.0.30 executable at `C:/Program Files (x86)/Steam/steamapps/common/Soundpad/Soundpad.exe`. The earlier installation-absence conclusion was incorrect: registry association absence did not imply installation absence. The help dialog confirms seek and category playback, but no real pipe call, large soundlist, spaced-path load, sequential/reconnect call, or V4 command was smoke-tested. Metadata parsing tests use synthetic XML, not captured native responses; color encoding and category-response metadata remain unconfirmed. The two existing opt-in integration tests replace a soundlist and are not a complete smoke matrix. Before publication, explicitly decide whether to obtain that coverage or accept these limits. Only use a disposable Soundpad session when testing mutating commands.

- [#2](https://github.com/medokin/soundpad-connector/issues/2): the obsolete Travis pipeline was retired and replaced by verified GitHub Actions. This supersedes the old CI setup; it does not claim to fix the upstream Travis defect. The historical issue remains open.
- [#10](https://github.com/medokin/soundpad-connector/issues/10): quoted path encoding is tested at the launch boundary; real soundlist loading remains unconfirmed.
- [#12](https://github.com/medokin/soundpad-connector/issues/12): no Steam-versus-standalone compatibility conclusion; remains open.
- [#13](https://github.com/medokin/soundpad-connector/issues/13): large message-mode responses pass isolated tests; actual Soundpad framing across versions remains unconfirmed. Byte-mode retains a single-read limitation; remains open.
- [#15](https://github.com/medokin/soundpad-connector/pull/15): contributor credited; remains open pending real-instance confirmation.

UWP and demo/trial editions are outside the verified matrix. `GetVersion` reads the remote control API version; `GetSoundpadVersion` reads the product version. Text replies matching the reserved protocol status form (`R-` plus three digits, optionally followed by a colon) are reported as errors, not values. Caller `WaitAsync` cancellation does not cancel a command by itself: disconnect/dispose when abandoning it.

## Controlled manual publication

Publication requires a separate maintainer decision. Nothing in the merged workflows publishes packages, creates releases or deploys documentation.

1. Review the candidate workflow result for the exact intended commit, compatibility notes and real-Soundpad test decision. Record tested product versions/distributions or an explicit acceptance of the unavailable checks.
2. Approve the version and release notes. Update the source version and README installation guidance to describe the approved stable release. The restored relative banner works on GitHub and in local docs, but NuGet.org does not render relative images: use an absolute image URL from an [allowed domain](https://learn.microsoft.com/en-us/nuget/nuget-org/package-readme-on-nuget-org#allowed-domains-for-images) and preview the package README before publishing. Review that change and merge only with green CI. Rebuild the candidate from that exact commit; do not publish an earlier development-source artifact.
3. Download/inspect the approved package and verify its recorded SHA256. Run the candidate checks on the final checkout if building locally. Use a NuGet credential scoped to this package; never commit it.
4. Only after explicit publication approval, run the command below from that final checkout. Confirm the published package version/content on nuget.org. Create the v1.5.0 tag and GitHub release for the same approved commit, with finalized notes. Do not reuse a tag for a different commit.

```powershell
dotnet nuget push artifacts/SoundpadConnector.1.5.0.nupkg --source https://api.nuget.org/v3/index.json --api-key $env:NUGET_API_KEY
```

Documentation deployment is a separate decision; local/CI documentation generation does not refresh GitHub Pages. Until publication, README examples and lifecycle guarantees describe current source, not the existing NuGet 1.3.1 package.
