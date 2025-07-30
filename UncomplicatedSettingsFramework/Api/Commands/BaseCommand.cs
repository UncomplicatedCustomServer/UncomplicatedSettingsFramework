﻿using CommandSystem;
using LabApi.Features.Permissions;
using System;
using System.Collections.Generic;
using System.Linq;
using UncomplicatedSettingsFramework.Api.Commands.Admin;
using UncomplicatedSettingsFramework.Api.Interfaces;

namespace UncomplicatedSettingsFramework.Api.Commands
{
    [CommandHandler(typeof(RemoteAdminCommandHandler))]
    internal class BaseCommand : ParentCommand
    {
        public BaseCommand() => LoadGeneratedCommands();

        public override string Command => "usf";

        public override string Description => "Manage the UncomplicatedSettingsFramework plugin features";

        public override string[] Aliases => [];

        public override void LoadGeneratedCommands()
        {
            Subcommands.Add(new Reload());
            Subcommands.Add(new Get());
            Subcommands.Add(new Give());
            Subcommands.Add(new List());
            Subcommands.Add(new Remove());
        }

        private List<ISubcommand> Subcommands { get; } = [];

        protected override bool ExecuteParent(ArraySegment<string> arguments, ICommandSender sender, out string response)
        {
            if (arguments.Count == 0)
            {
                response = $"UncomplicatedSettingsFramework v{Plugin.Instance.Version} by Mr. Baguetter\n\n<size=35>Available commands:</size>";
                foreach (ISubcommand command in Subcommands)
                    response += $"\n- usf {command.Name}{(command.VisibleArgs != string.Empty ? $" {command.VisibleArgs}" : "")} - {command.Description}";

                return true;
            }

            ISubcommand cmd = Subcommands.FirstOrDefault(cmd => cmd.Name == arguments.At(0));

            cmd ??= Subcommands.FirstOrDefault(cmd => cmd.Aliases.Contains(arguments.At(0)));

            if (cmd is null)
            {
                response = "Command not found!";
                return false;
            }

            if (!sender.HasPermissions(cmd.RequiredPermission))
            {
                response = $"You don't have permission to access that command! \n Required permission: {cmd.RequiredPermission}";
                return false;
            }

            if (arguments.Count < cmd.RequiredArgsCount)
            {
                response = $"Wrong usage!\nCorrect usage: usf {cmd.Name} {cmd.VisibleArgs}";
                return false;
            }

            List<string> args = [.. arguments];
            args.RemoveAt(0);

            return cmd.Execute(args, sender, out response);
        }
    }
}
