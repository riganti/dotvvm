using Redwood.Framework.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Redwood.Framework
{
    public static class ConfigurationHelper
    {
        public static void RegisterCommonResources(this RwResourceRepository repo, 
            string jqueryLocal = "Scripts/jquery-2.1.1.min.js",
            string jqueryCdn = "https://code.jquery.com/jquery-2.1.3.min.js",
            string knockoutLocal = "/Scripts/knockout-3.2.0.js",
            string knockoutMapper = "/Scripts/knockout.mapper.js",
            string redwood = "/Scripts/Redwood.js",
            string bootstrapCss = "Content/bootstrap/bootstrap.min.css",
            string bootstrapJsLocal = "Scripts/bootstrap.min.js",
            string bootstrapJsCdn = "https://maxcdn.bootstrapcdn.com/bootstrap/3.3.1/js/bootstrap.min.js"
            )
        {

            repo.Register("jquery", new ScriptResource(jqueryLocal, jqueryCdn, "window.jQuery"));
            repo.Register("knockout-core", new ScriptResource(knockoutLocal, "jquery"));
            repo.Register("knockout", new ScriptResource(knockoutMapper, "knockout-core"));
            repo.Register("redwood", new ScriptResource(redwood, "knockout"));
            repo.Register("bootstrap-css", new StyleResource(bootstrapCss));
            repo.Register("bootstrap", new ScriptResource(bootstrapJsLocal, bootstrapJsCdn, "$.fn.modal", "bootstrap-css", "jquery"));

        }
    }
}
