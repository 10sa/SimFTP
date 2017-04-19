using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;

using SimFTP.Net.Server;

namespace SimFTPV.Forms
{
	public partial class ReceiveConfigs : Form
	{
		private const string InvaildIPAddress = "IP 주소 탐색에 실패하였습니다.";
		private const string RequestRemoveUser = "사용자를 삭제하시겠습니까?";
		private const string UserRemove = "사용자 삭제";
		private const string AddressGetFailure = "불러오기 실패";
		private const string DataLoading = "불러오는 중...";
		Server server;

		public ReceiveConfigs(ref Server cfg)
		{
			InitializeComponent();
			server = cfg;

			textBox1.Text = DataLoading;
			textBox2.Text = DataLoading;

			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += (a, b) =>
			{
				b.Result = new string[] { GetPublicAddress(), GetLocalIPv4Address() };
			};

			worker.RunWorkerCompleted += (a, b) =>
			{
				string[] result = (string[])b.Result;
				textBox1.Text = result[0];
				textBox2.Text = result[1];
			};
			worker.RunWorkerAsync();
		}

		private void ReceiveConfigs_Load(object sender, EventArgs e)
		{
			RefreshConfigList();
			RefreshAccountList();
		}

		private string GetPublicAddress()
		{
			try {
				return new WebClient().DownloadString("https://api.ipify.org");
			}
			catch(Exception) { return AddressGetFailure; }
		}

		private string GetLocalIPv4Address()
		{
			try
			{
				foreach(var address in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
				{
					if(address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
						return address.ToString();
				}

				return AddressGetFailure;
			}
			catch(Exception) { return AddressGetFailure; }
		}

		private void RefreshConfigList()
		{
			foreach(var config in server.config.ConfigTable)
			{
				ListViewItem items = new ListViewItem(config.Key);
				items.SubItems.Add(config.Value);

				listView1.Items.Add(items);
			}

			foreach(ListViewItem item in listView1.Items)
			{
				if(item.SubItems[1].Text == bool.TrueString)
					item.Checked = true;
				else
					item.Checked = false;
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

		private void button1_Click(object sender, EventArgs e)
		{
			if (MessageBox.Show(RequestRemoveUser, UserRemove, MessageBoxButtons.YesNo) == DialogResult.Yes)
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

		private void listView1_ItemCheck(object sender, ItemCheckEventArgs e)
		{
			if(e.NewValue == CheckState.Checked)
				listView1.Items[e.Index].SubItems[1].Text = bool.TrueString;
			else
				listView1.Items[e.Index].SubItems[1].Text = bool.FalseString;

			server.config.SetConfigTable(listView1.Items[e.Index].SubItems[0].Text, listView1.Items[e.Index].SubItems[1].Text);
		}

		private void textBox3_TextChanged(object sender, EventArgs e)
		{
			server.config.SetConfigTable("Download_Folder", textBox3.Text);
		}
	}
}
