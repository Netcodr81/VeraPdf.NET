# veraPDF Library Integration Port Checkpoint

## Current status

The Java `veraPDF-library-integration` port to `VeraPdfNet.Library` is not
complete. This checkpoint covers the in-progress XMP metadata/model-adapter
work only. Continue in small, tested increments and stop for user approval
between chunks.

## Completed in the current XMP work

- Parse COS metadata XMP streams and expose packet/header information and raw
  XMP property nodes.
- Expose PDF/A extension-schema containers and schema, property, value-type,
  and field definitions, including supported child/value checks.
- Identify properties declared in the current packet with the built-in
  extension `Text` type.
- Check literal/URI node kinds for predefined and extension properties whose
  declared type is built-in `Text`; preserve unknown results for other types.
- Validate predefined `lang alt` properties as alternate arrays with language
  tags, allowing empty alternate arrays.
- Validate predefined Boolean, Integer, and Real scalar values using the
  corresponding Java regular-expression semantics.
- Validate predefined URI/URL values as simple nodes; validate extension
  XPath values as simple nodes with compilable XPath expressions.
- Recognize predefined XMP property membership and declared types for Core XMP,
  Dublin Core, XMP Rights, XMP Media Management, XMP Basic Job, XMP Paged Text,
  PDF/A identification, Adobe PDF, Photoshop, TIFF, EXIF, and EXIF Auxiliary.
- Cover known profile-specific membership for EXIF properties, PDF/A
  identification `corr`, XMP Basic `Label`/`Rating`, XMP Paged Text
  `Fonts`/`Colorants`/`PlateNames`, and EXIF Auxiliary `Lens`/`SerialNumber`.
- Recognize Photoshop `SupplementalCategories` in both profiles; its declared
  type remains unknown because it differs by profile and this adapter has no
  PDF/A flavour context.
- EXIF `GPSMeasureMode` is predefined in both profiles, but its declared type
  also differs between profiles and remains unknown without flavour context.
- EXIF `ColorSpace` and `LightSource` are predefined with integer types in both
  profiles; profile-specific closed-choice value validation is not implemented.
- All 41 Camera Raw properties listed in the reference schema are recognized
  and PDF/A-2/3-only. Choice validation for `WhiteBalance` and `ToneCurve`
  remains unsupported.
- All 57 Dynamic Media properties listed in the reference schema are recognized
  and PDF/A-2/3-only. Restricted-value checks for its choice properties are
  unsupported.
- Keep unsupported or flavour-dependent answers explicit rather than
  fabricating results.

