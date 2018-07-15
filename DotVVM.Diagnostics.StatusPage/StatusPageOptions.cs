using System;
using System.Collections.Generic;
using System.Text;

namespace DotVVM.Diagnostics.StatusPage
{
    public class StatusPageOptions
    {
        public string RouteName { get; set; } = "StatusPage";

        public string Url { get; set; } = "_diagnostics/status";

        public static StatusPageOptions CreateDefaultOptions()
        {
            return new StatusPageOptions()
            {
                RouteName = "StatusPage",
                Url = "_diagnostics/status"
            };

        }
    }
}
