// Some attributes and functions which ilc expects to be defined.
// (Normally, obj and lib files implementing these are provided by the SDK for every supported platform, however
// we go full "bare metal" here (no runtime, no GC), so we need to provide some stubs to make linker errors go away.)

// Based on: https://github.com/bflattened/bflat/blob/master/src/zerolib/Internal/Stubs.cs

using System;
using System.Runtime;

namespace System.Runtime
{
    internal sealed class RuntimeExportAttribute : Attribute
    {
        public RuntimeExportAttribute(string entry) { }
    }

    internal sealed class RuntimeImportAttribute : Attribute
    {
        public RuntimeImportAttribute(string lib) { }
        public RuntimeImportAttribute(string lib, string entry) { }
    }
}

namespace Internal.Runtime.CompilerHelpers
{
    partial class ThrowHelpers
    {
        static void ThrowIndexOutOfRangeException() => Environment.FailFast(null!);
    }

    // A class that the compiler looks for that has helpers to initialize the
    // process. The compiler can gracefully handle the helpers not being present,
    // but the class itself being absent is unhandled. Let's add an empty class.
    partial class StartupCodeHelpers
    {
        // A couple symbols the generated code will need we park them in this class
        // for no particular reason. These aid in transitioning to/from managed code.
        // Since we don't have a GC, the transition is a no-op.
        [RuntimeExport("RhpReversePInvoke")]
        static void RhpReversePInvoke(IntPtr frame) { }
        [RuntimeExport("RhpReversePInvokeReturn")]
        static void RhpReversePInvokeReturn(IntPtr frame) { }

        [RuntimeExport("RhpGcPoll")]
        static void RhpGcPoll() { }

        [RuntimeExport("RhpTrapThreads")]
        static void RhpTrapThreads() { }
    }
}
