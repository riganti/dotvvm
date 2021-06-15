using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.StaticCommand
{
    public class FoodService
    {
        [AllowStaticCommand]
        public AddFoodsResult AddFoodAsync(string newfood, List<string> foods)
        {
            foods.Add(newfood);

            return new AddFoodsResult {
                Foods = foods,
                Message = "A food item has been added"
            };
        }
    }

    public class AddFoodsResult
    {
        public string Message { get; set; }
        public List<string> Foods { get; set; }
    }

    public class StaticCommand_LoadComplexDataFromServiceViewModel : DotvvmViewModelBase
    {
        public string Message { get; set; }
        public List<string> Names { get; set; }
        public List<string> Foods { get; set; }

        public AddFoodsResult AddFoodsCallbackResult { get; set; }

        public override Task PreRender()
        {
            if (!Context.IsPostBack)
            {
                Names = new List<string> { "Martin", "Roman", "Igor" };
                Foods = new List<string> { "Burger", "Pizza" };
            }
           
            return base.PreRender();
        }
        public string NewFood { get; set; }
    }
}

