using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Controls;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.Controls
{
    public class ControlWithCustomPrimitiveTypeProperties : CompositeControl
    {
        public DotvvmControl GetContents(
            HtmlCapability htmlCapability,
            Point pointValue,
            Point pointValue2,
            IValueBinding<Point> pointBinding,
            ValueOrBinding<Point> pointValueOrBinding,
            ValueOrBinding<Point> pointValueOrBinding2,
            ValueOrBinding<Point> pointValueOrBinding3,
            ValueOrBinding<Point> pointValueOrBinding4)
        {
            return new HtmlGenericControl("ul", htmlCapability)
                .AppendChildren(
                    new HtmlGenericControl("li")
                        .AppendChildren(new Literal(pointValue.ToString())),
                    new HtmlGenericControl("li")
                        .AppendChildren(new Literal(pointValue2.ToString())),
                    new HtmlGenericControl("li")
                        .AppendChildren(new Literal(pointBinding)),
                    new HtmlGenericControl("li")
                        .AppendChildren(new Literal(pointValueOrBinding)),
                    new HtmlGenericControl("li")
                        .AppendChildren(new Literal(pointValueOrBinding2)),
                    new HtmlGenericControl("li")
                        .AppendChildren(new Literal(pointValueOrBinding3)),
                    new HtmlGenericControl("li")
                        .AppendChildren(new Literal(pointValueOrBinding4))
                );
        }

    }

    [CustomPrimitiveType]
    public struct Point
    {
        public int X { get; set; }
        public int Y { get; set; }

        public static bool TryParse(string value, out Point result)
        {
            var parts = value.Split(',');
            if (int.TryParse(parts[0], out var x) && int.TryParse(parts[1], out var y))
            {
                result = new Point { X = x, Y = y };
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }

        public static Point Parse(string value) => TryParse(value, out var result) ? result : throw new FormatException();

        public override string ToString() => $"{X},{Y}";
    }
}
