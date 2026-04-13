using ConsoleInk;
using System.Text.RegularExpressions;

namespace Fidus
{
    /// <summary>
    /// Provides helpers to convert markdown content (titles, code blocks, inline code) into
    /// styled terminal text using ANSI escape codes from ConsoleInk.
    /// </summary>
    public static class MarkdownFormatter
    {
        // Bash syntax highlighting patterns
        private static readonly Dictionary<string, string> BashSyntaxPatterns = new()
        {
            // Comments
            { @"#.*$", Ansi.FgBrightBlack },
            // Strings (single and double quoted)
            { @"""[^""]*""", Ansi.FgBrightGreen },
            { @"'[^']*'", Ansi.FgBrightGreen },
            // Commands and common executables
            { @"\b(echo|cd|ls|pwd|cat|grep|sed|awk|find|chmod|chown|sudo|apt|yum|dnf|systemctl|service|curl|wget|ssh|scp|rsync|tar|gzip|gunzip|zip|unzip|make|gcc|g\+\+|python|python3|node|npm|docker|kubectl|git)\b", Ansi.FgBrightCyan },
            // Variables
            { @"\$[\w\d_]+", Ansi.FgBrightYellow },
            // Special variables
            { @"\$[\*\#\@\?\!\-\_\$]", Ansi.FgBrightYellow },
            // Operators
            { @"[\|\&\;\<\>]", Ansi.FgBrightMagenta },
            // Numbers
            { @"\b\d+\b", Ansi.FgBrightBlue },
            // Function definitions
            { @"\bfunction\s+\w+", Ansi.FgBrightCyan },
            // Conditionals and loops
            { @"\b(if|then|else|elif|fi|for|while|do|done|case|esac|in)\b", Ansi.FgBrightMagenta }
        };

        /// <summary>
        /// Converts a markdown title line to a styled string suitable for terminal output.
        /// Supports heading levels 1‑6. Unrecognised lines are returned unchanged.
        /// </summary>
        /// <param name="markdownLine">A single line that may start with one or more '#'.</param>
        /// <returns>A string with ANSI styling applied.</returns>
        private static string FormatTitle(string markdownLine)
        {
            if (string.IsNullOrWhiteSpace(markdownLine))
                return markdownLine;

            var trimmed = markdownLine.Trim();

            // Count leading '#'
            int level = 0;
            while (level < trimmed.Length && trimmed[level] == '#')
                level++;

            // Not a markdown title
            if (level == 0)
                return markdownLine;

            // Extract the title text after the hashes
            var title = trimmed.Substring(level).Trim();

            // Choose a style based on heading level
            string style = level switch
            {
                1 => Ansi.Bold + Ansi.FgBrightWhite,   // Level 1 – bold white
                2 => Ansi.FgBrightCyan,                // Level 2 – cyan
                3 => Ansi.FgBrightGreen,               // Level 3 – green
                4 => Ansi.FgBrightYellow,              // Level 4 – yellow
                5 => Ansi.FgBrightMagenta,             // Level 5 – magenta
                6 => Ansi.FgBrightBlue,                // Level 6 – blue
                _ => Ansi.FgBrightWhite                // Fallback
            };

            return $"{style}{title}{Ansi.Reset}";
        }

        /// <summary>
        /// Formats a complete markdown document, including titles, code blocks, inline code, and tables.
        /// </summary>
        /// <param name="markdownContent">The complete markdown content to format.</param>
        /// <returns>A string with ANSI styling applied for all markdown elements.</returns>
        public static string FormatDocument(string markdownContent)
        {
            if (string.IsNullOrWhiteSpace(markdownContent))
                return markdownContent;

            var lines = markdownContent.Split('\n');
            var result = new List<string>();
            bool inCodeBlock = false;
            bool inBashCodeBlock = false;
            bool inTable = false;
            string currentCodeBlock = "";
            var currentTable = new List<string>();

            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var trimmed = line.Trim();

                // Handle code blocks
                if (trimmed.StartsWith("```"))
                {
                    if (inCodeBlock)
                    {
                        // End of code block
                        inCodeBlock = false;
                        if (inBashCodeBlock)
                        {
                            result.Add(FormatBashCodeBlock(currentCodeBlock));
                            inBashCodeBlock = false;
                            currentCodeBlock = "";
                        }
                        else
                        {
                            // Non-bash code block - just add with basic formatting
                            result.Add($"{Ansi.FgBrightBlack}{currentCodeBlock}{Ansi.Reset}");
                            currentCodeBlock = "";
                        }
                    }
                    else
                    {
                        // Start of code block
                        inCodeBlock = true;
                        inBashCodeBlock = trimmed.Contains("bash") || trimmed.Contains("sh");
                        // Add the opening fence with subtle styling
                        result.Add($"{Ansi.FgBrightBlack}{line}{Ansi.Reset}");
                    }
                    continue;
                }

                if (inCodeBlock)
                {
                    currentCodeBlock += line + "\n";
                    continue;
                }

                // Handle tables
                if (IsTableRow(trimmed))
                {
                    if (!inTable)
                    {
                        // Start of new table
                        inTable = true;
                        currentTable.Clear();
                    }
                    currentTable.Add(line);
                    continue;
                }
                else if (inTable)
                {
                    // End of table - format and add it
                    result.Add(FormatTable(currentTable));
                    currentTable.Clear();
                    inTable = false;

                    // Process the current line normally
                    if (line.Contains('`'))
                    {
                        line = FormatInlineCode(line);
                    }
                    if (IsTitleLine(trimmed))
                    {
                        result.Add(FormatTitle(line));
                    }
                    else
                    {
                        result.Add(line);
                    }
                    continue;
                }

                // Handle inline code (single backticks)
                if (line.Contains('`'))
                {
                    line = FormatInlineCode(line);
                }

                // Handle bold text (double asterisks)
                if (line.Contains("**"))
                {
                    line = FormatBoldText(line);
                }

                // Handle titles
                if (IsTitleLine(trimmed))
                {
                    result.Add(FormatTitle(line));
                }
                else
                {
                    result.Add(line);
                }
            }

