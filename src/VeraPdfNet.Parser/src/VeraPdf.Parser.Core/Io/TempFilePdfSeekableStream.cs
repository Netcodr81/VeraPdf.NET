namespace VeraPdf.Parser.Io;

internal sealed class TempFileOwner : IDisposable
{
    private readonly object _sync = new();
    private int _references;

    public string Path { get; }
    public TempFileOwner(string path) => Path = path;

    public void Retain()
    {
        lock (_sync)
        {
            _references++;
        }
    }

    public void Release()
    {
        var remove = false;
        lock (_sync)
        {
            _references--;
            remove = _references <= 0;
        }

        if (remove && File.Exists(Path))
        {
            File.Delete(Path);
        }
    }

    public void Dispose() => Release();
}

public sealed class TempFilePdfSeekableStream : PdfSeekableStream
{
    private readonly TempFileOwner _owner;
    private readonly FileStream _stream;
    private readonly long _start;
    private readonly long _length;
    private bool _disposed;

    internal TempFilePdfSeekableStream(byte[] bytes)
    {
        var path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"verapdf-{Guid.NewGuid():N}.tmp");
        File.WriteAllBytes(path, bytes);
        _owner = new TempFileOwner(path);
        _stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        _start = 0;
        _length = bytes.LongLength;
    }

    private TempFilePdfSeekableStream(TempFileOwner owner, FileStream stream, long start, long length)
    {
        _owner = owner;
        _stream = stream;
        _start = start;
        _length = length;
    }

    internal void Retain() => _owner.Retain();

    public override long Offset => Position;
    public override long StreamLength => _length;
    public override long AbsoluteOffset => _start + Position;
    public override bool CanRead => true;
    public override bool CanSeek => true;
    public override bool CanWrite => false;
    public override long Length => _length;
    public override long Position
    {
        get
        {
            EnsureOpen();
            return _stream.Position - _start;
        }
        set => Seek(value);
    }

    public override void Seek(long offset)
    {
        EnsureOpen();
        if (offset < 0 || offset > _length)
        {
            throw new IOException($"Cannot seek to PDF offset {offset}.");
        }
        _stream.Position = _start + offset;
    }

    public override void Unread()
    {
        if (Position == 0)
        {
            throw new IOException("Cannot unread at the start of a PDF stream.");
        }
        _stream.Position--;
    }

    public override int Peek()
    {
        EnsureOpen();
        if (IsEndOfStream)
        {
            return -1;
        }
        var value = _stream.ReadByte();
        _stream.Position--;
        return value;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        EnsureOpen();
        var available = (int)Math.Min(count, _length - Position);
        if (available == 0)
        {
            return 0;
        }
        return _stream.Read(buffer, offset, available);
    }

    public override int Read(Span<byte> buffer)
    {
        EnsureOpen();
        var available = (int)Math.Min(buffer.Length, _length - Position);
        return _stream.Read(buffer[..available]);
    }

    public override int ReadByte()
    {
        EnsureOpen();
        if (IsEndOfStream)
        {
            return -1;
        }
        return _stream.ReadByte();
    }

    public override PdfSeekableStream GetSubstream(long startOffset, long length)
    {
        if (startOffset < 0 || length < 0 || startOffset > _length || length > _length - startOffset)
        {
            throw new IOException("Substream range is outside the PDF stream.");
        }

        var substream = new FileStream(_owner.Path, FileMode.Open, FileAccess.Read, FileShare.Read);
        var result = new TempFilePdfSeekableStream(_owner, substream, _start + startOffset, length);
        _owner.Retain();
        return result;
    }

    private void EnsureOpen()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(TempFilePdfSeekableStream));
        }
    }

    public override void Flush()
    {
    }

    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        if (disposing)
        {
            _stream.Dispose();
            _owner.Release();
        }
        base.Dispose(disposing);
    }
}
