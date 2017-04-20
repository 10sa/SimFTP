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
using System.IO;


using SimFTP.Net.Server;
using SimFTP.Config;

namespace SimFTPV.Forms
{
	public partial class ReceiveConfigs : Form
	{
		private const string InvaildIPAddress = "IP 주소 탐색에 실패하였습니다.";
		private const string RequestRemoveUser = "사용자를 삭제하시겠습니까?";
		private const string UserRemove = "사용자 삭제";
		private const string AddressGetFailure = "불러오기 실패";
		private const string DataLoading = "불러오는 중...";
		private const string WrongFolderDirectory = "잘못된 폴더 경로입니다.";
		private const string Warning = "경고";
		private bool _IsLoaded = false;

		private readonly Dictionary<string, string> ConfigDesc = new Dictionary<string, string>()
		{
			{ TransferConfig.AcceptDefaultPacket, "기본 모드 승인" },
			{ TransferConfig.AcceptBasicSecurityPacket, "기초 보안 모드 승인" },
			{ TransferConfig.AcceptExpertSecurityPacket, "전문 보안 모드 승인" },
			{ TransferConfig.AccpetAnonymousUser, "익명 접속 승인" },
			{ TransferConfig.IsOverwrite, "덮어 씌우기 여부" },
			{ TransferConfig.IsSaveDateDirectory, "시간 폴더에 저장 여부" },
			{ TransferConfig.IsSaveUserDirectory, "송신자 이름 폴더 생성 후 저장 여부" },
		};

		Server server;

		public ReceiveConfigs(ref Server cfg)
		{
			InitializeComponent();
			server = cfg;

			listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
			listView2.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);

			textBox1.Text = DataLoading;
			textBox2.Text = DataLoading;

			BackgroundWorker worker = new BackgroundWorker();
			worker.DoWork += (a, b) => {
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
			textBox3.Text = server.config.GetConfigTable(TransferConfig.DownloadDirectory);
			_IsLoaded = true;
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
				if(config.Key == TransferConfig.DownloadDirectory)
					continue;

				ListViewItem item = new ListViewItem(ConfigDesc[config.Key]);
				item.SubItems.Add(config.Value);

				if(item.SubItems[1].Text == bool.TrueString)
					item.Checked = true;
				else
					item.Checked = false;

				listView1.Items.Add(item);
			}

			if(listView1.Items.Count > 0)
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

			if(listView2.Items.Count > 0)
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
			if(_IsLoaded)
			{
				if(e.NewValue == CheckState.Checked)
					listView1.Items[e.Index].SubItems[1].Text = bool.TrueString;
				else
					listView1.Items[e.Index].SubItems[1].Text = bool.FalseString;

				var data = ConfigDesc.FirstOrDefault(x => x.Value == listView1.Items[e.Index].SubItems[0].Text);
				if(!string.IsNullOrEmpty(data.Key))
					server.config.SetConfigTable(data.Key, listView1.Items[e.Index].SubItems[1].Text);
			}
		}


		private void button3_Click(object sender, EventArgs e)
		{
			if(folderBrowserDialog1.ShowDialog() == DialogResult.OK)
			{
				textBox3.Text = folderBrowserDialog1.SelectedPath;
				server.config.SetConfigTable(TransferConfig.DownloadDirectory, textBox3.Text);
			}
		}

		private void textBox3_Leave(object sender, EventArgs e)
		{
			DownloadFolderValidCheck();
		}

		private bool DownloadFolderValidCheck()
		{
			if(!Directory.Exists(textBox3.Text) && textBox3.Text != string.Empty)
			{
				MessageBox.Show(WrongFolderDirectory, Warning, MessageBoxButtons.OK);
				server.config.SetConfigTable(TransferConfig.DownloadDirectory, "");
				textBox3.Text = "";
				textBox3.Select();

				return false;
			}
			else
			{
				server.config.SetConfigTable(TransferConfig.DownloadDirectory, textBox3.Text);
				return true;
			}
		}

		private void ReceiveConfigs_FormClosing(object sender, FormClosingEventArgs e)
		{
			if(e.CloseReason == CloseReason.UserClosing)
			{
				if(!DownloadFolderValidCheck())
					e.Cancel = true;
			}
			else
				_IsLoaded = false;
		}
	}
}
