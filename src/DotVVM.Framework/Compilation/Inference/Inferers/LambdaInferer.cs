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
            var viableCandidates = new List<(Type, Type[], bool)>();

            if (contextStack.Count == 0)
            {
                if (expectedType != null && TryMatchDelegate(null, argsCount, expectedType, out var parameters, out var isFunc, out var _))
                {
                    found = true;
                    viableCandidates.Add((expectedType, parameters, isFunc));
                }
            }
            else
            {
                var context = contextStack.Peek();
                var index = context.CurrentArgumentIndex;
                
                // Check if we can match any method candidate
                foreach (var candidate in context.Target!.Candidates)
                {
                    var delegateType = candidate.GetParameters()[index].ParameterType;

                    if (TryMatchDelegate(context, argsCount, delegateType, out var parameters, out var isFunc, out var _))
                    {
                        found = true;
                        viableCandidates.Add((delegateType, parameters, isFunc));
                    }
                }
            }
            
            if (found)
            {
                (Type delegateType, Type[] delegateParams, bool hasReturnValue)? result;

                if (viableCandidates.Count > 1)
                {
                    if (!TrySelectBestDelegateCandidate(viableCandidates, out result))
                        return new LambdaTypeInferenceResult(result: false);
                }
                else
                {
                    result = viableCandidates.First();
                }

                return new LambdaTypeInferenceResult(
                    result: true,
                    type: result.Value.delegateType,
                    parameters: result.Value.delegateParams,
                    hasReturnValue: result.Value.hasReturnValue);
            }

            return new LambdaTypeInferenceResult(result: false);
        }

        private bool TrySelectBestDelegateCandidate(List<(Type, Type[], bool returnValue)> viableCandidates, [NotNullWhen(true)] out (Type, Type[], bool)? bestCandidate)
        {
            // TODO: at this point it is necessary to take into account lambda bodies to disambiguate between method overloads
            if (viableCandidates.Count == 2 && viableCandidates[0].returnValue != viableCandidates[1].returnValue)
            {
                bestCandidate = viableCandidates.Single(candidate => candidate.returnValue);
                return true;
            }

            bestCandidate = null;
            return false;
        }

        private bool TryMatchDelegate(InfererContext? context, int argsCount, Type delegateType, [NotNullWhen(true)] out Type[]? parameters, out bool isFunc, out bool isAction)
        {
            isFunc = false;
            isAction = false;
            parameters = null;

            if (!ReflectionUtils.IsDelegate(delegateType))
                return false;

            var delegateParameters = delegateType.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance).GetParameters();
            if (delegateParameters.Length != argsCount)
                return false;

            var generics = (context != null) ? context.Generics : new Dictionary<string, Type>();
            if (!TryInstantiateDelegateParameters(delegateType, argsCount, generics, out parameters))
                return false;

            isFunc = delegateType.Name.StartsWith("Func");
            isAction = delegateType.Name.StartsWith("Action");
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
                    // But we already infered its type
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
