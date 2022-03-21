using AMS.Profile;
using System.Windows.Forms;

namespace DemoApp
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();

			config = new Ini(Application.StartupPath + "\\config.ini");

			config.ChangeSectionName("Example-Settings", "Example Settings");
			config.ChangeEntryName("Example Settings", "Example-1", "Example 10");
			config.ChangeEntryName("Example Settings", "Example 2", "Example-20");

			config.ChangeSectionName("Example-Settings-2", "Example Settings 2");
			config.ChangeEntryName("Example Settings 2", "Example-1_2", "Example 10_2");
			config.ChangeEntryName("Example Settings 2", "Example 2_2", "Example-20_2");
		}

		private Ini config = null;
	}
}
