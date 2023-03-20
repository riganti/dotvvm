using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.TaskList;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.Serialization
{
    public class ListOfObjectsViewModel : DotvvmViewModelBase
    {
        public object[] ArrayPrimitives { get; set; } = new object[] {
            1, 2, "str", 
        };

        public List<object> ListPrimitives { get; set; } = new List<object> {
            3, "str"
        };

        public List<object> ListObjects { get; set; } = new List<object> {
            new { AnonymousObjects = 1 },
            1,
            DateTime.Parse("2019-01-01T10:10:10"),
            new TaskViewModel() { IsCompleted = false, TaskId = Guid.Parse("53321a76-9c67-483d-9f61-e4d96a4a50f3"), Title = "🤷" },
        };

        public void AddSomething(string x)
        {
            ArrayPrimitives = ArrayPrimitives.Append(x).ToArray();
            ListPrimitives.Add(x);
            ListObjects.Add(new { AnotherAnonymousObject = x });
        }
    }
}