The latest completed increment added PDF/A-2/3-only XMP Paged Text properties
`Fonts` (`bag font`), `Colorants` (`seq colorant`), and `PlateNames` (`seq text`),
alongside the common `MaxPageSize` (`dimensions`) and `NPages` (`integer`).
It also added the PDF/A-2/3-only EXIF Auxiliary properties `Lens` and
`SerialNumber`, both `text`, and recognizes Photoshop
`SupplementalCategories` membership in both profiles. Its declared type is
profile-dependent and remains unknown here. Corrected EXIF `GPSMeasureMode`
membership to both profiles and left its profile-dependent declared type
unknown. Added EXIF `ColorSpace` and `LightSource` membership/type metadata;
their profile-dependent value restrictions remain unvalidated. Added initial
Camera Raw coverage for `AutoBrightness`, `AutoContrast`, `AutoExposure`,
`AutoShadows`, `BlueHue`, `BlueSaturation`, `Brightness`, `CameraProfile`,
`ChromaticAberrationB`, `ChromaticAberrationR`, `ColorNoiseReduction`,
`Contrast`, `CropTop`, `CropLeft`, `CropBottom`, `CropRight`, `CropAngle`,
`CropWidth`, `CropHeight`, `CropUnits`, `Exposure`, `GreenHue`,
`GreenSaturation`, `HasCrop`, `HasSettings`, and `LuminanceSmoothing`;
`RawFileName`, `RedHue`, `RedSaturation`, `Saturation`, `Shadows`,
`ShadowTint`, `Sharpness`, `Temperature`, `Tint`, `ToneCurveName`, `Version`,
`VignetteAmount`, `VignetteMidpoint`, `WhiteBalance`, and `ToneCurve`.
Closed-choice and sequence-of-points validation for the last two remains
unsupported. Started Dynamic Media coverage for `artist`, `album`, `trackNumber`,
`genre`,
`copyright`, `releaseDate`, `composer`, `engineer`, `videoFrameRate`,
`videoFrameSize`, `videoPixelAspectRatio`, `videoAlphaUnityIsTransparent`,
`videoAlphaPremmultipleColor`, `videoCompressor`, `audioSampleRate`,
`audioCompressor`, `speakerPlacement`, `fileDataRate`, `tapeName`,
`altTapeName`, `startTimecode`, `altTimecode`, `duration`, `scene`, `shotName`,
`shotDate`, `shotLocation`, `logComment`, `markers`, `contributedMedia`,
`absPeakAudioFilePath`, `relativePeakAudioFilePath`, `videoModDate`,
`audioModDate`, `metadataModDate`, `tempo`, `instrument`, `introTime`,
`outCue`, `relativeTimestamp`, `loop`, `numberOfBeats`, `timeScaleParams`,
`resampleParams`, `beatSpliceParams`, `projectRef`, `videoPixelDepth`,
`videoColorSpace`, `videoAlphaMode`, `videoFieldOrder`, `pullDown`,
`audioSampleType`, `audioChannelType`, `key`, `stretchMode`, `timeSignature`,
and `scaleType`. Choice validation remains unsupported.
`dotnet test
.\src\VeraPdfNet.Library\tests\VeraPdf.Library.Tests\VeraPdf.Library.Tests.csproj
--no-restore` passed: 412 passed, 0 failed.

