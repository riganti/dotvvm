#nullable enable
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
        LambdaTypeInferenceResult IFluentInferer.Lambda(int argsCount)
        {
            var found = false;
            var viableCandidates = new List<(Type delegateType, Type[] delegateParams)>();

            if (contextStack.Count == 0)
            {
                if (expectedType != null && TryMatchDelegate(null, argsCount, expectedType, out var parameters))
                {
                    found = true;
                    viableCandidates.Add((expectedType, parameters));
                }
            }
            else
            {
                var context = contextStack.Peek();
                var index = context.CurrentArgumentIndex;
                
                // Check if we can match any method candidate
                foreach (var candidate in context.Target!.Candidates)
                {
                    var parameters = candidate.GetParameters();
                    if (index >= parameters.Length || parameters.Length > context.Arguments.Length)
                        continue;

                    var delegateType = parameters[index].ParameterType;
                    if (TryMatchDelegate(context, argsCount, delegateType, out var delegateParameters))
                    {
                        found = true;
                        viableCandidates.Add((delegateType, delegateParameters));
                    }
                }
            }
            
            if (found)
            {
                if (viableCandidates.Count > 1)
                {
                    TryDisambiguateCandidates(viableCandidates, out var result);
                    return result;
                }

                var candidate = viableCandidates.First();
                return new LambdaTypeInferenceResult(
                    result: true,
                    type: candidate.delegateType,
                    parameters: candidate.delegateParams);
            }

            return new LambdaTypeInferenceResult(result: false);
        }

        private bool TryDisambiguateCandidates(List<(Type type, Type[] parameters)> viableCandidates, out LambdaTypeInferenceResult result)
        {
            var parameters = viableCandidates.First().parameters;
            if (viableCandidates.All(candidate => Enumerable.SequenceEqual(parameters, candidate.parameters)))
            {
                // Delegates can be distinguished based on return type
                // In this case it is possible to generate lambda expression
                result = new LambdaTypeInferenceResult(
                    result: true,
                    type: /* ambiguous */ null,
                    parameters: parameters);
                return true;
            }

            // TODO: at this point it is necessary to take into account lambda bodies to disambiguate between method overloads
            result = new LambdaTypeInferenceResult(result: false);
            return false;
        }

        private bool TryMatchDelegate(InfererContext? context, int argsCount, Type delegateType, [NotNullWhen(true)] out Type[]? parameters)
        {
            parameters = null;

            if (!ReflectionUtils.IsDelegate(delegateType))
                return false;

            var delegateParameters = delegateType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance).GetParameters();
            if (delegateParameters.Length != argsCount)
                return false;

            var generics = (context != null) ? context.Generics : new Dictionary<string, Type>();
            if (!TryInstantiateDelegateParameters(delegateType, argsCount, generics, out parameters))
                return false;

            return true;
        }

        private bool TryInstantiateDelegateParameters(Type generic, int argsCount, Dictionary<string, Type> generics, [NotNullWhen(true)] out Type[]? instantiation)
        {
            var genericArgs = generic.GetGenericArguments();
            var substitutions = new Type[argsCount];

            for (var argIndex = 0; argIndex < argsCount; argIndex++)
            {
                var currentArg = genericArgs[argIndex];

                if (!currentArg.IsGenericParameter)
                {
                    // This is a known type
                    substitutions[argIndex] = currentArg;
                }
                else if (currentArg.IsGenericParameter && generics.ContainsKey(currentArg.Name))
                {
                    // This is a generic parameter
                    // But we already inferred its type
                    substitutions[argIndex] = generics[currentArg.Name];
                }
                else
                {
                    // This is an unknown type
                    instantiation = null;
                    return false;
                }
            }

            instantiation = substitutions;
            return true;
        }
    }
}
