using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Serialization;
using Microsoft.Extensions.Logging;

namespace Roadkill.Text.CustomTokens
{
	/// <summary>
	/// Deserializes and caches the custom tokens XML file, which contains a set of text replacements for the markup.
	/// </summary>
	public class CustomTokenParser
	{
		private static IEnumerable<TextToken> _tokens;

		private static bool _isTokensFileCached;

		private readonly ILogger _logger;

		static CustomTokenParser()
		{
			CacheTokensFile = true;
		}

		public CustomTokenParser(TextSettings settings, ILogger logger)
		{
			_logger = logger;

			if (CacheTokensFile && !_isTokensFileCached)
			{
				_tokens = Deserialize(settings);
				ParseTokenRegexes();
				_isTokensFileCached = true;
			}
			else
			{
				_tokens = Deserialize(settings);
				ParseTokenRegexes();
			}
		}

		public static bool CacheTokensFile { get; set; }

		public static IEnumerable<TextToken> Tokens => _tokens;

		public string ReplaceTokensAfterParse(string html)
		{
			foreach (TextToken token in _tokens)
			{
				try
				{
					html = token.CachedRegex.Replace(html, token.HtmlReplacement);
				}
				catch (Exception e)
				{
					// Make sure no plugins bring down the parsing
					_logger.LogWarning(e, "There was an error in replacing the html for the token {0} - {1}", token.Name, e);
				}
			}

			return html;
		}

		private void ParseTokenRegexes()
		{
			foreach (TextToken token in _tokens)
			{
				// Catch bad regexes
				try
				{
					var regex = new Regex(token.SearchRegex, RegexOptions.Compiled | RegexOptions.Singleline);
					token.CachedRegex = regex;
				}
				catch (ArgumentException e)
				{
					_logger.LogWarning(e, "There was an error in search regex for the token {0}", token.Name);
				}
			}
		}

		private IEnumerable<TextToken> Deserialize(TextSettings settings)
		{
			if (string.IsNullOrEmpty(settings.CustomTokensPath) || !File.Exists(settings.CustomTokensPath))
			{
				return new List<TextToken>();
			}

			try
			{
				using (FileStream stream = new FileStream(settings.CustomTokensPath, FileMode.Open, FileAccess.Read))
				{
					XmlSerializer serializer = new XmlSerializer(typeof(List<TextToken>));
					IEnumerable<TextToken> textTokens = (List<TextToken>)serializer.Deserialize(stream);

					if (textTokens == null)
					{
						return new List<TextToken>();
					}
					else
					{
						return textTokens;
					}
				}
			}
			catch (IOException e)
			{
				_logger.LogWarning(e, "An IO error occurred loading the custom tokens file {0}", settings.CustomTokensPath);
				return new List<TextToken>();
			}
			catch (FormatException e)
			{
				_logger.LogWarning(e, "A FormatException error occurred loading the custom tokens file {0}", settings.CustomTokensPath);
				return new List<TextToken>();
			}
			catch (InvalidOperationException e)
			{
				_logger.LogWarning(e, "An InvalidOperationException (bad XML file) error occurred loading the custom tokens file {0}", settings.CustomTokensPath);
				return new List<TextToken>();
			}
		}
	}
}
