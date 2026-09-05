using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace ArkProjects.UefiModTools.Commands.UefiTools;

public class CommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        var command = new Command("uefi", "UEFI related tools");
        parentCommand.Add(command);

        Fit.FitCommandRegistration.Register(command, services);
        SetupData.SetupDataCommandRegistration.Register(command, services);
        Sct.SctCommandRegistration.Register(command, services);
        Ifr.IfrCommandRegistration.Register(command, services);
        UefiEditor.UefiEditorCommandRegistration.Register(command, services);
        BiosDefaults.BiosDefaultsCommandRegistration.Register(command, services);
    }
}
