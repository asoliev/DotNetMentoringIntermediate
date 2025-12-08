using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace MyCalculatorv1;

public class MainWindow : Window, IComponentConnector
{
	internal TextBox tb;

	private bool _contentLoaded;

	public MainWindow()
	{
		InitializeComponent();
	}

	private void Button_Click_1(object sender, RoutedEventArgs e)
	{
		Button button = (Button)sender;
		tb.Text += button.Content.ToString();
	}

	private void Result_click(object sender, RoutedEventArgs e)
	{
		result();
	}

	private void result()
	{
		int num = -1;
		string operatorSymbol = "";
		
		// Find the operator
		if (tb.Text.Contains("+"))
		{
			num = tb.Text.IndexOf("+");
			operatorSymbol = "+";
		}
		else if (tb.Text.Contains("-"))
		{
			num = tb.Text.IndexOf("-");
			operatorSymbol = "-";
		}
		else if (tb.Text.Contains("*"))
		{
			num = tb.Text.IndexOf("*");
			operatorSymbol = "*";
		}
		else if (tb.Text.Contains("/"))
		{
			num = tb.Text.IndexOf("/");
			operatorSymbol = "/";
		}

		// Validate that an operator was found
		if (num < 0)
		{
			tb.Text = "Error: No operator found";
			return;
		}

		// Extract and validate the first number
		string firstNumberStr = tb.Text.Substring(0, num).Trim();
		if (string.IsNullOrEmpty(firstNumberStr))
		{
			tb.Text = "Error: Invalid input";
			return;
		}

		// Extract and validate the second number
		string secondNumberStr = tb.Text.Substring(num + 1).Trim();
		if (string.IsNullOrEmpty(secondNumberStr))
		{
			tb.Text = "Error: Invalid input";
			return;
		}

		// Try to parse the numbers safely
		if (!double.TryParse(firstNumberStr, out double num2))
		{
			tb.Text = "Error: Invalid number format";
			return;
		}

		if (!double.TryParse(secondNumberStr, out double num3))
		{
			tb.Text = "Error: Invalid number format";
			return;
		}

		// Perform the calculation
		double result = 0;
		switch (operatorSymbol)
		{
		case "+":
			result = num2 + num3;
			break;
		case "-":
			result = num2 - num3;
			break;
		case "*":
			result = num2 * num3;
			break;
		case "/":
			if (num3 == 0)
			{
				tb.Text = "Error: Division by zero";
				return;
			}
			result = num2 / num3;
			break;
		}

		tb.Text = tb.Text + "=" + result;
	}

	private void Off_Click_1(object sender, RoutedEventArgs e)
	{
		Application.Current.Shutdown();
	}

	private void Del_Click(object sender, RoutedEventArgs e)
	{
		tb.Text = "";
	}

	private void R_Click(object sender, RoutedEventArgs e)
	{
		if (tb.Text.Length > 0)
		{
			tb.Text = tb.Text.Substring(0, tb.Text.Length - 1);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	public void InitializeComponent()
	{
		if (!_contentLoaded)
		{
			_contentLoaded = true;
			Uri resourceLocator = new Uri("/MyCalculatorv1;component/mainwindow.xaml", UriKind.Relative);
			Application.LoadComponent(this, resourceLocator);
		}
	}

	[DebuggerNonUserCode]
	[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	void IComponentConnector.Connect(int connectionId, object target)
	{
		switch (connectionId)
		{
		case 1:
			((Button)target).Click += Button_Click_1;
			break;
		case 2:
			tb = (TextBox)target;
			break;
		case 3:
			((Button)target).Click += Button_Click_1;
			break;
		case 4:
			((Button)target).Click += Button_Click_1;
			break;
		case 5:
			((Button)target).Click += Button_Click_1;
			break;
		case 6:
			((Button)target).Click += Button_Click_1;
			break;
		case 7:
			((Button)target).Click += Button_Click_1;
			break;
		case 8:
			((Button)target).Click += Button_Click_1;
			break;
		case 9:
			((Button)target).Click += Button_Click_1;
			break;
		case 10:
			((Button)target).Click += Button_Click_1;
			break;
		case 11:
			((Button)target).Click += Button_Click_1;
			break;
		case 12:
			((Button)target).Click += Button_Click_1;
			break;
		case 13:
			((Button)target).Click += Button_Click_1;
			break;
		case 14:
			((Button)target).Click += Button_Click_1;
			break;
		case 15:
			((Button)target).Click += Result_click;
			break;
		case 16:
			((Button)target).Click += Button_Click_1;
			break;
		case 17:
			((Button)target).Click += Off_Click_1;
			break;
		case 18:
			((Button)target).Click += Del_Click;
			break;
		case 19:
			((Button)target).Click += R_Click;
			break;
		default:
			_contentLoaded = true;
			break;
		}
	}
}
