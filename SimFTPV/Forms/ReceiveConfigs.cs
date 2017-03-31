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

namespace SimFTPV.Forms
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
			RefreshConfigList();
			RefreshAccountList();
		}

		private void RefreshConfigList()
		{
			foreach(var config in server.config.ConfigTable)
			{
				ListViewItem items = new ListViewItem(config.Key);
				items.SubItems.Add(config.Value);

				listView1.Items.Add(items);
			}

			listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		private void RefreshAccountList()
		{
			foreach(var config in server.accountConfig.ConfigTable)
			{
				ListViewItem items = new ListViewItem(config.Key);
				items.SubItems.Add(config.Value);

				listView2.Items.Add(items);
			}

			listView2.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		private void listView1_DoubleClick(object sender, EventArgs e)
		{
			if(listView1.SelectedItems[0].SubItems[1].Text == bool.TrueString)
				listView1.SelectedItems[0].SubItems[1].Text = bool.FalseString;
			else
				listView1.SelectedItems[0].SubItems[1].Text = bool.TrueString;

			server.config.SetConfigTable(listView1.SelectedItems[0].Text, listView1.SelectedItems[0].SubItems[1].Text);
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show("사용자를 삭제하시겠습니까?", "사용자 삭제", MessageBoxButtons.YesNo) == DialogResult.Yes)
			{
				foreach(ListViewItem item in listView2.SelectedItems)
				{
					listView2.Items.Remove(item);
					server.accountConfig.RemoveAccount(item.SubItems[0].Text);
				}
			}

			RefreshAccountList();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			AddAcount accountAddHandler = new AddAcount(ref this.server.accountConfig);
			accountAddHandler.ShowDialog();

			RefreshAccountList();
		}
	}
}
