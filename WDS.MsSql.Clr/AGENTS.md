# AGENTS.md

## Scope
- These instructions apply to the `WDS.MsSql.Clr` project and extend the solution-level instructions in `../AGENTS.md`.

## Project Responsibilities
- `ClrFunctions` contains the SQL-callable functions and their adaptation to Dapi MSSQL API operations.
- `ClrFunctions/Server` owns HTTP request creation, error handling, XML transport, and `202 Accepted` polling through the response `Location` header.
- `DataContracts` contains API-facing request and response models plus SQL CLR UDT state and fluent configuration types.
- `Serialization` owns XML, enum, nullable, array, and `IBinarySerialize` helpers.

## SQL CLR Public-Member Rule
- Never declare overloaded public methods, properties, or fields on a class exposed as a SQL CLR UDT, including classes marked with `[SqlUserDefinedType]`.
- SQL Server binds UDT members by name and does not support overloads. Different parameter counts or parameter types do not make duplicate public member names valid.
- A SQL-visible class must have at most one public member with a given name. Avoid method/method, method/property, method/field, property/field, and other same-name combinations.
- If a new variation is needed, give it a distinct public name, replace the existing member and update every SQL caller, or keep the variation non-public when it is only an implementation helper.
- Before adding or renaming a public UDT member, search the project and SQL scripts for the name and check `WDS.MsSql.Scripts/WdsClrFunctions.sql` plus external SQL usage.
- Validate public UDT changes in SQL Server. A successful C# build and unit-test run cannot detect SQL error 6572 caused by duplicate public member names.

## Data Contracts and Serialization
- Keep API-facing public properties aligned with `WDS.Dapi.Core.PublicDataContracts` and add corresponding cases to `DataContractMatchTests` for newly introduced nested types.
- CLR-only properties such as server connection state and error state must not leak into API XML when marked for exclusion.
- Keep every `BinaryRead` operation in the same order and with the same nullability semantics as its corresponding `BinaryWrite` operation.
- When adding nested UDTs, include their source files in `WDS.MsSql.Clr.csproj` and register SQL-visible types in the deployment script before dependent types.

## API Client Changes
- Keep `ClrFunctions.API_VERSION` aligned with Dapi's MSSQL API route version.
- Use the HTTP method and XML body expected by the current endpoint; do not move structured request contracts back into query strings.
- For asynchronous task operations, follow `202 Accepted` responses using `Location` and deserialize only the final successful response.
- Preserve the `X-Tenant-Id` behavior and existing error format unless the API contract intentionally changes.

## Validation
- Build this project after every code change.
- For public UDT changes, inspect all public member names for duplicates and run the affected SQL installation or usage script in a SQL Server test environment.
- For API contract changes, build the test project and run contract-match, XML serialization, and binary round-trip coverage.

