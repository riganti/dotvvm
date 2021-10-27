using System.Collections.Generic;
using System.Linq;

namespace DotVVM.Framework.Hosting.ErrorPages
{
    public class CookiesSection : IErrorSectionFormatter
    {
        public string DisplayName => "Cookies";

        public string Id => "cookies";

        public ICookieCollection Cookies { get; }

        public CookiesSection(ICookieCollection cookies)
        {
            this.Cookies = cookies;
        }

        public void WriteBody(IErrorWriter writer)
        {
            // it's not a very great idea to copy all cookie values into the output,
            // since some of them might be HTTP only - so there should not be a way to read
            // them using Javascript. However, if error pages would be enabled in production,
            // this would allow it.
            // We will thus fill the cookie table with "redacted" values and then fill it in JS.
            var table = Cookies.Select(c => new KeyValuePair<string, string>(c.Key, "redacted value of HTTP only cookie"));
            writer.WriteKVTable(table, "cookie-table");

            writer.WriteUnencoded(@"
            <script>
            (function () {
                var cookies = {}
                document.cookie.split(';').forEach(function(c) {
                    let split = c.split('=', 2);
                    cookies[split[0].trim()] = split[1];
                })
                var table = document.querySelector('.cookie-table');
                var rows = table.tBodies[0].rows;
                for (var i = 0; i < rows.length; i++) {
                    var cookieName = rows[i].cells[0].textContent.trim();
                    if (cookieName in cookies) {
                        rows[i].cells[1].textContent = cookies[cookieName]
                    } else {
                        rows[i].cells[1].classList.add('hint-text')
                    }
                }
            }())
            </script>
            ");
        }
        public void WriteStyle(IErrorWriter writer)
        {
        }
    }
}
