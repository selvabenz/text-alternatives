# Changelog

## 0.1.6

- Standardize the Tamil display spelling to `யெகோவா`.
- Replace previous Tamil `யெஹோவா` examples and labels.
- Keep internal profile identifiers unchanged for registry/config compatibility.
- Keep the three visible Tamil profiles as `கர்த்தர்`, `யாவே`, and `யெகோவா`.
- No change to the rendering algorithm or Paratext 9.5 build compatibility.

## 0.1.5

- Fix a false positive in the C# 5 source compatibility checker.
- Valid verbatim regex strings ending with the regex `$` anchor are no longer mistaken for C# interpolated strings.
- Keep the direct build at C# 5 for the compiler available on the Paratext 9.5.110.1 computer.
- No changes to the Tamil algorithm, registry model, or Scripture rendering logic.

## 0.1.4

- Fix compilation on the legacy .NET Framework C# compiler present on the Paratext 9.5 computer.
- Change direct compiler language target from unsupported C# 7.3 to C# 5.
- Remove unsupported `csc /version` invocation; report the compiler executable file version instead.
- Remove `/utf8output` from the legacy direct compiler arguments.
- Add a C# 5 source-compatibility preflight check.
- Align the optional SDK-style project file to C# 5.
- No changes to the Tamil rendering algorithm or Scripture data.

## 0.1.3

- Search Paratext, NuGet cache, and installed .NET SDK packs for `netstandard.dll`.
- Add one-time bootstrap of official `NETStandard.Library 2.0.3` when no local reference exists.
- Store bootstrapped references only under `.build-deps`.
- Add `BUILD-WITH-AUTO-DEPENDENCY.cmd`.
- Keep live Paratext Scripture untouched until a plugin build succeeds.

## 0.1.2

- Fix `netstandard.dll` detection on machines without the .NET Framework Developer Pack.
- Search Windows .NET Framework runtime `Facades` folders.
- Add targeted fallback search for `netstandard.dll`.
- Add `CHECK-PREREQUISITES.cmd`.
- Keep Developer Pack installation as the final fallback.

## 0.1.1

- Fix build-and-install window closing immediately on errors.
- Add persistent `build-install.log`.
- Add diagnostic build and build-only launchers.
- Auto-detect Paratext 9 plugin interface DLLs.
- Report detected Paratext and interface DLL versions.
- Improve .NET Framework 4.8 facade detection.
- Compile through a response file to avoid Windows path/quoting issues.
- Add additional .NET Standard facade references.
- Request Administrator elevation only for the install step.
- Add installation verification and troubleshooting documentation.

## 0.1.0

- Initial Paratext 9.5.110.1-targeted implementation.
- Boundary-aware YHWH registry.
- Three profiles: கர்த்தர் / யாவே / யெகோவா.
- Current chapter and whole-Bible scanning.
- Context-staleness protection.
- Tamil sandhi suggestion engine.
- Human approval gating.
- Whole-Bible publication USFM export.
- Direct Windows build script against Paratext 9.5 interface assemblies.
