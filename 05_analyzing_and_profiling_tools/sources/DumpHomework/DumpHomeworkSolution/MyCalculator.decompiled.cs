using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Configuration;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

[assembly: CompilationRelaxations(8)]
[assembly: RuntimeCompatibility(WrapNonExceptionThrows = true)]
[assembly: Debuggable(DebuggableAttribute.DebuggingModes.Default | DebuggableAttribute.DebuggingModes.DisableOptimizations | DebuggableAttribute.DebuggingModes.IgnoreSymbolStoreSequencePoints | DebuggableAttribute.DebuggingModes.EnableEditAndContinue)]
[assembly: AssemblyTitle("MyCalculatorv1")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("MyCalculatorv1")]
[assembly: AssemblyCopyright("Copyright ©  2012")]
[assembly: AssemblyTrademark("")]
[assembly: ComVisible(false)]
[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
[assembly: AssemblyFileVersion("1.0.0.0")]
[assembly: TargetFramework(".NETFramework,Version=v4.5", FrameworkDisplayName = ".NET Framework 4.5")]
[assembly: AssemblyVersion("1.0.0.0")]
namespace MyCalculatorv1
{
	public class App : Application
	{
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		public void InitializeComponent()
		{
			base.StartupUri = new Uri("MainWindow.xaml", UriKind.Relative);
		}

		[STAThread]
		[DebuggerNonUserCode]
		[GeneratedCode("PresentationBuildTasks", "4.0.0.0")]
		public static void Main()
		{
			App app = new App();
			app.InitializeComponent();
			app.Run();
		}
	}
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
}
namespace MyCalculatorv1.Properties
{
	[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
	[DebuggerNonUserCode]
	[CompilerGenerated]
	internal class Resources
	{
		private static ResourceManager resourceMan;

		private static CultureInfo resourceCulture;

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static ResourceManager ResourceManager
		{
			get
			{
				if (resourceMan == null)
				{
					ResourceManager resourceManager = new ResourceManager("MyCalculatorv1.Properties.Resources", typeof(Resources).Assembly);
					resourceMan = resourceManager;
				}
				return resourceMan;
			}
		}

		[EditorBrowsable(EditorBrowsableState.Advanced)]
		internal static CultureInfo Culture
		{
			get
			{
				return resourceCulture;
			}
			set
			{
				resourceCulture = value;
			}
		}

		internal Resources()
		{
		}
	}
	[CompilerGenerated]
	[GeneratedCode("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "11.0.0.0")]
	internal sealed class Settings : ApplicationSettingsBase
	{
		private static Settings defaultInstance = (Settings)SettingsBase.Synchronized(new Settings());

		public static Settings Default => defaultInstance;
	}
}
