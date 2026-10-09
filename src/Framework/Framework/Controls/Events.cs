using System;
using System.Collections.Generic;
using System.Linq;
using DotVVM.Framework.Binding;
using DotVVM.Framework.Binding.Expressions;
using DotVVM.Framework.Compilation.ControlTree;
using DotVVM.Framework.Compilation.ControlTree.Resolved;
using DotVVM.Framework.Compilation.Validation;
using DotVVM.Framework.Hosting;

namespace DotVVM.Framework.Controls
{
    [ControlMarkupOptions(AllowContent = false)]
    public class Events : DotvvmControl
    {
        [AttachedProperty(typeof(Command))]
        public static ActiveDotvvmProperty ClickProperty =
            ActiveDotvvmProperty.RegisterCommandToAttribute<Events>("Click", "onclick");

        [AttachedProperty(typeof(Command))]
        public static ActiveDotvvmProperty DoubleClickProperty =
            ActiveDotvvmProperty.RegisterCommandToAttribute<Events>("DoubleClick", "ondblclick");

        [AttachedProperty(typeof(Command))]
        [PropertyGroup("Key-")]
        [MarkupOptions(AllowHardCodedValue = false)]
        public static DotvvmPropertyGroup KeyGroupDescriptor =
            DelegateActionPropertyGroup<Command>.Register<Events>("Key-", "Key", AddKeyBindings);

        [AttachedProperty(typeof(bool))]
        [PropertyGroup("KeyEnabled-")]
        public static DotvvmPropertyGroup KeyEnabledGroupDescriptor =
            DotvvmPropertyGroup.Register<bool, Events>("KeyEnabled-", "KeyEnabled");

        public Events()
        {
            SetValue(Validation.EnabledProperty, false);
        }

        private static void AddKeyBindings(
            IHtmlWriter writer,
            IDotvvmRequestContext context,
            DotvvmPropertyGroup group,
            DotvvmControl control,
            IEnumerable<DotvvmProperty> properties)
        {
            var shortcuts = properties
                .Cast<GroupedDotvvmProperty>()
                .Select(property => CreateShortcutBinding(control, property))
                .ToArray();
            var expression = $"[{string.Join(",", shortcuts)}]";

            if (control is Events)
            {
                writer.WriteKnockoutDataBindComment("dotvvm-command-shortcut", expression);
            }
            else
            {
                writer.AddKnockoutDataBind("dotvvm-command-shortcut", expression);
            }
        }

        private static string CreateShortcutBinding(DotvvmControl control, GroupedDotvvmProperty property)
        {
            if (!TryParseShortcut(property.GroupMemberName, out var shortcut))
            {
                throw new DotvvmControlException(control, $"Invalid keyboard shortcut '{property.GroupMemberName}'.");
            }

            var command = control.GetCommandBinding(property)
                ?? throw new DotvvmControlException(control, $"A command binding is required in {property.Name}.");
            var binding = new KnockoutBindingGroup();
            binding.Add("command", KnockoutHelper.GenerateClientPostbackLambda(property.Name, command, control));
            binding.AddValue("key", shortcut.Key);
            binding.AddValue("ctrl", shortcut.Ctrl);
            binding.AddValue("alt", shortcut.Alt);
            binding.AddValue("shift", shortcut.Shift);

            var enabledProperty = KeyEnabledGroupDescriptor.GetDotvvmProperty(property.GroupMemberName);
            if (control.IsPropertySet(enabledProperty, inherit: false))
            {
                binding.Add("enabled", control, enabledProperty);
            }
            else
            {
                binding.AddValue("enabled", true);
            }
            return binding.ToString();
        }

        protected override void RenderEndTag(IHtmlWriter writer, IDotvvmRequestContext context)
        {
            if (Properties.Keys.Any(
                property => property is GroupedDotvvmProperty grouped && grouped.PropertyGroup == KeyGroupDescriptor))
            {
                writer.WriteKnockoutDataBindEndComment();
            }
            base.RenderEndTag(writer, context);
        }

