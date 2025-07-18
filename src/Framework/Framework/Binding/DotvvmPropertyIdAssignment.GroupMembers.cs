using System;
using System.Collections.Immutable;

namespace DotVVM.Framework.Binding
{

    static partial class DotvvmPropertyIdAssignment
    {
        public static class GroupMembers
        {
            public const ushort id = 1;
            public const ushort @class = 2;
            public const ushort style = 3;
            public const ushort name = 4;
            public const ushort data_bind = 5;

            public static readonly ImmutableArray<(string Name, ushort ID)> List = ImmutableArray.Create(
                ("id", id),
                ("class", @class),
                ("style", style),
                ("name", name),
                ("data-bind", data_bind)
            );

            public static ushort TryGetId(ReadOnlySpan<char> attr) =>
                attr switch {
                    "id" => id,
                    "class" => @class,
                    "style" => style,
                    "name" => name,
                    "data-bind" => data_bind,
                    _ => 0,
                };

            // TODO
        }
    }
}
