using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotVVM.Framework.Configuration
{
	public class DefaultControlRegistrationStrategy : IControlRegistrationStrategy
	{
		private DotvvmConfiguration configuration;
		readonly string controlPrefix;
		readonly string controlsDirectory;
		readonly string filesFilter;

		public DefaultControlRegistrationStrategy(DotvvmConfiguration configuration, string controlPrefix, string controlsDirectory, string filesFilter = "*.dotcontrol")
		{
			this.configuration = configuration;
			this.controlPrefix = controlPrefix;
			this.controlsDirectory = Path.Combine(configuration.ApplicationPhysicalPath, controlsDirectory);
			this.filesFilter = filesFilter;
		}

		protected virtual IEnumerable<string> ListFiles()
			=> Directory.GetFiles(controlsDirectory, filesFilter, SearchOption.AllDirectories);

		protected virtual string GetControlName(string fileName) => Path.GetFileNameWithoutExtension(fileName);

		protected virtual string GetControlPrefix(string fileName) => controlPrefix;

		public IEnumerable<DotvvmControlConfiguration> GetControls()
			=> ListFiles()
			.Select(f => new DotvvmControlConfiguration { Src = f, TagPrefix = GetControlPrefix(f), TagName = GetControlName(f) });
	}
}
