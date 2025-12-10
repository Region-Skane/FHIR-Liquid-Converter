// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
namespace FLC.PackageManagement.Models;

public sealed class SequentialStream : Stream
{
    private readonly Stream _first;
    private readonly Stream _second;
    private readonly bool _leaveOpen;
    private bool _firstExhausted = false;

    public SequentialStream(Stream first, Stream second, bool leaveOpen = false)
    {
        _first = first ?? throw new ArgumentNullException(nameof(first));
        _second = second ?? throw new ArgumentNullException(nameof(second));
        _leaveOpen = leaveOpen;
    }

    public override bool CanRead => true;

    public override bool CanSeek => false;

    public override bool CanWrite => false;

    public override long Length => throw new NotSupportedException();

    public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

    public override int Read(byte[] buffer, int offset, int count)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if ((uint)offset > buffer.Length || (uint)count > buffer.Length - offset)
        {
            throw new ArgumentOutOfRangeException();
        }

        if (buffer.Length == 0 || count == 0)
        {
            return 0;
        }

        if (!_firstExhausted)
        {
            int read = _first.Read(buffer, offset, count);
            if (read > 0)
            {
                return read;
            }

            _firstExhausted = true;
        }

        return _second.Read(buffer, offset, count);
    }

    public override int Read(Span<byte> buffer)
    {
        if (buffer.Length == 0)
        {
            return 0;
        }

        if (!_firstExhausted)
        {
            int read = _first.Read(buffer);
            if (read > 0)
            {
                return read;
            }

            _firstExhausted = true;
        }

        return _second.Read(buffer);
    }

    public override async Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        if ((uint)offset > buffer.Length || (uint)count > buffer.Length - offset)
        {
            throw new ArgumentOutOfRangeException();
        }

        if (buffer.Length == 0 || count == 0)
        {
            return 0;
        }

        if (!_firstExhausted)
        {
            int read = await _first.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
            if (read > 0)
            {
                return read;
            }

            _firstExhausted = true;
        }

        return await _second.ReadAsync(buffer, offset, count, cancellationToken).ConfigureAwait(false);
    }

    public override async ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
    {
        if (buffer.Length == 0)
        {
            return 0;
        }

        if (!_firstExhausted)
        {
            int read = await _first.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
            if (read > 0)
            {
                return read;
            }

            _firstExhausted = true;
        }

        return await _second.ReadAsync(buffer, cancellationToken).ConfigureAwait(false);
    }

    public override int ReadByte()
    {
        if (!_firstExhausted)
        {
            int read = _first.ReadByte();
            if (read > 0)
            {
                return read;
            }

            _firstExhausted = true;
        }

        return _second.ReadByte();
    }

    public override void Flush()
    {
    }

    public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_leaveOpen)
        {
            _first.Dispose();
            _second.Dispose();
        }

        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        if (!_leaveOpen)
        {
            await _first.DisposeAsync().ConfigureAwait(false);
            await _second.DisposeAsync().ConfigureAwait(false);
        }

        await base.DisposeAsync().ConfigureAwait(false);
    }
}