        [ControlUsageValidator(IncludeAttachedProperties = true)]
        public static IEnumerable<ControlUsageError> ValidateUsage(ResolvedControl control)
        {
            var shortcuts = control.Properties
                .Where(p => p.Key is GroupedDotvvmProperty property && property.PropertyGroup == KeyGroupDescriptor)
                .ToArray();
            var shortcutNames = shortcuts
                .Select(p => ((GroupedDotvvmProperty)p.Key).GroupMemberName)
                .ToHashSet(StringComparer.Ordinal);

            foreach (var shortcut in shortcuts)
            {
                var property = (GroupedDotvvmProperty)shortcut.Key;
                if (!TryParseShortcut(property.GroupMemberName, out _))
                {
                    yield return new ControlUsageError(
                        $"The shortcut '{property.GroupMemberName}' must contain each of Ctrl, Alt and Shift at most once, followed by a supported key.",
                        shortcut.Value.DothtmlNode);
                }
            }

            foreach (var enabled in control.Properties
                .Where(p => p.Key is GroupedDotvvmProperty property && property.PropertyGroup == KeyEnabledGroupDescriptor))
            {
                var property = (GroupedDotvvmProperty)enabled.Key;
                if (!shortcutNames.Contains(property.GroupMemberName))
                {
                    yield return new ControlUsageError(
                        $"{property.Name} can only be used together with Key-{property.GroupMemberName}.",
                        enabled.Value.DothtmlNode);
                }
            }
        }

        private static bool TryParseShortcut(string value, out KeyboardShortcut shortcut)
        {
            shortcut = default;
            var parts = value.Split('+');
            if (parts.Length is < 1 or > 4 || parts.Any(string.IsNullOrEmpty))
            {
                return false;
            }

            var modifiers = new HashSet<string>(StringComparer.Ordinal);
            foreach (var modifier in parts.Take(parts.Length - 1))
            {
                if (modifier is not ("Ctrl" or "Alt" or "Shift") || !modifiers.Add(modifier))
                {
                    return false;
                }
            }

            var key = parts[^1];
            if (!Enum.TryParse<ShortcutKeys>(key, ignoreCase: false, out var parsedKey)
                || !Enum.IsDefined(typeof(ShortcutKeys), parsedKey))
            {
                return false;
            }

            shortcut = new KeyboardShortcut(
                key,
                modifiers.Contains("Ctrl"),
                modifiers.Contains("Alt"),
                modifiers.Contains("Shift"));
            return true;
        }

        private readonly record struct KeyboardShortcut(string Key, bool Ctrl, bool Alt, bool Shift);

        private enum ShortcutKeys
        {
            Backspace,
            Tab,
            Enter,
            Pause,
            CapsLock,
            Escape,
            Space,
            PageUp,
            PageDown,
            End,
            Home,
            Left,
            Up,
            Right,
            Down,
            Insert,
            Delete,
            D0,
            D1,
            D2,
            D3,
            D4,
            D5,
            D6,
            D7,
            D8,
            D9,
            A,
            B,
            C,
            D,
            E,
            F,
            G,
            H,
            I,
            J,
            K,
            L,
            M,
            N,
            O,
            P,
            Q,
            R,
            S,
            T,
            U,
            V,
            W,
            X,
            Y,
            Z,
            NumPad0,
            NumPad1,
            NumPad2,
            NumPad3,
            NumPad4,
            NumPad5,
            NumPad6,
            NumPad7,
            NumPad8,
            NumPad9,
            Multiply,
            Add,
            Subtract,
            DecimalPoint,
            Divide,
            F1,
            F2,
            F3,
            F4,
            F5,
            F6,
            F7,
            F8,
            F9,
            F10,
            F11,
            F12
        }
    }
}
