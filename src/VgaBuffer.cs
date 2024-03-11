using System;
using System.Runtime.CompilerServices;

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

    private static unsafe ref ushort GetCharAt(int top, int left)
    {
        if ((uint)top >= Height || (uint)left >= Width)
        {
            Environment.FailFast(null!);
        }

        return ref Unsafe.Add(ref Unsafe.AsRef<ushort>((void*)BaseAddress), top * Width + left);
    }

    public static char Read(int top, int left)
    {
        return (char)(byte)GetCharAt(top, left);
    }

    public static void Write(int top, int left, char ch)
    {
        GetCharAt(top, left) = (ushort)((ch <= 0xFF ? ch : '?') | (0x7 << 8));
    }

    public static void Write(int top, int left, char ch1, char ch2, char ch3, char ch4)
    {
        Unsafe.As<ushort, ulong>(ref GetCharAt(top, left)) = 0x0700070007000700ul
            | (ulong)ch4 << 48
            | (ulong)ch3 << 32
            | (ulong)ch2 << 16
            | (ulong)ch1;
    }
}
