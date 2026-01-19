private void result()
{
	int num = 0;
	if (tb.Text.Contains("+"))
	{
		num = tb.Text.IndexOf("+");
	}
	else if (tb.Text.Contains("-"))
	{
		num = tb.Text.IndexOf("-");
	}
	else if (tb.Text.Contains("*"))
	{
		num = tb.Text.IndexOf("*");
	}
	else if (tb.Text.Contains("/"))
	{
		num = tb.Text.IndexOf("/");
	}
	string text = tb.Text.Substring(num, 1);
	double num2 = Convert.ToDouble(tb.Text.Substring(0, num));
	double num3 = Convert.ToDouble(tb.Text.Substring(num + 1, tb.Text.Length - num - 1));
	switch (text)
	{
		case "+":
		{
			TextBox textBox = tb;
			textBox.Text = textBox.Text + "=" + (num2 + num3);
			break;
		}
		case "-":
		{
			TextBox textBox = tb;
			textBox.Text = textBox.Text + "=" + (num2 - num3);
			break;
		}
		case "*":
		{
			TextBox textBox = tb;
			textBox.Text = textBox.Text + "=" + num2 * num3;
			break;
		}
		default:
		{
			TextBox textBox = tb;
			textBox.Text = textBox.Text + "=" + num2 / num3;
			break;
		}
	}
}