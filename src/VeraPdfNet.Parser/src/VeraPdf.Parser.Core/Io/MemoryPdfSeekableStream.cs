namespace VeraPdf.Parser.Io;

public sealed class MemoryPdfSeekableStream : PdfSeekableStream
{
    private readonly byte[] _bytes;
    private long _offset;

    public MemoryPdfSeekableStream(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        _bytes = bytes.ToArray();
    }

    public override long Offset => _offset;
    public override long StreamLength => _bytes.LongLength;
    public override long AbsoluteOffset => _offset;
    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length => StreamLength;
    public override long Position
    {
        get => _offset;
        set => Seek(value);
    }

    public override void Seek(long offset)
    {
        if (offset < 0 || offset > StreamLength)
        {
            throw new IOException($"Cannot seek to PDF offset {offset}.");
        }
        _offset = offset;
    }

    public override void Unread()
    {
        if (_offset == 0)
        {
            throw new IOException("Cannot unread at the start of a PDF stream.");
        }
        _offset--;
    }

    public override int Peek()
    {
        if (IsEndOfStream)
        {
            return -1;
        }
        return _bytes[_offset] & 0xff;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(offset);
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        if (buffer.Length - offset < count)
        {
            throw new ArgumentException("The destination buffer is too small.", nameof(buffer));
        }

        var available = (int)Math.Min(count, StreamLength - _offset);
        if (available == 0)
        {
            return 0;
        }
        Array.Copy(_bytes, _offset, buffer, offset, available);
        _offset += available;
        return available;
    }

    public override int Read(Span<byte> buffer)
    {
        var available = (int)Math.Min(buffer.Length, StreamLength - _offset);
        _bytes.AsSpan((int)_offset, available).CopyTo(buffer);
        _offset += available;
        return available;
    }

    public override int ReadByte()
    {
        if (IsEndOfStream)
        {
            return -1;
        }
        return _bytes[_offset++] & 0xff;
    }

    public override PdfSeekableStream GetSubstream(long startOffset, long length)
    {
        if (startOffset < 0 || length < 0 || startOffset > StreamLength || length > StreamLength - startOffset)
        {
            throw new IOException("Substream range is outside the PDF stream.");
        }

        var bytes = new byte[length];
        Array.Copy(_bytes, startOffset, bytes, 0, length);
        return new MemoryPdfSeekableStream(bytes);
    }

    public override void Flush()
    {
    }

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
}
