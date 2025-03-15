using System;
using System.Collections.Immutable;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;

namespace DotVVM.Framework.Binding
{

    static partial class DotvvmPropertyIdAssignment
    {
        public static class TypeIds
        {
            public const ushort DotvvmBindableObject = 1;
            public const ushort DotvvmControl = 2;
            public const ushort HtmlGenericControl = 3;
            public const ushort RawLiteral = 4;
            public const ushort Literal = 5;
            public const ushort ButtonBase = 6;
            public const ushort Button = 7;
            public const ushort LinkButton = 8;
            public const ushort TextBox = 9;
            public const ushort RouteLink = 10;
            public const ushort CheckableControlBase = 11;
            public const ushort CheckBox = 12;
            public const ushort Validator = 13;
            public const ushort Validation = 14;
            public const ushort ValidationSummary = 15;
            public const ushort Internal = 16;

            public static readonly ImmutableArray<(Type type, ushort id)> List = ImmutableArray.Create(
                (typeof(DotvvmBindableObject), DotvvmBindableObject),
                (typeof(DotvvmControl), DotvvmControl),
                (typeof(HtmlGenericControl), HtmlGenericControl),
                (typeof(RawLiteral), RawLiteral),
                (typeof(Literal), Literal),
                (typeof(ButtonBase), ButtonBase),
                (typeof(Button), Button),
                (typeof(LinkButton), LinkButton),
                (typeof(TextBox), TextBox),
                (typeof(RouteLink), RouteLink),
                (typeof(CheckableControlBase), CheckableControlBase),
                (typeof(CheckBox), CheckBox),
                (typeof(Validator), Validator),
                (typeof(Validation), Validation),
                (typeof(ValidationSummary), ValidationSummary),
                (typeof(Internal), Internal)
            );
        }
    }
}
