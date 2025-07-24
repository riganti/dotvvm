using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Inference.Results;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Inference
{
    internal partial class TypeInferer
    {
        ConstructorTypeInferenceResult IFluentInferer.Constructor(int argsCount)
        {
            var viableCandidates = new List<Type>();

            if (contextStack.Count == 0)
            {
                if (expectedType is { } && HasMatchingConstructor(expectedType, argsCount))
                    viableCandidates.Add(expectedType);
            }
            else
            {
                var context = contextStack.Peek();
                var index = context.CurrentArgumentIndex;

                // Check if we can match any method candidate
                foreach (var candidate in context.Target?.Candidates ?? [])
                {
                    var parameters = candidate.GetParameters();
                    if (index >= parameters.Length || parameters.Length > context.Arguments.Length)
                        continue;

                    var type = parameters[index].ParameterType;
                    if (HasMatchingConstructor(type, argsCount))
                        viableCandidates.Add(type);
                }
            }

            if (viableCandidates.Count != 1)
                return new(null, false);

            return new(viableCandidates[0], true);
        }

        private static bool HasMatchingConstructor(Type type, int argumentCount)
        {
            if (type.IsAbstract)
                return false;

            if (type.IsValueType && argumentCount == 0)
                return true;

            var constructors = type.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            return constructors.Any(c => {
                var parameters = c.GetParameters();
                return parameters.Length >= argumentCount && parameters.Count(p => !p.IsOptional) <= argumentCount;
            });
        }
    }
}
