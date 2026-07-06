# DDD Item Templates

`dotnet new` item templates for the domain-type plumbing used in this codebase (and any project with the same SharedKernel base types: `AggregateRoot`, `Entity`, `EntityId`, `ValueObject`).

## Install

From the repository root:

```bash
npm run templates:install
```

or per template:

```bash
dotnet new install ./templates/ddd-aggregate --force
```

Installation is machine-global. Re-run with `--force` after editing a template. Uninstall with `npm run templates:uninstall`.

## Templates

| Short name | Emits | Notes |
|---|---|---|
| `ddd-aggregate` | `<Plural>/<Name>.cs`, `<Plural>/<Name>Id.cs`, `<Plural>/Events/<Name>Created.cs` | Event-sourced root with factory, `ApplyEvent` switch, Created event |
| `ddd-vo` | `<Name>.cs` | `ValueObject` with private ctor + `Create` factory |
| `ddd-entity` | `<Name>.cs`, `<Name>Id.cs` | `Entity<TId>` plus its ID |
| `ddd-enum` | `<Name>.cs` | `Enumeration<T>` with a single `Default` value to replace |

## Usage

Run from the directory the files should land in (e.g. a module's `Domain/` folder):

```bash
cd src/CommonGround.PropertyRegistry/Domain

# Aggregate: creates Garages/Garage.cs, Garages/GarageId.cs, Garages/Events/GarageCreated.cs
dotnet new ddd-aggregate -n Garage --namespace CommonGround.PropertyRegistry.Domain

# Irregular plural
dotnet new ddd-aggregate -n Property --plural Properties --namespace CommonGround.PropertyRegistry.Domain

# Custom ID prefix (default is the lower-cased name)
dotnet new ddd-aggregate -n HomeownerApplication --prefix hoapp --namespace CommonGround.HomeBuyers.Domain

# Standalone value object, entity, enumeration (namespace here is the full namespace, nothing is appended)
dotnet new ddd-vo -n StreetAddress --namespace CommonGround.PropertyRegistry.Domain.Garages
dotnet new ddd-entity -n InspectionRecord --namespace CommonGround.PropertyRegistry.Domain.Garages
dotnet new ddd-enum -n GarageStatus --namespace CommonGround.PropertyRegistry.Domain.Garages
```

### Parameters

- `-n <Name>` — the type name (PascalCase, singular).
- `--namespace` — required. For `ddd-aggregate` it is the *parent* namespace; the plural segment is appended. For the others it is the full namespace of the generated file.
- `--plural` (`ddd-aggregate` only) — folder/namespace segment; defaults to `<Name>s`.
- `--prefix` (`ddd-aggregate`, `ddd-entity`) — ID prefix; defaults to the lower-cased name.

## Notes

- Templates only add files. Aggregates need no registration (Marten live-aggregates by convention); read models/projections in `*MartenConfiguration` remain manual when you add them later.
- The `Ddd*` placeholder `.cs` files in this folder are template content, not part of the solution — they are not expected to compile in place.
- Cross-project reuse: the generated files hardcode `CommonGround.SharedKernel.Domain` usings. To use in another project (e.g. BindingChaos), either parameterize the kernel namespace or pack these as a NuGet template pack once the shape settles.
