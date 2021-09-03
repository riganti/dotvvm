using DotVVM.Framework.ViewModel;
using System.Collections.Generic;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.Repeater
{
    public class SeparatorViewModel : DotvvmViewModelBase
    {
        public List<Card> Cards { get; set; } = new List<Card>
        {
            new Card {From = "Alaska", Sender = "Yeti"},
            new Card {From = "New Zealand ", Sender = "John"},
            new Card {From = "Minnesota", Sender = "Lou"}
         };

        public class Card
        {
            public string From { get; set; }

            public string Sender { get; set; }
        }

        public void AddItem()
        {
            Cards.Add(new Card { From = "New York", Sender = "Timmy" });
        }
    }
}
