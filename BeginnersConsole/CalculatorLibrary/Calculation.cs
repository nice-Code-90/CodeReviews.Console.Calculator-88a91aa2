namespace CalculatorLibrary;

public class Calculation
{
    public double Operand1 { get; set; }
    public double Operand2 { get; set; }
    public string? Operation { get; set; }
    public double Result { get; set; }

    public override string ToString()
    {
        return $"{Operand1} {Operation} {Operand2} = {Result}";
    }
}