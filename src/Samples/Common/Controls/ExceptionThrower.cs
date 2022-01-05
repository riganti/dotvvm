using System;
using System.Collections.Generic;
using System.Text;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Controls;
using DotVVM.Framework.Hosting;

namespace DotVVM.Samples.Common.Controls
{
    public class ExceptionThrower : DotvvmControl 
    {

        public ExceptionThrowerStage Stage
        {
            get { return (ExceptionThrowerStage)GetValue(StageProperty); }
            set { SetValue(StageProperty, value); }
        }
        public static readonly DotvvmProperty StageProperty
            = DotvvmProperty.Register<ExceptionThrowerStage, ExceptionThrower>(c => c.Stage, ExceptionThrowerStage.Render);


        protected override void OnInit(IDotvvmRequestContext context)
        {
            if (Stage == ExceptionThrowerStage.Init)
            {
                throw new Exception("ExceptionThrower");
            }
            base.OnInit(context);
        }

        protected override void OnLoad(IDotvvmRequestContext context)
        {
            if (Stage == ExceptionThrowerStage.Load)
            {
                throw new Exception("ExceptionThrower");
            }
            base.OnLoad(context);
        }

        protected override void OnPreRender(IDotvvmRequestContext context)
        {
            if (Stage == ExceptionThrowerStage.PreRender)
            {
                throw new Exception("ExceptionThrower");
            }
            base.OnPreRender(context);
        }

        public override void Render(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (Stage == ExceptionThrowerStage.Render)
            {
                throw new Exception("ExceptionThrower");
            }
            base.Render(writer, context);
        }
    }

    public enum ExceptionThrowerStage
    {
        Render,
        Init,
        Load,
        PreRender
    }
}
