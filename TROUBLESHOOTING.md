# Troubleshooting build/install on Paratext 9.5.110.1

## Use this launcher

Double-click:

`build\BUILD-AND-INSTALL-DIAGNOSTIC.cmd`

Unlike the earlier launcher, this window does not close after an error.

It writes:

`build-install.log`

in the project root.

If the build fails, send that log file back for diagnosis.

## What the diagnostic build checks

- Paratext 9 installation folder
- `PluginInterfaces.dll`
- `CorePluginInterfaces.dll`
- `EmbeddedUiPluginInterfaces.dll`
- installed Paratext file version
- .NET Framework C# compiler
- .NET Framework 4.8 reference facades, including `netstandard.dll`
- actual C# compiler output
- whether `TamilDivineName.ptxplg` was really created

## Administrator permission

Building does not normally require Administrator permission.

Installing into:

`C:\Program Files\Paratext 9\plugins\TamilDivineName`

does.

The new installer requests Windows elevation only after the build succeeds.

## If .NET Framework 4.8 Developer Pack is missing

The script will explicitly report this rather than disappearing.

Install the **.NET Framework 4.8 Developer Pack** (not merely the runtime), and run the diagnostic launcher again.

## Expected successful output

After build:

`dist\TamilDivineName.ptxplg`

After install:

`C:\Program Files\Paratext 9\plugins\TamilDivineName\TamilDivineName.ptxplg`

Then restart Paratext.


## v0.1.2: netstandard.dll fallback

v0.1.1 only searched the .NET Framework Developer Pack reference-assembly folders.
On many Windows installations, `netstandard.dll` is already available in the runtime facades:

```text
C:\Windows\Microsoft.NET\Framework64\v4.0.30319\Facades\netstandard.dll
C:\Windows\Microsoft.NET\Framework\v4.0.30319\Facades\netstandard.dll
```

v0.1.2 checks these locations automatically before requiring the Developer Pack.

You can run:

```text
build\CHECK-PREREQUISITES.cmd
```

to see exactly which prerequisite is present or missing.


## v0.1.3 — no local netstandard.dll

The builder now searches Framework facades, the Paratext folder, the NuGet cache, installed .NET SDK packs, and `.build-deps`. If none contains `netstandard.dll`, it tries a one-time download of the official `NETStandard.Library 2.0.3` reference package into `.build-deps`. This is a build reference only and is not installed into Windows or Paratext.


## v0.1.4 — CS1617 `/langversion:7.3`

If the compiler reports:

```text
error CS1617: Invalid option '7.3' for /langversion;
must be ISO-1, ISO-2, 3, 4, 5 or Default
```

the machine is using the legacy .NET Framework compiler shipped with Windows.
That is acceptable for this plugin.

v0.1.4 compiles the plugin in **C# 5 mode** and includes a source check to ensure
we do not accidentally use syntax that compiler cannot parse.

No Visual Studio installation or newer C# compiler is required for this specific error.


## v0.1.5 — false "interpolated string" warning

v0.1.4's preflight checker searched for the raw substring `$"`.

That incorrectly flagged valid C# 5 verbatim regular expressions such as:

```csharp
@"...\s*$"
```

because the regex end-of-string anchor `$` appears immediately before the closing quote.

v0.1.5 fixes the preflight detection. Those two source lines were valid C# 5 and did not
need to be rewritten.
