using System.CommandLine;
using ArkProjects.UefiModTools.Services.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace ArkProjects.UefiModTools.Utils;

public static class CommandExtensions
{
    extension(Command command)
    {
        public void SetAction<T>(IServiceCollection serviceCollection,
            Func<T, ParseResult, int> action) where T : class
        {
            command.SetAction(args =>
            {
                using var services = serviceCollection.BuildServiceProvider();
                var service = services.GetRequiredService<T>();
                return action(service, args);
            });
        }

        public void SetAction<T>(IServiceCollection serviceCollection,
            Func<T, ParseResult, CancellationToken, Task<int>> action) where T : class
        {
            command.SetAction((args, ct) =>
            {
                using var services = serviceCollection.BuildServiceProvider();
                var service = services.GetRequiredService<T>();
                return action(service, args, ct);
            });
        }

        public Command AddCommand(Command childCommand)
        {
            command.Add(childCommand);
            return childCommand;
        }

        public Command AddCommand(string name, string? description = null)
        {
            return command.AddCommand(new Command(name, description));
        }

        public Option<T> AddOption<T>(Option<T> option)
        {
            command.Add(option);
            return option;
        }

        public Option<T> AddOption<T>(string name, params string[] aliases)
        {
            return command.AddOption(new Option<T>(name, aliases));
        }

        public Option<SerializationFormat> AddFileFormatOption(Option<string> fileOption, string? optionName = null, string? description = null)
        {
            optionName ??= fileOption.Name + "-format";
            description ??= fileOption.Description + " format";
            return command.AddOption(new Option<SerializationFormat>(optionName)
            {
                Description = description,
                Required = false,
                DefaultValueFactory = r =>
                {
                    var resolveOutFile = r.GetResult(fileOption)?.Errors.Any() == false;
                    var output = resolveOutFile ? r.GetRequiredValue(fileOption) : "";
                    var lastDot = output.LastIndexOf('.');
                    var extension = lastDot >= 0 ? output.Substring(lastDot + 1).ToLower() : "";

                    if (extension is "json" or "jsonc")
                    {
                        return SerializationFormat.Json;
                    }

                    if (extension is "yaml" or "yml")
                    {
                        return SerializationFormat.Yaml;
                    }

                    return SerializationFormat.Auto;
                }
            });
        }
    }
}
