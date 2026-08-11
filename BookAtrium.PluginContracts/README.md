# BookAtrium.PluginContracts 2.1.1

Canonical public package for BookAtrium plugins (Plugin API **2.1**). Reference this package only:

```xml
<PackageReference Include="BookAtrium.PluginContracts" Version="2.1.1" />
```

```powershell
bookatrium-plugin new store --name "My Store" --publisher "Me"
# edit Plugin.cs →
bookatrium-plugin test
bookatrium-plugin pack
```

Inherit a type base (`StorePlugin`, `MetadataSourcePlugin`, `AuthorMetadataSourcePlugin`,
`MetadataReaderPlugin`, `MetadataWriterPlugin`, `InputConverterPlugin`, `OutputConverterPlugin`,
`DevicePlugin`, `FileTypePlugin`, `InputProfilePlugin`, or `OutputProfilePlugin`),
implement the domain method(s), then pack. Do not hand-write `plugin.json`.

Do **not** reference `BookAtrium.PluginSdk` or any private BookAtrium application projects.

Quick start: [docs/plugins/sdk-2/getting-started.md](https://github.com/lgdeysel1980/BookAtrium/blob/main/docs/plugins/sdk-2/getting-started.md)

## Metadata writer fields

API 2.1 metadata writers receive a `PluginBookMetadataSnapshot` that includes
title, subtitle, sort title, authors, author sorts, series/index, publisher,
publication date, language, identifiers, tags, comments/description, rating,
ISBN, cover bytes/MIME, and page count. `MetadataWriteResult` reports
unsupported fields, warnings, and whether the temporary output file changed.
Writers must mutate only the host-owned temporary copy; final replacement and
backup remain application-controlled.

API 2.1 adds optional, provider-neutral audiobook fields to `BookMetadata` and
`PluginMetadataResult`: `EditionType`, `AudiobookAsin`, `Narrators`,
`ListeningLength`, `AudiobookPublicationDate`, `AudiobookVersion`, and
`AudiobookLanguage`. Use `ebook`, `audiobook`, or `print` for `EditionType`, and
`abridged` or `unabridged` for a known audiobook version. Omitted fields remain
safe and do not clear saved metadata.

The host applies audiobook fields through a restricted audiobook scope. They
cannot overwrite title, authors, description, cover, general publication data,
ebook identifiers, tags, series, custom fields, files, or reading state.

## Metadata reader fields

Metadata readers extend `MetadataReaderPlugin` and return a `BookMetadata` from
`ReadAsync`; the base class maps it into the shared `PluginBookMetadataSnapshot`
returned to the host, including `AuthorRoles` (aligned by index with `Authors`),
`Duration` (from `ListeningLength`), `Narrators`, and `SupplementalFields`
(format-specific, non-canonical key/value data, e.g. `RawMetadata`-style tags).

`MetadataReaderPlugin.ReadPriority` controls ordering when multiple readers can
read the same file; lower values run first. It defaults to `100`. Gap-filler /
optional-tool readers that only fill in fields left empty by a format-specific
reader (for example the official ExifTool reader) should override it with a
higher value (the official ExifTool reader uses `1000`) so they always run last
and never take priority over a native format reader.

## Conversion input plugins

`IConversionInputPlugin.ReadAsync` reads a source ebook file and reports success
by writing intermediates as plain files under
`ConversionInputRequest.IntermediateDirectory`, then returning a
`ConversionInputResult` pointing at that directory — a deliberately host-agnostic,
serialization-free contract so a third-party plugin never needs to reference (or
version-match) any private BookAtrium application assembly to hand data back to
the host. Do not attempt to construct or return `BookAtrium.Conversion` document
types directly; only official (first-party) packages that ship in lockstep with
the host application do that, via an internal bridge that still ends up writing
files to the same `IntermediateDirectory` your plugin would write to by hand.

Developer guides: [docs/plugins/sdk-2](https://github.com/lgdeysel1980/BookAtrium/tree/main/docs/plugins/sdk-2)

Community catalogue: [BookAtrium-Community-Plugins](https://github.com/lgdeysel1980/BookAtrium-Community-Plugins)
