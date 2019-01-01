using System;
using System.Collections.Generic;
using Java2SharpDoc.Helpers;

namespace Java2SharpDoc.Services
{
	public class DocConverter
	{
		private static class Java
		{
			public const string Param = "@param";
			public const string See = "@see";
			public const string Return = "@return";
			public const string Exception = "@throws";
			public const string Deprecated = "@deprecated";
			public const string DocStart = "/**";
			public const string DocEnd = "*/";
		}
		private static class CSharp
		{
			public const string Param = "param";
			public const string See = "see";
			public const string SeeAlso = "seealso";
			public const string Summary = "summary";
			public const string Return = "returns";
			public const string Exception = "exception";
			public const string Obsolete = "System.Obsolete";
		}

		private string obsoleteMessage;
		private string[] lines;

		/// <summary>
		/// Converts Java doc to C# doc
		/// </summary>
		public (string Doc, string Attrib) Convert(string input)
		{
			if (!input.TrimStart().StartsWith(Java.DocStart) || !input.TrimEnd().EndsWith(Java.DocEnd))
			{
				return ("","");
			}
			
			obsoleteMessage = null;
		    lines = input.Replace(Java.DocStart, "").Replace(Java.DocEnd, "")
		                 .Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
			var segments = FindSegments();

			var output = new List<string>();
			foreach (var (start, end) in segments)
			{
			    string line = ConvertSegment(start, end);
			    if (line != "")
			    {
			        output.Add(line);
			    }
			}
			
		    string doc = string.Join(Environment.NewLine, output);
            string attrib = obsoleteMessage != null ? DocHelper.CreateAttribute(CSharp.Obsolete, obsoleteMessage) : "";
		    if (doc != "" && attrib != "")
		    {
		        attrib = Environment.NewLine + attrib;
		    }
		    return (doc, attrib);
		}
        
        /// <summary>
        /// Return (start,end) indices of segments
        /// </summary>
		private IEnumerable<(int Start, int End)> FindSegments()
		{
			var output = new List<(int, int)>();
			int linesLength = lines.Length;
			int start = 0;
			int i;

			for (i = 0; i < linesLength; i++)
			{
				string line = lines[i].Replace("*", "").Trim();

				if (line.StartsWith(Java.Param) || line.StartsWith(Java.See) ||
				    line.StartsWith(Java.Exception) || line.StartsWith(Java.Return) ||
				    line.StartsWith(Java.Deprecated))
				{
					if (i - start > 0)
					{
						output.Add((start, i));
						start = i;
					}
				}
			}

			if (i - start > 0)
			{
				output.Add((start, i - 1));
			}
			return output;
		}

        /// <summary>
        /// Convert segment
        /// </summary>
		private string ConvertSegment(int startIndex, int endIndex)
		{
			if (startIndex >= lines.Length)
			{
				return "";
			}

			string lineContent = lines[startIndex].Replace("*", "").Trim();
			if (string.IsNullOrWhiteSpace(lineContent))
			{
				return "";
			}

			string output;

			if (lineContent.StartsWith(Java.Param))
			{
				lineContent = lineContent.Remove(0, Java.Param.Length).TrimStart();

				(string paramName, string content) = SplitNextWord(lineContent);
				
				output = content;
				for (int i = startIndex + 1; i < endIndex; i++)
				{
					output += AddLine(i);
				}
				output = ReplaceLinks(output);

				return "/// " + DocHelper.CreateTagWithVar(CSharp.Param, "name", paramName, output);
			}
			if (lineContent.StartsWith(Java.Exception))
			{
				lineContent = lineContent.Remove(0, Java.Exception.Length).TrimStart();

				(string paramName, string content) = SplitNextWord(lineContent);
				
				output = content;
				for (int i = startIndex + 1; i < endIndex; i++)
				{
					output += AddLine(i);
				}
				output = ReplaceLinks(output);

				return "/// " + DocHelper.CreateTagWithVar(CSharp.Exception, "cref", paramName, output);
			}
			if (lineContent.StartsWith(Java.Return))
			{
				lineContent = lineContent.Remove(0, Java.Return.Length).TrimStart();

				(string _, string content) = SplitNextWord(lineContent);
				output = ReplaceLinks(content);

				return "/// " + DocHelper.CreateTag(CSharp.Return, output);
			}
			if (lineContent.StartsWith(Java.See))
			{
				lineContent = lineContent.Remove(0, Java.See.Length).TrimStart();

				return "/// " + DocHelper.CreateTagWithVar(CSharp.SeeAlso, "cref", lineContent);
			}
			if (lineContent.StartsWith(Java.Deprecated))
			{
				lineContent = lineContent.Remove(0, Java.Deprecated.Length).TrimStart();

				output = lineContent;
				for (int i = startIndex + 1; i < endIndex; i++)
				{
					output += JoinLine(i);
				}
				obsoleteMessage = ReplaceLinks(output.Replace(Environment.NewLine, ""));
				
				return "";
			}
			//else
			{
				output = lineContent;
				for (int i = startIndex + 1; i < endIndex; i++)
				{
					output += AddLine(i);
				}
				output = ReplaceLinks(output);

				if (output.Length == 0)
				{
					return DocHelper.CreateTag(CSharp.Summary, output);
				}
				else
				{
					return "/// " + DocHelper.CreateStartTag(CSharp.Summary) +
						   Environment.NewLine + "/// " + output +
						   Environment.NewLine + "/// " + DocHelper.CreateEndTag(CSharp.Summary);
				}
			}
		}

		private string AddLine(int index)
		{
			if (index >= lines.Length)
				return "";

			string line = lines[index].Replace("*", "").Trim();

			if (string.IsNullOrWhiteSpace(line))
				return "";

			return Environment.NewLine + "/// " + line;
		}
		
		private string JoinLine(int index)
		{
			if (index >= lines.Length)
				return "";

			string line = lines[index].Replace("*", "").Trim();

			if (string.IsNullOrWhiteSpace(line))
				return "";

			return " " + line;
		}

        /// <summary>
        /// Replace @link by see
        /// </summary>
		private string ReplaceLinks(string input)
		{
			do
			{
				int startIndex = input.IndexOf("{@link ", StringComparison.InvariantCulture);
				if (startIndex == -1)
				{
					break;
				}

				int endIndex = input.IndexOf("}", startIndex, StringComparison.InvariantCulture);
			    if (endIndex == -1)
			    {
			        break;
			    }

                int crefIndex = startIndex + "{@link ".Length;

				string before = input.Substring(0, startIndex);
			    string middle = input.Substring(crefIndex, endIndex - crefIndex).Replace("#", "");
				string after = input.Substring(endIndex + 1);

				input = before + DocHelper.CreateTagWithVar(CSharp.See, "cref", middle) + after;
			} while (true);

			return input;
		}

        /// <summary>
        /// Split text to starting word and rest
        /// </summary>
		private static (string First, string Next) SplitNextWord(string input)
		{
			int firstSpace = input.IndexOf(' ');

			string firstWord = input.Substring(0, firstSpace);
			string rest = input.Substring(firstSpace + 1);

			return (firstWord, rest);
		}
	}
}
