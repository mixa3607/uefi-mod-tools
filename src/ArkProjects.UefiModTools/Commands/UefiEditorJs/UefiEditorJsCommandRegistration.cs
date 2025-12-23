using System.CommandLine;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.UefiModTools.Commands.UefiEditorJs;

public class UefiEditorJsCommandRegistration
{
    public static void Register(Command parentCommand, IServiceCollection services)
    {
        var command = new Command("uefi-editor-js", "BoringBoredom/UEFI-Editor related tools");
        parentCommand.Add(command);

        RegisterRenderCommands(command, services);
    }

    private static void RegisterRenderCommands(Command parentCommand, IServiceCollection services)
    {
        static IServiceCollection RegisterServices(IServiceCollection services)
        {
            return services
                    .AddSingleton<UefiEditorJsRenderCommandHandlers>()
                ;
        }

        // unpack
        {
            var command = new Command("render-menu", "Render data.json to tree table");
            parentCommand.Add(command);

            var inputOpt = new Option<string>("--input", "-i")
            {
                Description = "Exported data.json file",
                DefaultValueFactory = _ => "data.json"
            };
            command.Add(inputOpt);

            var outputOpt = new Option<string>("--output", "-o")
            {
                Description = "Markdown table output",
                DefaultValueFactory = _ => "-",
            };
            command.Add(outputOpt);

            command.SetAction(opts =>
            {
                var di = RegisterServices(services).BuildServiceProvider();
                var handler = di.GetRequiredService<UefiEditorJsRenderCommandHandlers>();
                handler.RenderMenu(
                    opts.GetRequiredValue(inputOpt),
                    opts.GetRequiredValue(outputOpt)
                );
            });
        }
    }
}
