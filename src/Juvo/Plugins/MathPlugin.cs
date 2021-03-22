// <copyright file="MathPlugin.cs" company="https://gitlab.com/edrochenski/juvo">
// Licensed under the MIT License. See LICENSE in the project root for license information.
// </copyright>

#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JuvoProcess;
using JuvoProcess.Bots;

/// <summary>
/// Math plugin that just forwards requests onto the .NET Math class.
/// </summary>
public class MathPlugin : IBotPlugin
{
    /// <inheritdoc/>
    public IList<string> Commands => new[]
    {
        "abs", "acos", "asin", "atan", "atan2", "atanh", "cbrt", "cos",
        "cosh", "log", "pow", "sin", "sinh", "sqrt", "tan", "tanh"
    };

    /// <inheritdoc/>
    public IList<int>? CommandTimeMin => default;

    /// <inheritdoc/>
    public Task<IBotCommand> Execute(IBotCommand cmd, IJuvoClient client)
    {
        try
        {
            var cmdParts = cmd.RequestText.Split(' ');
            switch (cmdParts[0].ToLowerInvariant())
            {
                case "abs":
                {
                    if (cmdParts.Length < 2) { cmd.ResponseText = "USAGE: abs <value>"; }
                    cmd.ResponseText = $"> {this.Abs(cmdParts[1])}";
                    break;
                }

                case "acos":
                {
                    if (cmdParts.Length < 2) { cmd.ResponseText = "USAGE: acos <value>"; }
                    cmd.ResponseText = $"> {this.Acos(cmdParts[1])}";
                    break;
                }

                case "asin":
                {
                    if (cmdParts.Length < 2) { cmd.ResponseText = "USAGE: asin <value>"; }
                    cmd.ResponseText = $"> {this.Asin(cmdParts[1])}";
                    break;
                }

                case "atan":
                {
                    if (cmdParts.Length < 2) { cmd.ResponseText = "USAGE: atan <value>"; }
                    cmd.ResponseText = $"> {this.Atan(cmdParts[1])}";
                    break;
                }

                case "atan2":
                {
                    if (cmdParts.Length < 3) { cmd.ResponseText = "USAGE: atan2 <y> <x>"; }
                    cmd.ResponseText = $"> {this.Atan2(cmdParts[1], cmdParts[2])}";
                    break;
                }

                case "atanh":
                {
                    if (cmdParts.Length < 2) { cmd.ResponseText = "USAGE: atanh <value>"; }
                    cmd.ResponseText = $"> {this.Atanh(cmdParts[1])}";
                    break;
                }

                case "cbrt":
                {
                    if (cmdParts.Length < 2) { cmd.ResponseText = "USAGE: cbrt <value>"; }
                    cmd.ResponseText = $"> {this.Cbrt(cmdParts[1])}";
                    break;
                }

                case "cos":
                {
                    if (cmdParts.Length < 2) { cmd.ResponseText = "USAGE: cos <value>"; }
                    cmd.ResponseText = $"> {this.Cos(cmdParts[1])}";
                    break;
                }

                case "cosh":
                {
                    if (cmdParts.Length < 2) { cmd.ResponseText = "USAGE: cosh <value>"; }
                    cmd.ResponseText = $"> {this.Cosh(cmdParts[1])}";
                    break;
                }

                case "log":
                {
                    if (cmdParts.Length < 3) { cmd.ResponseText = "USAGE: log <value> [base: e]"; }
                    cmd.ResponseText = $"> {this.Log(cmdParts[1], cmdParts.Length > 2 ? cmdParts[2] : string.Empty)}";
                    break;
                }

                case "pow":
                {
                    if (cmdParts.Length < 3) { cmd.ResponseText = "USAGE: pow <x> <y>"; }
                    cmd.ResponseText = $"> {this.Pow(cmdParts[1], cmdParts[2])}";
                    break;
                }

                case "sin":
                {
                    if (cmdParts.Length < 2) { cmd.ResponseText = "USAGE: sin <value>"; }
                    cmd.ResponseText = $"> {this.Sin(cmdParts[1])}";
                    break;
                }

                case "sinh":
                {
                    if (cmdParts.Length < 2) { cmd.ResponseText = "USAGE: sinh <value>"; }
                    cmd.ResponseText = $"> {this.Sinh(cmdParts[1])}";
                    break;
                }

                case "sqrt":
                {
                    if (cmdParts.Length < 2) { cmd.ResponseText = "USAGE: sqrt <value>"; }
                    cmd.ResponseText = $"> {this.Sqrt(cmdParts[1])}";
                    break;
                }

                case "tan":
                {
                    if (cmdParts.Length < 2) { cmd.ResponseText = "USAGE: tan <value>"; }
                    cmd.ResponseText = $"> {this.Tan(cmdParts[1])}";
                    break;
                }

                case "tanh":
                {
                    if (cmdParts.Length < 2) { cmd.ResponseText = "USAGE: tanh <value>"; }
                    cmd.ResponseText = $"> {this.Tanh(cmdParts[1])}";
                    break;
                }
            }
        }
        catch (ArgumentException exc)
        {
            cmd.ResponseText = $"! {exc.Message}";
        }

        return Task.FromResult(cmd);
    }

