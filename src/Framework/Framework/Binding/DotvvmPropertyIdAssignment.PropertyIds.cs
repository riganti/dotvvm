using DotVVM.Framework.Controls;
using DotVVM.Framework.Controls.Infrastructure;

namespace DotVVM.Framework.Binding
{

    static partial class DotvvmPropertyIdAssignment
    {
        public static class PropertyIds
        {
            // fields are looked-up automatically for type registered in TypeIds

            /// <seealso cref="DotvvmBindableObject.DataContextProperty" />
            public const uint DotvvmBindableObject_DataContext = TypeIds.DotvvmBindableObject << 16 | 1;

            /// <seealso cref="DotvvmControl.IDProperty" />
            public const uint DotvvmControl_ID = TypeIds.DotvvmControl << 16 | 2;
            /// <seealso cref="DotvvmControl.ClientIDProperty" />
            public const uint DotvvmControl_ClientID = TypeIds.DotvvmControl << 16 | 4;
            /// <seealso cref="DotvvmControl.IncludeInPageProperty" />
            public const uint DotvvmControl_IncludeInPage = TypeIds.DotvvmControl << 16 | 6;

            /// <seealso cref="DotvvmControl.ClientIDModeProperty" />
            public const uint DotvvmControl_ClientIDMode = TypeIds.DotvvmControl << 16 | 1;

            /// <seealso cref="HtmlGenericControl.VisibleProperty" />
            public const uint HtmlGenericControl_Visible = TypeIds.HtmlGenericControl << 16 | 2;
            /// <seealso cref="HtmlGenericControl.InnerTextProperty" />
            public const uint HtmlGenericControl_InnerText = TypeIds.HtmlGenericControl << 16 | 4;
            /// <seealso cref="HtmlGenericControl.HtmlCapabilityProperty" />
            public const uint HtmlGenericControl_HtmlCapability = TypeIds.HtmlGenericControl << 16 | 1;

            /// <seealso cref="Literal.TextProperty" />
            public const uint Literal_Text = TypeIds.Literal << 16 | 2;
            /// <seealso cref="Literal.FormatStringProperty" />
            public const uint Literal_FormatString = TypeIds.Literal << 16 | 4;
            /// <seealso cref="Literal.RenderSpanElementProperty" />
            public const uint Literal_RenderSpanElement = TypeIds.Literal << 16 | 6;
            /// <seealso cref="ButtonBase.ClickProperty" />
            public const uint ButtonBase_Click = TypeIds.ButtonBase << 16 | 2;
            /// <seealso cref="ButtonBase.ClickArgumentsProperty" />
            public const uint ButtonBase_ClickArguments = TypeIds.ButtonBase << 16 | 4;
            /// <seealso cref="ButtonBase.TextProperty" />
            public const uint ButtonBase_Text = TypeIds.ButtonBase << 16 | 8;
            /// <seealso cref="ButtonBase.EnabledProperty" />
            public const uint ButtonBase_Enabled = TypeIds.ButtonBase << 16 | 1;
            /// <seealso cref="ButtonBase.TextOrContentCapability" />
            public const uint ButtonBase_TextOrContentCapability = TypeIds.ButtonBase << 16 | 3;

            /// <seealso cref="Button.ButtonTagNameProperty" />
            public const uint Button_ButtonTagName = TypeIds.Button << 16 | 2;
            /// <seealso cref="Button.IsSubmitButtonProperty" />
            public const uint Button_IsSubmitButton = TypeIds.Button << 16 | 4;
            /// <seealso cref="TextBox.TextProperty" />
            public const uint TextBox_Text = TypeIds.TextBox << 16 | 2;
            /// <seealso cref="TextBox.ChangedProperty" />
            public const uint TextBox_Changed = TypeIds.TextBox << 16 | 4;
            /// <seealso cref="TextBox.TypeProperty" />
            public const uint TextBox_Type = TypeIds.TextBox << 16 | 6;
            /// <seealso cref="TextBox.TextInputProperty" />
            public const uint TextBox_TextInput = TypeIds.TextBox << 16 | 8;
            /// <seealso cref="TextBox.FormatStringProperty" />
            public const uint TextBox_FormatString = TypeIds.TextBox << 16 | 10;
            /// <seealso cref="TextBox.SelectAllOnFocusProperty" />
            public const uint TextBox_SelectAllOnFocus = TypeIds.TextBox << 16 | 12;
            /// <seealso cref="TextBox.EnabledProperty" />
            public const uint TextBox_Enabled = TypeIds.TextBox << 16 | 1;
            /// <seealso cref="TextBox.UpdateTextOnInputProperty" />
            public const uint TextBox_UpdateTextOnInput = TypeIds.TextBox << 16 | 3;
        }
    }
}
