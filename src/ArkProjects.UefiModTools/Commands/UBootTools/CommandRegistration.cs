using System.CommandLine;
using ArkProjects.UefiModTools.Utils;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.UefiModTools.Commands.UBootTools;

public class CommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        var command = parentCommand.AddCommand("uboot", "UBoot related tools");
        Env.CommandRegistration.Register(command, services);
    }
}
