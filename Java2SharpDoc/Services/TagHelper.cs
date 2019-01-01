
namespace Java2SharpDoc.Services
{
	public class DocHelper
	{		
		public static string CreateStartTag(string tag) => $"<{tag}>";

		public static string CreateEndTag(string tag) => $"</{tag}>";

		public static string CreateTag(string tag, string content)
		{
			return $"<{tag}>{content}</{tag}>";
		}
		
		public static string CreateTagWithVar(string tag, string variableName, string variableValue)
		{
			return $"<{tag} {variableName}=\"{variableValue}\"/>";
		}

		public static string CreateTagWithVar(string tag, string variableName, string variableValue, string content)
		{
			return $"<{tag} {variableName}=\"{variableValue}\">{content}</{tag}>";
		}
		
		public static string CreateAttribute(string name)
		{
			return $"[{name}]";
		}

		public static string CreateAttribute(string name, string value)
		{
			if (!value.StartsWith("\"") || !value.EndsWith("\""))
			{
				value = $"\"{value}\"";
			}
			return $"[{name}({value})]";
		}
	}
}
