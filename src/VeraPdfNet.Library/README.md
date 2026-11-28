# veraPDF Library for .NET

This is an early, incomplete native .NET port of veraPDF library integration.
The current preview can inspect the
bundled validation profiles, read and write custom profile XML, and read a
limited subset of XMP packets. It **does not validate
PDFs**, evaluate rules, fully parse XMP, extract features, repair metadata, or produce
validation reports. Do not use it to make conformance claims.

`BuiltInProfiles.Names` lists the bundled profile names; use
`BuiltInProfiles.Read(name)` or `ReadByFlavour(flavour)` to inspect one. `ProfileReader.Read(stream)` reads
custom profile XML without taking ownership of the stream. Rule expressions
are exposed as data, not executed. This reader is not an XML Schema validator:
it reads the profile fields it understands and does not retain unknown elements.
Text is exposed as XML text rather than applying the Java reader's whitespace
normalization.
`ProfileWriter.Write(profile, stream)` writes the represented profile fields as
UTF-8 and leaves the stream open. Unrecognized XML extensions are not preserved.
Profiles also support rule lookup, object lookup and include/exclude tag filtering.
`ValidationProfile.Create` constructs custom profiles and copies the supplied
rule, argument, reference, and variable collections.
Validation result data types are available for integration code; constructing
one does not perform validation. They preserve the supplied compliance state,
total check count, and termination status even when assertions are omitted.
`ValidatorConfiguration` carries reference-compatible defaults for a future
validator; it is not connected to a validation engine. `ValidatorConfigurationXml`
persists the Java-mapped fields without storing the password or non-PDF extension
option; both return to their defaults when read back.