The latest increment implemented simple-node correctness for predefined
`Text` properties, preserving unknown results for non-Text types and
profile-dependent declarations. Dublin Core `coverage` tests reject an array
as non-simple; Photoshop `Headline` tests accept a literal. The full library
test suite passed: 409 passed, 0 failed.
The next increment ported the Java `LangAltValidator` semantics for predefined
`lang alt` types: accept alternate arrays with at least one language-tagged
item, as well as empty alternate arrays; reject other node shapes. Tests cover
both valid forms and an alternate array without language tags. The full library
test suite passed again: 409 passed, 0 failed.
The latest increment ports Java's exact Boolean (`True`/`False`), decimal
Integer, and Real lexical validators for predefined scalar properties. Tests
cover accepted and rejected values for all three types. The full library suite
passed: 409 passed, 0 failed.
The next increment ported the predefined MIME type validator. It preserves
Java's ASCII token character set and checks that the value contains exactly
one slash with non-empty tokens on both sides. Tests cover a valid MIME type,
parameters (rejected by the Java validator), and non-ASCII token characters.
The full library test suite passed: 412 passed, 0 failed.
The latest increment ports Java's URI/URL simple-node checks for predefined
properties and its XPath compilation check for extension-declared `XPath`
types. URI/URL text is intentionally not parsed, matching Java's active
validators. Tests cover simple and composite URI values plus valid and invalid
XPath expressions. The full library suite passed: 412 passed, 0 failed.
The next increment ports Java's predefined Date validator using the existing
XMP ISO 8601 parser. It checks simple nodes without normalizing their source
values. Tests cover a partial date, a full date, and malformed input. The full
library suite passed: 412 passed, 0 failed.
The latest increment ports the remaining built-in scalar validators whose Java
rules are unconstrained simple-node checks: `ProperName`, `AgentName`, `Rational`,
and `RenditionClass`. Predefined and extension declarations now share one
built-in type dispatch. Tests cover the four scalar types and an extension
`Rational` value; the full library suite passed: 412 passed, 0 failed.
The next increment ports Java's RDF array-form check and recursive child
validation for array declarations whose child types have supported scalar
validators. Tests cover a valid `seq propername`, wrong array form, a composite
child where simple text is required, and an empty array. Unsupported child types
remain unknown. The full library suite passed: 412 passed, 0 failed.
The latest increment ports Java's predefined `dimensions` structured type for
`xmpTPg:MaxPageSize`: only `w`/`h` real values and a `unit` text value in the
Dimensions namespace are accepted, while unknown fields are rejected. Tests
cover a valid structure, invalid numeric text, and a field in the wrong
namespace. The full library suite passed: 414 passed, 0 failed.
The latest increment ports Java's predefined `job` structured type used by
`xmpBJ:JobRef`, including its `name`/`id` text and simple `url` fields. Array
validation now checks each Job structure item. Tests cover a valid Job and
reject an unknown field and a composite `name`. The full library suite passed:
416 passed, 0 failed.
The latest increment ports Java's predefined `font` structured type used by
`xmpTPg:Fonts`, including its text and Boolean fields and nested
`childFontFiles` sequence. Tests cover a valid Font and reject an unknown field
and an invalid Boolean. The full library suite passed: 418 passed, 0 failed.
The latest increment ports Java's predefined `resourceevent` structured type
used by XMP Media Management history, including its simple text, URI, agent
name, and date fields. Tests cover a valid event and reject an unknown field
and an invalid date. The full library suite passed: 420 passed, 0 failed.
The latest increment ports Java's predefined `version` structured type used by
`xmpMM:Versions`, recursively validating its nested ResourceEvent along with
its text, date, and proper-name fields. Tests cover a valid nested Version,
unknown fields, and an invalid nested event. The full library suite passed:
422 passed, 0 failed.
The latest increment ports Java's predefined `resourceref` structured type,
including its URI, text, agent-name, and rendition-class fields. Tests cover a
valid `xmpMM:DerivedFrom` reference and reject an unknown field and a composite
URI value. The full library suite passed: 424 passed, 0 failed.
The latest increment inspected Java's restricted-field handling and added
profile-independent validation for `xmp:Thumbnails`: array form and image
fields are checked, and `format=JPEG` passes both Java modes. Other format
values remain `null` because the Java closed-choice option is not available in
this adapter. Tests cover a valid thumbnail, an unknown field, and a
profile-dependent format. The full library suite passed: 426 passed, 0 failed.
The latest increment ports Java's profile-independent Dynamic Media `time`
structure used by properties such as `xmpDM:duration`. It validates `value` as
an integer and `scale` as a simple rational node. Tests cover a valid value,
invalid integer text, and an unknown field. The full library suite passed:
428 passed, 0 failed.
The latest increment ports Java's predefined Dynamic Media `media` structured
type used by `xmpDM:contributedMedia`. Its URI, text, time, Boolean, and
web-statement fields are validated, including nested Time structures. Tests
cover a valid Media and reject an unknown field and invalid Boolean text. The
full library suite passed: 430 passed, 0 failed.
The current increment ports the predefined Dynamic Media `beatsplicestretch`
structure used by `xmpDM:beatSpliceParams`. It validates `useFileBeatsMarker`
as Boolean, `riseInDecibel` as Real, and `riseInTimeDuration` as a nested Time
structure. Four focused cases passed: one valid structure and three invalid
field cases. The full library suite passed: 434 passed, 0 failed.
The latest increment ports the profile-independent fields of Dynamic Media
`timescalestretch` used by `xmpDM:timeScaleParams`. It validates
`frameSize` and `frameOverlappingPercentage` as Real, accepts the closed-choice
values `High`, `Medium`, and `Low`, and preserves `null` for other simple-text
`quality` values because the active profile mode is unavailable. Four focused
cases and the full library suite passed (438 passed, 0 failed).
The latest increment ports Dynamic Media `resamplestretch` used by
`xmpDM:resampleParams`. It validates the structure, accepts the closed-choice
values `High`, `Medium`, and `Low`, rejects unknown fields and non-text
`quality`, and preserves `null` for other simple-text choices. Four focused
cases and the full library suite passed (442 passed, 0 failed).
The latest increment ports Dynamic Media `marker` validation, including
recursive validation of the `xmpDM:markers` sequence. Its `startTime` and
`duration` fields follow Java's `ANY` behavior; text/URI fields are checked,
and `type` accepts the common closed-choice values while preserving `null` for
other simple-text values. Four focused cases and the full suite passed (446
passed, 0 failed).

