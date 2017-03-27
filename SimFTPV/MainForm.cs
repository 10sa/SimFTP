using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SimFTP.Net.Server;
using SimFTP.Net.Client;

namespace SimFTPV
{
	public partial class MainForm : Form
	{
		Server server = new Server();
		
		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			
		}

		private void SettingsReceive_Click(object sender, EventArgs e)
		{
			ReceiveConfigs receiveConfigsForm = new ReceiveConfigs(ref server);
			receiveConfigsForm.ShowDialog();
		}
	}
}
