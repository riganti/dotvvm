using System.Runtime.InteropServices;

namespace DotVVM.VS2015Extension.DotvvmPageWizard
{
    internal abstract class EnvDTEConstants
    {
        /// <summary>
        /// A text document, opened with a text editor.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsDocumentKindText = "{8E7B96A8-E33D-11D0-A6D5-00C04FB67F6A}";

        /// <summary>
        /// An HTML document. Can get the IHTMLDocument2 interface, also known as the Document Object Model (DOM).
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsDocumentKindHTML = "{C76D83F8-A489-11D0-8195-00A0C91BBEE3}";

        /// <summary>
        /// A resource file, opened with the resource editor.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsDocumentKindResource = "{00000000-0000-0000-0000-000000000000}";

        /// <summary>
        /// A binary file, opened with a binary file editor.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsDocumentKindBinary = "{25834150-CD7E-11D0-92DF-00A0C9138C45}";

        /// <summary>
        /// View in default viewer.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsViewKindPrimary = "{00000000-0000-0000-0000-000000000000}";

        /// <summary>
        /// Use the view that was last used.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsViewKindAny = "{FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF}";

        /// <summary>
        /// View in debugger.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsViewKindDebugging = "{7651A700-06E5-11D1-8EBD-00A0C90F26EA}";

        /// <summary>
        /// View in code editor.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsViewKindCode = "{7651A701-06E5-11D1-8EBD-00A0C90F26EA}";

        /// <summary>
        /// View in Visual Designer (forms designer).
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsViewKindDesigner = "{7651A702-06E5-11D1-8EBD-00A0C90F26EA}";

        /// <summary>
        /// View in text editor.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsViewKindTextView = "{7651A703-06E5-11D1-8EBD-00A0C90F26EA}";

        /// <summary>
        /// The Task List window.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindTaskList = "{4A9B7E51-AA16-11D0-A8C5-00A0C921A4D2}";

        /// <summary>
        /// The Toolbox.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindToolbox = "{B1E99781-AB81-11D0-B683-00AA00A3EE26}";

        /// <summary>
        /// The Call Stack window.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindCallStack = "{0504FF91-9D61-11D0-A794-00A0C9110051}";

        /// <summary>
        /// The Debugger window.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindThread = "{E62CE6A0-B439-11D0-A79D-00A0C9110051}";

        /// <summary>
        /// The Debugger window.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindLocals = "{4A18F9D0-B838-11D0-93EB-00A0C90F2734}";

        /// <summary>
        /// The Debugger window.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindAutoLocals = "{F2E84780-2AF1-11D1-A7FA-00A0C9110051}";

        /// <summary>
        /// The Watch window.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindWatch = "{90243340-BD7A-11D0-93EF-00A0C90F2734}";

        /// <summary>
        /// The Properties window.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindProperties = "{EEFA5220-E298-11D0-8F78-00A0C9110057}";

        /// <summary>
        /// The Solution Explorer.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindSolutionExplorer = "{3AE79031-E1BC-11D0-8F78-00A0C9110057}";

        /// <summary>
        /// The Output window.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindOutput = "{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}";

        /// <summary>
        /// The Object Browser window.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindObjectBrowser = "{269A02DC-6AF8-11D3-BDC4-00C04F688E50}";

        /// <summary>
        /// The Macro Explorer window.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindMacroExplorer = "{07CD18B4-3BA1-11D2-890A-0060083196C6}";

        /// <summary>
        /// The Dynamic Help window.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindDynamicHelp = "{66DBA47C-61DF-11D2-AA79-00C04F990343}";

        /// <summary>
        /// The Class View window.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindClassView = "{C9C0AE26-AA77-11D2-B3F0-0000F87570EE}";

        /// <summary>
        /// The Resource Editor.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindResourceView = "{2D7728C2-DE0A-45b5-99AA-89B609DFDE73}";

        /// <summary>
        /// The Document Outline window.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindDocumentOutline = "{25F7E850-FFA1-11D0-B63F-00A0C922E851}";

        /// <summary>
        /// The Server Explorer.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindServerExplorer = "{74946827-37A0-11D2-A273-00C04F8EF4FF}";

        /// <summary>
        /// The Command window.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindCommandWindow = "{28836128-FC2C-11D2-A433-00C04F72D18A}";

        /// <summary>
        /// The Find Symbol dialog box.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindFindSymbol = "{53024D34-0EF5-11D3-87E0-00C04F7971A5}";

        /// <summary>
        /// The Find Symbol Results window.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindFindSymbolResults = "{68487888-204A-11D3-87EB-00C04F7971A5}";

        /// <summary>
        /// The Find Replace dialog box.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindFindReplace = "{CF2DDC32-8CAD-11D2-9302-005345000000}";

