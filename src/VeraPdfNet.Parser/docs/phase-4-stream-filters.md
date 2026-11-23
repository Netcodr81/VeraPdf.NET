# Phase 4: Stream Filters

Phase 4 adds bounded stream decoding to the native C# parser.

## Implemented components

- filter registry through `PdfStreamFilterService`;
- ASCIIHex decoding;
- ASCII85 decoding;
- RunLength decoding;
- zlib and raw DEFLATE decoding;
- LZW decoding with `/EarlyChange`;
- PNG and TIFF predictor processing;
- bounded decoded output;
- expansion-ratio and absolute-size checks;
- `IPdfStreamDecryptor` extension point.

## Filter behavior

When `/Filter` is a name, one filter is applied. When it is an array, filters
are applied in array order. `/DecodeParms` is accepted as either a dictionary
or an array.

Malformed or unsupported filters return typed `IOException` messages. DCT,
JPX, and JBIG2 image encoding remain explicitly unsupported until the relevant
image format is implemented in a later phase.

## Bounds

`PdfStreamDecodeOptions` supports:

- `MaximumDecodedBytes`;
- `MaximumExpansionRatio`.

The default decoded-stream cap is 256 MB. A stream that exceeds the absolute
cap or a positive expansion-ratio limit raises
`PdfResourceLimitExceededException`.

## Encryption

`IPdfStreamDecryptor` defines the decryption boundary for strings and streams.
This interface allows Phase 5 or a dedicated security task to add standard
PDF security handlers without mixing cryptography into the filter registry.

Password handling and RC4/AES stream decryption are not claimed as complete
by Phase 4.

## Tests

Phase 4 tests cover:

- ASCIIHex whitespace and odd-nibble behavior;
- ASCII85 `z` expansion;
- RunLength literal and repeat runs;
- zlib Flate round trips;
- PNG-style Sub predictor rows;
- decoded-size limits.
