using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.Common.ViewModels.FeatureSamples.PostBack
{
    public class UniqueIdGenerationOnPostbackUpdateViewModel : DotvvmViewModelBase
    {
        public override Task PreRender()
        {
            if (!Context.IsPostBack)
            {
                Data = new List<TestDataItem>() {
                    new TestDataItem() {
                        Data = new List<TestDataItem>() {
                            new TestDataItem(),
                            new TestDataItem(),
                            new TestDataItem(),
                            new TestDataItem(),
                            new TestDataItem(),
                        }
                    },
                    new TestDataItem() {
                        Data = new List<TestDataItem>() {
                            new TestDataItem(){
                                Data = new List<TestDataItem>() {
                                    new TestDataItem(),
                                    new TestDataItem(),
                                    new TestDataItem(),
                                    new TestDataItem(),
                                    new TestDataItem(),
                                    new TestDataItem(),
                                    new TestDataItem(),
                                },
                            },
                            new TestDataItem(){
                                Data = new List<TestDataItem>() {
                                    new TestDataItem(){
                                        Data = new List<TestDataItem>() {
                                            new TestDataItem(){
                                                Data = new List<TestDataItem>() {
                                                    new TestDataItem(){
                                                        Data = new List<TestDataItem>() {
                                                            new TestDataItem(){
                                                                Data = new List<TestDataItem>() {
                                                                    new TestDataItem(),
                                                                    new TestDataItem(),
                                                                    new TestDataItem(),
                                                                    new TestDataItem(),
                                                                    new TestDataItem(),
                                                                    new TestDataItem(),
                                                                },
                                                            },
                                                            new TestDataItem(),
                                                            new TestDataItem(),
                                                            new TestDataItem(),
                                                            new TestDataItem(),
                                                            new TestDataItem(),
                                                        },
                                                    },
                                                    new TestDataItem(),
                                                    new TestDataItem(),
                                                    new TestDataItem(),
                                                    new TestDataItem(),
                                                    new TestDataItem(),
                                                },
                                            },
                                            new TestDataItem(),
                                            new TestDataItem(),
                                            new TestDataItem(),
                                            new TestDataItem(),
                                            new TestDataItem(),
                                        },
                                    },
                                    new TestDataItem(),
                                    new TestDataItem(),
                                    new TestDataItem(),
                                    new TestDataItem(),
                                    new TestDataItem(),
                                },
                            },new TestDataItem(){
                                Data = new List<TestDataItem>() {
                                    new TestDataItem(),
                                    new TestDataItem(),
                                    new TestDataItem(),
                                    new TestDataItem(),
                                    new TestDataItem(),
                                    new TestDataItem(),
                                    new TestDataItem(),
                                    new TestDataItem(),
                                },
                            },new TestDataItem(){
                                Data = new List<TestDataItem>() {
                                    new TestDataItem(),
                                    new TestDataItem(),
                                    new TestDataItem(),
                                    new TestDataItem(),
                                    new TestDataItem(),
                                    new TestDataItem(),
                                    new TestDataItem(),
                                },
                            },
                        }
                    }

                };
            }

            return base.PreRender();
        }

        public List<TestDataItem> Data { get; set; }
    }

    public class TestDataItem
    {
        public DateTime Now { get; set; } = DateTime.Now;
        public List<TestDataItem> Data { get; set; }
        public string Text { get; set; }

        public void Command()
        {
            Text = "Command executed";
        }
    }
}

