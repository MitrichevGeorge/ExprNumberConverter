using System;
using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using NCalc;
using Expression = NCalc.Expression;

class ExprNumberConverter : IYamlTypeConverter
{
    private readonly IReadOnlyDictionary<string, object> constants;

    public ExprNumberConverter(IReadOnlyDictionary<string, object> constants)
    {
        this.constants = constants ?? throw new ArgumentNullException(nameof(constants));
    }

    public bool Accepts(Type type) =>
        type == typeof(int) || type == typeof(int?) ||
        type == typeof(long) || type == typeof(long?) ||
        type == typeof(float) || type == typeof(float?) ||
        type == typeof(double) || type == typeof(double?);

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer _)
    {
        var scalar = parser.Consume<Scalar>();
        string s = scalar.Value?.Trim() ?? string.Empty;

        if (TryParseNumber(s, type, out object? direct))
            return direct;

        try
        {
            var expr = new Expression(s);
            foreach (var kv in constants)
            {
                expr.Parameters[kv.Key] = kv.Value;
            }

            object result = expr.Evaluate();
            return ConvertToTargetType(result, type);
        }
        catch (Exception ex)
        {
            throw new YamlException(scalar.Start, scalar.End,
                $"Не удалось вычислить '{s}' как число или выражение\n{ex.Message}", ex);
        }
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer _)
    {
        emitter.Emit(new Scalar(value?.ToString() ?? "null"));
    }

    private static bool TryParseNumber(string s, Type targetType, out object? value)
    {
        value = null;

        if (targetType == typeof(int) || targetType == typeof(int?))
            return int.TryParse(s, out int i) && (value = i) != null;

        if (targetType == typeof(long) || targetType == typeof(long?))
            return long.TryParse(s, out long l) && (value = l) != null;

        if (targetType == typeof(float) || targetType == typeof(float?))
            return float.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out float f)
                && (value = f) != null;

        if (targetType == typeof(double) || targetType == typeof(double?))
            return double.TryParse(s, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double d)
                && (value = d) != null;

        return false;
    }

    private static object ConvertToTargetType(object result, Type targetType)
    {
        if (targetType == typeof(int) || targetType == typeof(int?))
            return Convert.ToInt32(result);

        if (targetType == typeof(long) || targetType == typeof(long?))
            return Convert.ToInt64(result);

        if (targetType == typeof(float) || targetType == typeof(float?))
            return Convert.ToSingle(result);

        if (targetType == typeof(double) || targetType == typeof(double?))
            return Convert.ToDouble(result);

        throw new InvalidOperationException($"Unsupported target type: {targetType}");
    }
}
