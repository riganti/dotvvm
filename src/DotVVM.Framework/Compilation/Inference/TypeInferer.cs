#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Compilation.Binding;
using DotVVM.Framework.Compilation.Inference.Results;
using DotVVM.Framework.Utils;

namespace DotVVM.Framework.Compilation.Inference
{
    internal partial class TypeInferer : ITypeInferer, IFluentInferer
    {
        private readonly Stack<InfererContext> contextStack;
        private Type? expectedType;

        public TypeInferer()
        {
            this.contextStack = new Stack<InfererContext>();
        }

        public void BeginFunctionCall(MethodGroupExpression? target, int argsCount)
        {
            if (target != null && target.HasExtensionCandidates)
            {
                contextStack.Push(new InfererContext(target, argsCount + 1) { IsExtensionCall = true });
                SetArgumentInternal(target.Target, 0);
            }
            else
            {
                contextStack.Push(new InfererContext(target, argsCount));
            }
        }

        public void EndFunctionCall()
        {
            contextStack.Pop();
        }

        public void SetArgument(Expression expression, int index)
        {
            var context = contextStack.Peek();
            index = (context.IsExtensionCall) ? index + 1 : index;
            SetArgumentInternal(expression, index);
        }

        private void SetArgumentInternal(Expression expression, int index)
        {
            var context = contextStack.Peek();
            context.CurrentArgumentIndex = index;
            context.Arguments[index] = expression;

            RefineCandidates(index);
        }

        public void SetProbedArgumentIndex(int index)
        {
            var context = contextStack.Peek();
            context.CurrentArgumentIndex = (context.IsExtensionCall) ? index + 1 : index;
        }

        private void RefineCandidates(int index)
        {
            var context = contextStack.Peek();
            var argument = context.Arguments[index];
            if (argument == null)
                return;

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
                    // Check if we already inferred instantiation for these generics
                    if (!parameterType.GetGenericArguments().Any(param => !context.Generics.ContainsKey(param.Name)))
                        continue;

                    // Try to infer instantiation based on given argument
                    tempInstantiations.Clear();
                    var result = TryInferInstantiation(parameterType, argumentType, tempInstantiations);
                    if (!result)
                        continue;
                }

                // Fill instantiations
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
                if (!concrete.IsAssignableToGenericType(genericTypeDef, out var commonType))
                    return false;

                var genericElementTypes = generic.GetGenericArguments();
                var concreteElementTypes = commonType.GetGenericArguments();
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

        public IFluentInferer Infer(Type? expectedType = null)
        {
            this.expectedType = expectedType;
            return this;
        }
    }
}
