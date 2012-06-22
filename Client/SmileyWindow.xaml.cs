/*
 * Created by SharpDevelop.
 * User: t.ertl
 * Date: 22.06.2012
 * Time: 16:22
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Client
{
	/// <summary>
	/// Interaction logic for ShowAllSmileys.xaml
	/// </summary>
	public partial class SmileyWindow : Window
	{		
		public SmileyWindow(Smiley smileyclass)
		{
			InitializeComponent();
		}
	}
}