using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVM.Samples.BasicSamples.ViewModels;

namespace DotVVM.Samples.Common.ViewModels.ControlSamples.HierarchyRepeater
{
    public class BasicViewModel : SamplesViewModel
    {
        public List<Node> Roots { get; set; }

        public override Task Load()
        {
            Roots = new() {
                new Node {
                    Name = "Root",
                    Children = {
                        new Node {
                            Name = "-- 0",
                        },
                        new Node {
                            Name = "-- 1",
                            Children = {
                                new Node {
                                    Name = "-- 1 -- 0"
                                }
                            }
                        },
                        new Node {
                            Name = "-- 2",
                        },
                    }
                }
            };
            return base.Load();
        }

        public class Node
        {
            public string Name { get; set; } = string.Empty;

            public List<Node> Children { get; set; } = new();
        }
    }
}

