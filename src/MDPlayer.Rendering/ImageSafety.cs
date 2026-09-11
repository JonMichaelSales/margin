using System.Buffers.Binary;

namespace MDPlayer.Rendering;

public readonly record struct ImageDimensions(int Width, int Height)
{
    public long Pixels => (long)Width * Height;
}

public static class ImageSafety
{
    public const int MaximumEncodedBytes = 16 * 1024 * 1024;
    public const int MaximumDimension = 16_384;
    public const long MaximumPixels = 64_000_000;

    public static ImageDimensions Validate(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length > MaximumEncodedBytes) throw new IOException("Image exceeds the 16 MB encoded-size limit.");
        var dimensions = ReadDimensions(bytes) ?? throw new IOException("The image format is unsupported or its header is invalid.");
        if (dimensions.Width <= 0 || dimensions.Height <= 0 || dimensions.Width > MaximumDimension || dimensions.Height > MaximumDimension || dimensions.Pixels > MaximumPixels)
            throw new IOException($"Image dimensions {dimensions.Width} × {dimensions.Height} exceed the safe decode limit.");
        return dimensions;
    }

    public static ImageDimensions? ReadDimensions(ReadOnlySpan<byte> bytes)
    {
        if (bytes.Length >= 24 && bytes[..8].SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            return new(ReadInt32BigEndian(bytes[16..20]), ReadInt32BigEndian(bytes[20..24]));
        if (bytes.Length >= 10 && (bytes[..6].SequenceEqual("GIF87a"u8) || bytes[..6].SequenceEqual("GIF89a"u8)))
            return new(BinaryPrimitives.ReadUInt16LittleEndian(bytes[6..8]), BinaryPrimitives.ReadUInt16LittleEndian(bytes[8..10]));
        if (bytes.Length >= 26 && bytes[0] == (byte)'B' && bytes[1] == (byte)'M')
            return new(Math.Abs(BinaryPrimitives.ReadInt32LittleEndian(bytes[18..22])), Math.Abs(BinaryPrimitives.ReadInt32LittleEndian(bytes[22..26])));
        if (bytes.Length >= 30 && bytes[..4].SequenceEqual("RIFF"u8) && bytes[8..12].SequenceEqual("WEBP"u8))
            return ReadWebP(bytes);
        if (bytes.Length >= 12 && bytes[0] == 0xff && bytes[1] == 0xd8)
            return ReadJpeg(bytes);
        return null;
    }

    private static ImageDimensions? ReadJpeg(ReadOnlySpan<byte> bytes)
    {
        var offset = 2;
        while (offset + 8 < bytes.Length)
        {
            if (bytes[offset] != 0xff) { offset++; continue; }
            while (offset < bytes.Length && bytes[offset] == 0xff) offset++;
            if (offset >= bytes.Length) break;
            var marker = bytes[offset++];
            if (marker is 0xd8 or 0xd9 || marker is >= 0xd0 and <= 0xd7) continue;
            if (offset + 2 > bytes.Length) break;
            var length = BinaryPrimitives.ReadUInt16BigEndian(bytes[offset..(offset + 2)]);
            if (length < 2 || offset + length > bytes.Length) break;
            if (marker is >= 0xc0 and <= 0xc3 or >= 0xc5 and <= 0xc7 or >= 0xc9 and <= 0xcb or >= 0xcd and <= 0xcf)
            {
                if (length < 7) break;
                return new(BinaryPrimitives.ReadUInt16BigEndian(bytes[(offset + 5)..(offset + 7)]), BinaryPrimitives.ReadUInt16BigEndian(bytes[(offset + 3)..(offset + 5)]));
            }
            offset += length;
        }
        return null;
    }

    private static ImageDimensions? ReadWebP(ReadOnlySpan<byte> bytes)
    {
        if (bytes[12..16].SequenceEqual("VP8X"u8) && bytes.Length >= 30)
            return new(1 + ReadUInt24LittleEndian(bytes[24..27]), 1 + ReadUInt24LittleEndian(bytes[27..30]));
        if (bytes[12..16].SequenceEqual("VP8L"u8) && bytes.Length >= 25 && bytes[20] == 0x2f)
        {
            var width = 1 + bytes[21] + ((bytes[22] & 0x3f) << 8);
            var height = 1 + ((bytes[22] & 0xc0) >> 6) + (bytes[23] << 2) + ((bytes[24] & 0x0f) << 10);
            return new(width, height);
        }
        if (bytes[12..16].SequenceEqual("VP8 "u8) && bytes.Length >= 30 && bytes[23] == 0x9d && bytes[24] == 0x01 && bytes[25] == 0x2a)
            return new(BinaryPrimitives.ReadUInt16LittleEndian(bytes[26..28]) & 0x3fff, BinaryPrimitives.ReadUInt16LittleEndian(bytes[28..30]) & 0x3fff);
        return null;
    }

    private static int ReadInt32BigEndian(ReadOnlySpan<byte> bytes) => BinaryPrimitives.ReadInt32BigEndian(bytes);
    private static int ReadUInt24LittleEndian(ReadOnlySpan<byte> bytes) => bytes[0] | bytes[1] << 8 | bytes[2] << 16;
}
