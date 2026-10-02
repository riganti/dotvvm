using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    /// <summary>
    /// Invokes a command when the specified keyboard shortcut is pressed.
    /// </summary>
    [ControlMarkupOptions(AllowContent = false)]
    public class CommandShortcut : DotvvmControl
    {
        [MarkupOptions(AllowHardCodedValue = false, Required = true)]
        public ICommandBinding Command
        {
            get { return (ICommandBinding)GetValue(CommandProperty)!; }
            set { SetValue(CommandProperty, value); }
        }
        public static readonly DotvvmProperty CommandProperty =
            DotvvmProperty.Register<ICommandBinding, CommandShortcut>(c => c.Command, null);

        [MarkupOptions(Required = true)]
        public ShortcutKeys Key
        {
            get { return (ShortcutKeys)GetValue(KeyProperty)!; }
            set { SetValue(KeyProperty, value); }
        }
        public static readonly DotvvmProperty KeyProperty =
            DotvvmProperty.Register<ShortcutKeys, CommandShortcut>(c => c.Key);

        public bool Ctrl
        {
            get { return (bool)GetValue(CtrlProperty)!; }
            set { SetValue(CtrlProperty, value); }
        }
        public static readonly DotvvmProperty CtrlProperty =
            DotvvmProperty.Register<bool, CommandShortcut>(c => c.Ctrl, false);

        public bool Shift
        {
            get { return (bool)GetValue(ShiftProperty)!; }
            set { SetValue(ShiftProperty, value); }
        }
        public static readonly DotvvmProperty ShiftProperty =
            DotvvmProperty.Register<bool, CommandShortcut>(c => c.Shift, false);

        public bool Alt
        {
            get { return (bool)GetValue(AltProperty)!; }
            set { SetValue(AltProperty, value); }
        }
        public static readonly DotvvmProperty AltProperty =
            DotvvmProperty.Register<bool, CommandShortcut>(c => c.Alt, false);

        public bool Enabled
        {
            get { return (bool)GetValue(EnabledProperty)!; }
            set { SetValue(EnabledProperty, value); }
        }
        public static readonly DotvvmProperty EnabledProperty =
            DotvvmProperty.Register<bool, CommandShortcut>(c => c.Enabled, true);

        /// <summary>
        /// Gets or sets the ID of the control in which the shortcut is active.
        /// If omitted, the shortcut is active in the entire document.
        /// </summary>
        [MarkupOptions(AllowBinding = false)]
        public string? TargetControlID
        {
            get { return (string?)GetValue(TargetControlIDProperty); }
            set { SetValue(TargetControlIDProperty, value); }
        }
        public static readonly DotvvmProperty TargetControlIDProperty =
            DotvvmProperty.Register<string?, CommandShortcut>(c => c.TargetControlID);

        public CommandShortcut()
        {
            SetValue(Validation.EnabledProperty, false);
        }

        protected override void RenderBeginTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            var group = new KnockoutBindingGroup();
            group.Add("command", KnockoutHelper.GenerateClientPostbackLambda(nameof(Command), Command, this));
            group.Add("key", this, KeyProperty);
            group.Add("ctrl", this, CtrlProperty);
            group.Add("shift", this, ShiftProperty);
            group.Add("alt", this, AltProperty);
            group.Add("enabled", this, EnabledProperty);

            if (TargetControlID is { } targetControlId)
            {
                var target = GetNamingContainer().FindControlInContainer(targetControlId, throwIfNotFound: true)!;
                group.Add("targetId", target.ClientID!.Value.GetJsExpression(this));
            }

            writer.WriteKnockoutDataBindComment("dotvvm-command-shortcut", group.ToString());
            base.RenderBeginTag(writer, context);
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            base.RenderEndTag(writer, context);
            writer.WriteKnockoutDataBindEndComment();
        }
    }

    public enum ShortcutKeys
    {
        None = 0,
        Backspace = 8,
        Tab = 9,
        Enter = 13,
        Pause = 19,
        CapsLock = 20,
        Escape = 27,
        Space = 32,
        PageUp = 33,
        PageDown = 34,
        End = 35,
        Home = 36,
        Left = 37,
        Up = 38,
        Right = 39,
        Down = 40,
        Insert = 45,
        Delete = 46,
        D0 = 48,
        D1 = 49,
        D2 = 50,
        D3 = 51,
        D4 = 52,
        D5 = 53,
        D6 = 54,
        D7 = 55,
        D8 = 56,
        D9 = 57,
        A = 65,
        B = 66,
        C = 67,
        D = 68,
        E = 69,
        F = 70,
        G = 71,
        H = 72,
        I = 73,
        J = 74,
        K = 75,
        L = 76,
        M = 77,
        N = 78,
        O = 79,
        P = 80,
        Q = 81,
        R = 82,
        S = 83,
        T = 84,
        U = 85,
        V = 86,
        W = 87,
        X = 88,
        Y = 89,
        Z = 90,
        NumPad0 = 96,
        NumPad1 = 97,
        NumPad2 = 98,
        NumPad3 = 99,
        NumPad4 = 100,
        NumPad5 = 101,
        NumPad6 = 102,
        NumPad7 = 103,
        NumPad8 = 104,
        NumPad9 = 105,
        Multiply = 106,
        Add = 107,
        Subtract = 109,
        DecimalPoint = 110,
        Divide = 111,
        F1 = 112,
        F2 = 113,
        F3 = 114,
        F4 = 115,
        F5 = 116,
        F6 = 117,
        F7 = 118,
        F8 = 119,
        F9 = 120,
        F10 = 121,
        F11 = 122,
        F12 = 123
    }
}