        /// <summary>
        /// The Find Results 1 window.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindFindResults1 = "{0F887920-C2B6-11D2-9375-0080C747D9A0}";

        /// <summary>
        /// The Find Results 2 window.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindFindResults2 = "{0F887921-C2B6-11D2-9375-0080C747D9A0}";

        /// <summary>
        /// The Visual Studio IDE window.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindMainWindow = "{9DDABE98-1D02-11D3-89A1-00C04F688DDE}";

        /// <summary>
        /// A linked window frame.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindLinkedWindowFrame = "{9DDABE99-1D02-11D3-89A1-00C04F688DDE}";

        /// <summary>
        /// A Web browser window hosted in Visual Studio.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWindowKindWebBrowser = "{E8B06F52-6D01-11D2-AA7D-00C04F990343}";

        /// <summary>
        /// Represents the "AddSubProject" wizard type.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWizardAddSubProject = "{0F90E1D2-4999-11D1-B6D1-00A0C90F2744}";

        /// <summary>
        /// Represents the "AddItem" wizard type.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWizardAddItem = "{0F90E1D1-4999-11D1-B6D1-00A0C90F2744}";

        /// <summary>
        /// Represents the "NewProject" wizard type.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsWizardNewProject = "{0F90E1D0-4999-11D1-B6D1-00A0C90F2744}";

        /// <summary>
        /// A miscellaneous files project.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsProjectKindMisc = "{66A2671D-8FB5-11D2-AA7E-00C04F688DDE}";

        /// <summary>
        /// A project item located in the miscellaneous files folder of the solution.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsProjectItemsKindMisc = "{66A2671E-8FB5-11D2-AA7E-00C04F688DDE}";

        /// <summary>
        /// A project item in the miscellaneous files folder of the solution.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsProjectItemKindMisc = "{66A2671F-8FB5-11D2-AA7E-00C04F688DDE}";

        /// <summary>
        /// An unmodeled project.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsProjectKindUnmodeled = "{67294A52-A4F0-11D2-AA88-00C04F688DDE}";

        /// <summary>
        /// A solution items project.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsProjectKindSolutionItems = "{66A26720-8FB5-11D2-AA7E-00C04F688DDE}";

        /// <summary>
        /// A collection of items in the solution items folder of the solution.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsProjectItemsKindSolutionItems = "{66A26721-8FB5-11D2-AA7E-00C04F688DDE}";

        /// <summary>
        /// A project item type in the solution.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsProjectItemKindSolutionItems = "{66A26722-8FB5-11D2-AA7E-00C04F688DDE}";

        /// <summary>
        /// The <see cref="T:EnvDTE.Projects"/> collection's <see cref="P:EnvDTE.Projects.Kind"/> property returns a GUID identifying the collection of project types that it contains.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsProjectsKindSolution = "{96410B9F-3542-4A14-877F-BC7227B51D3B}";

        /// <summary>
        /// The GUID that is used for a command when you call <see cref="M:EnvDTE.Commands.AddNamedCommand(EnvDTE.AddIn,System.String,System.String,System.String,System.Boolean,System.Int32,System.Object[]@,System.Int32)"/>. Each command has a GUID and an ID associated with it.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsAddInCmdGroup = "{1E58696E-C90F-11D2-AAB2-00C04F688DDE}";

        /// <summary>
        /// Indicates that a solution is currently being built.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsContextSolutionBuilding = "{ADFC4E60-0397-11D1-9F4E-00A0C911004F}";

        /// <summary>
        /// Indicates that the IDE is in Debugging mode.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsContextDebugging = "{ADFC4E61-0397-11D1-9F4E-00A0C911004F}";

        /// <summary>
        /// Indicates that the view of the integrated development environment (IDE) is full screen.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsContextFullScreenMode = "{ADFC4E62-0397-11D1-9F4E-00A0C911004F}";

        /// <summary>
        /// Indicates that the IDE is in Design view.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsContextDesignMode = "{ADFC4E63-0397-11D1-9F4E-00A0C911004F}";

        /// <summary>
        /// Indicates that the integrated development environment (IDE) has no solution.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsContextNoSolution = "{ADFC4E64-0397-11D1-9F4E-00A0C911004F}";

        /// <summary>
        /// Indicates that the solution has no projects.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsContextEmptySolution = "{ADFC4E65-0397-11D1-9F4E-00A0C911004F}";

        /// <summary>
        /// Indicates that the solution contains only one project.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsContextSolutionHasSingleProject = "{ADFC4E66-0397-11D1-9F4E-00A0C911004F}";

        /// <summary>
        /// Indicates that the solution contains multiple projects.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsContextSolutionHasMultipleProjects = "{93694FA0-0397-11D1-9F4E-00A0C911004F}";

