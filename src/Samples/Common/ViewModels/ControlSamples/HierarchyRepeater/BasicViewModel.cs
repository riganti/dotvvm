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

        public List<Node> Empty { get; set; }

        public override Task Load()
        {
            if (Context.IsPostBack)
                return base.Load();

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

        public string GlobalLabel { get; set; } = "Test";

        public void ClickNode(Node node)
        {
            node.ClickCount++;
        }

        public class Node
        {
            public string Name { get; set; } = string.Empty;

            public int ClickCount { get; set; } = 0;

            public List<Node> Children { get; set; } = new();

            public void ClickNode()
            {
                ClickCount++;
            }
        }
    }
}

