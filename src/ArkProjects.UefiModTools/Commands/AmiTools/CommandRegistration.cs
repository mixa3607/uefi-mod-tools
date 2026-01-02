using System.CommandLine;
using ArkProjects.UefiModTools.Misc;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.UefiModTools.Commands.AmiTools;

public static class CommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        var command = parentCommand.AddCommand("ami", "AMI bin dumps related tools");

        BmcBackup.CommandRegistration.Register(command, services);
        BmcFmh.CommandRegistration.Register(command, services);
#if DEBUG
        BmcHpm.Registration.Register(command, services);
#endif
    }
}
