using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using DotVVM.Framework.Compilation.Parser.Dothtml.Parser;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Parser;

namespace DotVVM.Framework.Compilation.Directives
{
    public abstract class DirectiveResolver<TDirective>
        where TDirective : IAbstractDirective
    {
        public static HashSet<string> SingleValueDirectives = new(StringComparer.OrdinalIgnoreCase)
        {
            ParserConstants.BaseTypeDirective,
            ParserConstants.MasterPageDirective,
            ParserConstants.ResourceTypeDirective,
            ParserConstants.ViewModelDirectiveName
        };

        protected IReadOnlyDictionary<string, IReadOnlyList<DothtmlDirectiveNode>> DirectiveNodesByName { get; }

        public DirectiveResolver(IReadOnlyDictionary<string, IReadOnlyList<DothtmlDirectiveNode>> directiveNodesByName)
        {
            DirectiveNodesByName = directiveNodesByName;
        }

        protected ImmutableList<TDirective> Resolve(string directiveName)
        {
            if (!DirectiveNodesByName.TryGetValue(directiveName, out var directivesToProcess))
            {
                return ImmutableList<TDirective>.Empty;
            }

            if (SingleValueDirectives.Contains(directiveName) && directivesToProcess.Count > 1)
            {
                foreach (var d in directivesToProcess)
                {
                    Resolve(d);
                    d.AddError($"Directive '{d.Name}' cannot be present multiple times.");
                }
                return ImmutableList.Create(Resolve(directivesToProcess.First()));
            }
            else
            {
                return directivesToProcess.Select(Resolve).ToImmutableList();
            }
        }

        protected abstract TDirective Resolve(DothtmlDirectiveNode d);
    }

}
