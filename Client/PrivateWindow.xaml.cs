/*
 * Created by SharpDevelop.
 * User: t.ertl
 * Date: 14.02.2014
 * Time: 14:58
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Client
{
	/// <summary>
	/// Interaction logic for PrivateWindow.xaml
	/// </summary>
	public partial class PrivateWindow : Window
	{
		protected Window1 mainWindow;
		
		protected string chatPartner;
		
		public PrivateWindow(string chatPartner)
		{
			InitializeComponent();
			txtLog.Document.Blocks.Clear();
			this.chatPartner = chatPartner;
			this.Title = "Privater Chat mit " + chatPartner;
		}
		
		public void setOwner(Window1 owner)
		{
			mainWindow = owner;
		}
		
    	public void closeWindow(object sender, EventArgs e)
    	{
    		this.mainWindow.closePrivateChat(this.chatPartner);
    	}
    	
		public void BtnSend_Click(object sender, RoutedEventArgs e)
		{
			mainWindow.SendMessage(txtMessage.Text, chatPartner);
		}
		
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
		    base.OnClosing(e);
		    e.Cancel = true;
		    this.Hide();
		}
	}
}