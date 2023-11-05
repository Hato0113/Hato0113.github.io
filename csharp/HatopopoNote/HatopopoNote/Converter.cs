using System.Text.RegularExpressions;

internal class Parser
{
	private List<string> m_htmlLines = new List<string>();
	private Stack<ParseStatus> m_statusStack = new Stack<ParseStatus>();

	private enum ParseStatus
	{
		Normal,
		Pre,
		NumberList,
		AsteriskList,
		HyphenList,
		Table,
	}
	public string[] ParseMarkdown(string[] lines)
	{

		m_htmlLines.Add("<!DOCTYPE html>");
		m_htmlLines.Add("<html>");
		m_htmlLines.Add("<head>");
		m_htmlLines.Add("</head>");
		m_htmlLines.Add("<body>");
		m_htmlLines.Add("");

		foreach (var line in lines)
		{
			// 一行解析
			ParseLine(line);
		}

		CloseHtmlTag();

		m_htmlLines.Add("");
		m_htmlLines.Add("</body>");
		m_htmlLines.Add("</html>");

		return m_htmlLines.ToArray();
	}
	private void ParseLine(string targetLine)
	{
		// match link
		Regex regex_link = new Regex(@"(?<start>.*)\[(?<message>.*)\]\((?<url>.*)\)(?<end>.*)");
		Match match_link = regex_link.Match(targetLine);

		// parse
		if (targetLine.StartsWith("```"))
		{
			if (m_statusStack.Count() > 0 && m_statusStack.Peek() == ParseStatus.Pre)
			{
				// pre end
				CloseHtmlTag();
			}
			else
			{
				// classValue
				string classValue = string.Empty;
				Regex r = new Regex(@"```(?<classValue>.*)");
				Match m = r.Match(targetLine);
				if (m.Success == true)
				{
					classValue = m.Groups["classValue"].Value;
				}

				// pre start
				CloseHtmlTag();
				OpenHtmlTag(ParseStatus.Pre, classValue);
			}
		}
		else if (m_statusStack.Count() > 0 && m_statusStack.Peek() == ParseStatus.Pre)
		{
			// エスケープ処理
			string escapeLine = EscapeHtml(targetLine);
			m_htmlLines.Add(escapeLine);
		}
		else if (match_link.Success)
		{
			string result_link = string.Format("{0}<a href=\"{1}\">{2}</a>{3}"
												, match_link.Groups["start"].Value
												, match_link.Groups["url"].Value
												, match_link.Groups["message"].Value
												, match_link.Groups["end"].Value
												);
			m_htmlLines.Add(result_link);
		}
		else if (targetLine == "***" || targetLine == "---")
		{
			CloseHtmlTag();
			m_htmlLines.Add("<hr>");
		}
		else if (targetLine.StartsWith("# "))
		{
			CloseHtmlTag();
			m_htmlLines.Add("<h1>" + targetLine.Replace("# ", "") + "</h1>");
		}
		else if (targetLine.StartsWith("## "))
		{
			CloseHtmlTag();
			m_htmlLines.Add("<h2>" + targetLine.Replace("## ", "") + "</h2>");
		}
		else if (targetLine.StartsWith("### "))
		{
			CloseHtmlTag();
			m_htmlLines.Add("<h3>" + targetLine.Replace("### ", "") + "</h3>");
		}
		else if (targetLine.StartsWith("#### "))
		{
			CloseHtmlTag();
			m_htmlLines.Add("<h4>" + targetLine.Replace("#### ", "") + "</h4>");
		}
		else if (targetLine.StartsWith("##### "))
		{
			CloseHtmlTag();
			m_htmlLines.Add("<h5>" + targetLine.Replace("##### ", "") + "</h5>");
		}
		else if (targetLine.StartsWith("###### "))
		{
			CloseHtmlTag();
			m_htmlLines.Add("<h6>" + targetLine.Replace("###### ", "") + "</h6>");
		}
		else if (targetLine.StartsWith("1. "))
		{
			if (m_statusStack.Count() > 0 && m_statusStack.Peek() == ParseStatus.NumberList)
			{
				// Nothing
			}
			else
			{
				CloseHtmlTag();
				OpenHtmlTag(ParseStatus.NumberList);
			}
			m_htmlLines.Add("<li>" + targetLine.Replace("1. ", "") + "</li>");
		}
		else if (targetLine.StartsWith("* "))
		{
			if (m_statusStack.Count() > 0 && m_statusStack.Peek() == ParseStatus.AsteriskList)
			{
				// Nothing
			}
			else
			{
				CloseHtmlTag();
				OpenHtmlTag(ParseStatus.AsteriskList);
			}
			m_htmlLines.Add("<li>" + targetLine.Replace("* ", "") + "</li>");
		}
		else if (targetLine.StartsWith("- "))
		{
			if (m_statusStack.Count() > 0 && m_statusStack.Peek() == ParseStatus.HyphenList)
			{
				// Nothing
			}
			else
			{
				CloseHtmlTag();
				OpenHtmlTag(ParseStatus.HyphenList);
			}
			m_htmlLines.Add("<li>" + targetLine.Replace("- ", "") + "</li>");
		}
		else if (targetLine.StartsWith("|"))
		{
			if (m_statusStack.Count() > 0 && m_statusStack.Peek() == ParseStatus.Table)
			{
				// Nothing
			}
			else
			{
				CloseHtmlTag();
				OpenHtmlTag(ParseStatus.Table);
			}

			// table row start
			m_htmlLines.Add("<tr>");

			// table data
			var columns = targetLine.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);
			foreach (var column in columns)
			{
				m_htmlLines.Add("<td>" + column + "</td>");
			}

			// table row end
			m_htmlLines.Add("</tr>");
		}
		else if (targetLine == "")
		{
			CloseHtmlTag();
			m_htmlLines.Add(targetLine);
		}
		else
		{
			CloseHtmlTag();
			m_htmlLines.Add("<p>" + targetLine + "</p>");
		}
	}
	private void OpenHtmlTag(ParseStatus status, string classValue = null)
	{
		// 追加
		m_statusStack.Push(status);

		// TAG開始
		switch (m_statusStack.Peek())
		{
			case ParseStatus.Normal:
				break;
			case ParseStatus.Pre:
				if (string.IsNullOrEmpty(classValue))
				{
					m_htmlLines.Add("<pre>");
				}
				else
				{
					m_htmlLines.Add(string.Format("<pre class=\"{0}\">", classValue));
				}
				break;
			case ParseStatus.NumberList:
				m_htmlLines.Add("<ol>");
				break;
			case ParseStatus.AsteriskList:
				m_htmlLines.Add("<ul>");
				break;
			case ParseStatus.HyphenList:
				m_htmlLines.Add("<ul>");
				break;
			case ParseStatus.Table:
				m_htmlLines.Add("<table>");
				break;
			default:
				break;
		}
	}
	private void CloseHtmlTag()
	{
		// 空チェック
		if (m_statusStack.Count() == 0)
		{
			return;
		}

		// TAG終了
		switch (m_statusStack.Peek())
		{
			case ParseStatus.Normal:
				break;
			case ParseStatus.Pre:
				m_htmlLines.Add("</pre>");
				break;
			case ParseStatus.NumberList:
				m_htmlLines.Add("</ol>");
				break;
			case ParseStatus.AsteriskList:
				m_htmlLines.Add("</ul>");
				break;
			case ParseStatus.HyphenList:
				m_htmlLines.Add("</ul>");
				break;
			case ParseStatus.Table:
				m_htmlLines.Add("</table>");
				break;
			default:
				break;
		}

		// 削除
		m_statusStack.Pop();
	}
	private string EscapeHtml(string target)
	{
		string result = target;

		// 変換
		result = result.Replace("&", "&");
		result = result.Replace('\"', '"');
		result = result.Replace("\'", "'");
		result = result.Replace("¥", "¥");
		result = result.Replace("<", "<");
		result = result.Replace(">", ">");
		//result = result.Replace(" ", " ");
		return result;
	}
}
