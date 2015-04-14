using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Win32;
using Redwood.VS2015Extension.RwHtmlEditorExtensions.Completions.RwHtml.Base;

namespace Redwood.VS2015Extension.RwHtmlEditorExtensions.Classification
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
            string themeId = GetThemeId();
            if (string.IsNullOrWhiteSpace(themeId) == false)
            {
                VisualStudioTheme theme;
                if (Themes.TryGetValue(themeId, out theme))
                {
                    return theme;
                }
            }

            return VisualStudioTheme.Unknown;
        }

        public static string GetThemeId()
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