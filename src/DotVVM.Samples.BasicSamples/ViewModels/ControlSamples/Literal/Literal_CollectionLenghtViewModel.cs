using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotVVM.Samples.BasicSamples.ViewModels.ControlSamples.Literal
{
	public class Literal_CollectionLengthViewModel : DotvvmViewModelBase
	{
	    public List<string> MyCollection { get; set; } = new List<string>();

	    public void AddItemToCollection()
	    {
	        MyCollection.Add("Item");
	    }
	}


}