        /// <summary>
        /// Indicates that a macro is being recorded.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsContextMacroRecording = "{04BBF6A5-4697-11D2-890E-0060083196C6}";

        /// <summary>
        /// Indicates that the Macro Recorder toolbar is displayed.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsContextMacroRecordingToolbar = "{85A70471-270A-11D2-88F9-0060083196C6}";

        /// <summary>
        /// The unique name for the Miscellaneous files project. Can be used to index the Solution.Projects object, such as: DTE.Solution.Projects.Item(vsMiscFilesProjectUniqueName).
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsMiscFilesProjectUniqueName = "<MiscFiles>";

        /// <summary>
        /// The unique name for projects in the solution. Can be used to index the <see cref="T:EnvDTE.SolutionClass"/> object's <see cref="P:EnvDTE.SolutionClass.Projects"/> collection, such as: DTE.Solution.Projects.Item(vsProjectsKindSolution).
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsSolutionItemsProjectUniqueName = "<SolnItems>";

        /// <summary>
        /// A file in the system.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsProjectItemKindPhysicalFile = "{6BB5F8EE-4483-11D3-8BCF-00C04F8EC28C}";

        /// <summary>
        /// A folder in the system.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsProjectItemKindPhysicalFolder = "{6BB5F8EF-4483-11D3-8BCF-00C04F8EC28C}";

        /// <summary>
        /// Indicates that the folder in the project does not physically appear on disk.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsProjectItemKindVirtualFolder = "{6BB5F8F0-4483-11D3-8BCF-00C04F8EC28C}";

        /// <summary>
        /// A subproject under the project. If returned by <see cref="P:EnvDTE.ProjectItem.Kind"/>, then <see cref="P:EnvDTE.ProjectItem.SubProject"/> returns as a <see cref="T:EnvDTE.Project"/> object.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsProjectItemKindSubProject = "{EA6618E8-6E24-4528-94BE-6889FE16485C}";

        /// <summary>
        /// See <see cref="F:EnvDTE.Constants.vsViewKindPrimary"/>.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_vk_Primary = "{00000000-0000-0000-0000-000000000000}";

        /// <summary>
        /// See <see cref="F:EnvDTE.Constants.vsViewKindDebugging"/>.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_vk_Debugging = "{7651A700-06E5-11D1-8EBD-00A0C90F26EA}";

        /// <summary>
        /// See <see cref="F:EnvDTE.Constants.vsViewKindCode"/>.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_vk_Code = "{7651A701-06E5-11D1-8EBD-00A0C90F26EA}";

        /// <summary>
        /// See <see cref="F:EnvDTE.Constants.vsViewKindDesigner"/>.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_vk_Designer = "{7651A702-06E5-11D1-8EBD-00A0C90F26EA}";

        /// <summary>
        /// See <see cref="F:EnvDTE.Constants.vsViewKindTextView"/>.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_vk_TextView = "{7651A703-06E5-11D1-8EBD-00A0C90F26EA}";

        /// <summary>
        /// See <see cref="F:EnvDTE.Constants.vsWindowKindTaskList"/>.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_wk_TaskList = "{4A9B7E51-AA16-11D0-A8C5-00A0C921A4D2}";

        /// <summary>
        /// See <see cref="F:EnvDTE.Constants.vsWindowKindToolbox"/>.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_wk_Toolbox = "{B1E99781-AB81-11D0-B683-00AA00A3EE26}";

        /// <summary>
        /// See <see cref="F:EnvDTE.Constants.vsWindowKindCallStack"/>.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_wk_CallStackWindow = "{0504FF91-9D61-11D0-A794-00A0C9110051}";

        /// <summary>
        /// See <see cref="F:EnvDTE.Constants.vsWindowKindThread"/>.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_wk_ThreadWindow = "{E62CE6A0-B439-11D0-A79D-00A0C9110051}";

        /// <summary>
        /// See <see cref="F:EnvDTE.Constants.vsWindowKindLocals"/>.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_wk_LocalsWindow = "{4A18F9D0-B838-11D0-93EB-00A0C90F2734}";

        /// <summary>
        /// See <see cref="F:EnvDTE.Constants.vsWindowKindAutoLocals"/>.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_wk_AutoLocalsWindow = "{F2E84780-2AF1-11D1-A7FA-00A0C9110051}";

        /// <summary>
        /// See <see cref="F:EnvDTE.Constants.vsWindowKindWatch"/>.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_wk_WatchWindow = "{90243340-BD7A-11D0-93EF-00A0C90F2734}";

        /// <summary>
        /// Refers to the Immediate window, used to execute commands in Debug mode.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_wk_ImmedWindow = "{98731960-965C-11D0-A78F-00A0C9110051}";

