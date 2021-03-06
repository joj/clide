﻿using Clide.Properties;
using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel.Design;
using System.Diagnostics;
namespace Clide.Commands
{

    /// <summary>
    /// Base Visual Studio adapter command that invokes an <see cref="ICommandExtension"/> implementation, 
    /// automatically shielding it for errors to avoid VS hangs, and starting an activity before 
    /// executing it.
    /// </summary>
    public class VsCommandExtensionAdapter : OleMenuCommand
    {
        private readonly ITracer tracer;

        /// <summary>
        /// Initializes a new instance of the <see cref="VsCommandExtensionAdapter"/> class with a 
        /// specific command identifier and implementation.
        /// </summary>
        public VsCommandExtensionAdapter(CommandID id, ICommandExtension implementation)
            : base(OnExecute, id)
        {
            tracer = Tracer.Get(this.GetType());

            this.Implementation = implementation;
            this.BeforeQueryStatus += this.OnBeforeQueryStatus;
        }

        /// <summary>
        /// Gets the command implementation.
        /// </summary>
        protected ICommandExtension Implementation { get; private set; }

        private static void OnExecute(object sender, EventArgs e)
        {
            var command = (VsCommandExtensionAdapter)sender;
            var menu = new MenuCommand { Enabled = command.Enabled, Text = command.Text, Visible = command.Visible };

            using (command.tracer.StartActivity(Strings.VsCommandExtensionAdapter.ExecutingCommand(
                command.Text, command.Implementation.GetType().Name)))
            {
                command.tracer.ShieldUI(() =>
                {
                    command.Implementation.QueryStatus(menu);

                    if (menu.Enabled)
                    {
                        command.Implementation.Execute(menu);
                    }
                    else
                    {
                        command.tracer.Warn(Strings.VsCommandExtensionAdapter.CannotExecute(
                            menu.Text, command.Implementation.GetType().Name));
                    }
                }, Strings.VsCommandExtensionAdapter.ExecuteShieldMessage);
            }
        }

        private void OnBeforeQueryStatus(object sender, EventArgs e)
        {
            var command = (VsCommandExtensionAdapter)sender;
            // Preventively set to false.
            command.Enabled = false;
            var menu = new MenuCommand
            {
                Enabled = command.Enabled,
                Text = command.Implementation.Text,
                Visible = command.Visible
            };

            try
            {
                command.Implementation.QueryStatus(menu);
                command.Enabled = menu.Enabled;
                command.Visible = menu.Visible;
                command.Text = menu.Text;
                command.Checked = menu.Checked;
            }
            catch (Exception ex)
            {
                command.Enabled = command.Visible = false;
                command.tracer.Error(ex, Strings.VsCommandExtensionAdapter.QueryStatusShieldMessage);
            }
        }

        class MenuCommand : IMenuCommand
        {
            public bool Enabled { get; set; }
            public string Text { get; set; }
            public bool Visible { get; set; }
            public bool Checked { get; set; }
        }
    }
}