using System;
using System.Collections.Generic;

namespace Java2SharpDoc.Services
{
	public class Converter
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
			public const string Obsolete = "Obsolete";
		}

		private string obsoleteMessage;
		private string[] lines;

		/// <summary>
		/// Converts Java doc to C# doc
		/// </summary>
		/// <param name="input">Java documentation</param>
		/// <returns>C# documentation</returns>
		public string Convert(string input)
		{
			input = input.Trim();
			if (!input.StartsWith(Java.DocStart) || !input.EndsWith(Java.DocEnd))
			{
				return null;
			}
			
			obsoleteMessage = null;
			input = input.Replace(Java.DocStart, "").Replace(Java.DocEnd, "");
			lines = input.Split(new[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
			var chunks = FindChunks();

			var output = new List<string>();
			foreach (var (start, end) in chunks)
			{
				output.Add(Convert(start, end));
			}
			if (obsoleteMessage != null)
			{
				output.Add(DocHelper.CreateAttribute(CSharp.Obsolete, obsoleteMessage));
			}
			return string.Join(Environment.NewLine, output);
		}

		private IEnumerable<(int Start, int End)> FindChunks()
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

		private string Convert(int start, int end)
		{
			if (start >= lines.Length)
			{
				return "";
			}

			string lineContent = lines[start].Replace("*", "").Trim();
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
				for (int i = start + 1; i < end; i++)
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
				for (int i = start + 1; i < end; i++)
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
				for (int i = start + 1; i < end; i++)
				{
					output += JoinLine(i);
				}
				obsoleteMessage = ReplaceLinks(output.Replace(Environment.NewLine, ""));
				
				return "";
			}
			//else
			{
				output = lineContent;
				for (int i = start + 1; i < end; i++)
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
				int crefIndex = startIndex + "{@link ".Length;

				string before = input.Substring(0, startIndex);
				string middle = input.Substring(crefIndex, endIndex - crefIndex);
				string after = input.Substring(endIndex + 1);

				input = before + DocHelper.CreateTagWithVar(CSharp.See, "cref", middle) + after;
			} while (true);

			return input;
		}

		private static (string First, string Next) SplitNextWord(string input)
		{
			int firstSpace = input.IndexOf(' ');

			string firstWord = input.Substring(0, firstSpace);
			string rest = input.Substring(firstSpace + 1);

			return (firstWord, rest);
		}
	}
}
