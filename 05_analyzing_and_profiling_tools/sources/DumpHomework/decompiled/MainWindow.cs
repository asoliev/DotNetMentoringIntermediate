using System;
using System.Windows;
using System.Windows.Controls;

namespace MyCalculatorv1
{
	public partial class MainWindow : Window
	{
		internal TextBox tb;

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

			// Try to parse the numbers
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
	}
}

