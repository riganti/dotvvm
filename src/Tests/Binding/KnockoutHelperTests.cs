using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.Javascript;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DotVVM.Framework.Tests.Binding;
[TestClass]
public class KnockoutHelperTests
{
    static readonly DotvvmConfiguration config = DotvvmTestHelper.DebugConfig;
    static readonly BindingCompilationService bindingService = config.ServiceProvider.GetRequiredService<BindingCompilationService>();
    static readonly ICommandBinding command = bindingService.Cache.CreateCommand("_this.BoolMethod()", DataContextStack.Create(typeof(TestViewModel)));
    static readonly IStaticCommandBinding clientOnlyStaticCommand = bindingService.Cache.CreateStaticCommand("_this.BoolProp = true", DataContextStack.Create(typeof(TestViewModel)));


    [TestMethod]
    public void NormalEvent_Command()
    {
        var result = KnockoutHelper.GenerateClientPostBackExpression("Click", command, new PlaceHolder(), new PostbackScriptOptions(abortSignal: CodeParameterAssignment.FromIdentifier("signal")));
        Assert.AreEqual("dotvvm.postBack(this,[],\"aS4YcJnv6U6PpCmC\",\"\",null,[\"validate-root\"],[],signal)", result);
    }

    [TestMethod]
    public void NormalEvent_StaticCommand()
    {
        var result = KnockoutHelper.GenerateClientPostBackExpression("Click", clientOnlyStaticCommand, new PlaceHolder(), new PostbackScriptOptions(abortSignal: CodeParameterAssignment.FromIdentifier("signal")));
        Assert.AreEqual("dotvvm.applyPostbackHandlers((options) => options.viewModel.BoolProp(true).BoolProp(),this,[],[],undefined,signal)", result);
    }

    [TestMethod]
    public void KnockoutExpression_Command()
    {
        var result = KnockoutHelper.GenerateClientPostBackExpression("Click", command, new PlaceHolder(), PostbackScriptOptions.KnockoutBinding with { AbortSignal = CodeParameterAssignment.FromIdentifier("signal") });
        Assert.AreEqual("dotvvm.postBack($element,[],\"aS4YcJnv6U6PpCmC\",\"\",$context,[\"validate-root\"],[],signal)", result);
    }

    [TestMethod]
    public void KnockoutExpression_StaticCommand()
    {
        var result = KnockoutHelper.GenerateClientPostBackExpression("Click", clientOnlyStaticCommand, new PlaceHolder(), new PostbackScriptOptions(abortSignal: CodeParameterAssignment.FromIdentifier("signal")).WithDefaults(PostbackScriptOptions.KnockoutBinding));
        Assert.AreEqual("dotvvm.applyPostbackHandlers((options) => options.viewModel.BoolProp(true).BoolProp(),$element,[],[],$context,signal)", result);
    }

    [TestMethod]
    public void Lambda_Command()
    {
        var result = KnockoutHelper.GenerateClientPostbackLambda("Click", command, new PlaceHolder(), new PostbackScriptOptions(abortSignal: CodeParameterAssignment.FromIdentifier("signal")));
        Assert.AreEqual("()=>(dotvvm.postBack($element,[],\"aS4YcJnv6U6PpCmC\",\"\",$context,[\"validate-root\"],[],signal))", result);
    }

    [TestMethod]
    public void Lambda_StaticCommand()
    {
        var result = KnockoutHelper.GenerateClientPostbackLambda("Click", clientOnlyStaticCommand, new PlaceHolder(), new PostbackScriptOptions(abortSignal: CodeParameterAssignment.FromIdentifier("signal")));
        Assert.AreEqual("(...args)=>(dotvvm.applyPostbackHandlers((options) => options.viewModel.BoolProp(true).BoolProp(),$element,[],args,$context,signal))", result);
    }
}
