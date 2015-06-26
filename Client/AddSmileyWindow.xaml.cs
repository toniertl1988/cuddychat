/*
 * Created by SharpDevelop.
 * User: t.ertl
 * Date: 08.06.2015
 * Time: 10:41
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */
using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows;
using System.Collections;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WpfAnimatedGif;

namespace Client
{
	/// <summary>
	/// Interaction logic for AddSmileyWindow.xaml
	/// </summary>
	public partial class AddSmileyWindow : Window
	{
		protected BaseWindow mainWindow;
		
		protected int row = 0;
		
		protected int column = 0;
		
		public AddSmileyWindow(BaseWindow mainWindow)
		{
			this.mainWindow = mainWindow;
			InitializeComponent();
		}
		
		public new void Show()
		{
			this.row = 0;
			this.column = 0;
			changeBackgroundOnEnterEvent((Border) windowGrid.FindResource(new {Row = this.row, Column = this.column}));
			base.Show();
		}
		
		public void addSmileysToGrid(Hashtable smileys)
		{
			int columnCount = 0;
			int rowCount = 0;
			
    		// Add Rows
    		int tmpRowCount = (int) Math.Ceiling((decimal) (smileys.Count/3));
    		for (int tmp = 0; tmp < tmpRowCount; tmp++) {
    			RowDefinition gridRow = new RowDefinition();
				gridRow.Height = new GridLength(35);
				windowGrid.RowDefinitions.Add(gridRow);
    		}

    		List<string> alreadyShownSmileys = new List<string>();
    		
    		foreach (var element in smileys.Keys) {
    			string uri = (string)smileys[element];
    			// dont show duplettes
    			if (alreadyShownSmileys.Contains(uri)) {
    				continue;
    			}
    			alreadyShownSmileys.Add(uri);
                BitmapImage bitmapSmiley = new BitmapImage();
            	bitmapSmiley.BeginInit();
            	bitmapSmiley.UriSource = new Uri("pack://application:,,,/Smileys/" + uri);
            	bitmapSmiley.EndInit();
            	
                Image image = new Image();
            	image.Source = bitmapSmiley;
                image.Width = bitmapSmiley.Width;
                image.Height = bitmapSmiley.Height;
                image.SnapsToDevicePixels = true;
                image.VerticalAlignment = VerticalAlignment.Center;
                image.HorizontalAlignment = HorizontalAlignment.Center;
                ImageBehavior.SetAnimatedSource(image, bitmapSmiley);            
            	image.ToolTip = element;
            	
            	Border border = new Border();
            	border.BorderBrush = System.Windows.Media.Brushes.WhiteSmoke;
            	border.CornerRadius = new CornerRadius(5.0, 5.0, 5.0, 5.0);
            	border.BorderThickness = new System.Windows.Thickness(1.0, 1.0, 1.0, 1.0);
            	border.Margin = new System.Windows.Thickness(2.0);
            	border.MouseDown += new MouseButtonEventHandler(addSmileyClickEvent);
            	border.MouseEnter += new MouseEventHandler(smileyImageMouseEnter);
            	border.MouseLeave += new MouseEventHandler(smileyImageMouseLeave);
            	
            	StackPanel panel = new StackPanel();
            	panel.Background = System.Windows.Media.Brushes.Transparent;
            	panel.HorizontalAlignment = HorizontalAlignment.Center;
            	panel.VerticalAlignment = VerticalAlignment.Center;
            	            
            	panel.Children.Add(image);
            	
            	border.Child = panel;
            	border.Resources.Add("smiley", image);
                
                Grid.SetRow(border, rowCount);
    			Grid.SetColumn(border, columnCount);
    			
    			windowGrid.Children.Add(border);
    			windowGrid.Resources.Add(new { Row = rowCount, Column = columnCount }, border);
    			
				columnCount++;
				
				if (columnCount == 3) {
					columnCount = 0;
					rowCount++;
				}
			}
    		changeBackgroundOnEnterEvent((Border) windowGrid.FindResource(new {Row = 0, Column = 0}));
		}
		
		public void smileyImageMouseEnter(object sender, RoutedEventArgs e)
		{
			Border border = (Border) sender;
			this.row = Grid.GetRow(border);
			this.column = Grid.GetColumn(border);
			changeBackgroundOnEnterEvent(border);
		}
		
		public void smileyImageMouseLeave(object sender, RoutedEventArgs e)
		{
			Border border = (Border) sender;
			changeBackgrounOnLeaveEvent(border);
		}
		
		public void addSmileyClickEvent(object sender, RoutedEventArgs e)
		{
			var actualPosition = this.mainWindow.getMessageElement().SelectionStart;
			Border border = (Border) sender;
			System.Windows.Controls.Image image = (System.Windows.Controls.Image) border.FindResource("smiley");
			string text = image.ToolTip.ToString();
			this.mainWindow.getMessageElement().Text = this.mainWindow.getMessageElement().Text.Insert(actualPosition, text);
			this.mainWindow.getMessageElement().SelectionStart = actualPosition + text.Length;
			Thread.Sleep(150);
			this.Hide();
			this.mainWindow.getMessageElement().Focus();
		}
		
		protected void changeBackgroundOnEnterEvent(Border border)
		{
			resetBorderBackgrounds();
			border.Background = System.Windows.Media.Brushes.LightGray;
			border.BorderBrush = System.Windows.Media.Brushes.Silver;
		}
		
		protected void changeBackgrounOnLeaveEvent(Border border)
		{
			border.Background = System.Windows.Media.Brushes.WhiteSmoke;
			border.BorderBrush = System.Windows.Media.Brushes.WhiteSmoke;
		}
		
		protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
		{
			resetBorderBackgrounds();
		    base.OnClosing(e);
		    e.Cancel = true;
		    this.Hide();
		    this.mainWindow.getMessageElement().Focus();
		}
		
		protected void resetBorderBackgrounds()
		{
			foreach (var element in windowGrid.Children) {
				changeBackgrounOnLeaveEvent((Border) element);
			}
		}
		
		public void Window_KeyDown(object sender, KeyEventArgs e)
		{
			int tmpRow = this.row;
			int tmpColumn = this.column;
			
			Border oldBorder;
			Border newBorder;
			
			if (e.Key == Key.Up || e.Key == Key.Down || e.Key == Key.Left || e.Key == Key.Right) {
				oldBorder = (Border) windowGrid.TryFindResource(new {Row = tmpRow, Column = tmpColumn});
				switch (e.Key) {
					case Key.Up:
						tmpRow--;
						break;
					case Key.Down:
						tmpRow++;
						break;
					case Key.Left:
						tmpColumn--;
						break;
					case Key.Right:
						tmpColumn++;
						break;
				}
				newBorder = (Border) windowGrid.TryFindResource(new {Row = tmpRow, Column = tmpColumn});
				if (newBorder != null) {
					changeBackgrounOnLeaveEvent(oldBorder);
					changeBackgroundOnEnterEvent(newBorder);
					this.row = tmpRow;
					this.column = tmpColumn;
				}
			}
			if (e.Key == Key.Enter) {
				Border border = (Border) windowGrid.TryFindResource(new {Row = tmpRow, Column = tmpColumn});
				if (border != null) {
					addSmileyClickEvent(border, new RoutedEventArgs());
				}
			}
		}
	}
}