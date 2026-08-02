# AGENTS.md

## Scope
- These instructions apply to the entire `WDS.MsSql.Clr.sln` workspace unless a more specific `AGENTS.md` in a child directory overrides them.
- Do not overwrite unrelated user work. This repository may contain active changes from API-contract migrations or deployment work.
- Do not edit generated `bin/` or `obj/` output, restored `packages/`, signed binaries, or hash artifacts directly.

## Solution Purpose
- This solution exposes WebDataSource operations to Microsoft SQL Server through a signed .NET Framework SQL CLR assembly.
- The CLR library communicates with the Dapi MSSQL-compatible HTTP API, translates XML request and response contracts, and exposes SQL functions and user-defined types (UDTs).
- SQL-visible types and functions are installed by `WDS.MsSql.Scripts/WdsClrFunctions.sql`; treat that script and the CLR public surface as one contract.

## Project Map
- `WDS.MsSql.Clr`: The `net48` signed SQL CLR assembly. It contains SQL functions, HTTP API clients, SQL UDT data contracts, XML and binary serialization, and supporting extensions.
- `WDS.MsSql.Clr.Tests`: NUnit tests for parsing, XML serialization, binary serialization, and matching CLR contracts against Dapi public contracts.
- `WDS.MsSql.Scripts`: SQL Server installation assets. `WdsClrFunctions.sql` installs the trusted assembly, UDTs, and SQL functions; `Install.bat` drives installation.
- `.github/workflows`: Release automation for building and publishing the signed assembly and related assets.

## Compatibility Boundaries
- Preserve SQL schema name `wds`, SQL function names and signatures, UDT names, assembly name, public UDT member names, and serialized XML shapes unless the task explicitly changes them.
- When a CLR UDT or SQL function surface changes, update `WDS.MsSql.Scripts/WdsClrFunctions.sql` in the same change.
- Keep the MSSQL API version, routes, HTTP methods, request bodies, response contracts, and asynchronous polling behavior aligned with the Dapi MSSQL API.
- Treat Dapi public data contracts as the source of truth for API-facing properties and enum values, while retaining SQL CLR-only state such as `Server`, `Error`, and `IsNull` where required.
- Preserve the symmetry and ordering of every `BinaryRead`/`BinaryWrite` pair. Add matching binary serialization whenever a persisted UDT property is added.

## Coding Conventions
- Target .NET Framework 4.8 and C# 9; do not introduce APIs unavailable to SQL Server's supported CLR runtime.
- Follow the existing global data-contract types, `SqlUserDefinedType`, `SqlFunction`, `INullable`, `IBinarySerialize`, `XmlSerializer`, and `DataContractSerializer<T>` patterns.
- Keep SQL entry points thin. Put HTTP request construction and polling in `ClrFunctions/Server`, contract behavior in `DataContracts`, and reusable serialization logic in `Serialization`.
- Preserve XML exclusions such as `[XmlIgnore]` on CLR-only properties.
- Do not commit signing keys or other secrets. The Debug build expects the private signing key through its existing pre-build path.

## Validation
- Build the smallest affected project first with `msbuild WDS.MsSql.Clr/WDS.MsSql.Clr.csproj /t:Build /p:Configuration=Debug`.
- For contract or behavior changes, also build `WDS.MsSql.Clr.Tests/WDS.MsSql.Clr.Tests.csproj` and run the applicable NUnit tests when a runner is available.
- Validate API-facing contract changes against the current Dapi Core assembly using `DataContractMatchTests`.
- Validate request-contract changes with XML serialization tests and UDT changes with binary round-trip tests.
- For changes to SQL-visible members, install the assembly in a SQL Server test instance and exercise the affected script or function; compilation alone does not validate SQL CLR member binding.
- Before handing off, run `git diff --check` and confirm that only intended files changed.