            // Handle case where document ends with a table
            if (inTable && currentTable.Count > 0)
            {
                result.Add(FormatTable(currentTable));
            }

            return string.Join("\n", result);
        }

        /// <summary>
        /// Formats inline code within a line (text between single backticks).
        /// </summary>
        /// <param name="line">The line containing inline code.</param>
        /// <returns>The line with inline code formatted.</returns>
        private static string FormatInlineCode(string line)
        {
            // Match text between single backticks, but not triple backticks (code blocks)
            var pattern = @"(?<!`)`([^`]+)`(?!`)";
            return Regex.Replace(line, pattern, match =>
            {
                var code = match.Groups[1].Value;
                return $"{Ansi.FgBrightGreen}{code}{Ansi.Reset}";
            });
        }

        /// <summary>
        /// Formats bold text within a line (text between double asterisks).
        /// </summary>
        /// <param name="line">The line containing bold text.</param>
        /// <returns>The line with bold text formatted.</returns>
        private static string FormatBoldText(string line)
        {
            // Match text between double asterisks
            var pattern = @"\*\*([^*]+)\*\*";
            return Regex.Replace(line, pattern, match =>
            {
                var boldText = match.Groups[1].Value;
                return $"{Ansi.Bold}{boldText}{Ansi.Reset}";
            });
        }

        /// <summary>
        /// Checks if a line is a markdown title (starts with #).
        /// </summary>
        /// <param name="line">The line to check.</param>
        /// <returns>True if the line is a markdown title.</returns>
        private static bool IsTitleLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            var trimmed = line.Trim();
            int level = 0;
            while (level < trimmed.Length && trimmed[level] == '#')
                level++;

            return level > 0 && level <= 6 && level < trimmed.Length && trimmed[level] == ' ';
        }

        /// <summary>
        /// Formats bash code with syntax highlighting.
        /// </summary>
        /// <param name="code">The bash code to format.</param>
        /// <returns>The formatted bash code with ANSI styling.</returns>
        private static string FormatBashCodeBlock(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                return code;

            var lines = code.Split('\n');
            var formattedLines = new List<string>();

            foreach (var line in lines)
            {
                if (string.IsNullOrWhiteSpace(line))
                {
                    formattedLines.Add("");
                    continue;
                }

                var formattedLine = line;

                // Apply syntax highlighting patterns
                foreach (var pattern in BashSyntaxPatterns)
                {
                    try
                    {
                        formattedLine = Regex.Replace(formattedLine, pattern.Key, match =>
                        {
                            return $"{pattern.Value}{match.Value}{Ansi.Reset}";
                        }, RegexOptions.Multiline);
                    }
                    catch (System.Exception)
                    {

                        formattedLine = line;
                    }

                }

                // Add a subtle background styling for better readability
                formattedLines.Add($"{Ansi.FgBrightWhite}{formattedLine}{Ansi.Reset}");
            }

            return string.Join("\n", formattedLines);
        }

        /// <summary>
        /// Checks if a line is a markdown table row.
        /// </summary>
        /// <param name="line">The line to check.</param>
        /// <returns>True if the line is a table row.</returns>
        private static bool IsTableRow(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return false;

            return line.StartsWith('|') && line.EndsWith('|');
        }

        /// <summary>
        /// Formats a markdown table with ANSI styling.
        /// </summary>
        /// <param name="tableLines">The lines that make up the table.</param>
        /// <returns>The formatted table with ANSI styling.</returns>
        private static string FormatTable(List<string> tableLines)
        {
            if (tableLines == null || tableLines.Count == 0)
                return "";

            var tableLinesAndColumns = tableLines.Select(line => line[1..^1].Split('|')).ToArray();

            var columnsNb = tableLinesAndColumns[0].Length;

            for (int i = 0; i < columnsNb; i++)
            {
                var linesForColumn = tableLinesAndColumns.Select(line => line[i]);
                var maxColumnLenght = linesForColumn.Max(line => line.Length);

                for (int j = 0; j < tableLinesAndColumns.Length; j++)
                {
                    var padChar = tableLinesAndColumns[j][i].Replace("-", string.Empty).Trim().Length == 0 ? '-' : ' ';
                    if (tableLinesAndColumns[j][i].Contains("**"))
                    {
                        tableLinesAndColumns[j][i] = FormatBoldText(tableLinesAndColumns[j][i]);
                        tableLinesAndColumns[j][i] = tableLinesAndColumns[j][i].PadRight(tableLinesAndColumns[j][i].Length + 4, padChar);
                    }
                    tableLinesAndColumns[j][i] = tableLinesAndColumns[j][i].PadRight(maxColumnLenght, padChar);
                }
            }

            var formattedLines = tableLinesAndColumns.Select(line => "|" + string.Join("|", line) + "|");

            return string.Join("\n", formattedLines);
        }
    }
}
