# Integration port status

The source reference is the veraPDF library integration snapshot at repository
commit `1190d874f084864fd23dc145fc44e55e36b20c52`. That snapshot
contains 299 Java files in `core` and 52 in `xmp-core`. Its core resource tree
contains 15 profile XML files, other XML files, schemas, properties and
stylesheets. This is a preliminary count, not a complete API mapping.
The semantic-version utility parses three-part versions and compares major,
minor and patch values; prerelease and build metadata are ignored.
The core exception hierarchy includes the base, encrypted-PDF, profile,
validation, model-parsing and feature-parsing exceptions; each preserves
messages and inner exceptions.
Generic read-only directories and map-backed registries support lookup,
registration, update and removal operations.
The component audit-duration value supports endpoint validation, elapsed-time
formatting and aggregation of overlapping durations; XML adapters are not yet
ported.
Component details are represented as immutable values with the reference
defaults and equality behavior. `ComponentDetailsXmlAdapter` maps its XML
attributes and child elements without making the model mutable.
`AuditDurationXmlAdapter` maps the start/finish attributes and formatted
duration text without making the duration model mutable.
Component log entries and immutable log-summary snapshots are represented.
`LogsSummary.GetSummary` reads the process log file, groups identical log
messages, and preserves multiline content. `LogXmlAdapter` and
`LogsSummaryXmlAdapter` map the Java XML element, attribute and text structure
without making the model classes mutable.
Component factories expose reference default values, validated details, veraPDF
provider metadata and wall-clock audit timers.
The component contract maps Java's closeable lifecycle to `IDisposable` and
exposes immutable details.
`ValidationProgress` tracks check, failure, and processed-object counts and can
write the reference progress display to standard error.
`FileUtils` extracts and matches filename extensions and replaces or adds an
extension without mistaking dotted directory names for file extensions.
`LogsFilter` accepts source-class names using the reference's case-sensitive
`org.verapdf` prefix check.
`ConsoleLogsFilter` accepts those sources at any level and other sources only
above the Java `FINE` threshold (numeric value 500).
`LogsFormatter` formats source class, method, current process ID, level, and
message using platform line endings.
`LogsFileHandler` creates/reuses one temporary `.log` path per process, can
create or truncate that file, and attempts cleanup at process exit. Java global
logging configuration, rotation, and configurable patterns are not ported.
`IFileOutputMapper` defines prefix/suffix metadata and mapping from an input
`FileInfo` to a non-guaranteed output `FileInfo`; concrete mapping strategies
remain to be ported. `AbstractFileOutputMapper` provides the `veraPDF_`
default prefix and Java-compatible suffix placement before or after a filename
extension. `RelativeDirectoryMapper` keeps output beside the source by default
or creates a configured relative directory. `SingleOutputDirMapper` maps
outputs to one configured directory without creating it. `RelativeRootMapper`
resolves the configured source root relative to each input file's parent and
creates the resulting destination directory, matching the Java implementation.
`VersioningMapper` wraps another mapper and selects the next numeric `_(n)`
filename when the mapped output already exists. `FileOutputMappers` provides
sibling, relative-subfolder, relative-destination and single-output-directory
factories, including the versioned variants available upstream.
Release details now support immutable metadata, equality, a synchronized
registry, and parsing basic key/value properties from streams or embedded
resources. The local library resource identifies the .NET assembly and version;
its timestamp is initialized when the resource is loaded. Missing embedded
resources return the default details. Java
properties escaping/continuations and exact Java date display are not
implemented; invalid properties or dates raise format errors. XML attributes
support round-trip serialization through `XmlSerializer`.
`XmlSerialiser` supports typed string/stream deserialization and string/stream
serialization with formatting and XML-declaration controls. It prohibits DTDs;
file and text-reader/writer overloads are also available and leave caller-owned
streams open. Basic homogeneous collection serialization uses a custom wrapper
element and preserves item order; polymorphic collection handling is not yet
fully automatic: derived item types must be supplied as known types when reading.
Collection APIs also support file and text-reader/writer overloads.
`Schema` generates an XSD for a single namespace, while `SchemaDocuments`
returns separate XSD documents for each namespace in multi-namespace mappings.
`SchemaDocuments`, which returns one XSD per namespace.