The latest increment ports Dynamic Media `projectlink` used by
`xmpDM:projectRef`. It validates `path` as URI, accepts `movie`, `still`,
`audio`, and `custom` for `type`, and preserves `null` for other simple-text
type values because profile mode is unavailable. Four focused cases and the
full suite passed (450 passed, 0 failed).
The latest increment ports Dynamic Media `timecode` validation for
`startTimecode` and `altTimecode`. It accepts known time-value formats and
timecode format choices, returns `null` for simple-text values whose
restricted-field outcome depends on profile mode, and rejects unknown fields
or non-text restricted values. Four focused cases and the full suite passed
(454 passed, 0 failed).
The latest increment ports XMP Graphics `colorant` validation, including
recursive validation of `xmpTPg:Colorants`. It checks scalar field types,
validates colorant choices and ranges, and preserves `null` when the values
are type-correct but profile-dependent restrictions may differ. Five focused
cases and the full suite passed (459 passed, 0 failed).
The latest increment ports Java's simple `locale` value type and validates
`dc:language` as a Bag of simple locale values. Tests cover a valid Bag, the
wrong array form, and a structured child. All 462 library tests passed.
The latest increment ports the EXIF `gpscoordinate` simple type. Values
matching both PDF/A profile patterns pass, values matching only one pattern
remain unknown, and values matching neither fail. Three focused cases and the
full suite passed (465 passed, 0 failed).

The latest increment ports the EXIF `flash` structure. Common fields are
validated as Booleans, allowed `Return` and `Mode` values pass, and
profile-dependent restricted values remain `null`. Four focused cases and the
full suite passed (469 passed, 0 failed).
The latest increment ports EXIF `cfapattern` validation. `Columns` and `Rows`
are checked as integers, and `Values` is recursively checked as a sequence of
integers. Four focused cases and the full suite passed (473 passed, 0 failed).
The latest increment ports EXIF `oecf/sfr` validation: `Columns` and `Rows`
must be integers, `Names` a sequence of text, and `Values` a sequence of
rational simple nodes. Four focused cases and the full suite passed (477
passed, 0 failed).
The latest increment ports EXIF `devicesettings`: `Columns` and `Rows` are
integers, and `Settings` is a sequence of text. Four focused cases and the
full suite passed (481 passed, 0 failed).
The latest increment aligns `GetisDefinedInCurrentPackage` with Java:
declaration presence reports `true` independently from type validation.
Regression coverage confirms an unsupported declared type is defined while
its value-type result remains `null`. The full suite passed (481 passed, 0
failed).

The latest increment ports `GetisDefinedInMainPackage` with catalog context.
Catalog XML metadata is now exposed and identified, and properties in other
metadata packets are checked against the catalog packet's extension schemas.
Known absent declarations report false; unavailable or invalid catalog context
remains unknown. A regression test covers the main packet, a matching and
non-matching page property, and an unscoped packet. The full suite passed
(482 passed, 0 failed).
The following increment aligns extension `GetpredefinedType` results with
Java: any declared type name is returned even if the corresponding validator
is unsupported. Coverage includes `Text`, `XPath`, `Rational`, and an
unsupported declared type. The full suite passed (482 passed, 0 failed).

