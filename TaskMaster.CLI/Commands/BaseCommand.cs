using CommandLine;

namespace TaskMaster.CLI.Commands
{
    public abstract class BaseCommand
    {
        public abstract Task<int> ExecuteAsync();
    }
}