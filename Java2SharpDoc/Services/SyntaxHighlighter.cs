using System;
using System.Collections.Generic;

namespace Java2SharpDoc.Services
{
    /// <summary>
    /// Because RichTextBox :(
    /// </summary>
    public class SyntaxHighlighter
    {
        /// <summary>
        /// :(
        /// </summary>
        public (string, string) Convert(string input)
        {
            return Convert(input?.Trim().Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries));
        }

        /// <summary>
        /// :(
        /// </summary>
        public (string, string) Convert(IEnumerable<string> lines)
        {
            throw new NotImplementedException(); // TODO
        }
    }
}
