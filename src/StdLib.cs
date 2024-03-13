// A minimal subset of the .NET BCL (standard library) which is necessary to implement the program.
// (Actually, not everything here is strictly necessary. E.g. we could as well use pointers instead of
// Span/ReadOnlySpan and Unsafe, but then that would be just plain C programming in C#... ;)

// Based on: https://github.com/bflattened/bflat/tree/master/src/zerolib/System

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System
{
    public struct Void { }

    // The layout of primitive types is special cased because it would be recursive.
    // These really don't need any fields to work.
    public struct Boolean { }
    public struct Char { }
    public struct SByte { }
    public struct Byte { }
    public struct Int16 { }
    public struct UInt16 { }
    public struct Int32 { }
    public struct UInt32 { }
    public struct Int64 { }
    public struct UInt64 { }
    public struct IntPtr { }
    public struct UIntPtr { }
    public struct Single { }
    public struct Double { }

    public class Object
    {
        // The layout of object is a contract with the compiler.
#pragma warning disable CS0649 // Field 'object.m_pMethodTable' is never assigned to, and will always have its default value
        internal IntPtr m_pMethodTable;
#pragma warning restore CS0649 // Field 'object.m_pMethodTable' is never assigned to, and will always have its default value
    }

    public class Type { }
    public class RuntimeType : Type { }

    public struct RuntimeTypeHandle { }
    public struct RuntimeMethodHandle { }
    public struct RuntimeFieldHandle { }

    public class Attribute { }

    public enum AttributeTargets { }

    public sealed class AttributeUsageAttribute : Attribute
    {
        public AttributeUsageAttribute(AttributeTargets validOn) { }
        public bool AllowMultiple { get; set; }
        public bool Inherited { get; set; }
    }

    public abstract class ValueType { }

    public abstract class Enum : ValueType { }

    public struct Nullable<T> where T : struct
    {
        private readonly bool _hasValue;
        private T _value;

        public Nullable(T value)
        {
            _hasValue = true;
            _value = value;
        }

        public readonly bool HasValue => _hasValue;

        public readonly T Value
        {
            get
            {
                if (!_hasValue)
                    Environment.FailFast(null!);
                return _value;
            }
        }

        public static implicit operator T?(T value) => new T?(value);

        public static explicit operator T(T? value) => value!.Value;
    }

    public sealed class String
    {
        // The layout of the string type is a contract with the compiler.
        public readonly int Length;
        private char _firstChar;

        public char this[int index] { [Intrinsic] get => Unsafe.Add(ref _firstChar, index); }
    }

    public abstract class Delegate { }
    public abstract class MulticastDelegate : Delegate { }

    public abstract class Array
    {
        public readonly int Length;
    }

    public class Array<T> : Array { }

    public readonly ref struct ReadOnlySpan<T>
    {
        private readonly ref T _reference;
        public readonly int Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan(T[] array)
        {
            if (array == null)
            {
                this = default;
                return;
            }

            _reference = ref MemoryMarshal.GetArrayDataReference(array);
            Length = array.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe ReadOnlySpan(void* pointer, int length)
        {
            _reference = ref Unsafe.As<byte, T>(ref *(byte*)pointer);
            Length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ReadOnlySpan(T[] array, int start, int length)
        {
            if (array == null)
            {
                if (start != 0 || length != 0)
                {
                    Environment.FailFast(null!);
                }
                this = default;
                return; // returns default
            }
#if X64 || ARM64
            if ((ulong)(uint)start + (ulong)(uint)length > (ulong)(uint)array.Length)
                Environment.FailFast(null!);
#elif X86 || ARM
            if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
                Environment.FailFast(null!);
#else
#error Not implemented.
#endif

            _reference = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), (nint)(uint)start);
            Length = length;
        }

        public ref readonly T this[int index]
        {
            [Intrinsic]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index >= (uint)Length)
                    Environment.FailFast(null!);
                return ref Unsafe.Add(ref _reference, (nint)(uint)index);
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator ReadOnlySpan<T>(T[] array) => new ReadOnlySpan<T>(array);
    }

    public readonly ref struct Span<T>
    {
        private readonly ref T _reference;
        public readonly int Length;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span(T[] array)
        {
            if (array == null)
            {
                this = default;
                return;
            }

            _reference = ref MemoryMarshal.GetArrayDataReference(array);
            Length = array.Length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe Span(void* pointer, int length)
        {
            _reference = ref Unsafe.As<byte, T>(ref *(byte*)pointer);
            Length = length;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Span(T[] array, int start, int length)
        {
            if (array == null)
            {
                if (start != 0 || length != 0)
                {
                    Environment.FailFast(null!);
                }
                this = default;
                return; // returns default
            }
#if X64 || ARM64
            if ((ulong)(uint)start + (ulong)(uint)length > (ulong)(uint)array.Length)
                Environment.FailFast(null!);
#elif X86 || ARM
            if ((uint)start > (uint)array.Length || (uint)length > (uint)(array.Length - start))
                Environment.FailFast(null!);
#else
#error Not implemented.
#endif

            _reference = ref Unsafe.Add(ref MemoryMarshal.GetArrayDataReference(array), (nint)(uint)start);
            Length = length;
        }

        public ref T this[int index]
        {
            [Intrinsic]
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                if ((uint)index >= (uint)Length)
                    Environment.FailFast(null!);
                return ref Unsafe.Add(ref _reference, (nint)(uint)index);
            }
        }
    }

    public static partial class Environment
    {
        public static void FailFast(string message)
        {
            // TODO: do whatever needed to kill the program...
            for (; ; ) { }
        }
    }
}

namespace System.Runtime.CompilerServices
{
    public static class RuntimeFeature
    {
        public const string PortablePdb = nameof(PortablePdb);
        public const string DefaultImplementationsOfInterfaces = nameof(DefaultImplementationsOfInterfaces);
        public const string UnmanagedSignatureCallingConvention = nameof(UnmanagedSignatureCallingConvention);
        public const string CovariantReturnsOfClasses = nameof(CovariantReturnsOfClasses);
        public const string ByRefFields = nameof(ByRefFields);
        public const string VirtualStaticsInInterfaces = nameof(VirtualStaticsInInterfaces);
        public const string NumericIntPtr = nameof(NumericIntPtr);
    }

    public class RuntimeHelpers
    {
        public static unsafe int OffsetToStringData => sizeof(IntPtr) + sizeof(int);
    }

    [StructLayout(LayoutKind.Sequential)]
    internal class RawArrayData
    {
        public uint Length;
#if X64 || ARM64
        public uint Padding;
#elif X86 || ARM
        // No padding on 32bit
#else
#error Not implemented.
#endif
        public byte Data;
    }

    public sealed class InlineArrayAttribute : Attribute
    {
        public InlineArrayAttribute(int length) { Length = length; }

        public int Length { get; }
    }

    internal sealed class IntrinsicAttribute : Attribute { }

    public enum MethodImplOptions
    {
        Unmanaged = 0x0004,
        NoInlining = 0x0008,
        ForwardRef = 0x0010,
        Synchronized = 0x0020,
        NoOptimization = 0x0040,
        PreserveSig = 0x0080,
        AggressiveInlining = 0x0100,
        AggressiveOptimization = 0x0200,
        InternalCall = 0x1000
    }

    public sealed class MethodImplAttribute : Attribute
    {
        public MethodImplAttribute(MethodImplOptions methodImplOptions) { }
    }

    public class CallConvCdecl { }
    public class CallConvFastcall { }
    public class CallConvStdcall { }
    public class CallConvSuppressGCTransition { }
    public class CallConvThiscall { }
    public class CallConvMemberFunction { }

    public static partial class Unsafe
    {
        // The body of this method is generated by the compiler.
        // It will do what Unsafe.Add is expected to do. It's just not possible to express it in C#.
        [Intrinsic]
        public static extern ref T Add<T>(ref T source, int elementOffset);
        [Intrinsic]
        public static extern ref T Add<T>(ref T source, IntPtr elementOffset);
        [Intrinsic]
        public static extern ref TTo As<TFrom, TTo>(ref TFrom source);
        [Intrinsic]
        public static extern T As<T>(object o) where T : class;
        [Intrinsic]
        public static unsafe extern void* AsPointer<T>(ref T value);
        [Intrinsic]
        public static unsafe extern ref T AsRef<T>(void* source);
        [Intrinsic]
        public static extern ref T AsRef<T>(in T source);
    }

    // A class responsible for running static constructors. The compiler will call into this
    // code to ensure static constructors run and that they only run once.
    internal static partial class ClassConstructorRunner
    {
        private static IntPtr CheckStaticClassConstructionReturnNonGCStaticBase(ref StaticClassConstructionContext context, IntPtr nonGcStaticBase)
        {
            CheckStaticClassConstruction(ref context);
            return nonGcStaticBase;
        }

        private static unsafe void CheckStaticClassConstruction(ref StaticClassConstructionContext context)
        {
            // Not dealing with multithreading issues.
            if (context.cctorMethodAddress != default)
            {
                IntPtr address = context.cctorMethodAddress;
                context.cctorMethodAddress = default;
                ((delegate*<void>)address)();
            }
        }
    }

    // This data structure is a contract with the compiler. It holds the address of a static
    // constructor and a flag that specifies whether the constructor already executed.
    [StructLayout(LayoutKind.Sequential)]
    public struct StaticClassConstructionContext
    {
        public IntPtr cctorMethodAddress;
    }
}

namespace System.Runtime.InteropServices
{
    public enum UnmanagedType { }

    public sealed class UnmanagedCallersOnlyAttribute : Attribute
    {
        public Type[]? CallConvs;
        public string? EntryPoint;

        public UnmanagedCallersOnlyAttribute() { }
    }

    public enum LayoutKind
    {
        Sequential = 0,
        Explicit = 2,
        Auto = 3,
    }

    public sealed class StructLayoutAttribute : Attribute
    {
        public int Size;
        public int Pack;
        public StructLayoutAttribute(LayoutKind layoutKind) { }
    }

    public sealed class InAttribute : Attribute { }

    public sealed class OutAttribute : Attribute { }

    public sealed class SuppressGCTransitionAttribute : Attribute
    {
        public SuppressGCTransitionAttribute() { }
    }

    public static partial class MemoryMarshal
    {
        [Intrinsic]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ref T GetArrayDataReference<T>(T[] array) => ref Unsafe.As<byte, T>(ref Unsafe.As<RawArrayData>(array).Data);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe Span<T> CreateSpan<T>(ref T reference, int length)
            => new Span<T>(Unsafe.AsPointer(ref reference), length);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe ReadOnlySpan<T> CreateReadOnlySpan<T>(ref T reference, int length)
            => new ReadOnlySpan<T>(Unsafe.AsPointer(ref reference), length);
    }
}
