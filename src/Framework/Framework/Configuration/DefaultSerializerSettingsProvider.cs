using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using DotVVM.Framework.Utils;
using System.Text.Json.Serialization.Metadata;

namespace DotVVM.Framework.Configuration
{
    public sealed class DefaultSerializerSettingsProvider
    {
        private const int defaultMaxSerializationDepth = 64;
        public readonly JsonSerializerOptions SettingsHtmlUnsafe;
        public readonly JsonSerializerOptions Settings;

        // We need to encode for script tags (i.e. either < or / has to go) and for HTML comments (< and > have to go - https://html.spec.whatwg.org/#comments)
        // The default JavaScriptEncoder is just annoyingly paranoid, I'm not interested in having all non-ASCII characters escaped to unicode codepoints
        // Newtonsoft's EscapeHtml setting just escapes HTML (<, >, &, ', ") and control characters (e.g. newline); and it doesn't have any CVEs open
        // 
        // then they claim it isn't safe to use in JavaScript context because the ECMA-262 standard doesn't allow something in string literals
        // https://tc39.es/ecma262/multipage/ecmascript-language-lexical-grammar.html#prod-DoubleStringCharacter
        //   - it says « SourceCharacter [=any Unicode code point] but not one of " or \ or LineTerminator »
        // ...which isn't allowed in JSON in any context anyway
        public readonly JavaScriptEncoder HtmlSafeLessParanoidEncoder;

        private JsonSerializerOptions CreateSettings()
        {
            return new JsonSerializerOptions()
            {
                Converters = {
                    new DotvvmDateTimeConverter(),
                    new DotvvmObjectConverter(),
                    new DotvvmEnumConverter(),
                    new DotvvmDictionaryConverter(),
                    new DotvvmByteArrayConverter(),
                    new DotvvmCustomPrimitiveTypeConverter(),
#if !DotNetCore
                    new DotvvmTimeOnlyJsonConverter(),
                    new DotvvmDateOnlyJsonConverter(),
#endif
#if NET6_0_OR_GREATER
                    new HalfJsonConverter(),
#endif
                },
                TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
                NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
                Encoder = HtmlSafeLessParanoidEncoder,
                MaxDepth = defaultMaxSerializationDepth
            };
        }

        public static DefaultSerializerSettingsProvider Instance
        {
            get
            {
                if (instance == null)
                    instance = new DefaultSerializerSettingsProvider();
                return instance;
            }
        }
        private static DefaultSerializerSettingsProvider? instance;

        private DefaultSerializerSettingsProvider()
        {
            var encoderSettings = new TextEncoderSettings();
            encoderSettings.AllowRange(UnicodeRanges.All);
            encoderSettings.ForbidCharacters('>', '<');
            HtmlSafeLessParanoidEncoder = JavaScriptEncoder.Create(encoderSettings);
            Settings = CreateSettings();
            Settings.MakeReadOnly();
            SettingsHtmlUnsafe = new JsonSerializerOptions(Settings)
            {
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            SettingsHtmlUnsafe.MakeReadOnly();
        }
    }
}
