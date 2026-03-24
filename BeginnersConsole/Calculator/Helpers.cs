namespace CalculatorProgram;

using CalculatorLibrary;
using System;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using Spectre.Console;

internal class Helpers
{
    private const string FooterText = "Commands: 'v' - view history, 'c' - clear history, 's' - speak.";

    internal static bool TryGetHistoryValue(Calculator calculator, string? input, out double value)
    {
        value = 0;
        if (string.IsNullOrEmpty(input) || input.Length != 1) return false;

        string allowedLetters = "abcdefghij";
        int index = allowedLetters.IndexOf(input.ToLower());

        if (index != -1 && index < calculator.History.Count)
        {
            value = calculator.History[index].Result;
            return true;
        }
        return false;
    }

    internal static async Task<double> GetUserInputAsync(Calculator calculator, string prompt, SpeechRecognizer recognizer)
    {
        while (true)
        {
            Console.Write(prompt);
            string input = await ReadInputWithSpeechAsync(recognizer);

            switch (input.ToLower())
            {
                case "v":
                    PrintHistory(calculator);
                    continue;
                case "c":
                    calculator.ClearHistory();
                    continue;
            }

            if (double.TryParse(input, out double cleanNum) || TryGetHistoryValue(calculator, input, out cleanNum))
            {
                return cleanNum;
            }

            AnsiConsole.MarkupLine("[red]This is not valid input. Please enter a numeric value or valid history letter (a-j).[/]");
        }
    }

    internal static async Task<string> ReadInputWithSpeechAsync(SpeechRecognizer recognizer, string listeningMessage = "[cyan]Listening...[/]")
    {
        ShowFooter();
        string? input = Console.ReadLine()?.Trim();
        ClearFooter();

        if (input?.ToLower() == "s")
        {
            AnsiConsole.MarkupLine(listeningMessage);
            input = await GetSpeechInputAsync(recognizer);
        }

        return input ?? string.Empty;
    }

    internal static async Task<string> GetSpeechInputAsync(SpeechRecognizer recognizer)
    {
        var result = await recognizer.RecognizeOnceAsync();
        if (result.Reason == ResultReason.RecognizedSpeech)
        {
            string text = result.Text.Trim().ToLower().Replace(".", "").Replace("?", "");
            AnsiConsole.MarkupLine($"Recognized: [green]{text}[/]");
            return text;
        }
        else if (result.Reason == ResultReason.Canceled)
        {
            var cancellation = CancellationDetails.FromResult(result);
            AnsiConsole.MarkupLine($"\n[bold red][[ERROR]] Speech Recognition Canceled: {cancellation.Reason}[/]");
            if (cancellation.Reason == CancellationReason.Error)
            {
                AnsiConsole.MarkupLine($"[bold red][[ERROR]] ErrorCode: {cancellation.ErrorCode}[/]");
                AnsiConsole.MarkupLine($"[bold red][[ERROR]] ErrorDetails: {cancellation.ErrorDetails}[/]");
            }
        }

        AnsiConsole.MarkupLine("[yellow]Could not recognize speech.[/]");
        return string.Empty;
    }

    internal static string ConvertSpokenOperator(string input)
    {
        return input switch
        {
            "add" or "plus" => "+",
            "subtract" or "minus" => "-",
            "multiply" or "times" => "*",
            "divide" or "divided by" => "/",
            _ => input
        };
    }

    internal static void PrintHistory(Calculator calculator)
    {
        if (calculator.History.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]History is empty.[/]\n");
            return;
        }

        var table = new Table().Centered();
        table.AddColumn("[bold yellow]Id[/]");
        table.AddColumn("[bold yellow]Operation[/]");
        table.AddColumn("[bold yellow]Result[/]");

        for (int i = 0; i < calculator.History.Count; i++)
        {
            char letter = (char)('a' + i);
            var calc = calculator.History[i];
            table.AddRow($"[green]{letter}[/]", $"[cyan]{calc.Operand1} {calc.Operation} {calc.Operand2}[/]", $"[bold red]{calc.Result}[/]");
        }

        AnsiConsole.Write(
            new Panel(table)
            {
                Header = new PanelHeader("[underline cyan]Calculation History[/]"),
                Border = BoxBorder.Rounded,
                BorderStyle = new Style(Color.GreenYellow),
                Padding = new Padding(1)
            });
        AnsiConsole.WriteLine();
    }

    private static (int CursorColumnPosition, int CursorRowPosition, int BottomRowPosition, int FooterColumnPosition) GetFooterPositions()
    {
        return (
            Console.CursorLeft,
            Console.CursorTop,
            Console.WindowTop + Console.WindowHeight - 1,
            Console.WindowWidth - FooterText.Length - 1
        );
    }

    private static void ShowFooter()
    {
        var pos = GetFooterPositions();

        if (pos.FooterColumnPosition > 0 && (pos.CursorRowPosition < pos.BottomRowPosition || pos.CursorColumnPosition < pos.FooterColumnPosition))
        {
            Console.SetCursorPosition(pos.FooterColumnPosition, pos.BottomRowPosition);
            AnsiConsole.Markup($"[grey]{FooterText}[/]");
            Console.SetCursorPosition(pos.CursorColumnPosition, pos.CursorRowPosition);
        }
    }

    private static void ClearFooter()
    {
        var pos = GetFooterPositions();

        if (pos.FooterColumnPosition > 0)
        {
            Console.SetCursorPosition(pos.FooterColumnPosition, pos.BottomRowPosition);
            Console.Write(new string(' ', FooterText.Length));

            if (pos.BottomRowPosition - 1 >= 0 && (pos.CursorRowPosition < pos.BottomRowPosition - 1 || pos.CursorColumnPosition < pos.FooterColumnPosition))
            {
                Console.SetCursorPosition(pos.FooterColumnPosition, pos.BottomRowPosition - 1);
                Console.Write(new string(' ', FooterText.Length));
            }

            Console.SetCursorPosition(pos.CursorColumnPosition, pos.CursorRowPosition);
        }
    }
}