        /// <summary>
        /// See <see cref="F:EnvDTE.Constants.vsWindowKindProperties"/>.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_wk_PropertyBrowser = "{EEFA5220-E298-11D0-8F78-00A0C9110057}";

        /// <summary>
        /// The Project window, where the solution and its projects display.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_wk_SProjectWindow = "{3AE79031-E1BC-11D0-8F78-00A0C9110057}";

        /// <summary>
        /// See <see cref="F:EnvDTE.Constants.vsWindowKindOutput"/>.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_wk_OutputWindow = "{34E76E81-EE4A-11D0-AE2E-00A0C90FFFC3}";

        /// <summary>
        /// See <see cref="F:EnvDTE.Constants.vsWindowKindObjectBrowser"/>.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_wk_ObjectBrowser = "{269A02DC-6AF8-11D3-BDC4-00C04F688E50}";

        /// <summary>
        /// Refers to the Dynamic Help window.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_wk_ContextWindow = "{66DBA47C-61DF-11D2-AA79-00C04F990343}";

        /// <summary>
        /// See <see cref="F:EnvDTE.Constants.vsWindowKindClassView"/>.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_wk_ClassView = "{C9C0AE26-AA77-11D2-B3F0-0000F87570EE}";

        /// <summary>
        /// See <see cref="F:EnvDTE.Constants.vsWizardAddItem"/>.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_GUID_AddItemWizard = "{0F90E1D1-4999-11D1-B6D1-00A0C90F2744}";

        /// <summary>
        /// See <see cref="F:EnvDTE.Constants.vsWizardNewProject"/>.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsext_GUID_NewProjectWizard = "{0F90E1D0-4999-11D1-B6D1-00A0C90F2744}";

        /// <summary>
        /// Deprecated in Visual Studio. They are available only for backward compatibility with earlier versions of Visual Studio. For details, see the documentation for the previous version.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string dsCPP = "C/C++";

        /// <summary>
        /// Deprecated in Visual Studio. They are available only for backward compatibility with earlier versions of Visual Studio. For details, see the documentation for the previous version.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string dsHTML_IE3 = "HTML - IE 3.0";

        /// <summary>
        /// Deprecated in Visual Studio. They are available only for backward compatibility with earlier versions of Visual Studio. For details, see the documentation for the previous version.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string dsHTML_RFC1866 = "HTML 2.0 (RFC 1866)";

        /// <summary>
        /// Deprecated in Visual Studio. They are available only for backward compatibility with earlier versions of Visual Studio. For details, see the documentation for the previous version.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string dsFortran_Fixed = "Fortran Fixed";

        /// <summary>
        /// Deprecated in Visual Studio. They are available only for backward compatibility with earlier versions of Visual Studio. For details, see the documentation for the previous version.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string dsFortran_Free = "Fortran Free";

        /// <summary>
        /// Deprecated in Visual Studio. They are available only for backward compatibility with earlier versions of Visual Studio. For details, see the documentation for the previous version.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string dsJava = "Java";

        /// <summary>
        /// Deprecated in Visual Studio. They are available only for backward compatibility with earlier versions of Visual Studio. For details, see the documentation for the previous version.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string dsVBSMacro = "VBS Macro";

        /// <summary>
        /// Deprecated in Visual Studio. They are available only for backward compatibility with earlier versions of Visual Studio. For details, see the documentation for the previous version.
        /// </summary>
        [TypeLibVar((short)64)]
        [MarshalAs(UnmanagedType.LPStr)]
        public const string dsIDL = "ODL/IDL";

        /// <summary>
        /// The CATID for the solution.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsCATIDSolution = "{52AEFF70-BBD8-11d2-8598-006097C68E81}";

        /// <summary>
        /// The CATID for items in the Property window when the solution node is selected in Solution Explorer.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsCATIDSolutionBrowseObject = "{A2392464-7C22-11d3-BDCA-00C04F688E50}";

        /// <summary>
        /// The CATID for the miscellaneous files project.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsCATIDMiscFilesProject = "{610d4612-d0d5-11d2-8599-006097c68e81}";

        /// <summary>
        /// The CATID for the miscellaneous files project item.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsCATIDMiscFilesProjectItem = "{610d4613-d0d5-11d2-8599-006097c68e81}";

        /// <summary>
        /// The CATID for generic projects — that is, projects without a specific object model.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsCATIDGenericProject = "{610d4616-d0d5-11d2-8599-006097c68e81}";

        /// <summary>
        /// The CATID for documents.
        /// </summary>
        [MarshalAs(UnmanagedType.LPStr)]
        public const string vsCATIDDocument = "{610d4611-d0d5-11d2-8599-006097c68e81}";
    }
}