using System;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

class Program
{
    private const int IDTSize = 48;

    [StructLayout(LayoutKind.Sequential)]
    unsafe struct AlmostEfiSystemTableButNotQuite
    {
        public delegate* unmanaged[SuppressGCTransition]<void*, ushort, void> LoadIdt;
        public delegate* unmanaged[SuppressGCTransition]<void> EnableInterrupts;
        public delegate* unmanaged[SuppressGCTransition]<void> DisableInterrupts;
        public delegate* unmanaged[SuppressGCTransition]<int, void> Iretq;
    }

    // https://wiki.osdev.org/Interrupt_Descriptor_Table#Gate_Descriptor_2
    [StructLayout(LayoutKind.Sequential)]
    struct InterruptDescriptor64
    {
        public ushort Offset1; // offset bits 0..15
        public ushort Selector; // a code segment selector in GDT or LDT
        public byte Ist; // bits 0..2 holds Interrupt Stack Table offset, rest of bits zero.
        public byte TypeAttributes; // gate type, dpl, and p fields
        public ushort Offset2; // offset bits 16..31
        public uint Offset3; // offset bits 32..63
        public uint Zero; // reserved
    }

    [InlineArray(IDTSize)]
    struct IDTArray { private InterruptDescriptor64 _element0; }

    // A normal C# static method won't work as an interrupt handler for some reason. Most likely, the machine code generated by ilc
    // messes up some CPU register it shouldn't, which makes the program crash. From C# code we can't directly save, then restore
    // registers before calling iretq, so we need to resort to write a minimal assembly stub, which calls our "real" handler, then iretq.
    // (For the sake of simplicity, we include the stub as data and patch it with the actual function addresses at program startup,
    // but in principle we could write the stub in a seperate assembly module and import it using [RuntimeImport].)
    //
    // movabs rbx, <addr-of-real-handler>
    // call rbx
    // sub rsp, 8
    // xor rcx, rcx
    // xor rdi, rdi
    // movabs rbx, <addr-of-iretq>
    // call rbx
    private static ReadOnlySpan<byte> InterruptHandlerStub =>
    [
        0x48, 0xBB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xFF, 0xD3,
        0x48, 0x83, 0xEC, 0x08,
        0x48, 0x31, 0xC9,
        0x48, 0x31, 0xFF,
        0x48, 0xBB, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
        0xFF, 0xD3,
    ];

    private static IDTArray s_idt;
    private static Span<InterruptDescriptor64> IDT => MemoryMarshal.CreateSpan(ref Unsafe.As<IDTArray, InterruptDescriptor64>(ref s_idt), IDTSize);

    private static int counter;

    static void Main() { } // unused, just to keep csc happy

    [RuntimeExport("ep")]
    static unsafe int EntryPoint(in AlmostEfiSystemTableButNotQuite sysTable)
    {
        delegate* unmanaged[SuppressGCTransition]<void> interruptHandler = &InterruptHandler;

        // Patch the interrupt handler stub with the actual addresses of the functions to call
        ref var interruptHandlerStub = ref Unsafe.AsRef(InterruptHandlerStub[0]);
        Unsafe.As<byte, ulong>(ref Unsafe.Add(ref interruptHandlerStub, 2)) = (ulong)interruptHandler;
        Unsafe.As<byte, ulong>(ref Unsafe.Add(ref interruptHandlerStub, 24)) = (ulong)sysTable.Iretq;

        // Setup interrupts
        var idt = IDT;
        ref var entry = ref idt[32];
        entry.Offset1 = (ushort)(nuint)Unsafe.AsPointer(ref interruptHandlerStub);
        entry.Offset2 = (ushort)((nuint)Unsafe.AsPointer(ref interruptHandlerStub) >> 16);
        entry.Offset3 = (uint)((nuint)Unsafe.AsPointer(ref interruptHandlerStub) >> 32);
        entry.Selector = 32;
        entry.TypeAttributes = 0x8E;
        entry.Zero = entry.Ist = 0;

        sysTable.LoadIdt(Unsafe.AsPointer(ref idt[0]), (ushort)(idt.Length * sizeof(InterruptDescriptor64)));

        // Reset counter
        VgaBuffer.Write(0, 0, '0', '0', '0', '0');

        // Enable timer
        sysTable.EnableInterrupts();

        for (; ; ) { }
    }

    [UnmanagedCallersOnly(CallConvs = [typeof(CallConvSuppressGCTransition)])]
    static void InterruptHandler()
    {
        if (++counter < 100)
            return;

        counter = 0;

        for (var left = 3;  left >= 0; left--)
        {
            var ch = VgaBuffer.Read(0, left);
            if (ch < '9')
            {
                VgaBuffer.Write(0, left, ++ch);
                return;
            }
            VgaBuffer.Write(0, left, '0');
        }
    }
}
