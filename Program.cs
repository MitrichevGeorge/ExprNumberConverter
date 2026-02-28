using NCalc;
using System;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Expression = NCalc.Expression;

public partial class Program
{
    static void Main()
    {
        string yaml = """
            count:     A * 2
            bigValue:  A * 1000000000
            ratio:     1 / A
            precision: A / 127.5
            """;

        var myConstants = new Dictionary<string, object>
        {
            ["A"] = 401,
        };

        var converter = new ExprNumberConverter(myConstants);

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .WithTypeConverter(converter)
            .Build();
        var cfg = deserializer.Deserialize<Config>(yaml);

        Console.WriteLine($"A (const) = {myConstants["A"]}");
        Console.WriteLine($"Count     = {cfg.Count}");
        Console.WriteLine($"BigValue  = {cfg.BigValue}");
        Console.WriteLine($"Ratio     = {cfg.Ratio}");
        Console.WriteLine($"Precision = {cfg.Precision}");
    }
}

class Config
{
    public int Count { get; set; }
    public long BigValue { get; set; }
    public float Ratio { get; set; }
    public double Precision { get; set; }
}