The additional veraPDF validation integration modules contain
312 validation-model, 26 feature-reporting, 10 metadata-fixer and 102
WCAG-validation Java files. These are potential prerequisites; their own
external dependencies and implementation scope still need an inventory. The
existing .NET model provides contracts, while the parser has documented
limitations in its README.
The validation-model's `RenderingMode` maps PDF text rendering modes 0 through
7, including fill/stroke classification and null results for out-of-range
numeric values.
`CosNumberModelAdapter` maps parser integer and real values to the generated COS
number model contracts, preserving type identity, real projections and
truncating integer projections. Other COS types remain to be adapted.
`CosNameModelAdapter` maps the parser's already-decoded name bytes to the COS
name contract. It preserves the Latin-1 internal representation returned by
Java `GFCosName`; the separate `COSName.getUnicodeValue` conversion is not used
by that adapter.
`CosBoolModelAdapter` maps parser booleans to the COS boolean contract and
canonicalizes true and false values to shared instances, as `GFCosBool` does.
`CosNullModelAdapter` represents explicit PDF null separately from a missing
value and exposes one canonical model instance, as `GFCosNull` does.
`CosStringModelAdapter` maps PDF string bytes and literal/hex source metadata.
Its value projection uses UTF-8 for Java's default byte-to-string behavior on
standard runtimes; PUA checks use PDFDocEncoding or the recognized UTF-16BE/UTF-8
BOM decoding used by the Java COS string implementation.
`CosIndirectModelAdapter` maps indirect-object IDs and exposes supported direct
values through the `directObject` link. References inside arrays, dictionaries,
and streams resolve when a `PdfDocumentReader` is supplied; without one they
fail explicitly. Unavailable spacing metadata is null, and unsupported
direct-value types are rejected.
`CosArrayModelAdapter` exposes array size and a read-only `elements` link for
supported COS objects, including nested arrays; unported object types fail
explicitly.
`CosDictionaryModelAdapter` exposes size, the joined key string, and read-only
key/value links for supported values. References require a `PdfDocumentReader`;
its `metadata` link exposes qualifying catalog and non-catalog XML metadata
streams and identifies catalog metadata. The metadata object's XMP package link
decodes and parses XML streams, exposes serialization status, packet-header
attributes, and raw property objects.
Extension-schema containers expose their namespace prefix and Bag validity.
Extension-schema definitions expose declared-field validation and field prefixes.
Extension-schema property definitions expose allowed-child validation,
category, simple-text checks and source prefixes. A present value type remains
unknown because PDF/A flavour context is unavailable. Extension-schema value
type definitions expose allowed-child checks, scalar/sequence validation and
prefixes; their field definitions expose allowed-child checks, text validation
and prefixes. A present field value type is likewise unknown without PDF/A
flavour context. XMP properties declared in the current packet expose their
declared extension type name independently of whether that type has a validator.
Properties declared with the built-in `Text` type can also be checked for
simple values. Predefined
membership and declared types are recognized for core XMP, Dublin Core, XMP
Rights, XMP Media Management, XMP Basic Job, XMP Paged Text, PDF/A identification,
Adobe PDF, Photoshop, TIFF, EXIF and EXIF Auxiliary properties. XMP Paged Text `Fonts`,
`Colorants` and `PlateNames` are PDF/A-2/3-only; the common `MaxPageSize` and
`NPages` properties are recognized for both profiles. EXIF Auxiliary `Lens`
and `SerialNumber` are PDF/A-2/3-only. Photoshop `SupplementalCategories` is
recognized in both profiles, but its profile-dependent type remains unknown.
EXIF `GPSMeasureMode` is predefined in both profiles, but its declared type
differs between them and remains unknown without flavour context. EXIF
`ColorSpace` and `LightSource` membership and integer types are recognized in
both profiles; profile-specific value restrictions are not checked here. All
41 Camera Raw properties in the reference schema are recognized and are
PDF/A-2/3-only: `AutoBrightness`,
`AutoContrast`, `AutoExposure`, `AutoShadows`, `BlueHue`, `BlueSaturation`,
`Brightness`, `CameraProfile`, `ChromaticAberrationB`, `ChromaticAberrationR`,
`ColorNoiseReduction`, `Contrast`, `CropTop`, `CropLeft`, `CropBottom`,
`CropRight`, `CropAngle`, `CropWidth`, `CropHeight`, `CropUnits`, `Exposure`,
`GreenHue`, `GreenSaturation`, `HasCrop`, `HasSettings` and
`LuminanceSmoothing`, `RawFileName`, `RedHue`, `RedSaturation`, `Saturation`,
`Shadows`, `ShadowTint`, `Sharpness`, `Temperature`, `Tint`, `ToneCurveName`,
`Version`, `VignetteAmount`, `VignetteMidpoint`, `WhiteBalance` and `ToneCurve`.
The closed-choice checks for `WhiteBalance` and the sequence-of-points check
for `ToneCurve` are not implemented. All 57 Dynamic Media properties in the
reference schema are recognized and PDF/A-2/3-only:
audio, video and production metadata properties are recognized as PDF/A-2/3-only:
`artist`, `album`, `trackNumber`, `genre`, `copyright`, `releaseDate`,
`composer`, `engineer`, `videoFrameRate`, `videoFrameSize`,
`videoPixelAspectRatio`, `videoAlphaUnityIsTransparent`,
`videoAlphaPremultipleColor`, `videoCompressor`, `audioSampleRate`, and
`audioCompressor`, `speakerPlacement`, `fileDataRate`, `tapeName`,
`altTapeName`, `startTimecode`, `altTimecode`, `duration`, `scene`, `shotName`,
`shotDate`, `shotLocation`, `logComment`, `markers`, `contributedMedia`,
`absPeakAudioFilePath`, `relativePeakAudioFilePath`, `videoModDate`,
`audioModDate`, `metadataModDate`, `tempo`, `instrument`, `introTime`,
`outCue`, `relativeTimestamp`, `loop`, `numberOfBeats`, `timeScaleParams`,
`resampleParams`, `beatSpliceParams`, `projectRef`, `videoPixelDepth`,
`videoColorSpace`, `videoAlphaMode`, `videoFieldOrder`, `pullDown`,
`audioSampleType`, `audioChannelType`, `key`, `stretchMode`, `timeSignature`,
and `scaleType`. Restricted-value checks for these closed/open choices are
not implemented; other names in the namespace are reported as not predefined.
Predefined Boolean, Integer, Real, MIME type, and Date values are checked using
Java-compatible lexical/parsing rules. Predefined `Text` properties are checked
for literal/URI node kind, as do predefined `ProperName`, `AgentName`, `Rational`,
and `RenditionClass` values. Predefined `lang alt` properties are checked for an
alternate array with language-tagged items (empty alternate arrays are
accepted). Extension properties declared with supported built-in scalar types
use the same checks. Predefined URI/URL values require simple nodes, matching
Java's current validators without parsing the URI/URL text. Extension XPath
properties require a simple node whose expression compiles. Other predefined
array properties validate their RDF array form and recursively validate children
when the child type has a ported built-in scalar validator; unknown child types
remain unknown. TIFF `YCbCrSubSampling` and EXIF `ComponentsConfiguration`
validate their ordered integer sequences; known closed-choice values pass,
structurally valid but unlisted values remain unknown because the Java
closed-choice mode is unavailable, and malformed sequences fail. The predefined
TIFF `Compression`, `PhotometricInterpretation`, `Orientation`,
`PlanarConfiguration`, `ResolutionUnit`, and `YCbCrPositioning` integer
properties likewise validate known choices and preserve unknown for other
well-formed integers. EXIF `ExposureProgram`, `MeteringMode`,
`FocalPlaneResolutionUnit`, `SensingMethod`, `FileSource`, `SceneType`,
`CustomRendered`, and `ExposureMode` validate their integer choices, preserving
unknown for other well-formed integers. The predefined
`dimensions` structure checks the `w`, `h`, and
`unit` fields using their Java-declared types and rejects unknown fields or
namespaces. Other predefined value-type checks and unsupported namespaces
remain explicit limitations. Main-package membership checks use the catalog's
XMP extension declarations when a reader provides catalog context; an absent
catalog packet yields false, while unavailable or invalid context remains
unknown. Catalog XML metadata is exposed and identified separately from page
metadata. The predefined `job`
structure validates its `name`, `id`, and `url` fields in the Job namespace.
The predefined `font` structure checks its declared scalar fields and nested
`childFontFiles` sequence in the Font namespace. The predefined
`resourceevent` structure checks its `action`, `instanceID`, `parameters`,
`softwareAgent`, and `when` fields in the ResourceEvent namespace. The
predefined `version` structure checks its fields and recursively validates the
nested `event` as a ResourceEvent. The predefined `resourceref` structure checks
its URI, text, URL-like, agent-name, and rendition-class fields in the
ResourceRef namespace. Predefined thumbnails validate the RDF alternate-array
shape and profile-independent fields; non-`JPEG` `format` results remain unknown
because the Java validator's result depends on the closed-choice setting.
The predefined Dynamic Media `time` structure validates its `value` integer and
`scale` rational fields. The predefined Dynamic Media `media` structure checks
its URI, text, time, Boolean, and web-statement fields. The predefined
`beatsplicestretch` structure checks its Boolean, Real, and nested Time fields.
The predefined `timescalestretch` structure checks its Real fields and
preserves unknown results for profile-dependent `quality` values.
The predefined `resamplestretch` structure checks its structure and known
`quality` choices, preserving unknown results for other simple-text choices.
The predefined Dynamic Media `marker` structure validates its `ANY`, text,
and URI fields and preserves unknown results for profile-dependent `type`
values.
The predefined Dynamic Media `projectlink` structure validates its URI `path`
and preserves unknown results for profile-dependent `type` values.
The predefined Dynamic Media `timecode` structure validates its
profile-independent structure shape and preserves unknown results for
profile-dependent time-value patterns and format choices.
The predefined XMP Graphics `colorant` structure validates scalar types and
preserves unknown results when restricted-field choices or ranges may vary by
profile.
The `locale` value type validates simple text recursively in its declared
arrays, including `dc:language`.
The EXIF `gpscoordinate` type accepts values valid under both PDF/A profile
patterns and preserves unknown results when only one profile pattern matches.
Extension properties declared in the current package now report as defined
independently of whether their value type is supported.
The EXIF `flash` structure validates its common Boolean fields and preserves
unknown results for profile-dependent `Return` and `Mode` choices.
The EXIF `cfapattern` structure validates integer `Columns` and `Rows` fields
and recursively validates `Values` as a sequence of integers.
The EXIF `oecf/sfr` structure validates integer dimensions and recursively
validates its `Names` and `Values` sequences.
The EXIF `devicesettings` structure validates integer dimensions and
recursively validates `Settings` as a sequence of text.
Decode or XML parse failures produce an invalid package.
`CosStreamModelAdapter` exposes dictionary links, a direct numeric `/Length`,
stored raw-byte length, and ordered `CosFilter` links for name/array `/Filter`
values. Filter decode parameters classify as `Default`, `Custom`, or the
`Crypt` filter's `/Name` value; references resolve when a reader is supplied.
Malformed filter forms fail explicitly. Parsed streams retain keyword-spacing
evidence; programmatically constructed streams report unknown spacing.
External-file `F` and `FDecodeParms` fields use the reference COS string
projection, while `FFilter` names are joined in declaration order; indirect
values resolve when a reader is supplied.

