/****************************** Module Header ******************************\
 * Module Name:  DotvvmPropertyPageProjectFactory.cs
 * Project:      CSVSXProjectSubType
 * Copyright (c) Microsoft Corporation.
 * 
 * This is the project factory for our project flavor.
 * 
 * How Project SubType Work:
 * 
 * First, we have to register our DotvvmPropertyPageProjectFactory to Visual Studio.
 * 
 * Second, we need a Project Template, which is created by the CSVSXProjectSubTypeTemplate
 * project.
 * 
 * The ProjectTemplate.csproj in CSVSXProjectSubTypeTemplate contains following script 
 *     <ProjectTypeGuids>
 *         {3C53C28F-DC44-46B0-8B85-0C96B85B2042};
 *         {FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}
 *     </ProjectTypeGuids>
 * 
 *     {3C53C28F-DC44-46B0-8B85-0C96B85B2042} is the Guid of the DotvvmPropertyPageProjectFactory.
 *     {FAE04EC0-301F-11D3-BF4B-00C04F79EFBC} means CSharp project. 
 * 
 * At last, When Visual Studio is creating or opening a CSharp project with above ProjectTypeGuids,
 * 1. The environment calls the base project (CSharp Project)'s CreateProject, and while the 
 *    project parses its project file it discovers that the aggregate project type GUIDs list
 *    is not null. The project discontinues directly creating its project.
 * 
 * 2. If there are multiple project type GUIDs, the environment makes recursive function calls to 
 *    your implementations of PreCreateForOuter, 
 *    Microsoft.VisualStudio.Shell.Interop.IVsAggregatableProject.SetInnerProject(System.Object) 
 *    and InitializeForOuter methods while it is walking the list of project type GUIDs, 
 *    starting with the outermost project subtype.
 * 
 * 3. In the PreCreateForOuter method of the ProjectFactory, we can return our ProjectFlavor object,
 *    which can customize the Property Page. 
 * 
 * This source is subject to the Microsoft Public License.
 * See http://www.microsoft.com/opensource/licenses.mspx#Ms-PL.
 * All other rights reserved.
 * 
 * THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
 * EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED 
 * WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
\***************************************************************************/

using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell.Flavor;

namespace DotVVM.VS2015Extension.ProjectExtensions.PropertyPages
{

    /// <summary>
    /// The project factory for our project flavor.
    /// </summary>
    [Guid(GuidList.DotvvmPropertyPageProjectFactory)]
    public class DotvvmPropertyPageProjectFactory : FlavoredProjectFactoryBase
    {
        

        // With this package, we can get access to VS services.
        private DotvvmSubTypePackage package;

        public DotvvmPropertyPageProjectFactory(DotvvmSubTypePackage package) : base()
        {
            this.package = package;
        }

        #region IVsAggregatableProjectFactory
        
        /// <summary>
        /// Create an instance of DotvvmPropertyPageProjectFlavor. 
        /// The initialization will be done later when Visual Studio calls
        /// InitalizeForOuter on it.
        /// </summary>
        /// <param name="outerProjectIUnknown">
        /// This value points to the outer project. It is useful if there is a 
        /// Project SubType of this Project SubType.
        /// </param>
        /// <returns>
        /// An DotvvmPropertyPageProjectFlavor instance that has not been initialized.
        /// </returns>
        protected override object PreCreateForOuter(IntPtr outerProjectIUnknown)
        {
            var newProject = new DotvvmPropertyPageProjectFlavor();
            newProject.Package = this.package;
            return newProject;
        }

        #endregion
    }
}
