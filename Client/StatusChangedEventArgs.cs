/*
 * Created by SharpDevelop.
 * User: Toni
 * Date: 23.06.2012
 * Time: 13:56
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;

namespace Client
{
	/// <summary>
	/// Description of Class1.
	/// </summary>
	public delegate void StatusChangedEventHandler(object sender, StatusChangedEventArgs e);
	
	public class StatusChangedEventArgs : EventArgs
	{
		private string EventMsg;
		
		public string EventMessage
		{
        get { return EventMsg; }
        set { EventMsg = value; }
		}
		
		public StatusChangedEventArgs(string strEventMsg)
		{
			EventMsg = strEventMsg;
		}
	}
}