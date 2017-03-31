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
using System.Threading;

using SimFTP.Net.Server;
using SimFTP.Net.Client;
using SimFTP.Net.DataPackets;
using SimFTP.Net.MetadataPackets;

using SimFTPV.Forms;

namespace SimFTPV.Forms
{
	public partial class MainForm : Form
	{
		#region Private Const Language Define

		private const string serverTurnOn = "서버 켜기";
		private const string serverTurnOff = "서버 끄기";

		private const string serverOnline = "서버 켜짐";
		private const string serverRunningOnBackground = "서버가 백그라운드에서 가동 중입니다.";

		private const string programRunningBackground = "백그라운드 가동";
		private const string programRunningOnBackground = "프로그렘이 백그라운드에서 가동 중입니다.";

		private const string startSendingFiles = "전송 시작";
		private const string sendingFiles = "파일을 보내는 중 입니다.";
		private const string sendingFailure = "전송 실패";

		private const string inputWrongAddress_desc = "잘못된 주소입니다.";
		private const string inputWrongAddress = "주소 오류";

		#endregion

		#region Private Const Configs Define

		private const int notifyShowTime = 5;

		#endregion

		// ICON LINK : https://www.iconfinder.com/icons/103291/arrow_down_full_icon //

		ClientConfig clConfig = new ClientConfig();
		Server server = new Server();
		
		public MainForm()
		{
			InitializeComponent();
		}

		private void MainForm_Load(object sender, EventArgs e)
		{
			this.contextMenuStrip1.Visible = false;
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
				button3.Text = serverTurnOn;
			}
			else
			{
				server.Start();
				button3.Text = serverTurnOff;

				notifyIcon1.ShowBalloonTip(notifyShowTime, serverOnline, serverRunningOnBackground, ToolTipIcon.Info);
			}
		}

		private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			this.Visible = true;
			this.Activate();
		}

		private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
		{
			if(e.CloseReason == CloseReason.ApplicationExitCall)
			{
				notifyIcon1.Visible = false;
				return;
			}

			notifyIcon1.ShowBalloonTip(notifyShowTime, programRunningBackground, programRunningOnBackground, ToolTipIcon.Info);

			e.Cancel = true;
			this.Hide();
			this.Visible = false;
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
			{
				MessageBox.Show(inputWrongAddress_desc, inputWrongAddress, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			notifyIcon1.ShowBalloonTip(notifyShowTime, startSendingFiles, sendingFiles, ToolTipIcon.Info);
			Thread clientSendlingHandler = new Thread(() => {
				try
				{
					Client client = new Client(textBox1.Text, (PacketType)Enum.Parse(typeof(PacketType), clConfig.GetConfigTable("Using_Mode")));

					if(client.SendType == PacketType.BasicFrame)
					{
						SendingBasicDataFiles(client);
					}
					else if(client.SendType == PacketType.BasicSecurity)
					{
						SendingBasicSecurityFiles(client);
					}
					else if(client.SendType == PacketType.ExpertSecurity)
					{
						SendingExpertDataFiles(client);
					}
				}
				catch(Exception excpt)
				{
					notifyIcon1.ShowBalloonTip(notifyShowTime, sendingFailure, excpt.Message, ToolTipIcon.Error);
				}
			});
			clientSendlingHandler.Start();
		}

		private void SendingExpertDataFiles(Client client)
		{
			List<ExpertSecurityDataPacket> files = new List<ExpertSecurityDataPacket>();

			foreach(ListViewItem value in listView1.Items)
			{
				FileInfo fileInfo = new FileInfo(value.Text);
				files.Add(new ExpertSecurityDataPacket(new BasicDataPacket(fileInfo.Name, File.Open(fileInfo.FullName, FileMode.Open))));
			}

			client.SendFile(files.ToArray());
		}

		private void SendingBasicDataFiles(Client client)
		{
			List<BasicDataPacket> files = new List<BasicDataPacket>();

			foreach(ListViewItem value in listView1.Items)
			{
				FileInfo fileInfo = new FileInfo(value.Text);
				files.Add(new BasicDataPacket(fileInfo.Name, File.Open(fileInfo.FullName, FileMode.Open)));
			}

			client.SendFile(files.ToArray());
		}

		private void SendingBasicSecurityFiles(Client client)
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
