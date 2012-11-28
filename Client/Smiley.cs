/*
 * Created by SharpDevelop.
 * User: t.ertl
 * Date: 21.06.2012
 * Time: 16:27
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Windows.Media.Effects;

namespace Client
{
	/// <summary>
	/// Description of Smiley.
	/// </summary>
	public class Smiley
	{
		
		private Hashtable _fixSmileys = new Hashtable();
		private Regex _fixSmileyRegex;
		
		public Smiley()
		{
			buildFixSmileys();
			buildFixSmileysRegex();
		}
		
		public void buildFixSmileys()
		{
			_fixSmileys.Add(":)", "smile.gif");
			_fixSmileys.Add(":-)", "smile.gif");
			_fixSmileys.Add(":D", "laugh.gif");
			_fixSmileys.Add(":-D", "laugh.gif");
			_fixSmileys.Add(":(", "sad.gif");
			_fixSmileys.Add(":-(", "sad.gif");
			_fixSmileys.Add(";)", "wink.gif");
			_fixSmileys.Add(";-)", "wink.gif");
			_fixSmileys.Add(":asia:", "asia.gif");
		}
		
		public void buildFixSmileysRegex()
		{
			string tmp = "";
			foreach (string key in _fixSmileys.Keys) {
				tmp = tmp + key + "|";
			}
			tmp = tmp.Substring(0, tmp.Length-1).Replace("(", "\\(").Replace(")", "\\)");
			_fixSmileyRegex = new Regex(@tmp);
		}
		
		public String getRegex()
		{
			return _fixSmileyRegex.ToString();
		}
		
		public Paragraph insertSmileys(Paragraph p, string text)
		{
			int lastPos = 0;
    		foreach (Match match in _fixSmileyRegex.Matches(text)) {
            	if (match.Index != lastPos) {
					p.Inlines.Add(text.Substring(lastPos, match.Index - lastPos));
	                // Bild hinzufugen falls vorhanden
            		try {
	                	BitmapImage bitmapSmiley = new BitmapImage(new Uri("pack://application:,,,/Smileys/" + (string)_fixSmileys[match.Value]) );
                		System.Windows.Controls.Image smiley = new System.Windows.Controls.Image();
                		smiley.Source = bitmapSmiley;
                        smiley.Width = bitmapSmiley.Width;
                        smiley.Height = bitmapSmiley.Height;
                        smiley.SnapsToDevicePixels = true;
                		p.Inlines.Add(smiley);
            		}
            		catch (FileNotFoundException)
            		{
                		// Loggen file not found
                		
                		// Text einfuegen statt Smiley
                		p.Inlines.Add(match.Value);
            		}
					lastPos = match.Index + match.Length;
				}
			}
			if (lastPos < text.Length) {
        		p.Inlines.Add(text.Substring(lastPos));
    		}
    		return p;
		}
		
		public Hashtable getAllSmileys()
		{
			return _fixSmileys;
		}
	}
}
