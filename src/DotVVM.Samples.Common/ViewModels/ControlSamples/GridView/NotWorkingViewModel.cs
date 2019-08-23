using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Controls;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.GridView
{
    public class NotWorkingViewModel
    {
        public GridViewDataSet<RegistrationDto> EventRegistrations { get; set; } = new GridViewDataSet<RegistrationDto> {
            RowEditOptions = new RowEditOptions {
                PrimaryKeyPropertyName = nameof(RegistrationDto.Id)
            },
            Items = new List<RegistrationDto> {
                new RegistrationDto {
                    Id = "r1",
                    Sku = "Sku1",
                    SkuDescription = "Sku1 Description",
                    FirstName = "John",
                    LastName = "Smith",
                    Email = "john@example.com",
                    OrderId = "o1",
                    Notes = "Help"
                },
                new RegistrationDto {
                    Id = "r2",
                    Sku = "Sku2",
                    SkuDescription = "Sku2 Description",
                    FirstName = "Joe",
                    LastName = "Doe",
                    Email = "joe@example.com",
                    OrderId = "o2",
                    Notes = "Help"
                },
                new RegistrationDto {
                    Id = "r3",
                    Sku = "Sku3",
                    SkuDescription = "Sku3 Description",
                    FirstName = "Jane",
                    LastName = "Doe",
                    Email = "jane@example.com",
                    OrderId = "o3",
                    Notes = "Help"
                }
            }
        };

        public void EditEventRegistration(string id)
        {
            EventRegistrations.RowEditOptions.EditRowId = id;
        }

        public Task CancelEventRegistration()
        {
            EventRegistrations.RowEditOptions.EditRowId = null;
            EventRegistrations.RequestRefresh();
            return Task.CompletedTask;
        }

        public Task SaveEventRegistration(RegistrationDto item)
        {
            EventRegistrations.RowEditOptions.EditRowId = null;
            EventRegistrations.RequestRefresh();
            return Task.CompletedTask;
        }

        public class RegistrationDto
        {
            public string Id { get; set; }

            public string Sku { get; set; }

            public string SkuDescription { get; set; }

            public string FirstName { get; set; }

            public string LastName { get; set; }

            public string Email { get; set; }

            public string OrderId { get; set; }

            public string Notes { get; set; }
        }
    }
}
