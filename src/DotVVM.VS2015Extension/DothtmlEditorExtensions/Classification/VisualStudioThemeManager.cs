using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using DotVVM.VS2015Extension.DothtmlEditorExtensions.Completions.Dothtml.Base;

namespace DotVVM.VS2015Extension.DothtmlEditorExtensions.Classification
{
    public class VisualStudioThemeManager
    {
        private static readonly IDictionary<string, VisualStudioTheme> Themes = new Dictionary<string, VisualStudioTheme>()
        {
            { "de3dbbcd-f642-433c-8353-8f1df4370aba", VisualStudioTheme.Light },
            { "1ded0138-47ce-435e-84ef-9ec1f439b749", VisualStudioTheme.Dark },
            { "a4d6a176-b948-4b29-8c66-53c97a1ed7d0", VisualStudioTheme.Blue }
        };

        public static VisualStudioTheme GetCurrentTheme()
        {
            string themeId = GetThemeIdFromRegistry();
            if (string.IsNullOrWhiteSpace(themeId))
            {
                return GetThemeFromColorSettings();
            }
            else
            { 
                VisualStudioTheme theme;
                if (Themes.TryGetValue(themeId, out theme))
                {
                    return theme;
                }
            }

            return VisualStudioTheme.Unknown;
        }

        private static VisualStudioTheme GetThemeFromColorSettings()
        {
            try
            {
                var background = CompletionHelper.DTE.Properties["FontsAndColors", "TextEditor"].Item("FontsAndColorsItems").Object.Item("Plain Text").Background;
                var plainTextBackgroundColor = System.Drawing.ColorTranslator.FromOle((int)background);
                if (plainTextBackgroundColor.R < 100)
                {
                    return VisualStudioTheme.Dark;
                }
            }
            catch (Exception ex)
            {
                LogService.LogError(new Exception("Cannot detect current theme!", ex));
            }
            return VisualStudioTheme.Unknown;
        }

        public static string GetThemeIdFromRegistry()
        {
            const string CategoryName = "General";
            const string ThemePropertyName = "CurrentTheme";
            string keyName = string.Format(@"Software\Microsoft\VisualStudio\{0}\{1}", CompletionHelper.DTE.Version, CategoryName);
            
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyName))
            {
                if (key != null)
                {
                    return (string)key.GetValue(ThemePropertyName, string.Empty);
                }
            }

            return null;
        }
    }
}