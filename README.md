# System progamming in C# using Native AOT

The project implemented in this repo is a solution to the programming contest announced [here](https://gitlab.com/bztsrc/langcontest).

But it is even more of an experiment to see how low-level you can go with the the current AOT compiler of .NET 8. In other words, can you do system programming using C# nowadays?

### What is the answer?

Definitely positive - with some caveats.

Although the primary goal of Native AOT is to compile managed C# apps with garbage collection to machine code, not programs for bare metal, it's capable to do that with some plumbing.
However, it's obviously not optimized for this task (yet) so it's a pretty rough experience for now and won't give you optimal results in terms of size and execution time.

But hey, a platform originally designed for application programming being able to compile your program to a few kilobytes executable while you can use the full power of the C# language - that's already pretty awesome!

### Difficulties/shortcomings

The program works (achieves the end goal) but doesn't fully meet the requirements of the contest for the following reasons:
* I couldn't make the interrupt handler work with normal C# static methods, had to add an assembly stub to crank things up. It could be just me missing something, but as far as I can see, we'd need support (e.g. a special calling convention) from the AOT compiler for this.
* Exceptions are part of the language but they won't work (not that we need them on such low levels...) Anyway, probably it's possible to write the necessary plumbing to make them work, but I don't think it's possible without some assembly code.
* The code is written using the latest, "safe" low-level constructs of C# (`Span`, `Unsafe`, `MemoryMarshal`, etc.), without direct pointer operations, however at some places the code needs to obtain pointers, which requires unsafe context.
* Might not be built on other platforms than Windows. Native AOT works on Linux and macOS, but I'm not sure if it can build Windows PE binaries with MS fastcall on those platforms.

### How to build?

To build the project, you need
* Windows
* .NET 8 SDK
* MSVC linker (installed along with Visual Studio 2022 or VS Build Tools)

(Other platforms/linkers should work as well but will need some manual work, the MSBuild doesn't include support for them now.)

When having all this installed, issue

`dotnet publish -c Release`

### Credits

The "mini-BCL" and runtime plumbing were mostly borrowed from Michal Strehovský's [bflat](https://github.com/MichalStrehovsky) project.

His articles on the topic and other works like [zerosharp](https://github.com/MichalStrehovsky/zerosharp) and [SeeSharpSnake](https://github.com/MichalStrehovsky/SeeSharpSnake) also helped me a lot.