using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SampleApp1.Models
{
    public class CounterDTO
    {

        public int Count { get; set; }

        public void Increment()
        {
            Count++;
        }

    }
}
