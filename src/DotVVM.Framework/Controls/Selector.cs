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
		{ }

        /// <summary>
        /// Gets or sets the name of property in the DataSource collection that will be displayed in the control.
        /// </summary>
        public string DisplayMember
		{
			get { return (string)GetValue(DisplayMemberProperty); }
			set { SetValue(DisplayMemberProperty, value); }
		}
		public static readonly DotvvmProperty DisplayMemberProperty =
			DotvvmProperty.Register<string, Selector>(t => t.DisplayMember, "");

        /// <summary>
        /// Gets or sets the name of property in the DataSource collection that will be passed to the SelectedValue property when the item is selected.
        /// </summary>
        public string ValueMember
		{
			get { return (string)GetValue(ValueMemberProperty); }
			set { SetValue(ValueMemberProperty, value); }
		}
		public static readonly DotvvmProperty ValueMemberProperty =
			DotvvmProperty.Register<string, Selector>(t => t.ValueMember, "");


		/// <summary>
		/// Gets or sets the value of the selected item.
		/// </summary>
		public object SelectedValue
		{
			get { return GetValue(SelectedValueProperty); }
			set { SetValue(SelectedValueProperty, value); }
		}
		public static readonly DotvvmProperty SelectedValueProperty =
			DotvvmProperty.Register<object, Selector>(t => t.SelectedValue);

		/// <summary>
		/// Gets or sets the command that will be triggered when the selection is changed.
		/// </summary>
		public Command SelectionChanged
		{
			get { return (Command)GetValue(SelectionChangedProperty); }
			set { SetValue(SelectionChangedProperty, value); }
		}
		public static readonly DotvvmProperty SelectionChangedProperty =
			DotvvmProperty.Register<Command, Selector>(t => t.SelectionChanged, null);
	}
}