The library currently embeds and reads the 15 profile XML files, can write
the represented profile fields, filter by tags, and represent assertion and
validation result data. Result construction requires an explicit compliance
value instead of inferring success from a potentially incomplete list;
stripping passed assertions retains failed-check aggregates. Validator
configuration defaults are represented as data but are not yet applied to a
validator. XMP's namespace and standard-alias registry, path expansion and path
construction, partial-date parsing and rendering, Boolean, integer, long,
date and Base64 scalar conversions, and double parsing are available. An unnormalized XMP packet
reader supports a limited RDF subset and rejects unsupported forms explicitly;
it includes attribute structures, typed resources and qualified values, with
non-language qualifiers exposed separately from `xml:lang`; the legacy Dublin
Core namespace is mapped to its current URI. RDF node identities are validated
for mutual exclusion, and `rdf:ID` and `rdf:nodeID` on descriptions and nested
resources are ignored rather than retained. Read-only path lookup supports
fields, qualifiers, array selectors and alias targets on the raw tree, without
creating missing targets. Array items can be counted and accessed by index.
Immutable array-item replacement and insertion operate on existing arrays,
accept one-based indexes, and create literal items. Replacement discards the
old item's structure and qualifiers; insertion supports appending with -1.
These operations do not repair alt-text arrays and reject null values.
Immutable literal append supports existing Bag, Seq and Alt arrays and creates
a top-level array or a field of an existing structure when an explicit kind is
provided. Duplicate values are retained. Unlike the reference, an explicitly
specified kind must match an existing array, missing intermediate nodes are
not created, and null values are rejected. Alt-text arrays are not repaired.
Array-item replacement, insertion and append also support empty structure,
Bag, Seq and Alt items, which can subsequently be populated via paths. Composite
items are not valid alt-text items, and the operations do not repair alt arrays.
URI-valued array items can likewise be replaced, inserted and appended, with
the same creation and index rules. Replacement discards the old item and its
qualifiers; URI syntax is not validated and alt-text is not repaired.
Simple Bag and Seq items can be catenated with the reference's separator and
quoting behavior, including its embedded-quote quirk. Edit strings can be
separated into distinct values using the reference's quote, separator and comma
rules. An immutable append operation updates existing Bags and Seqs or creates
an array at the top level or as a field of an existing structure. It preserves
existing array items and does not create missing intermediate nodes. An explicit
form must match an existing array, rather than reproducing the reference's
inverted form check. General metadata mutation is not yet implemented.
Structure fields and qualifiers can be accessed by namespace and name.
An empty packet can be created without parsing XML, and immutable composite
creation supports structures and Bag, Seq and Alt arrays at top level or as
fields of existing structures. Existing composites of the same kind are
preserved instead of being cleared by the reference setter; different kinds
and missing intermediate nodes are rejected. Composite qualifiers and array
items cannot be created by this operation.
Immutable string-property updates support existing simple nodes and creation
of top-level properties, fields of existing structures, and qualifiers. ASCII
controls are replaced with spaces and language qualifiers are normalized.
Unlike the reference setter, missing intermediate nodes and array items are
not created, null values and composite targets are rejected, and alt-text
arrays are not normalized or synchronized.
Immutable URI-property updates share these path rules, convert existing
literal nodes to URI nodes, retain qualifiers, and reject language qualifier
targets. They do not validate URI syntax, and string updates retain URI kinds.
Named RDF/XML qualifiers remain subject to the writer's restrictions.
Typed immutable Boolean, integer, long, date and Base64 updates reuse the
string-property path rules and scalar formatting. Empty dates serialize to an
empty string, which the date getter rejects; null date and byte-array inputs
are rejected. Typed double updates use invariant shortest-round-trip digits
with Java-style decimal and exponent notation, but rare values may differ from
Java's `Double.toString`.
Immutable property removal supports expanded paths, array items, qualifiers and
aliases without pruning empty parent properties; unlike upstream it reports
invalid paths. Removing qualifiers does not normalize or repair alt-text arrays.
Qualifier lookup uses namespace URIs even where the Java parser retains an
attribute's source prefix and may fail to resolve a differently registered prefix.
Typed property getters support strings, booleans, integers, longs, doubles,
dates and Base64 values.
Read-only packet, schema and property iteration supports children-only, leaf-only,
leaf-name and qualifier-omission options. Property-, packet- and schema-scoped
cursors can skip the current subtree or the remaining siblings at the same
level. This follows the documented skip semantics, not the reference's unused
subtree-skip flag.
Iterator display paths follow Java's conventions and are not always usable for lookup.
Localized text selection supports language fallback
and applies the reference's default-item adjustments without mutating the raw
tree. Immutable localized-text updates support exact and generic matching,
x-default synchronization, creation of missing top-level arrays or fields of
existing structures, and moving x-default first. All items are validated;
null values and missing intermediate nodes are rejected, and ASCII control
characters in new values are replaced with spaces. An empty or fully
language-qualified alternative array is treated as alt-text because the raw
model does not retain the reference's alt-text flag. An opt-in discovery reader searches nested XML,
supports requiring an XMP wrapper, and preserves packet header instructions;
the original strict reader still rejects unsupported roots. String discovery
optionally retries after repairing ASCII controls and numeric control references.
Stream discovery can also opt into control repair and the reference's
mixed UTF-8/Windows-1252 fallback, buffering input for retries. The reader does
not implement full RDF or alias normalization. A restricted RDF/XML writer
supports typed-resource `rdf:type` qualifiers and the represented raw tree
except other RDF/XML named qualifiers. Optional
output settings create writable/read-only packet wrappers, use explicit padding
or an exact byte length, and omit `x:xmpmeta`. Output is UTF-8 by default;
UTF-16BE and UTF-16LE are also supported without an external byte-order mark,
and require even padding and exact byte lengths. The existing unwrapped
default is unchanged, and wrapped output defaults to zero padding. Optional
thumbnail padding adds 10,000 encoding units when `xmp:Thumbnails` is absent;
it requires a writable wrapper and cannot be combined with exact length.
Optional indentation and configurable XML-whitespace line breaks are supported.
An opt-in sort orders schemas, properties, structure fields and non-special
qualifiers without modifying the tree or reordering arrays. Generated prefixes
are assigned by namespace URI when sorting, so field and qualifier order can
differ from upstream registry-prefix ordering.
Wrapped
output is buffered, and input packet instructions are not preserved. Other
serializer options remain unimplemented. Fixed-offset
`DateTimeOffset` creation and UTC, selected-time-zone and local-time conversion
for complete valid zoned dates are available at millisecond precision. Date
comparison for this same supported subset orders instants at nanosecond
precision. Immutable factories create empty, date-only and unzoned date/time
values with component clamping and validated nanoseconds; fixed offsets can be
assigned or removed without changing local components. Lenient calendar conversion and mutable dates are not implemented. Rare
double-formatting cases can differ from Java's `Double.toString`.
Base64 decoding is intentionally permissive
like the Java reference. It has not ported
the complete upstream profile API, the rest of XMP, rule
execution, parser-backed model implementations, features, metadata repair,
policy, processing or reports. It does not include the other resources.
A Java-generated fixture covers all default XMP namespace and alias mappings;
other fixtures cover representative path expansions, path construction,
date parsing/rendering and UTC conversion, scalar conversions, raw RDF packets with qualified values,
path lookup, localized text selection, packet discovery, string and stream recovery,
and error codes. Other Java differential tests and the comprehensive compatibility inventory
remain to be established. Building and testing the port does not require Java.

Do not treat this preliminary inspection or a successful package build as
evidence of validation or full reference parity.
