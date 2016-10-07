using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.ServerRendering
{
    public class ArticleDTO
    {
        public int Id { get; set; }
        public DateTime DateSubmitted { get; set; }
        public string Message { get; set; }
    }
}
