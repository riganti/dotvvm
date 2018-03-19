using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if DotNetCore
#else
using System.Threading;
#endif

using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.PostBack
{
    public class PostBackHandlersViewModel : DotvvmViewModelBase
    {


        public int LastCommandValue { get; set; }

        public bool IsEnabled { get; set; }

        public List<MessageDate> Generated { get; set; } = new List<MessageDate>()
        {
            new MessageDate() { Message = "Generated 1", Value = 4 },
            new MessageDate() { Message = "Generated 2", Value = 5 }
        };

        public override Task Init()
        {
            var lang = Context.Query["lang"];
            if (lang != null)
            {
                var culture = CultureInfo.GetCultureInfo(lang);
#if DotNetCore
                CultureInfo.CurrentCulture = CultureInfo.CurrentUICulture = culture;
#else
                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture = culture;
#endif
            }
            return base.Init();
        }

        public void DoWork(int value)
        {
            LastCommandValue = value;
        }

        public void ChangeLanguageEN()
        {
            var culture = CultureInfo.GetCultureInfo("en-US");
            SetCulture(culture);
        }

        private void SetCulture(CultureInfo culture)
        {

            var builder = new UriBuilder(Context.HttpContext.Request.Url);
            builder.Query = $"lang={culture.Name}";
            Context.RedirectToUrl(builder.Uri.AbsoluteUri);

        }

        public void ChangeLanguageCZ()
        {
            var culture = CultureInfo.GetCultureInfo("cs-CZ");
            SetCulture(culture);

        }

    }
    public class MessageDate
    {
        public string Message { get; set; }

        public int Value { get; set; }
    }
}