The latest increment ports TIFF `YCbCrSubSampling` and EXIF
`ComponentsConfiguration` sequence-choice validation: allowed sequences pass,
invalid sequence shape or integer values fail, and other valid integer
sequences remain unknown because Java's closed-choice mode is unavailable.
Ten focused cases and the full suite passed (492 passed, 0 failed).

The latest increment ports TIFF `ResolutionUnit` and `YCbCrPositioning`
restricted integer choices. Listed values pass, unlisted valid integers remain
unknown because closed-choice mode is unavailable, and malformed integer
values fail. Ten focused cases and the full suite passed (502 passed, 0 failed).

The latest increment ports TIFF `Compression`, `PhotometricInterpretation`,
`Orientation`, and `PlanarConfiguration` restricted integer choices. Listed
values pass, unlisted valid integers remain unknown, and malformed integer
values fail. Seventeen focused cases and the full suite passed (519 passed, 0
failed).

The latest increment ports EXIF `ExposureProgram`, `MeteringMode`,
`FocalPlaneResolutionUnit`, and `SensingMethod` restricted integer choices.
Allowed values pass, unlisted valid integers remain unknown because Java's
closed-choice mode is unavailable, and malformed values fail. Nineteen focused
cases and the full suite passed (538 passed, 0 failed).

The latest increment ports EXIF `FileSource`, `SceneType`, `CustomRendered`,
and `ExposureMode` restricted integer choices. Allowed values pass, unlisted
valid integers remain unknown, and malformed values fail. Fourteen focused
cases and the full suite passed (552 passed, 0 failed).

## Recommended next increment

Port the next EXIF restricted integer choices: `WhiteBalance`,
`SceneCaptureType`, `GainControl`, and `Contrast`.

## Known limitations to preserve

- Extension property value validation is implemented for supported built-in
  scalar types and arrays with supported scalar child types; structured and
  otherwise unsupported extension types can remain unknown. The predefined
  `dimensions`, `job`, `font`, `resourceevent`, `version`, and `resourceref`
  and Dynamic Media `time`, `media`, `beatsplicestretch`, `timescalestretch`,
  `resamplestretch`, `marker`, `projectlink`, `timecode`, `colorant`, and
  `flash`, `cfapattern`, `oecf/sfr`, and `devicesettings` structures are
  validated. The
  `locale` and `gpscoordinate` scalar types are also validated. Thumbnail,
  TimeScaleStretch, ResampleStretch, Marker, ProjectLink, Timecode, and Colorant
  field validation is profile-aware by preserving `null` for conflicting
  closed-choice outcomes. Flash field validation is profile-aware by
  preserving `null` for restricted outcomes.
- Predefined value-type checks outside the explicitly listed scalar and
  structured types remain incomplete.
- Extension property/value-type/field correctness may depend on PDF/A flavour
  or a type registry unavailable to these adapters; preserve `null` where the
  answer cannot be determined.
- Photoshop `SupplementalCategories` has a profile-dependent declared type.
  Do not report one type as universally correct without flavour context.
- Do not add runtime references to the Java source tree; keep the C# port
  self-contained.

## Working files and validation

- Implementation: `src\VeraPdfNet.Library\src\VeraPdf.Library.Core\Validation\Gf\Model\Cos\CosXmpPackageModelAdapter.cs`
- Tests: `src\VeraPdfNet.Library\tests\VeraPdf.Library.Tests\CosDictionaryModelAdapterTests.cs`
- Progress notes: `src\VeraPdfNet.Library\docs\port-status.md`
- Last verified command:
  `dotnet test .\src\VeraPdfNet.Library\tests\VeraPdf.Library.Tests\VeraPdf.Library.Tests.csproj --no-restore`

No commit was made. At checkpoint time, the implementation, test, and status
documentation files above were untracked in Git; preserve them and any other
worktree changes when resuming.
