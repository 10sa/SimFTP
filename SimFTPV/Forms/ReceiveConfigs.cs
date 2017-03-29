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

			foreach(var config in server.config.ConfigTable)
			{
				ListViewItem items = new ListViewItem(config.Key);
				items.SubItems.Add(config.Value);

				listView1.Items.Add(items);
			}

			listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
		}

		private void listView1_DoubleClick(object sender, EventArgs e)
		{
			if(listView1.SelectedItems[0].SubItems[1].Text == bool.TrueString)
				listView1.SelectedItems[0].SubItems[1].Text = bool.FalseString;
			else
				listView1.SelectedItems[0].SubItems[1].Text = bool.TrueString;

			server.config.SetConfigTable(listView1.SelectedItems[0].Text, listView1.SelectedItems[0].SubItems[1].Text);
		}
	}
}
