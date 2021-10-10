using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Configuration;
using DotVVM.Framework.Controls;

namespace DotVVM.Framework.Compilation.Styles
{
    public class StyleRepository
    {
        private readonly DotvvmConfiguration configuration;

        public StyleRepository(DotvvmConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public IList<IStyle> Styles
        {
            get => _styles;
            set
            {
                ThrowIfFrozen();
                _styles = value;
            }
        }
        private IList<IStyle> _styles = new FreezableList<IStyle>();

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
        public StyleBuilder<T> Register<T>(Func<StyleMatchContext<T>, bool>? matcher = null, bool allowDerived = true)
            where T : DotvvmBindableObject
        {
            ThrowIfFrozen();
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
        public StyleBuilder<DotvvmBindableObject> Register(Type type, Func<IStyleMatchContext, bool>? matcher = null, bool allowDerived = true)
        {
            ThrowIfFrozen();
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
        public StyleBuilder<HtmlGenericControl> Register(string tagName, Func<StyleMatchContext<HtmlGenericControl>, bool>? matcher = null)
        {
            ThrowIfFrozen();
            if (matcher != null)
            {
                return Register<HtmlGenericControl>(m => (string)m.Control.ConstructorParameters![0] == tagName && matcher(m), false);
            }
            else
            {
                return Register<HtmlGenericControl>(m => (string)m.Control.ConstructorParameters![0] == tagName, false);
            }
        }

        /// <summary>
        /// Registers a server-side style for a root component.
        /// </summary>
        /// <returns>A <see cref="StyleBuilder{T}"/> that can be used to style the control.</returns>
        public StyleBuilder<DotvvmControl> RegisterRoot() =>
            Register<DotvvmControl>(c => c.Parent is null);

        /// <summary>
        /// Registers a server-side style for any component of type DotvvmControl (which is almost everything with exception of special objects like GridViewColumn)
        /// </summary>
        /// <returns>A <see cref="StyleBuilder{T}"/> that can be used to style the control.</returns>
        public StyleBuilder<DotvvmControl> RegisterAnyControl(Func<StyleMatchContext<DotvvmControl>, bool>? matcher = null) =>
            Register<DotvvmControl>(matcher);

        /// <summary>
        /// Registers a server-side style for any component (including special objects like GridViewColumn)
        /// </summary>
        /// <returns>A <see cref="StyleBuilder{T}"/> that can be used to style the control.</returns>
        public StyleBuilder<DotvvmBindableObject> RegisterAnyObject(Func<StyleMatchContext<DotvvmBindableObject>, bool>? matcher = null) =>
            Register<DotvvmBindableObject>(matcher);

        /// <summary>
        /// Registers a server-side style for any component that have specified Styles.Tag (including special objects like GridViewColumn)
        /// </summary>
        /// <returns>A <see cref="StyleBuilder{T}"/> that can be used to style the control.</returns>
        public StyleBuilder<DotvvmBindableObject> RegisterForTag(string tag, Func<StyleMatchContext<DotvvmBindableObject>, bool>? matcher = null) =>
            Register<DotvvmBindableObject>(m => m.HasTag(tag) && matcher?.Invoke(m) != false);

        
        

        private bool isFrozen = false;
        private void ThrowIfFrozen()
        {
            if (isFrozen)
                throw FreezableUtils.Error(nameof(StyleRepository));
        }
        public void Freeze()
        {
            this.isFrozen = true;
            FreezableList.Freeze(ref this._styles);
        }

        public override string ToString() => $"Style Repository:\n{string.Join("\n---\n", Styles)}";
    }
}
