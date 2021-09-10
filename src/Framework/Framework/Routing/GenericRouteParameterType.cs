﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Routing
{
    public class GenericRouteParameterType : IRouteParameterConstraint
    {
        public delegate bool TryParseDelegate<T>(string val, [MaybeNullWhen(false)] out T value);
        Func<string?, string> partRegex;
        public string GetPartRegex(string? parameter)
            => partRegex(parameter);

        Func<string, string?, ParameterParseResult>? parser;
        public ParameterParseResult ParseString(string value, string? parameter)
            => parser == null ? ParameterParseResult.Create(value) : parser(value, parameter);

        public static GenericRouteParameterType Create(string partRegex) => new GenericRouteParameterType(s => partRegex);
        public static GenericRouteParameterType Create<T>(string partRegex, TryParseDelegate<T> parser)
        {
            return new GenericRouteParameterType(s => partRegex, (s, p) =>
            {
#pragma warning disable CS8717
                if (parser(s, out var r)) return ParameterParseResult.Create((object)r!);
#pragma warning restore CS8717
                else return ParameterParseResult.Failed;
            });
        }

        public GenericRouteParameterType(Func<string?, string> partRegex, Func<string, string?, ParameterParseResult>? parser = null)
        {
            this.partRegex = partRegex;
            this.parser = parser;
        }
    }
}
