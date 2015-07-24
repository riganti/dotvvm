using DotVVM.Framework.Binding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Controls
{
	/// <summary>
	/// Base class for control that allows to select one of its item.
	/// </summary>
	public abstract class Selector : ItemsControl
	{
		protected Selector(string tagName)
			: base(tagName)
		{

		}

		/// <summary>
		/// Gets or sets the name of property in the <see cref="ItemsControl.DataSource"/> collection that will be displayed in the <see cref="Selector"/>.
		/// </summary>
		public string DisplayMember
		{
			get { return (string)GetValue(DisplayMemberProperty); }
			set { SetValue(DisplayMemberProperty, value); }
		}
		public static readonly DotvvmProperty DisplayMemberProperty =
			DotvvmProperty.Register<string, Selector>(t => t.DisplayMember, "");

		/// <summary>
		/// Gets or sets the name of property in the <see cref="ItemsControl.DataSource"/> collection that will be passed to the <see cref="SelectedValue"/> property.
		/// </summary>
		public string ValueMember
		{
			get { return (string)GetValue(ValueMemberProperty); }
			set { SetValue(ValueMemberProperty, value); }
		}
		public static readonly DotvvmProperty ValueMemberProperty =
			DotvvmProperty.Register<string, Selector>(t => t.ValueMember, "");


		/// <summary>
		/// Gets or sets the value selected in the <see cref="Selector"/>.
		/// </summary>
		public object SelectedValue
		{
			get { return GetValue(SelectedValueProperty); }
			set { SetValue(SelectedValueProperty, value); }
		}
		public static readonly DotvvmProperty SelectedValueProperty =
			DotvvmProperty.Register<object, Selector>(t => t.SelectedValue);

		/// <summary>
		/// Gets or sets the command that will be triggered when selected item is changed.
		/// </summary>
		public Action SelectionChanged
		{
			get { return (Action)GetValue(SelectionChangedProperty); }
			set { SetValue(SelectionChangedProperty, value); }
		}
		public static readonly DotvvmProperty SelectionChangedProperty =
			DotvvmProperty.Register<Action, Selector>(t => t.SelectionChanged, null);
	}
}
