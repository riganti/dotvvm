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

        /// <summary>
        /// Registers a server-side style for <typeparamref name="T"/> that is applied upon compilation.
        /// </summary>
        /// <typeparam name="T">All objects of this type will have the style applied to them unless <paramref name="matcher"/> is specified.</typeparam>
        /// <param name="matcher">If this function returns true, the style will be applied, otherwise not.</param>
        /// <param name="allowDerived">Also allow classes that are derived from <typeparamref name="T"/>.</param>
        /// <returns>A <see cref="StyleBuilder{T}"/> that can be used to style the control.</returns>
        public StyleBuilder<T> Register<T>(Func<StyleMatchContext, bool> matcher = null, bool allowDerived = true)
            where T : DotvvmBindableObject
        {
            var styleBuilder = new StyleBuilder<T>(matcher, allowDerived);
            Styles.Add(styleBuilder.GetStyle());
            return styleBuilder;
        }

        /// <summary>
        /// Registers a server-side style for <paramref name="type"/> that is applied upon compilation.
        /// </summary>
        /// <param name="type">All objects of this type will have the style applied to them unless <paramref name="matcher"/> is specified.</param>
        /// <param name="matcher">If this function returns true, the style will be applied, otherwise not.</param>
        /// <param name="allowDerived">Also allow classes that are derived from <paramref name="type"/>.</param>
        /// <returns>A <see cref="StyleBuilder{T}"/> that can be used to style the control.</returns>
        public StyleBuilder<DotvvmBindableObject> Register(Type type, Func<StyleMatchContext, bool> matcher = null, bool allowDerived = true)
        {
            var styleBuilder = new StyleBuilder<DotvvmBindableObject>(matcher, allowDerived);
            Styles.Add(styleBuilder.GetStyle());
            return styleBuilder;
        }

        /// <summary>
        /// Registers a server-side style for a HTML tag that is applied upon compilation.
        /// </summary>
        /// <param name="tagName">The HTML tag the style is applied to.</param>
        /// <param name="matcher">If this function returns true, the style will be applied, otherwise not.</param>
        /// <returns>A <see cref="StyleBuilder{T}"/> that can be used to style the control.</returns>
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
