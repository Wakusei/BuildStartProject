//------------------------------------------------------------------------------
// <copyright file="BuildStartProject.cs" company="Company">
//     Copyright (c) Company.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel.Design;
using System.Globalization;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using EnvDTE80;
using EnvDTE;
using Microsoft.VisualStudio;

namespace BuildStartProject
{
    /// <summary>
    /// Command handler
    /// </summary>
    internal sealed class BuildStartProject : IVsSolutionEvents, IDisposable
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("80ae959f-2520-4c36-95f3-058020433217");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly Package package;


        uint m_EventSinkCookie;




        /// <summary>
        /// Initializes a new instance of the <see cref="BuildStartProject"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        private BuildStartProject(Package package)
        {
            if (package == null)
            {
                throw new ArgumentNullException("package");
            }

            this.package = package;

            // Add a menu item.
            OleMenuCommandService commandService = this.ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (commandService != null)
            {
                var menuCommandID = new CommandID(CommandSet, CommandId);
                var menuItem = new MenuCommand(this.MenuItemCallback, menuCommandID);
                commandService.AddCommand(menuItem);
            }

            // Register as an advisory interface.
            var solutionService = ServiceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            if (solutionService != null)
            {     
                solutionService.AdviseSolutionEvents(this, out m_EventSinkCookie);
            }

        }

        public void Dispose()
        {
            var solutionService = ServiceProvider.GetService(typeof(SVsSolution)) as IVsSolution;
            if (solutionService != null && m_EventSinkCookie != 0)
            {
                solutionService.UnadviseSolutionEvents(m_EventSinkCookie);
            }
        }
        

        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static BuildStartProject Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private IServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static void Initialize(Package package)
        {
            Instance = new BuildStartProject(package);
        }

        public int OnAfterCloseSolution(object pUnkReserved)
        {
            OleMenuCommandService mcs = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                var menu = mcs.FindCommand(new CommandID(CommandSet, CommandId));
                menu.Visible = false;
            }
            
            return VSConstants.S_OK;
        }

        public int OnAfterLoadProject(IVsHierarchy pStubHierarchy, IVsHierarchy pRealHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenProject(IVsHierarchy pHierarchy, int fAdded)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterOpenSolution(object pUnkReserved, int fNewSolution)
        {
            OleMenuCommandService mcs = ServiceProvider.GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if (null != mcs)
            {
                var menu = mcs.FindCommand(new CommandID(CommandSet, CommandId));
                menu.Visible = true;
            }

            return VSConstants.S_OK;
        }

        public int OnBeforeCloseProject(IVsHierarchy pHierarchy, int fRemoved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeCloseSolution(object pUnkReserved)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeUnloadProject(IVsHierarchy pRealHierarchy, IVsHierarchy pStubHierarchy)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseProject(IVsHierarchy pHierarchy, int fRemoving, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryCloseSolution(object pUnkReserved, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }

        public int OnQueryUnloadProject(IVsHierarchy pRealHierarchy, ref int pfCancel)
        {
            return VSConstants.S_OK;
        }




        /// <summary>
        /// This function is the callback used to execute the command when the menu item is clicked.
        /// See the constructor to see how the menu item is associated with this function using
        /// OleMenuCommandService service and MenuCommand class.
        /// </summary>
        /// <param name="sender">Event sender.</param>
        /// <param name="e">Event args.</param>
        private void MenuItemCallback(object sender, EventArgs e)
        {
            DTE2 dte = (DTE2)ServiceProvider.GetService(typeof(DTE));
            var sb = (SolutionBuild2) dte.Solution.SolutionBuild;

            if( !dte.Solution.IsOpen)
            {
                return;
            }
                        
            string active_config = (string)dte.Solution.Properties.Item("ActiveConfig").Value;

            foreach (string item in (Array)sb.StartupProjects)
            {
                bool found = false;
                foreach (Project proj in dte.Solution.Projects)
                {
                    if (proj.UniqueName == item)
                    {
                        sb.BuildProject(active_config, proj.UniqueName, false);
                        found = true;
                        break;
                    }
                }
                if (found)
                {
                    break;
                }
            }

            // Activate the output window.
            dte.Windows.Item(EnvDTE.Constants.vsWindowKindOutput).Activate();


        }


    }
}
