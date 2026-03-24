namespace CalculatorLibrary;

using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

public class Calculator
{
    private const string HistoryFilePath = "history.json";
    public List<Calculation> History { get; private set; } = new();

    public Calculator()
    {
        if (File.Exists(HistoryFilePath))
        {
            string json = File.ReadAllText(HistoryFilePath);
            History = JsonConvert.DeserializeObject<List<Calculation>>(json) ?? new();
        }
    }

    public void ClearHistory()
    {
        History.Clear();
        SaveHistory();
        Console.WriteLine("History cleared.\n");
    }

    private void SaveHistory()
    {
        string json = JsonConvert.SerializeObject(History, Formatting.Indented);
        File.WriteAllText(HistoryFilePath, json);
    }

    public double DoOperation(double num1, double num2, string op)
    {
        double result = op switch
        {
            "+" => num1 + num2,
            "-" => num1 - num2,
            "*" => num1 * num2,
            "/" => num2 != 0 ? num1 / num2 : double.NaN,
            _ => double.NaN
        };

        if (!double.IsNaN(result))
        {
            History.Add(new Calculation { Operand1 = num1, Operand2 = num2, Operation = op, Result = result });
            if (History.Count > 10)
            {
                History.RemoveAt(0);
            }
            SaveHistory();
        }

        return result;
    }
}