/*
 * Created by SharpDevelop.
 * User: t.ertl
 * Date: 16.01.2013
 * Time: 16:06
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;

namespace Client
{
	/// <summary>
	/// Description of Parser.
	/// </summary>
	public class Parser
	{
		private string _defaultBrowser;
		
		private Smiley _smiley = new Smiley();
		
		private static readonly Regex _regexUrl = new Regex(@"(?#Protocol)(?:(?:ht|f)tp(?:s?)\:\/\/|~/|/)?(?#Username:Password)(?:\w+:\w+@)?(?#Subdomains)(?:(?:[-\w]+\.)+(?#TopLevel Domains)(?:com|org|net|gov|mil|biz|info|mobi|name|aero|jobs|museum|travel|[a-z]{2}))(?#Port)(?::[\d]{1,5})?(?#Directories)(?:(?:(?:/(?:[-\w~!$+|.,=]|%[a-f\d]{2})+)+|/)+|\?|#)?(?#Query)(?:(?:\?(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)(?:&(?:[-\w~!$+|.,*:]|%[a-f\d{2}])+=(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)*)*(?#Anchor)(?:#(?:[-\w~!$+|.,*:=]|%[a-f\d]{2})*)?");
		
		public Parser()
		{
			_defaultBrowser = _findDefaultBrowser();
		}
		
		public Paragraph parse(string message)
		{
			Paragraph p = new Paragraph();
			p.LineHeight = 1;
			DateTime today = DateTime.Now;
			p.Inlines.Add("(" + today.ToString("HH:mm:ss") + ") ");
			int lastPos = 0;
			foreach (Match match in _regexUrl.Matches(message.Replace(Environment.NewLine,"")))
			{
				if (match.Index != lastPos) {
					// parse smileys before hyperlink
					p = _smiley.insertSmileys(p, message.Substring(lastPos, match.Index - lastPos));
					lastPos = match.Index + match.Length;
					// parse hyperlink
					Hyperlink textLink = new Hyperlink(new Run(match.Value));
					textLink.NavigateUri = new Uri(match.Value);
					textLink.RequestNavigate += new System.Windows.Navigation.RequestNavigateEventHandler(_openHyperlink);
					p.Inlines.Add(textLink);
					lastPos = match.Index + match.Length;
				}
			}
			// wenn text noch nicht zu ende, noch mal nach smileys schauen
    		p = _smiley.insertSmileys(p, message.Substring(lastPos));
			return p;
		}
		
		private string _findDefaultBrowser()
		{
			string browser = string.Empty;
			Microsoft.Win32.RegistryKey key = null;
		  	try {
    			key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command");
    			// trim off quotes
    			if (key != null)
    			{
    				browser = key.GetValue(null).ToString().ToLower();
    			}
				// get rid of everything after the ".exe"
				if (!browser.EndsWith("exe")) {
                	browser = browser.Substring(0, browser.LastIndexOf(".exe")+4);
                	browser = browser.Substring(1);
    			}
  			}
  			finally
  			{
    			if (key != null)
    			{
    				key.Close();
    			}
  			}
  			return browser;
		}
		
		private void _openHyperlink(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
		{
			System.Diagnostics.Process.Start(_defaultBrowser, e.Uri.AbsoluteUri);
            e.Handled = true;
		}
	}
}
