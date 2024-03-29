using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.FeatureSamples.Validation
{
    public class ClientSideRulesViewModel : DotvvmViewModelBase
    {
        [Range(10, 20)]
        public int? RangeInt32 { get; set; } = null;

        [Range(12.345678901, double.PositiveInfinity)]
        public double? RangeFloat64 { get; set; } = null;

        [Range(typeof(DateTime), "2015-01-01T00:00:00", "2015-12-31T23:59:59", ErrorMessage = "{0} must be between {1:s} and {2:s}.")]
        public DateTime? RangeDate { get; set; } = null;

        [Required(AllowEmptyStrings = false)]
        public string RequiredString { get; set; } = "abc";

        [Required(AllowEmptyStrings = true)]
        public string NotNullString { get; set; } = "";

        [EmailAddress]
        public string EmailString { get; set; } = "test@something.somewhere";

        public string Result { get; set; }
        [Bind(Direction.ServerToClientFirstRequest)]
        public int ServerRequestCount { get; set; }
        [Bind(Direction.ServerToClientFirstRequest)]
        public int ClientPostbackCount { get; set; }

        public void Command()
        {
            Result = "Valid";
        }
        
    }
}
