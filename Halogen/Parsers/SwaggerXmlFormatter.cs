using System.Text.RegularExpressions;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Halogen.Parsers; 

public class SwaggerXmlFormatter: IOperationFilter {
    
    public void Apply(OpenApiOperation operation, OperationFilterContext context) {
            operation.Description = Formatted(operation.Description);
            operation.Summary = Formatted(operation.Summary);
        }

        private static string? Formatted(string? text) {
            if (text is null) return null;

            // Strip out the whitespace that messes up the markdown in the xml comments,
            // but don't touch the whitespace in <code> blocks. Those get fixed below.
            var resultString = Regex.Replace(text, @"(^[ \t]+)(?![^<]*>|[^>]*<\/)", "", RegexOptions.Multiline);
            resultString = Regex.Replace(resultString, @"<code[^>]*>", "<pre>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline);
            resultString = Regex.Replace(resultString, @"</code[^>]*>", "</pre>", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline);
            resultString = Regex.Replace(resultString, @"<!--", "", RegexOptions.Multiline);
            resultString = Regex.Replace(resultString, @"-->", "", RegexOptions.Multiline);

            try {
                const string pattern = @"<pre\b[^>]*>(.*?)</pre>";

                foreach (Match match in Regex.Matches(resultString, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Multiline )) {
                    var formattedPreBlock = FormatPreBlock(match.Value);
                    resultString = resultString.Replace(match.Value, formattedPreBlock);
                }
                
                return resultString;
            }
            catch (Exception) {
                return resultString; // The original string
            }
        }

        private static string FormatPreBlock(string preBlock) {
            var linesArray = preBlock.Split('\n'); // Split the <pre> block into multiple lines
            if (linesArray.Length < 2) return preBlock;
            
            // Get the 1st line after the <pre>
            var line = linesArray[1];
            var lineLength = line.Length;
            var formattedLine = line.TrimStart(' ', '\t');
            var paddingLength = lineLength - formattedLine.Length;

            // Remove the padding from all of the lines in the <pre> block
            for (var i = 1; i < linesArray.Length-1; i++) linesArray[i] = linesArray[i][paddingLength..];

            var formattedPreBlock = string.Join(string.Empty, linesArray);
            return formattedPreBlock;
        }
}