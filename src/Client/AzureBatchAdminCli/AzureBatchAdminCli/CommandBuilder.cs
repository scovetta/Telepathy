namespace AzureBatchAdminCli
{
    using System.CommandLine;
    using System.CommandLine.Invocation;

    using AzureBatchAdminCli.Session;

    internal class CommandBuilder
    {
        public RootCommand BuildRootCommand()
        {
            var rootCommand = new RootCommand { Description = "Telepathy Azure Batch Admin Client" };

            // Sub-commands
            rootCommand.AddCommand(this.BuildSessionCommand());

            return rootCommand;
        }

        private Command BuildSessionCommand()
        {
            var sessionCommand = new Command("session");

            // Sub-commands
            sessionCommand.AddCommand(this.BuildSessionLauncherCommand());

            return sessionCommand;
        }

        private Command BuildSessionLauncherCommand()
        {
            var sessionLauncherCommand = new Command("launcher");

            // Leaf commands
            var startCommand = new Command("start");
            startCommand.Handler = CommandHandler.Create(async () => await new SessionLauncherStarter().StartAsync());
            sessionLauncherCommand.AddCommand(startCommand);

            var stopCommand = new Command("stop");
            stopCommand.Handler = CommandHandler.Create(async () => await new SessionLauncherStarter().StopAsync());
            sessionLauncherCommand.AddCommand(stopCommand);

            return sessionLauncherCommand;
        }
    }
}
