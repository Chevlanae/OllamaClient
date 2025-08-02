using OllamaClient.Models.Json;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace OllamaClient.Models
{
    public class ModelFile
    {
        public static class RegularExpressions
        {
            public static class Patterns
            {
                private static string TripleQuoteString = "\"\"\"(\\s|\\S)*?\"\"\"";
                private static string DoubleQuoteString = "\"(\\s|\\S)*?\"";
                private static string DoubleOrTripleQuoteString = $"({TripleQuoteString}|{DoubleQuoteString})";
                public static string FROM = "\nFROM\\s.*";
                public static string TEMPLATE = $"\nTEMPLATE\\s{DoubleOrTripleQuoteString}";
                public static string PARAMETER = "\nPARAMETER\\s.*\\s.*?";
                public static string SYSTEM = $"\nSYSTEM\\s.*";
                public static string ADAPTER = "\nADAPTER\\s.*";
                public static string LICENSE = $"\nLICENSE\\s{DoubleOrTripleQuoteString}";
                public static string MESSAGE = "\nMESSAGE\\s(user|assistant|system|tool)\\s.*";
            }

            public static Regex FROM = new(Patterns.FROM, RegexOptions.Compiled);
            public static Regex TEMPLATE = new(Patterns.TEMPLATE, RegexOptions.Compiled);
            public static Regex PARAMETER = new(Patterns.PARAMETER, RegexOptions.Compiled);
            public static Regex SYSTEM = new(Patterns.SYSTEM, RegexOptions.Compiled);
            public static Regex ADAPTER = new(Patterns.ADAPTER, RegexOptions.Compiled);
            public static Regex LICENSE = new(Patterns.LICENSE, RegexOptions.Compiled);
            public static Regex MESSAGE = new(Patterns.MESSAGE, RegexOptions.Compiled);
        }

        private enum Instruction
        {
            FROM,
            TEMPLATE,
            PARAMETER,
            SYSTEM,
            ADAPTER,
            LICENSE,
            MESSAGE
        }

        public string From { get; set; }
        public string? Template { get; set; }
        public List<IModelParameter>? Parameters { get; set; }
        public string? System { get; set; }
        public string? Adapter { get; set; }
        public string? License { get; set; }
        public List<Message>? Messages { get; set; }

        /// <summary>
        ///  Parses the given file string into a ModelFile object.
        /// </summary>
        /// <param name="fileString"></param>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="OverflowException" />
        public ModelFile(string fileString)
        {
            Match fromMatch = RegularExpressions.FROM.Match(fileString);
            Match templateMatch = RegularExpressions.TEMPLATE.Match(fileString);
            MatchCollection parameterMatches = RegularExpressions.PARAMETER.Matches(fileString);
            Match systemMatch = RegularExpressions.SYSTEM.Match(fileString);
            Match adapterMatch = RegularExpressions.ADAPTER.Match(fileString);
            Match licenseMatch = RegularExpressions.LICENSE.Match(fileString);
            MatchCollection messageMatches = RegularExpressions.MESSAGE.Matches(fileString);

            if (fromMatch.Success) From = ParseInstructionValue(Instruction.FROM, fromMatch.Value);
            else throw new ArgumentException($"Invalid file string. {RegularExpressions.Patterns.FROM} pattern is missing from the source.");

            if (templateMatch.Success) Template = ParseInstructionValue(Instruction.TEMPLATE, templateMatch.Value);

            if (parameterMatches.Count > 0) Parameters = [];

            foreach (Match parameter in parameterMatches)
            {
                if (parameter.Success)
                {
                    ParseAndAggregateModelParameter(parameter.Value);
                }
            }

            if (systemMatch.Success) System = ParseInstructionValue(Instruction.SYSTEM, systemMatch.Value);

            if (adapterMatch.Success) Adapter = ParseInstructionValue(Instruction.ADAPTER, adapterMatch.Value);

            if (licenseMatch.Success) License = ParseInstructionValue(Instruction.LICENSE, licenseMatch.Value);

            if (messageMatches.Count > 0) Messages = [];

            foreach (Match message in messageMatches)
            {
                if (message.Success)
                {
                    ParseAndAggregateMessage(message.Value);
                }
            }
        }

        /// <summary>
        /// Parses the given source string into a ModelParameter object and aggregates it into Parameters.
        /// </summary>
        /// <param name="sourceString"></param>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        /// <exception cref="OverflowException" />
        private void ParseAndAggregateModelParameter(string sourceString)
        {
            string parameterValueString = ParseInstructionValue(Instruction.PARAMETER, sourceString);

            string[] keyValue = parameterValueString.Split(" ", 2, StringSplitOptions.TrimEntries);

            Parameters?.Add(new ModelParameterKeyValue(Enum.Parse<ModelParameterKey>(keyValue[0]), keyValue[1]));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="sourceString"></param>
        /// <exception cref="FormatException"></exception>
        /// <exception cref="ArgumentException" />
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="InvalidOperationException" />
        private void ParseAndAggregateMessage(string sourceString)
        {
            string parameterValueString = ParseInstructionValue(Instruction.MESSAGE, sourceString);

            string[] keyValue = parameterValueString.Split(" ", 2, StringSplitOptions.TrimEntries);

            Messages?.Add(new(Enum.Parse<Role>(keyValue[0]), keyValue[1]));
        }

        /// <summary>
        /// Parses the given string for a model file parameter ("FROM input", "TEMPLATE \"\"\"input\"\"\"", etc.), and returns the value (input).
        /// </summary>
        /// <param name="sourceString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException" />
        /// <exception cref="ArgumentException" />
        private string ParseInstructionValue(Instruction instruction, string sourceString)
        {
            switch (instruction)
            {
                case Instruction.FROM:
                    return sourceString.Replace("\nFROM ", string.Empty);
                case Instruction.TEMPLATE:
                    return sourceString.Replace("\nTEMPLATE ", string.Empty);
                case Instruction.PARAMETER:
                    return sourceString.Replace("\nPARAMETER ", string.Empty);
                case Instruction.SYSTEM:
                    return sourceString.Replace("\nSYSTEM ", string.Empty);
                case Instruction.ADAPTER:
                    return sourceString.Replace("\nADAPTER ", string.Empty);
                case Instruction.LICENSE:
                    return sourceString.Replace("\nLICENSE ", string.Empty);
                case Instruction.MESSAGE:
                    return sourceString.Replace("\nMESSAGE ", string.Empty);
                default:
                    return sourceString;
            }
        }

        public new string ToString()
        {
            StringBuilder sb = new();
            sb.AppendLine($"FROM {From}");
            sb.Append($"{Environment.NewLine}");
            if (Template is not null)
            {
                sb.AppendLine($"TEMPLATE {Template}"); 
                sb.Append($"{Environment.NewLine}");
            }
            if (Adapter is not null)
            {
                sb.AppendLine($"ADAPTER {Adapter}");
                sb.Append($"{Environment.NewLine}");
            }
            if (System is not null)
            {
                sb.AppendLine($"SYSTEM {System}");
                sb.Append($"{Environment.NewLine}");
            }
            if (Parameters is not null)
            {
                foreach (IModelParameter parameter in Parameters)
                {
                    sb.AppendLine($"PARAMETER {parameter.Key.ToString()} {parameter.Value}");
                }
                sb.Append($"{Environment.NewLine}");
            }
            if (Messages is not null)
            {
                foreach (IMessage message in Messages)
                {
                    sb.AppendLine($"MESSAGE {message.role} {message.content}");
                }
                sb.Append($"{Environment.NewLine}");
            }
            if (License is not null)
            {
                sb.AppendLine($"LICENSE {License}");
                sb.Append($"{Environment.NewLine}");
            }
            return sb.ToString();
        }
    }
}
