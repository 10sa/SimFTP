using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

using SimFTP.Net.Server;
using SimFTP.Net.Client;
using SimFTP.Net.DataPackets;
using SimFTP.Net.MetadataPackets;

using SimFTPV.Forms;

namespace SimFTPV
{
	public partial class MainForm : Form
	{
		// ICON LINK : https://www.iconfinder.com/icons/103291/arrow_down_full_icon //

		ClientConfig clConfig = new ClientConfig();
		Server server = new Server();
		Client client;
		
		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			this.contextMenuStrip1.Visible = false;
			this.notifyIcon1.Visible = false;
			this.notifyIcon1.ContextMenuStrip = this.contextMenuStrip1;
		}

		private void SettingsReceive_Click(object sender, EventArgs e)
		{
			ReceiveConfigs receiveConfigsForm = new ReceiveConfigs(ref server);
			receiveConfigsForm.ShowDialog();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			openFileDialog1.ShowDialog();
			foreach(var file in openFileDialog1.FileNames)
				listView1.Items.Add(new ListViewItem(file));

			listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
			listView1.Update();
		}

		private void button4_Click(object sender, EventArgs e)
		{
			listView1.Items.Clear();
		}

		private void button3_Click(object sender, EventArgs e)
		{
			if(server.IsRunning)
			{
				server.Stop();
				button3.Text = "서버 켜기";
			}
			else
			{
				server.Start();
				button3.Text = "서버 끄기";
			}
		}

		private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			this.Visible = true;
			this.notifyIcon1.Visible = false;
			this.Activate();
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if(e.CloseReason == CloseReason.ApplicationExitCall)
				return;
			
			e.Cancel = true;
			this.Hide();
			this.Visible = false;
			this.notifyIcon1.Visible = true;
		}

		private void 종료ToolStripMenuItem1_Click(object sender, EventArgs e)
		{
			server.Dispose();
			clConfig.Dispose();

			Application.Exit();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if(textBox1.Text == string.Empty)
				return;

			client = new Client(textBox1.Text, (PacketType)Enum.Parse(typeof(PacketType), clConfig.GetConfigTable("Using_Mode")));

			if(client.SendType == PacketType.BasicFrame)
			{
				SendingBasicDataFiles();
			}
			else if(client.SendType == PacketType.BasicSecurity)
			{
				SendingBasicSecurityFiles();
			}
			else if(client.SendType == PacketType.ExpertSecurity)
			{
				SendingExpertDataFiles();
			}
		}

		private void SendingExpertDataFiles()
		{
			List<ExpertSecurityDataPacket> files = new List<ExpertSecurityDataPacket>();

			foreach(ListViewItem value in listView1.Items)
			{
				FileInfo fileInfo = new FileInfo(value.Text);
				files.Add(new ExpertSecurityDataPacket(new BasicDataPacket(fileInfo.Name, File.Open(fileInfo.FullName, FileMode.Open))));
			}

			client.SendFile(files.ToArray());
		}

		private void SendingBasicDataFiles()
		{
			List<BasicDataPacket> files = new List<BasicDataPacket>();

			foreach(ListViewItem value in listView1.Items)
			{
				FileInfo fileInfo = new FileInfo(value.Text);
				files.Add(new BasicDataPacket(fileInfo.Name, File.Open(fileInfo.FullName, FileMode.Open)));
			}

			client.SendFile(files.ToArray());
		}

		private void SendingBasicSecurityFiles()
		{
			List<BasicSecurityDataPacket> files = new List<BasicSecurityDataPacket>();

			foreach(ListViewItem value in listView1.Items)
			{
				FileInfo fileInfo = new FileInfo(value.Text);
				files.Add(new BasicSecurityDataPacket(fileInfo.Name, File.Open(fileInfo.FullName, FileMode.Open)));
			}

			client.SendFile(files.ToArray());
		}

		private void 송신설정ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SenderConfigs senderConfigForm = new SenderConfigs(ref clConfig);
			senderConfigForm.ShowDialog();
		}
	}
}