    private string Abs(string value)
    {
        var valobj = this.Test(value);
        switch (valobj.GetType().Name)
        {
            case "Single": return Math.Abs((float)valobj).ToString();
            case "Double": return Math.Abs((double)valobj).ToString();
            case "Decimal": return Math.Abs((decimal)valobj).ToString();

            case "Int16":
            case "UInt16":
            case "Int32":
            case "UInt32": return Math.Abs((int)valobj).ToString();

            case "Int64":
            case "UInt64": return Math.Abs((long)valobj).ToString();
        }

        throw new ArgumentException($"abs({value}) failed");
    }

    private string Acos(string value)
    {
        if (!double.TryParse(value, out var result)) { throw new ArgumentException($"{value} could not be parsed as double"); }
        if (result < -1.0d || result > 1.0d) { throw new ArgumentException($"value must be between -1.0 and 1.0"); }

        return Math.Acos(result).ToString();
    }

    private string Asin(string value)
    {
        if (!double.TryParse(value, out var result)) { throw new ArgumentException($"{value} could not be parsed as double"); }
        if (result < -1.0d || result > 1.0d) { throw new ArgumentException($"value must be between -1.0 and 1.0"); }

        return Math.Asin(result).ToString();
    }

    private string Atan(string value)
    {
        if (!double.TryParse(value, out var result)) { throw new ArgumentException($"{value} could not be parsed as double"); }

        return Math.Atan(result).ToString();
    }

    private string Atan2(string y, string x)
    {
        if (!double.TryParse(y, out var result1)) { throw new ArgumentException($"{y} could not be parsed as double"); }
        if (!double.TryParse(x, out var result2)) { throw new ArgumentException($"{x} could not be parsed as double"); }

        return Math.Atan2(result1, result2).ToString();
    }

    private string Atanh(string value)
    {
        if (!double.TryParse(value, out var result)) { throw new ArgumentException($"{value} could not be parsed as double"); }

        return Math.Atanh(result).ToString();
    }

    private string Cbrt(string value)
    {
        if (!double.TryParse(value, out var result)) { throw new ArgumentException($"{value} could not be parsed as double"); }

        return Math.Cbrt(result).ToString();
    }

    private string Cos(string value)
    {
        if (!double.TryParse(value, out var result)) { throw new ArgumentException($"{value} could not be parsed as double"); }

        return Math.Cos(result).ToString();
    }

    private string Cosh(string value)
    {
        if (!double.TryParse(value, out var result)) { throw new ArgumentException($"{value} could not be parsed as double"); }

        return Math.Cosh(result).ToString();
    }

    private string Log(string value, string logbase)
    {
        if (!double.TryParse(value, out var result1)) { throw new ArgumentException($"{value} could not be parsed as double"); }

        var lb = Math.E;
        if (!string.IsNullOrEmpty(logbase))
        {
            if (!double.TryParse(logbase, out var result2)) { throw new ArgumentException($"{logbase} could not be parsed as double"); }
            lb = result2;
        }

        return Math.Log(result1, lb).ToString();
    }

    private string Pow(string x, string y)
    {
        if (!double.TryParse(x, out var result1)) { throw new ArgumentException($"{x} could not be parsed as double"); }
        if (!double.TryParse(y, out var result2)) { throw new ArgumentException($"{y} could not be parsed as double"); }

        return Math.Pow(result1, result2).ToString();
    }

    private string Sin(string value)
    {
        if (!double.TryParse(value, out var result)) { throw new ArgumentException($"{value} could not be parsed as double"); }

        return Math.Sin(result).ToString();
    }

    private string Sinh(string value)
    {
        if (!double.TryParse(value, out var result)) { throw new ArgumentException($"{value} could not be parsed as double"); }

        return Math.Sinh(result).ToString();
    }

    private string Sqrt(string value)
    {
        if (!double.TryParse(value, out var result)) { throw new ArgumentException($"{value} could not be parsed as double"); }

        return Math.Sqrt(result).ToString();
    }

    private string Tan(string value)
    {
        if (!double.TryParse(value, out var result)) { throw new ArgumentException($"{value} could not be parsed as double"); }

        return Math.Tan(result).ToString();
    }

    private string Tanh(string value)
    {
        if (!double.TryParse(value, out var result)) { throw new ArgumentException($"{value} could not be parsed as double"); }

        return Math.Tanh(result).ToString();
    }

    private object Test(string value)
    {
        if (value.EndsWith("UL"))
        {
            if (ulong.TryParse(value, out var result)) { return result; }
            throw new ArgumentException($"{value} could not be parsed as ulong");
        }

        if (value.EndsWith("L"))
        {
            if (long.TryParse(value, out var result)) { return result; }
            throw new ArgumentException($"{value} could not be parsed as long");
        }

        if (value.EndsWith("U"))
        {
            if (uint.TryParse(value, out var result)) { return result; }
            throw new ArgumentException($"{value} could not be parsed as uint");
        }

        if (value.EndsWith("F"))
        {
            if (float.TryParse(value, out var result)) { return result; }
            throw new ArgumentException($"{value} could not be parsed as float");
        }

        if (value.EndsWith("D"))
        {
            if (double.TryParse(value, out var result)) { return result; }
            throw new ArgumentException($"{value} could not be parsed as double");
        }

        if (value.EndsWith("M"))
        {
            if (decimal.TryParse(value, out var result)) { return result; }
            throw new ArgumentException($"{value} could not be parsed as decimal");
        }

        if (value.Contains("."))
        {
            if (decimal.TryParse(value, out var result)) { return result; }
            throw new ArgumentException($"{value} could not be parsed as decimal");
        }

        if (long.TryParse(value, out var valResult)) { return valResult; }
        throw new ArgumentException($"{value} could not be parsed as long");
    }
}
