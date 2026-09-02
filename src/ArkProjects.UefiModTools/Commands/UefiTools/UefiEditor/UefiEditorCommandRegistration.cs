using ArkProjects.UefiModTools.Utils;
using Microsoft.Extensions.DependencyInjection;
using System.CommandLine;

namespace ArkProjects.UefiModTools.Commands.UefiTools.UefiEditor;

public static class UefiEditorCommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        services.AddSingleton<UefiEditorHtmlRenderer>().AddSingleton<UefiEditorServer>().AddSingleton<UefiEditorCommandHandlers>();
        var editor = parentCommand.AddCommand("editor", "UEFI editor");
        var serve = editor.AddCommand("serve", "Serve an IFR document in the UEFI editor");
        var input = serve.AddOption(new Option<string>("--input", "-i") { Required = true });
        var address = serve.AddOption(new Option<string>("--address", "-a") { Required = true });
        serve.SetAction<UefiEditorCommandHandlers>(services,
            (handler, options) => handler.Serve(options.GetRequiredValue(input), options.GetRequiredValue(address)));
    }
}
