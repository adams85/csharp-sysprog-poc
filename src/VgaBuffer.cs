using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

/// <summary>
/// A thin wrapper around the VGA display buffer.
/// </summary>
public static class VgaBuffer
{
    public const int BaseAddress = 0xB8000;

    // We assume VGA text mode 7 (80 x 25)
    // See also: https://en.wikipedia.org/wiki/VGA_text_mode
    private const int Width = 80;
    private const int Height = 25;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static unsafe Span<T> AsSpan<T>() where T : unmanaged
    {
        if (sizeof(T) == 2)
        {
            return MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>((void*)BaseAddress), Width * Height);
        }
        else if (sizeof(T) <= Width * Height * 2)
        {
            return MemoryMarshal.CreateSpan(ref Unsafe.AsRef<T>((void*)BaseAddress), Width * Height * 2 / sizeof(T));
        }
        else
        {
            Environment.FailFast(null!);
            return default;
        }
    }

    public static char Read(int top, int left)
    {
        return (char)(byte)AsSpan<ushort>()[top * Width + left];
    }

    public static void Write(int top, int left, char ch)
    {

        AsSpan<ushort>()[top * Width + left] = (ushort)((ch <= 0xFF ? ch : '?') | (0x7 << 8));
    }
}
