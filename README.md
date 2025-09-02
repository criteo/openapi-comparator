# Open API Comparator

An OpenAPI tool to compare OpenAPI Specifications.

## C# Library

The tool is available as a [nuget package](https://www.nuget.org/packages/Criteo.OpenApi.Comparator), directly usable into your C# application.

To install it run the command:
```bash
dotnet add package Criteo.OpenApi.Comparator
```

Here is an example of how to use the Comparator:
```C#
var result = OpenApiComparator.Compare(
    out var differences,
    oldOpenApiSpec,
    newOpenApiSpec
);
```

## Command line tool

The comparator is also available as a [command line tool](https://www.nuget.org/packages/Criteo.OpenApi.Comparator.Cli/0.1.0).

To install it, run the command:
```bash
dotnet tool install -g Criteo.OpenApi.Comparator.Cli
```

You can then use the tool through the `openapi-compare` command:
```bash
openapi-compare -o new_oas.json -n old_oas.json -f Json
```

Available options:
| Option           | Small   | Required | Description                                                                                                         |
|------------------|---------|----------|---------------------------------------------------------------------------------------------------------------------|
| `--old`          | `-o`    | `true`   | Path or URL to old OpenAPI Specification.                                                                           |
| `--new`          | `-n`    | `true`   | Path or URL to new OpenAPI Specification.                                                                           |
| `--outputFormat` | `-f`    | `false`  | (Default: `Json`) Specifies in which format the differences should be displayed. Possible values: `Json` \| `Text`. |
| `--strict`       | `-s`    | `false`  | (Default: `false`) Enable strict mode: breaking changes are errors instead of warnings.                             |
| `--help`         | `-h`    | `false`  | Log available options                                                                                               |

## Comparison rules

Each comparison rule is documented in the [documentation section](https://github.com/criteo/openapi-comparator/tree/main/documentation).

## OpenAPI version support

Internally, the comparator uses [microsoft/OpenAPI.NET](https://github.com/microsoft/OpenAPI.NET/) which currently supports OpenAPI 2.0 to 3.0.0.

## Versions

### 1.0
- Made breaking changes to support that additive changes are allowed by default
- Compare now returns a Change level that reflects the result or all found changes. 
- Removed strict mode. 
  - When not in strict mode there were very few errors in previous versions
  - A major version update in the spec is now considered an Breaking rule in that your API can intentionally change.
- Combined the Parsing Errors with the Comparsion Messages
- On the message itself a code will now always uniquely identify the message severity
  - Severity now includes a Breaking severity
- Split up several Code so that they are easier to identify and customize
  - ConstrantIsStronger
    - ResponseConstraintIsStronger
    - EnumConstrantIsStronger
  - AddedPropertyInResponse
    - AddedBreakingPropertyInResponse
  - ConstraintChanged
    - EnumConstraintChanged
    - MultipleOfConstraintChanged
    - UniqueItemsConstraintChanged
  - Added new Codes
    - MajorVersionChange
    - MinorVersionChange
    - NonSemanticVersion
- Removed .NET 6 support as its no longer supported by Microsoft

## Contributing

Any contribution is more than welcomed. For now, no specific rule must be applied to contribute, just create an Issue or a Pull Request and we'll try to handle it ASAP.

## License

OpenApi Comparator is an Open Source software released under the [Apache 2.0 license](https://github.com/criteo/openapi-comparator/blob/main/LICENCE).

## Developer guide

Simply use the dotnet cli. For example, to run the tests:
```bash
dotnet test
```
