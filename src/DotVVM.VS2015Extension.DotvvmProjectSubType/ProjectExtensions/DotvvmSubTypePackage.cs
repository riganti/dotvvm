using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using DotVVM.VS2015Extension.ProjectExtensions.PropertyPages;
using Microsoft.VisualStudio.Shell;

namespace DotVVM.VS2015Extension.ProjectExtensions
{

    [PackageRegistration(UseManagedResourcesOnly = true)]
    [ProvideObject(typeof(DotvvmPropertyPage), RegisterUsing = RegistrationMethod.CodeBase)]
    [ProvideProjectFactory(typeof(DotvvmPropertyPageProjectFactory), "Task Project", null, null, null, @"..\Templates\Projects")]
    [Guid(GuidList.DotvvmProjectSubTypePackage)]
    public sealed class DotvvmSubTypePackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public DotvvmSubTypePackage()
        {
            Trace.WriteLine(string.Format("Entering constructor for: {0}", this.ToString()));
        }

        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine(string.Format("Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();
            this.RegisterProjectFactory(new DotvvmPropertyPageProjectFactory(this));
        }
        #endregion

        /// <summary>
        /// Allow a component such as project, factory, toolwindow,... to
        /// get access to VS services.
        /// </summary>
        internal object GetVsService(Type type)
        {
            return this.GetService(type);
        }

    }
}
