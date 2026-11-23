namespace VeraPdf.Parser.Io;

/// <summary>
/// A random-access PDF stream that is independent of the stream originally supplied by the caller.
/// </summary>
public abstract class PdfSeekableStream : Stream
{
    public const int DefaultMemoryThreshold = 10240;

    public static long? MaximumStreamSize { get; set; }
    public static int MemoryThreshold { get; set; } = DefaultMemoryThreshold;

    public abstract long Offset { get; }
    public abstract long StreamLength { get; }
    public abstract long AbsoluteOffset { get; }

    public abstract void Seek(long offset);
    public override long Seek(long offset, SeekOrigin origin)
    {
        var target = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Offset + offset,
            SeekOrigin.End => StreamLength + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };
        Seek(target);
        return Offset;
    }
    public abstract void Unread();
    public abstract int Peek();
    public abstract PdfSeekableStream GetSubstream(long startOffset, long length);

    public int ReadByteOrThrow()
    {
        var value = ReadByte();
        if (value < 0)
        {
            throw new EndOfStreamException("End of PDF stream reached.");
        }
        return value;
    }

    public bool IsEndOfStream => Offset >= StreamLength;

    public static PdfSeekableStream Create(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);

        using var copy = new MemoryStream();
        source.CopyTo(copy);
        var bytes = copy.ToArray();

        if (MaximumStreamSize is > 0 && bytes.LongLength > MaximumStreamSize.Value)
        {
            throw new PdfResourceLimitExceededException(
                $"A PDF stream exceeds the configured {MaximumStreamSize.Value} byte limit.");
        }

        var threshold = MemoryThreshold > 0 ? MemoryThreshold : DefaultMemoryThreshold;
        if (bytes.LongLength <= threshold)
        {
            return new MemoryPdfSeekableStream(bytes);
        }

        var temp = new TempFilePdfSeekableStream(bytes);
        temp.Retain();
        return temp;
    }
}
