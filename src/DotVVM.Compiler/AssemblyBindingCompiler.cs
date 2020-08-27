using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Compilation;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Utils;

namespace DotVVM.Compiler
{
    public class AssemblyBindingCompiler : BindingCompiler
    {

#if NET461
        private AssemblyBuilder assemblyBuilder;
        private ModuleBuilder moduleBuilder;
        private TypeBuilder bindingsClass;
#endif
        private int methodCounter;
        public string OutputFileName { get; set; }

        private object locker = new object();

        public AssemblyBindingCompiler(string assemblyName, string className, string outputFileName, DotvvmConfiguration configuration)
            : base(configuration)
        {
#if NET461
            assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.RunAndSave, Path.GetDirectoryName(outputFileName));
            moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName, Path.GetFileName(outputFileName));
            bindingsClass = moduleBuilder.DefineType(className, TypeAttributes.Class | TypeAttributes.Public);
#endif
            OutputFileName = outputFileName;
        }

        protected MethodInfo CompileMethod(LambdaExpression expr)
        {
            var returnType = expr.Type;
            var parameters = expr.Parameters.Select(p => p.Type).ToArray();
            lock (locker)
            {
                var name = "Binding_" + (methodCounter++);
#if  NET461
                
                var method = bindingsClass.DefineMethod(name, MethodAttributes.Public | MethodAttributes.Static, returnType, parameters);
                expr.CompileToMethod(method);
                return method;
#endif
                throw new NotImplementedException();

            }
        }



        //public IExpressionToDelegateCompiler GetExpressionToDelegateCompiler()
        //{
        //    return new ExpressionToDelegateCompiler(this);
        //}
        //class ExpressionToDelegateCompiler : IExpressionToDelegateCompiler
        //{
        //    private AssemblyBindingCompiler assemblyBindingCompiler;

        //    public ExpressionToDelegateCompiler(AssemblyBindingCompiler assemblyBindingCompiler)
        //    {
        //        this.assemblyBindingCompiler = assemblyBindingCompiler;
        //    }

        //    public Delegate Compile(LambdaExpression expression)
        //    {
        //        //assemblyBindingCompiler.CompileMethod(expression);
        //        var realDelegate = expression.Compile();
        //        RefObjectSerializer.RegisterDelegateTranslation(realDelegate, expression);
        //        return realDelegate;
        //    }
        //}

        //public BindingExpressionCompilationInfo PrecompileBinding(ResolvedBinding binding, string id, Type expectedType)
        //{
        //    var compilerAttribute = GetCompilationAttribute(binding.BindingType);
        //    var requirements = compilerAttribute.GetRequirements(binding.BindingType);

        //    var result = new BindingExpressionCompilationInfo();
        //    result.MethodName = TryExecute(binding.BindingNode, "Error while compiling binding to delegate.", requirements.Delegate, () => CompileMethod(compilerAttribute.CompileToDelegate(binding.GetExpression(), binding.DataContextTypeStack, expectedType)));
        //    result.UpdateMethodName = TryExecute(binding.BindingNode, "Error while compiling update delegate.", requirements.UpdateDelegate, () => CompileMethod(compilerAttribute.CompileToUpdateDelegate(binding.GetExpression(), binding.DataContextTypeStack)));
        //    result.OriginalString = TryExecute(binding.BindingNode, "hey, no, that should not happen. Really.", requirements.OriginalString, () => binding.Value);
        //    result.Expression = TryExecute(binding.BindingNode, "Could not get binding expression.", requirements.Expression, () => binding.GetExpression());
        //    result.ActionFilters = TryExecute(binding.BindingNode, "", requirements.ActionFilters, () => GetActionAttributeData(binding.GetExpression()));
        //    result.Javascript = TryExecute(binding.BindingNode, "Could not compile binding to Javascript.", requirements.Javascript, () => compilerAttribute.CompileToJavascript(binding, new CompiledBindingExpression()
        //    {
        //        Expression = result.Expression,
        //        Id = id,
        //        OriginalString = result.OriginalString
        //    }, configuration));
        //    return result;
        //}

        //public override ExpressionSyntax EmitCreateBinding(DefaultViewCompilerCodeEmitter emitter, ResolvedBinding binding)
        //{
        //    throw new NotImplementedException();
        //var info = PrecompileBinding(binding, id, expectedType);
        //if (emitter != null)
        //{
        //    return GetCachedInitializer(emitter, GetCompiledBindingCreation(emitter, info.MethodName, info.UpdateMethodName, info.OriginalString, this.GetAttributeInitializers(info.ActionFilters, emitter)?.ToArray(), info.Javascript, id));
        //}
        //else return null;
        //}

        //protected ExpressionSyntax GetCompiledBindingCreation(DefaultViewCompilerCodeEmitter emitter, string methodName, string updateMethodName, string originalString, ExpressionSyntax[] actionFilters, string javascript, string id)
        //{
        //    var dict = new Dictionary<string, ExpressionSyntax>();
        //    if (methodName != null) dict.Add(nameof(CompiledBindingExpression.Delegate), SyntaxFactory.ParseName(methodName));
        //    if (updateMethodName != null) dict.Add(nameof(CompiledBindingExpression.UpdateDelegate), SyntaxFactory.ParseName(updateMethodName));
        //    if (originalString != null) dict.Add(nameof(CompiledBindingExpression.OriginalString), emitter.EmitValue(originalString));
        //    if (javascript != null) dict.Add(nameof(CompiledBindingExpression.Javascript), emitter.EmitValue(javascript));
        //    if (id != null) dict.Add(nameof(CompiledBindingExpression.Id), emitter.EmitValue(id));
        //    if (actionFilters != null)
        //        dict.Add(nameof(CompiledBindingExpression.ActionFilters),
        //            SyntaxFactory.ArrayCreationExpression(
        //                SyntaxFactory.ArrayType(SyntaxFactory.ParseTypeName(typeof(ActionFilterAttribute).FullName))
        //                    .WithRankSpecifiers(
        //                        SyntaxFactory.SingletonList<ArrayRankSpecifierSyntax>(
        //                            SyntaxFactory.ArrayRankSpecifier(
        //                                SyntaxFactory.SingletonSeparatedList<ExpressionSyntax>(
        //                                    SyntaxFactory.OmittedArraySizeExpression())))),
        //                SyntaxFactory.InitializerExpression(SyntaxKind.CollectionInitializerExpression,
        //                    SyntaxFactory.SeparatedList(actionFilters))));

        //    return SyntaxFactory.ObjectCreationExpression(
        //        SyntaxFactory.ParseTypeName(typeof(CompiledBindingExpression).FullName),
        //        SyntaxFactory.ArgumentList(),
        //        SyntaxFactory.InitializerExpression(SyntaxKind.ObjectInitializerExpression,
        //            SyntaxFactory.SeparatedList(
        //                dict.Select(p =>
        //                     (ExpressionSyntax)SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
        //                        SyntaxFactory.IdentifierName(p.Key),
        //                        p.Value
        //                    )
        //                )
        //            )
        //        )
        //    );
        //}

        //protected NameSyntax GetCacheName(Type type)
        //{
        //    lock (locker)
        //    {
        //        var name = "cache_" + methodCounter++;
        //        bindingsClass.DefineField(name, type, FieldAttributes.Public | FieldAttributes.Static);
        //        return SyntaxFactory.ParseName(bindingsClass.FullName + "." + name);
        //    }
        //}

        //protected ExpressionSyntax GetCachedInitializer(DefaultViewCompilerCodeEmitter emitter, ExpressionSyntax constructor)
        //{
        //    // emit (cache ?? (cache = ctor()))
        //    var cacheId = GetCacheName(typeof(CompiledBindingExpression));
        //    return SyntaxFactory.BinaryExpression(SyntaxKind.CoalesceExpression, cacheId,
        //        SyntaxFactory.ParenthesizedExpression(
        //            SyntaxFactory.AssignmentExpression(SyntaxKind.SimpleAssignmentExpression,
        //                cacheId, constructor)));
        //}

        //protected List<CustomAttributeData> GetActionAttributeData(Expression expression)
        //{
        //    var attributes = new List<CustomAttributeData>();
        //    expression.ForEachMember(m =>
        //    {
        //        attributes.AddRange(m.CustomAttributes.Where(a => typeof(ActionFilterAttribute).IsAssignableFrom(a.AttributeType)));
        //    });
        //    return attributes;
        //}

        //protected IEnumerable<ExpressionSyntax> GetAttributeInitializers(IEnumerable<CustomAttributeData> attributes, DefaultViewCompilerCodeEmitter emitter)
        //{
        //    return attributes?.Select(a => emitter?.EmitAttributeInitializer(a)).ToArray();
        //}

        public void SaveAssembly()
        {
#if NET461
            bindingsClass.CreateType();
            assemblyBuilder.Save(Path.GetFileName(OutputFileName));
#endif
        }
        public IExpressionToDelegateCompiler GetExpressionToDelegateCompiler()
        {
            return new ExpressionToDelegateCompiler(this);
        }
        class ExpressionToDelegateCompiler : IExpressionToDelegateCompiler
        {
            private AssemblyBindingCompiler assemblyBindingCompiler;

            public ExpressionToDelegateCompiler(AssemblyBindingCompiler assemblyBindingCompiler)
            {
                this.assemblyBindingCompiler = assemblyBindingCompiler;
            }

            public Delegate Compile(LambdaExpression expression)
            {
                //assemblyBindingCompiler.CompileMethod(expression);
                var realDelegate = expression.Compile();
                RefObjectSerializer.RegisterDelegateTranslation(realDelegate, expression);
                return realDelegate;
            }
        }

        public void AddSerializedObjects(string typeName, Expression builder, ParameterExpression[] fields)
        {
#if  NET461
            
            var type = moduleBuilder.DefineType(typeName, TypeAttributes.Class | TypeAttributes.Public);
            var builtFields = fields.Select(f => type.DefineField(f.Name, f.Type, FieldAttributes.Static | FieldAttributes.Public)).ToArray();
            var expandedBuilder = ExpressionUtils.Replace(
                Expression.Lambda(builder, fields),
                builtFields.Select(f => Expression.Field(null, f)).ToArray()
            );
            var initMethod = type.DefineMethod("Init", MethodAttributes.Static | MethodAttributes.Public, typeof(void), new[] { typeof(DotvvmConfiguration), typeof(IServiceProvider) });
            var methodExpression = Expression.Lambda(expandedBuilder, RefObjectSerializer.DotvvmConfigurationParameter, RefObjectSerializer.ServiceProviderParameter);
            methodExpression.CompileToMethod(initMethod);
            type.CreateType();
#endif
        }

        public class BindingExpressionCompilationInfo
        {
            public List<CustomAttributeData> ActionFilters { get; internal set; }
            public Expression Expression { get; internal set; }
            public string Javascript { get; internal set; }
            public string MethodName { get; internal set; }
            public string OriginalString { get; internal set; }
            public string UpdateMethodName { get; internal set; }
        }
    }
}
