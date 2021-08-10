using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ComplexSamples.ChangedEvent
{
    public class ChangedEventViewModel : DotvvmViewModelBase
    {
        public string Text { get; set; }

        public string Text2 { get; set; }

        public string Text3 { get; set; }

        public bool IsChecked { get; set; }

        public int IdChange { get; set; }

        public ChangedEventViewModel()
        {
            Text = "Value";
            Text2 = "Value";
        }

        public void OnChanged()
        {
            IdChange++;
        }

        public string DBRB { get; set; }

        public IEnumerable<DRBChoice> DBRBChoices
        {
            get
            {
                return new DRBChoice[]
                {
                    new DRBChoice{ Id = 1, Title = "One" },
                    new DRBChoice{ Id = 2, Title = "Two" },
                    new DRBChoice{ Id = 3, Title = "Three" },
                    new DRBChoice{ Id = 4, Title = "Four" },
                    new DRBChoice{ Id = 5, Title = "Five" }
                };
            }
        }

        public class DRBChoice
        {
            public int Id { get; set; }
            public string Title { get; set; }
        }


        public string SelectedName { get; set; }

        public IEnumerable<CityModel> Cities
        {
            get
            {
                return new CityModel[]
                {
                    new CityModel { Id = 1, Name = "Prague" },
                    new CityModel { Id = 2, Name = "Germany" },
                    new CityModel { Id = 3, Name = "UK" }
                };
            }
        }

        public class CityModel
        {
            public string Name { get; set; }

            public int Id { get; set; }
        }
    }
}
