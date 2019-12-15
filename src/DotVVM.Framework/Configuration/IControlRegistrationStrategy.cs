using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Configuration
{
	public interface IControlRegistrationStrategy
	{
		IEnumerable<DotvvmControlConfiguration> GetControls();
	}
}
