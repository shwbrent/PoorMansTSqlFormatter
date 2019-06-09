/*
Poor Man's T-SQL Formatter - a small free Transact-SQL formatting 
library for .Net 2.0 and JS, written in C#. 
Copyright (C) 2011-2017 Tao Klerks

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <http://www.gnu.org/licenses/>.

*/

using System;
using PoorMansTSqlFormatterLib.Interfaces;
using System.Text.RegularExpressions;
using PoorMansTSqlFormatterLib.ParseStructure;

namespace PoorMansTSqlFormatterLib.Formatters
{
    public class HtmlPageWrapper : ISqlTreeFormatter
    {
        ISqlTreeFormatter _underlyingFormatter;

        public HtmlPageWrapper(ISqlTreeFormatter underlyingFormatter)
        {
            if (underlyingFormatter == null)
                throw new ArgumentNullException("underlyingFormatter");

            _underlyingFormatter = underlyingFormatter;
        }

        private const string HTML_OUTER_PAGE = @"<!DOCTYPE html >
<html>
<head>
</head>
<body>
<style type=""text/css"">
.SQLCode {{
	font-size: 13px;
	font-weight: bold;
	font-family: monospace;;
	white-space: pre;
    -o-tab-size: 4;
    -moz-tab-size: 4;
    -webkit-tab-size: 4;
}}
.SQLComment {{
	color: #00AA00;
}}
.SQLString {{
	color: #AA0000;
}}
.SQLFunction {{
	color: #AA00AA;
}}
.SQLKeyword {{
	color: #0000AA;
}}
.SQLOperator {{
	color: #777777;
}}
.SQLErrorHighlight {{
	background-color: #FFFF00;
}}


</style>
<pre class=""SQLCode"">{0}</pre>
</body>
</html>
";

        public bool HTMLFormatted { get { return true; } }
        public string ErrorOutputPrefix { 
            get 
            { 
                return _underlyingFormatter.ErrorOutputPrefix; 
            } 
            set 
            {
                throw new NotSupportedException("Error output prefix should be set on the underlying formatter - it cannot be set on the Html Page Wrapper.");
            }
        }

        public string FormatSQLTree(PoorMansTSqlFormatterLib.ParseStructure.Node sqlTree)
        {
            string formattedResult = _underlyingFormatter.FormatSQLTree(sqlTree);
          
            if (_underlyingFormatter.HTMLFormatted)
                return string.Format(HTML_OUTER_PAGE, formattedResult);
            else
                return string.Format(HTML_OUTER_PAGE, Utils.HtmlEncode(formattedResult));
        }

        public string FormatSQLTreeByPrefix(Node sqlTree, string prefix)
        {
            string formattedResult = _underlyingFormatter.FormatSQLTree(sqlTree);
            String[] test = formattedResult.Split(new String[] { "</span>\r\n" }, StringSplitOptions.RemoveEmptyEntries);
            String Result = "";
            for (int i = 0; i < test.Length; i++)
            {
                if (Regex.IsMatch(test[i].Trim(), "^\\<span class=\"SQLComment\"\\>.*", RegexOptions.IgnoreCase))
                {
                    Result += Regex.Replace(test[i], "\\t", "") + "</span>\r\n";
                }
                else if (Regex.IsMatch(test[i].Trim(), "\\<span class=\"SQLComment\"\\>.*", RegexOptions.IgnoreCase))
                {
                    String[] tes = test[i].Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (tes.Length > 0)
                    {
                        for (int j = 0; j < tes.Length; j++)
                        {
                            if ((tes.Length - 1) == j)
                            {
                                tes[j] += "</span>";
                            }
                            if (Regex.IsMatch(tes[j].Trim(), "^\\<span class=\"SQLComment\"\\>.*\\</span\\>$", RegexOptions.IgnoreCase))
                            {
                                string pattern = "(?<comment>\\<span class=\"SQLComment\"\\>.*|//.*)";
                                Result += Regex.Replace(tes[j], pattern, "${comment}\r\n", RegexOptions.IgnoreCase);
                            }else if (Regex.IsMatch(tes[j].Trim(), "\\<span class=\"SQLComment\"\\>.*\\</span\\>$", RegexOptions.IgnoreCase))
                            {
                                string pattern = "(?<comment>\\<span class=\"SQLComment\"\\>.*|//.*)";
                                Result += (prefix + ".append(\"\t" + Regex.Replace(tes[j], pattern, "\");\r\n${comment}\r\n", RegexOptions.IgnoreCase));
                            }
                            else
                            {
                                Result += (prefix + ".append(\"\t" + tes[j] + "\");\r\n");
                            }
                        }
                    }
                    else
                    {
                        Result += (prefix + ".append(\"\t" + tes[i] + "\");\r\n");
                    }

                }
                else
                {
                    String[] tes = test[i].Split(new String[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                    if (tes.Length > 1)
                    {
                        for (int j = 0; j < tes.Length; j++)
                        {
                            if ((tes.Length - 1) == j)
                            {
                                tes[j] += "</span>";
                            }
                            Result += (prefix + ".append(\"\t" + tes[j] + "\");\r\n");
                        }
                    }
                    else if(tes.Length > 0)
                    {
                        Result += (prefix + ".append(\"\t" + tes[0] + "</span>\");\r\n");
                    }
                }
                Result = Result.Replace("--", "//");
            }
            if (_underlyingFormatter.HTMLFormatted)
                return string.Format(HTML_OUTER_PAGE, Result);
            else
                return string.Format(HTML_OUTER_PAGE, Utils.HtmlEncode(formattedResult));
        }
    }
}
