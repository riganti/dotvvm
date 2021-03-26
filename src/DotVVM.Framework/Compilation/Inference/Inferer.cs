#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.Parser.Binding.Parser;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Inference
{
    public class Inferer
    {
        private readonly Stack<InfererContext> stack;

        public Inferer()
        {
            stack = new Stack<InfererContext>();
        }

        public void BeginFunctionCall(MethodGroupExpression? target, int argsCount)
        {
            if (target != null && target.HasExtensionCandidates)
            {
                stack.Push(new InfererContext(target, argsCount + 1) { IsExtensionCall = true });
                SetArgumentInternal(target.Target, 0);
            }
            else
            {
                stack.Push(new InfererContext(target, argsCount));
            }
        }

        public void EndFunctionCall()
        {
            stack.Pop();
        }

        public void SetArgument(Expression expression, int index)
        {
            var context = stack.Peek();
            index = (context.IsExtensionCall) ? index + 1 : index;
            SetArgumentInternal(expression, index);
        }

        private void SetArgumentInternal(Expression expression, int index)
        {
            var context = stack.Peek();
            context.CurrentArgumentIndex = index;
            context.Arguments[index] = expression;

            RefineCandidates(index);
        }

        public void SetProbedArgumentIndex(int index)
        {
            var context = stack.Peek(); 
            context.CurrentArgumentIndex = (context.IsExtensionCall) ? index + 1 : index;
        }

        public bool TryInferLambdaParameters(int argsCount, [NotNullWhen(true)] out Type[]? lambdaParameters)
        {
            if (stack.Count == 0)
            {
                lambdaParameters = null;
                return false;
            }

            var context = stack.Peek();
            var index = context.CurrentArgumentIndex;

            foreach (var candidate in context.Target!.Candidates)
            {
                var parameter = candidate.GetParameters()[index].ParameterType;
                if (!ReflectionUtils.IsDelegate(parameter))
                    continue;

                var delegateParameters = parameter.GetMethod("Invoke", BindingFlags.Public | BindingFlags.Instance).GetParameters();
                if (delegateParameters.Length != argsCount)
                    continue;

                if (!TryInstantiateLambdaParameters(parameter, argsCount, context.Generics, out var parameters))
                    continue;

                lambdaParameters = parameters;
                return true;
            }

            lambdaParameters = null;
            return false;
        }

        private void RefineCandidates(int index)
        {
            var context = stack.Peek();
            var argument = context.Arguments[index];
            var argumentType = argument.Type;
            if (context.Target == null)
                return;

            var newCandidates = new List<MethodInfo>();
            var newInstantiations = new Dictionary<string, HashSet<Type>>();

            // Check if we can remove some candidates
            // Also try to infer generics based on provided argument
            var tempInstantiations = new Dictionary<string, Type>();
            foreach (var candidate in context.Target.Candidates.Where(c => c.GetParameters().Length > index))
            {
                var parameters = candidate.GetParameters();
                var parameterType = parameters[index].ParameterType;

                if (parameterType.IsGenericParameter)
                {
                    tempInstantiations.Add(parameterType.Name, argumentType);
                }
                else if (parameterType.ContainsGenericParameters)
                {
                    // Check if we already infered instantion for these generics
                    if (!parameterType.GetGenericArguments().Any(param => !context.Generics.ContainsKey(param.Name)))
                        continue;

                    // Try to infer instantiation based on given argument
                    tempInstantiations.Clear();
                    var result = TryInferInstantiation(parameterType, argumentType, tempInstantiations);
                    if (!result)
                        continue;
                }

                // Fill instantations
                foreach (var (key, val) in tempInstantiations)
                {
                    if (!newInstantiations.ContainsKey(key))
                        newInstantiations[key] = new HashSet<Type>();
                    newInstantiations[key].Add(val);
                }

                newCandidates.Add(candidate);
            }

            // Check if we can infer some generics
            foreach (var (key, val) in newInstantiations.Where(inst => inst.Value.Count == 1))
                context.Generics[key] = val.First();

            context.Target.Candidates = newCandidates;
        }

        private bool TryInferInstantiation(Type generic, Type concrete, Dictionary<string, Type> generics)
        {
            if (generic.IsGenericParameter)
            {
                // We found the instantiation
                generics.Add(generic.Name, concrete);
                return true;
            }
            else if (ReflectionUtils.IsEnumerable(generic))
            {
                if (!ReflectionUtils.IsEnumerable(concrete))
                    return false;

                var genericElementType = ReflectionUtils.GetEnumerableType(generic);
                var concreteElementType = ReflectionUtils.GetEnumerableType(concrete);
                if (genericElementType == null || concreteElementType == null)
                    return false;

                return TryInferInstantiation(genericElementType, concreteElementType, generics);
            }
            else if (generic.IsGenericType)
            {
                // Check that the given types can be compatible after instantiation
                // TODO: we should also check for any generic constraints
                var genericTypeDef = generic.GetGenericTypeDefinition();
                if (!concrete.IsAssignableToGenericType(genericTypeDef))
                    return false;

                var genericElementTypes = generic.GetGenericArguments();
                var concreteElementTypes = concrete.GetGenericArguments();
                for (var index = 0; index < genericElementTypes.Length; index++)
                {
                    var genericArg = genericElementTypes[index];
                    var concreteArg = concreteElementTypes[index];

                    if (!TryInferInstantiation(genericArg, concreteArg, generics))
                        return false;
                }

                return true;
            }

            return false;
        }

        private bool TryInstantiateLambdaParameters(Type generic, int argsCount, Dictionary<string, Type> generics, [NotNullWhen(true)] out Type[]? instantiation)
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
