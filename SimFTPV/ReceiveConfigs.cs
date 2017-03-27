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

namespace SimFTPV
{
	public partial class ReceiveConfigs : Form
	{
		Server server;
		public ReceiveConfigs(ref Server cfg)
		{
			InitializeComponent();
			server = cfg;
		}

		private void ReceiveConfigs_Load(object sender, EventArgs e)
		{
			ListViewItem transferItemKey = new ListViewItem("이름");
			ListViewItem transferItemValue = new ListViewItem("값");

			foreach(var config in server.config.ConfigTable)
			{
				transferItemKey.SubItems.Add(config.Key);
				transferItemValue.SubItems.Add(config.Value);
			}
		}
	}
}
