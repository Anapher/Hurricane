Guidelines
==============
Here you can see some guidelines I want to use in this project.

Order of items in class
--------------
Within a class, struct or interface: (SA1201 and SA1203)

- Constant Fields
- Fields
- Constructors
- Finalizers (Destructors)
- Delegates
- Events
- Enums
- Interfaces
- Properties
- Indexers
- Methods
- Structs
- Classes

Within each of these groups order by access: (SA1202)

- public
- internal
- protected internal
- protected
- private

Within each of the access groups, order by static, then non-static: (SA1204)

- static
- non-static

Within each of the static/non-static groups of fields, order by readonly, then non-readonly : (SA1214 and SA1215)

- readonly
- non-readonly

Naming of items in class
--------------
See [Microsoft Naming Guidelines](https://msdn.microsoft.com/en-us/library/xzf533w0(v=vs.71).aspx).

Other
--------------
- All functions in `Hurricane.Utilities.UnsafeNativeMethods` begin with `internal static extern`
- All converters in `Hurricane.Converter` are `internal`
- Try to avoid uneccesary 'this' qualifiers
- Avoid `#region`s
- String comparing thought string.Equals() (instead of `s1.ToUpper() == s2.ToUppter()`)