`XmpSchemaRegistry` provides the reference standard namespaces and property
aliases, and registers custom prefixes. Custom registrations are thread-local
and shared between registry instances on that thread; creating a registry
clears them on the current thread, matching the reference implementation.
`XmpPath.Parse` expands XMP property paths and standard aliases;
`XmpPathFactory` constructs common paths and selectors. Malformed truncated
paths produce an `XmpException` rather than the reference's occasional
index exception. `XmpDateTime.Parse` preserves partial date and time components,
nanoseconds and explicit fixed timezone offsets, using the reference parser's
permissive normalization and rendering conventions. `FromDateTimeOffset`
creates a complete fixed-offset date. `Create` constructs empty, date-only or
unzoned date/time values; date and time fields are clamped to XMP component
ranges, while nanoseconds must be between 0 and 999,999,999. `WithOffsetMinutes`
returns a copy with a fixed offset without changing its local date or time.
`ConvertToUtc`, `ConvertToTimeZone`, and
`ConvertToLocalTime` convert complete, valid dates with an explicit offset;
time-zone conversion uses the destination zone's offset at the represented
instant, including daylight-saving transitions. These operations use the
reference calendar's millisecond precision and reject partial, invalid,
unzoned and out-of-range dates rather than applying lenient calendar defaults.
`CompareTo` compares complete zoned dates by instant, retaining nanoseconds,
and applies the same date validity limits. Mutable fields and general lenient
calendar conversion are not yet implemented.
`XmpScalarConverter` handles Boolean, integer, long, date and Base64 values,
and parses Java-compatible decimal and hexadecimal double strings. Double
updates use invariant shortest-round-trip digits with Java-style decimal and
exponent notation; rare values may format differently from Java's
`Double.toString`.
Base64 decoding follows the reference's permissive handling of padding and
partial groups; it is not a strict Base64 validator. Java-compatible
double-to-string formatting is not yet implemented.
The RDF writer's optional `Sort` setting orders schemas, properties, structure
fields and non-special qualifiers while preserving array order and leaving the
packet unchanged. Because the raw tree does not retain source prefixes, it
assigns prefixes in namespace-URI order and uses those prefixes to sort fields
and qualifiers; ordering can therefore differ from the Java writer's registry
prefixes.
`XmpPacket.Create()` starts an empty packet without a subject or packet header.
`WithCompositeProperty` creates an empty structure, Bag, Seq or Alt at a
top-level name or a field of an existing structure; missing intermediate nodes,
array items and composite qualifiers cannot be created. An existing node of the
same kind is left unchanged, including its contents, rather than cleared as by
the reference's composite setter. Different existing kinds are rejected.
`XmpPacket.Parse(stream)` reads a **limited, unnormalized** RDF subset: literal
properties, property attributes, URI values, structures, typed resources,
qualified values and Bag/Seq/Alt arrays. The legacy Dublin Core namespace is
mapped to its current URI. Non-language qualifiers are exposed on `XmpNode.Qualifiers`.
`GetPropertyByPath(registry, schemaUri, path)` traverses structures, arrays and
qualifiers, including selectors and standard aliases. Register custom namespaces
with the supplied registry before using their prefixes. Lookup reads the raw tree
without normalizing aliases or creating missing alias targets; a language
qualifier is returned as a simple node. Non-language qualifier selectors follow
the Java reference's behavior of skipping the last array item.
`WithPropertyString` returns a new packet with a simple value updated by path,
or creates a top-level property, field of an existing structure, or qualifier.
It preserves existing node kinds and qualifiers, replaces ASCII control
characters with spaces, and normalizes `xml:lang` values. It rejects null values,
composite targets, missing intermediate nodes and missing array items; unlike the
reference setter it does not create intermediate nodes. Updating a language
qualifier or localized item does not normalize or synchronize alt-text arrays.
`WithPropertyUri` uses the same path and creation rules to store a simple value
as an RDF URI resource. It converts literal targets to URI nodes, retains
qualifiers, and rejects `xml:lang` targets; it does not validate URI syntax.
Subsequent string updates retain the URI kind. As with other mutations, named
RDF/XML qualifiers remain subject to the writer's serialization restrictions.
`WithPropertyBoolean`, `WithPropertyInteger`, `WithPropertyLong`,
`WithPropertyDate` and `WithPropertyBase64` use the same path and immutability
rules with the scalar converter's XMP formatting. Null date and byte-array
values are rejected. An empty XMP date is stored as an empty string, which the
date getter rejects as an invalid date. A double setter is not yet available
because reference-compatible double formatting is not implemented.
`WithoutProperty` returns a new packet without the node at an expanded path,
including array items and qualifiers, without pruning empty parent properties.
Missing nodes leave the packet unchanged; invalid paths throw rather than being
silently ignored as in the reference. Removing language qualifiers can leave
an alternate array without valid alt-text metadata.
`CountArrayItems` and `GetArrayItem` provide read-only access to array paths;
indexes are one-based, with `-1` selecting the last item.
`WithArrayItemString` replaces an existing item with a new literal, discarding
the old item's structure and qualifiers. `WithInsertedArrayItemString` inserts
before a one-based index; count + 1 and `-1` append. Both require an existing
array, preserve the original packet, reject null values, and replace ASCII
controls with spaces. They do not repair or synchronize alt-text arrays, so
replacing or inserting a localized item can leave it without a language.
`WithAppendedArrayItemString` appends a literal, including duplicate values,
to an existing array or creates a top-level array or field of an existing
structure. Creation requires an explicit Bag, Seq or Alt kind; a supplied kind
must match an existing array, unlike the reference's unchecked form. Missing
intermediate nodes and array qualifiers cannot be created. Null values are
rejected, and appending does not repair or synchronize alt-text arrays.
`WithArrayItemComposite`, `WithInsertedArrayItemComposite`, and
`WithAppendedArrayItemComposite` replace, insert or append empty structures or
arrays using the same index and creation rules. Replacement discards the old
item and its qualifiers; inserting a composite item into an Alt array does not
make it valid alt-text.
`WithArrayItemUri`, `WithInsertedArrayItemUri`, and `WithAppendedArrayItemUri`
apply the same rules to URI-valued items. URI syntax is not validated; replacing
an item discards its qualifiers and none of these operations repairs alt-text.
`XmpArrayUtilities.CatenateArrayItems` joins simple values in Bag or Seq arrays
using the reference's separators and quote-pair rules. It retains the reference's
quirk of not doubling embedded quotes that precede the character triggering
outer quoting. `SeparateArrayItems` parses an edit string into distinct values.
`AppendSeparatedArrayItems` returns a new packet with new, distinct values
appended to a Bag or Seq, preserving existing items and the original packet. It
can create a top-level array or a field of an existing structure; missing
intermediate nodes are not created. An explicitly requested array form must
match an existing array (unlike the reference's inverted form check).
`GetStructField` and `GetQualifier` resolve names against the supplied registry.
Unlike the Java parser for some attribute qualifiers, lookup uses the namespace
URI even when the XML source prefix differs from the registered prefix.
Typed property getters read strings, booleans, integers, longs, doubles, dates
and Base64 values through the same paths; non-string getters reject composite
properties.
`IterateProperty` traverses a property subtree, optionally limiting children,
leaf nodes or names and omitting qualifiers. Register all encountered namespaces
before enumeration. `CreatePropertyIterator` provides a forward-only cursor with
`SkipSubtree` and `SkipSiblings`; sibling skips apply to the current child or
qualifier list, and both controls require a current entry. These implement the
documented skip semantics rather than reproducing the reference's unused
subtree-skip flag. Its paths follow Java's iterator display conventions: qualifier
paths omit `?`, and a selected array root is displayed as `[1]`, so they are not
necessarily reusable as lookup paths. `Iterate` traverses the whole packet or a
single schema and reports schema entries separately. `CreateIterator` provides
the same skip controls for packet and schema traversal: skipping schema siblings
ends traversal, while skipping top-level property siblings advances to the next
schema, if any.
`GetLocalizedText` selects an alt-text item using the reference's language
fallback rules. It applies the reference's x-default ordering and two-item
value adjustment for that lookup without changing the raw parsed tree.
`WithLocalizedText` returns a new packet with a language value updated using
exact, single/multiple generic, x-default and first-item matching. It moves
x-default first and synchronizes matching values as appropriate. It creates a
missing top-level array or field of an existing structure; missing intermediate
nodes are not created. It rejects null values and malformed alternative arrays,
validating every item even where the reference may stop at an earlier match.
ASCII control characters in new values become spaces, unlike the reference's
localized setter. An unnormalized alternative array is treated as alt-text when
empty or when all items are simple and have language qualifiers.
`XmpPacket.ParseDiscovered(stream, requireXmpMeta)` searches nested XML for
an RDF root and returns empty metadata if none is found. The optional flag
requires an XMP wrapper, and `PacketHeader` exposes the preceding `xpacket`
processing instruction. `Parse(stream)` retains strict root handling and
rejects absent or unsupported roots. Both readers leave the stream open and
prohibit DTDs. Unsupported forms throw `NotSupportedException` or `XmpException`.
For string input, `ParseDiscovered(text, fixControlCharacters: true)` retries
malformed XML after repairing ASCII controls and numeric control references,
matching the reference's recovery behavior. Stream discovery accepts an
`XmpRecoveryOptions` instance to opt into control repair and/or mixed
UTF-8/Windows-1252 recovery. Recovery is disabled by default; enabling it
buffers the remaining stream. As in the Java reference, a UTF-8 BOM prevents
a successful control-repair retry. `XmpPacketWriter.Write` writes supported raw
trees as RDF/XML, leaving the output stream open. The optional
`XmpPacketWriterOptions` supports new writable or read-only xpacket wrappers,
explicit byte padding or an exact packet byte length, and omission of the
`x:xmpmeta` element. Output can use UTF-8 (default), UTF-16BE or UTF-16LE
without an external byte-order mark; UTF-16 padding and exact byte lengths
must be even. The existing overload remains unwrapped; wrapping defaults
to zero padding, rather than the upstream default. `IncludeThumbnailPadding`
adds 10,000 UTF-8 bytes or 20,000 UTF-16 bytes when `xmp:Thumbnails` is
absent; it requires a writable wrapper and cannot be combined with an exact
length. Optional indentation uses configurable XML whitespace for indentation
and line breaks (also used around the packet body). Wrapped output is buffered
and does not preserve an input packet's processing instructions. Other upstream
serialization formats and options are not implemented. Typed-resource
`rdf:type` qualifiers are supported; other RDF and XML named qualifiers are
rejected.
For RDF descriptions and nested resource nodes, `rdf:ID` and `rdf:nodeID`
identify RDF nodes but are not retained as metadata; mutually exclusive node
identifiers are rejected. Parsing does not normalize aliases or metadata. This
is not a full XMP implementation.

The project targets .NET 10 and references the existing parser and model
projects. Run local builds and tests through `VeraPdfNet.slnx`.
`VeraPdf.Library.Versioning.Versions` parses and compares three-part release
versions, ignoring prerelease and build metadata.
`VeraPdf.Library.Components.AuditDuration` represents and aggregates audit
durations; XML serialization adapters are not included yet.
`ComponentDetails` represents component identity and descriptive metadata with
the reference default instance; `ComponentDetailsXmlAdapter` maps its Java
attributes and child elements through `XmlSerialiser` without making the model
mutable. `AuditDurationXmlAdapter` maps the start/finish attributes and
formatted duration text without making the model mutable.
`Log` and `LogsSummary` represent log records and immutable summary snapshots.
`LogsSummary.GetSummary` reads the process log file, groups identical log
messages, and preserves multiline content. `LogXmlAdapter` and
`LogsSummaryXmlAdapter` map their Java XML element, attribute, and text
structure through `XmlSerialiser` without making the model classes mutable.
`Components` exposes default component values, validated metadata factories,
veraPDF provider metadata, and wall-clock audit timers.
`IComponent` exposes the component details and maps its closeable lifecycle to
`IDisposable`.
`ReleaseDetails` provides immutable release metadata and registry lookups; its
properties parser supports basic key/value lines but not Java properties
escaping or continuations.
`ValidationProgress` tracks validation counters and optionally reports progress
to standard error.
`FileUtils` provides extension extraction, matching, and path-safe replacement.
`LogsFilter` applies the reference source-prefix check to log source names.
`ConsoleLogsFilter` adds the reference's level-above-`FINE` rule for other
sources.
`LogsFormatter` formats the log context and message with the current process ID.
`LogsFileHandler` provides a per-process temporary `.log` file and can reset it;
Java logging configuration and file rotation are not included.
`IFileOutputMapper` defines the contract for mapping input files to output paths;
`AbstractFileOutputMapper` provides default prefix and suffix placement around
file extensions. `RelativeDirectoryMapper` keeps output beside the source or
creates a configured relative directory; `SingleOutputDirMapper` maps outputs
to one directory without creating it. `RelativeRootMapper` resolves the
configured source root relative to each input file's parent, then creates the
corresponding destination directory, matching the Java implementation.
`VersioningMapper` wraps another mapper and chooses the next `_(n)` filename
when its mapped output already exists. `FileOutputMappers` provides sibling,
relative-subfolder, relative-destination and single-output-directory factories,
including versioned variants where the Java API provides them.
`XmlSerialiser` supports basic typed XML string/stream serialization and
deserialization, plus file and text-reader/writer overloads; it does not yet
automatically discover polymorphic collection types when reading; pass derived
types explicitly as known types. The same basic formats are available for
collections; `Schema` generates a single XSD, while
`SchemaDocuments` returns separate XSD documents for each XML namespace.

The reference inventory and remaining port work are described in
`docs\port-status.md`.

The Java core and bundled profiles are dual-licensed under GPL-3.0-or-later or
MPL-2.0; the ported XMP components are BSD-3-Clause. See
`THIRD-PARTY-NOTICES.md` for source and licensing details.
