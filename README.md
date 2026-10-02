## Divine-name profiles

Visible Tamil profiles:

- கர்த்தர்
- யாவே
- யெகோவா

Internal profile keys remain stable for backward compatibility.


### v0.1.6 preflight fix

The previous C# 5 checker falsely identified the `$` regex end-anchor in two valid
verbatim regex strings as C# string interpolation. v0.1.6 corrects that check.

Run:

```text
build\BUILD-AND-INSTALL-DIAGNOSTIC.cmd
```


### v0.1.6 compiler compatibility

Paratext 9.5 machines may expose the older .NET Framework C# compiler, which supports
C# 5 but not `/langversion:7.3`. v0.1.6 explicitly compiles the direct Windows build
in C# 5 mode.

Run:

```text
build\BUILD-AND-INSTALL-DIAGNOSTIC.cmd
```

The launcher first checks that the source remains compatible with C# 5.

### v0.1.6 build fix

If your computer has no local `netstandard.dll`, run:

```text
build\BUILD-WITH-AUTO-DEPENDENCY.cmd
```

It searches all known local locations first. If still missing, it downloads the official NETStandard.Library 2.0.3 reference package into `.build-deps` and continues.


### If the build previously reported that the Facades folder was missing

v0.1.6 now checks the normal Windows .NET Framework runtime Facades folders as well as the Developer Pack paths.

First run:

```text
build\CHECK-PREREQUISITES.cmd
```

Then run:

```text
build\BUILD-AND-INSTALL-DIAGNOSTIC.cmd
```


## Recommended Windows build/install

For Paratext 9.5.110.1, use:

```text
build\BUILD-AND-INSTALL-DIAGNOSTIC.cmd
```

Do **not** use a command window that disappears on error. The diagnostic launcher always pauses and writes `build-install.log`. If compilation fails, that log contains the exact compiler error.

The install phase requests Administrator permission only after compilation succeeds.

# Tamil Divine Name — Paratext 9.5 Plugin

Target: **Paratext 9.5.110.1** on Windows, Plugin API 2.0.100 / .NET Framework 4.8.

Purpose: keep one authoritative Tamil Scripture project in Paratext while managing three editorially approved render profiles for YHWH:

- கர்த்தர்
- யாவே
- யெகோவா

The plugin is deliberately **non-destructive** in v0.1.6. It reads Scripture, builds a registry, runs Tamil boundary-sandhi QA, previews profiles, and exports publication USFM. It does **not** automatically rewrite the live Paratext project.

## Why boundary-aware rendering is required

The selectable unit is not merely the divine-name word. Tamil can change at both boundaries:

`previous word | left sandhi | divine-name morphology | right sandhi | next word`

Example conceptually:

- கர்த்தர் profile: `நம்மைக் கர்த்தர் ...`
- யெகோவா profile: `நம்மை யெகோவா ...`

The preceding word changed because the initial consonant of the selected divine-name form changed.

## v0.1 features

- Embedded Paratext panel under Scripture Text > Tools.
- Scan current chapter or the whole project.
- Detect `\nd ...\nd*` divine-name spans.
- Optional lexical fallback for Tamil divine-name forms when `\nd` is absent.
- Registry stored outside Scripture under `%LOCALAPPDATA%\TamilDivineName\`.
- Three profile columns: Karthar / Yahweh / Jehovah.
- Grammar tag per occurrence (`NOM`, `ACC`, `DAT`, `GEN`, `INS`, `LOC`, `VOC`, `COMPOUND`, `OTHER`).
- Left-boundary and right-boundary sandhi fields.
- Deterministic suggestion engine for high-confidence boundary cases.
- Human approval required before a profile can be exported.
- Context fingerprint invalidates stale approvals after Scripture context changes.
- Whole-Bible publication USFM export.
- Integrity rule: only registered divine-name zones may differ.

## Build

On the Paratext Windows computer:

1. Confirm Paratext is installed in `C:\Program Files\Paratext 9`.
2. Open PowerShell as Administrator if the plugin folder is protected.
3. Run:

```powershell
Set-ExecutionPolicy -Scope Process Bypass
.\build\build-paratext95.ps1
.\build\install-paratext95.ps1
```

4. Restart Paratext.
5. Open a Scripture project.
6. Use the plugin from the Scripture Text tools menu.

The build script compiles directly against the Paratext 9.5 assemblies installed on the computer:

- `PluginInterfaces.dll`
- `CorePluginInterfaces.dll`
- `EmbeddedUiPluginInterfaces.dll`

It also locates the .NET Framework 4.8 `netstandard.dll` facade to avoid the common `IPluginObject` / `netstandard` compiler error.


## Alternative build: GitHub Actions

The repository also contains `src/TamilDivineName.csproj`, which references the public Paratext Plugin API 2.0.100 NuGet packages. Push this package to GitHub and run **Build Paratext 9.5 Plugin**. The workflow produces:

- `TamilDivineName.ptxplg`
- `TamilDivineName-Paratext95.zip`

This avoids committing Paratext installation DLLs to GitHub.

## Important editorial rule

The algorithm may **suggest** Tamil sandhi. It never turns an unapproved suggestion into publication Scripture.

Precedence:

1. occurrence-specific approved override
2. project house-style exception
3. approved morphology table
4. Tamil boundary rule suggestion
5. manual review

See `ALGORITHM.md`.


## Git repository hygiene

A project-specific `.gitignore` is included. It excludes Visual Studio/MSBuild outputs, generated `.ptxplg` binaries, copied Paratext interface DLLs, local registries, publication exports, temporary files, and generated Scripture files. Checked-in sample USFM and sample configuration files remain allowed.
