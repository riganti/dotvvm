using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace DotVVM.Samples.BasicSamples.Api.AspNetCore.Controllers
{
    [Route("api/[controller]")]
    public class BindingSharingController : Controller
    {

        [HttpGet]
        [Route("get")]
        public BindingSharingItemDTO[] GetItems(int category)
        {
            if (category == 1)
            {
                return new[] {
                    new BindingSharingItemDTO() { Id = category * 10 + 1, Name = $"Category {category} / Item 1" },
                    new BindingSharingItemDTO() { Id = category * 10 + 2, Name = $"Category {category} / Item 2" },
                    new BindingSharingItemDTO() { Id = category * 10 + 3, Name = $"Category {category} / Item 3" }
                };
            }
            else if (category == 2)
            {
                return new[] {
                    new BindingSharingItemDTO() { Id = category * 10 + 1, Name = $"Category {category} / Item 1" },
                    new BindingSharingItemDTO() { Id = category * 10 + 2, Name = $"Category {category} / Item 2" },
                    new BindingSharingItemDTO() { Id = category * 10 + 3, Name = $"Category {category} / Item 3" },
                    new BindingSharingItemDTO() { Id = category * 10 + 4, Name = $"Category {category} / Item 4" },
                    new BindingSharingItemDTO() { Id = category * 10 + 5, Name = $"Category {category} / Item 5" }
                };
            }
            else if (category == 3)
            {
                return new[] {
                    new BindingSharingItemDTO() { Id = category * 10 + 1, Name = $"Category {category} / Item 1" },
                };
            }
            else
            {
                throw new ArgumentException(nameof(category));
            }
        }

        [HttpGet]
        [Route("getWithRouteParam/{category}")]
        public BindingSharingItemDTO[] GetItemsWithRouteParam(int category)
        {
            if (category == 1)
            {
                return new[] {
                    new BindingSharingItemDTO() { Id = category * 10 + 1, Name = $"Category {category} / Item 1" },
                    new BindingSharingItemDTO() { Id = category * 10 + 2, Name = $"Category {category} / Item 2" },
                    new BindingSharingItemDTO() { Id = category * 10 + 3, Name = $"Category {category} / Item 3" }
                };
            }
            else if (category == 2)
            {
                return new[] {
                    new BindingSharingItemDTO() { Id = category * 10 + 1, Name = $"Category {category} / Item 1" },
                    new BindingSharingItemDTO() { Id = category * 10 + 2, Name = $"Category {category} / Item 2" },
                    new BindingSharingItemDTO() { Id = category * 10 + 3, Name = $"Category {category} / Item 3" },
                    new BindingSharingItemDTO() { Id = category * 10 + 4, Name = $"Category {category} / Item 4" },
                    new BindingSharingItemDTO() { Id = category * 10 + 5, Name = $"Category {category} / Item 5" }
                };
            }
            else if (category == 3)
            {
                return new[] {
                    new BindingSharingItemDTO() { Id = category * 10 + 1, Name = $"Category {category} / Item 1" },
                };
            }
            else
            {
                throw new ArgumentException(nameof(category));
            }
        }

        [HttpPost]
        [Route("post")]
        public BindingSharingItemDTO[] GetItemsWithHttpPost(int category)
        {
            if (category == 1)
            {
                return new[] {
                    new BindingSharingItemDTO() { Id = category * 10 + 1, Name = $"Category {category} / Item 1" },
                    new BindingSharingItemDTO() { Id = category * 10 + 2, Name = $"Category {category} / Item 2" },
                    new BindingSharingItemDTO() { Id = category * 10 + 3, Name = $"Category {category} / Item 3" }
                };
            }
            else if (category == 2)
            {
                return new[] {
                    new BindingSharingItemDTO() { Id = category * 10 + 1, Name = $"Category {category} / Item 1" },
                    new BindingSharingItemDTO() { Id = category * 10 + 2, Name = $"Category {category} / Item 2" },
                    new BindingSharingItemDTO() { Id = category * 10 + 3, Name = $"Category {category} / Item 3" },
                    new BindingSharingItemDTO() { Id = category * 10 + 4, Name = $"Category {category} / Item 4" },
                    new BindingSharingItemDTO() { Id = category * 10 + 5, Name = $"Category {category} / Item 5" }
                };
            }
            else if (category == 3)
            {
                return new[] {
                    new BindingSharingItemDTO() { Id = category * 10 + 1, Name = $"Category {category} / Item 1" },
                };
            }
            else
            {
                throw new ArgumentException(nameof(category));
            }
        }

    }

    public class BindingSharingItemDTO
    {
        [Required]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
