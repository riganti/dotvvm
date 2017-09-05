using System;
using System.Collections.Generic;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation.Styles
{
    public class StyleRepository
    {
        private DotvvmConfiguration configuration;

        public StyleRepository(DotvvmConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public List<IStyle> Styles { get; set; } = new List<IStyle>();

        public StyleMatcher CreateMatcher()
        {
            return new StyleMatcher(Styles, configuration);
        }

        public StyleBuilder<T> Register<T>(Func<StyleMatchContext, bool> matcher = null, bool allowDerived = true)
            where T : DotvvmBindableObject
        {
            var styleBuilder = new StyleBuilder<T>(matcher, allowDerived);
            Styles.Add(styleBuilder.GetStyle());
            return styleBuilder;
        }

        public StyleBuilder<HtmlGenericControl> Register(string tagName, Func<StyleMatchContext, bool> matcher = null)
        {
            if (matcher != null)
            {
                return Register<HtmlGenericControl>(m => (string)m.Control.ConstructorParameters[0] == tagName && matcher(m), false);
            }
            else
            {
                return Register<HtmlGenericControl>(m => (string)m.Control.ConstructorParameters[0] == tagName, false);
            }
        }
    }
}
