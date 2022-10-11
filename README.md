# Open API Comparator

An API change detector.

## Installation

The tool is available as a [nuget package](https://www.nuget.org/packages/Criteo.OpenApi.Comparator).

## Code Sample

```C#
var comparator = new OpenApiComparator();

var messages = comparator.Compare(
    oldSpecName,
    oldSpec,
    newSpecName,
    newSpec
);
```

## Documentation

Each comparison rule is documented in the [documentation section](https://github.com/criteo/openapi-comparator/tree/main/documentation).

## License

OpenApi Comparator is an Open Source software released under the [Apache 2.0 license](https://github.com/criteo/openapi-comparator/blob/main/LICENCE).


