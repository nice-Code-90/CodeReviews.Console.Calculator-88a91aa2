namespace CalculatorProgram;

using CalculatorLibrary;
using Microsoft.CognitiveServices.Speech;
using Microsoft.Extensions.Configuration;
using Spectre.Console;
using System.Threading.Tasks;

internal class Program
{
    private static async Task Main(string[] args)
    {
        string environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .Build();

        string? speechKey = configuration["AzureSpeachService:Key"];
        string? speechLocation = configuration["AzureSpeachService:Location"];

        var config = SpeechConfig.FromSubscription(speechKey, speechLocation);
        using var recognizer = new SpeechRecognizer(config);

        int usedTimes = 0;

        AnsiConsole.Write(new Rule("[bold green]Console Calculator in C#[/]").RuleStyle("yellow"));
        AnsiConsole.WriteLine();
        Calculator calculator = new Calculator();
        Console.Clear();
        while (true)
        {
            double cleanNum1 = await Helpers.GetUserInputAsync(calculator, "Type a number, or a-j for use previous results as an input and then press Enter: ", recognizer);
            double cleanNum2 = await Helpers.GetUserInputAsync(calculator, "Type another number, or a-j for use previous results, and then press Enter: ", recognizer);

            var panel = new Panel("\t[bold]+[/] - Add\n\t[bold]-[/] - Subtract\n\t[bold]*[/] - Multiply\n\t[bold]/[/] - Divide")
            {
                Header = new PanelHeader("[cyan]Choose an operator[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.DeepSkyBlue2),
                Padding = new Padding(1)
            };
            AnsiConsole.Write(panel);
            Console.Write("Your option? ");

            string opInput = await Helpers.ReadInputWithSpeechAsync(recognizer, "[cyan]Listening for operator...[/]");
            string op = Helpers.ConvertSpokenOperator(opInput);

            if (op is not ("+" or "-" or "*" or "/"))
            {
                AnsiConsole.MarkupLine("[red]Error: Unrecognized input.[/]");
            }
            else
            {
                try
                {
                    double result = calculator.DoOperation(cleanNum1, cleanNum2, op);
                    if (double.IsNaN(result))
                    {
                        AnsiConsole.MarkupLine("[red]This operation will result in a mathematical error.[/]\n");
                    }
                    else
                    {
                        AnsiConsole.MarkupLine($"Your result: [bold red]{result:0.##}[/]\n");
                    }
                }
                catch (Exception e)
                {
                    AnsiConsole.MarkupLine($"[red]Oh no! An exception occurred trying to do the math.\n - Details: {e.Message}[/]");
                }
            }
            AnsiConsole.Write(new Rule().RuleStyle("grey"));
            AnsiConsole.WriteLine();
            usedTimes++;

            Console.Write("Press 'n' and Enter to close the app, or press any other key and Enter to continue, or press 's' and say 'Exit' to quit:");
            string closeApp = await Helpers.ReadInputWithSpeechAsync(recognizer);
            if (closeApp.ToLower() is "n" or "exit")
            {
                Console.WriteLine($"Calculator was used {usedTimes} times.");
                break;
            }

            Console.WriteLine("\n");
        }

        return;
    }
}