using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace ArkProjects.UefiModTools.Commands.UefiTools;

public class CommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        var command = new Command("uefi", "UEFI related tools");
        parentCommand.Add(command);

        Microcodes.CommandRegistration.Register(command, services);
        Fit.CommandRegistration.Register(command, services);
        SetupData.SetupDataCommandRegistration.Register(command, services);
        IfrSct.CommandRegistration.Register(command, services);
        IfrRender.CommandRegistration.Register(command, services);
        IfrBiosDefaults.CommandRegistration.Register(command, services);
    }
}
