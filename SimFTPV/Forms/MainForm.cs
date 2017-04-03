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
using SimFTPV.Configs;

namespace SimFTPV.Forms
{
	public partial class MainForm : Form
	{
		#region Private Const Language Define

		private const string serverTurnOn = "서버 켜기";
		private const string serverTurnOff = "서버 중지";

		private const string serverOnline = "서버 켜짐";
		private const string serverRunningOnBackground = "서버가 백그라운드에서 가동 중입니다.";

		private const string programRunningBackground = "백그라운드 가동";
		private const string programRunningOnBackground = "프로그렘이 백그라운드에서 가동 중입니다.";

		private const string startSendingFiles = "전송 시작";
		private const string sendingFiles = "파일을 보내는 중 입니다.";
		private const string sendingFailure = "전송 실패";

		private const string inputWrongAddress_desc = "잘못된 주소입니다.";
		private const string inputWrongAddress = "주소 오류";

		private const string noAddedFiles = "추가된 파일이 없습니다.";
		private const string noFiles = "파일 없음";

		#endregion

		#region Private Const Config Values

		private const int notifyShowTime = 5;

		#endregion

		// ICON LINK : https://www.iconfinder.com/icons/103291/arrow_down_full_icon //

		SendConfig sendConfig = new SendConfig();
		ProgramConfig programConfig = new ProgramConfig();
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
			if(e.CloseReason == CloseReason.ApplicationExitCall || programConfig.GetConfigTable("Using_Program_Tray") == bool.FalseString)
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
			Application.Exit();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			if(textBox1.Text == string.Empty)
			{
				MessageBox.Show(inputWrongAddress_desc, inputWrongAddress, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}
			else if(listView1.Items.Count <= 0)
			{
				MessageBox.Show(noAddedFiles, noFiles, MessageBoxButtons.OK, MessageBoxIcon.Error);
				return;
			}

			notifyIcon1.ShowBalloonTip(notifyShowTime, startSendingFiles, sendingFiles, ToolTipIcon.Info);
			Thread clientSendlingHandler = new Thread((a) => {
				ListView.ListViewItemCollection items = new ListView.ListViewItemCollection(new ListView());
				Invoke(new MethodInvoker(delegate () { }));

				try
				{
					Client client = new Client(textBox1.Text, (PacketType)Enum.Parse(typeof(PacketType), sendConfig.GetConfigTable("Using_Mode")));

					if(client.SendType == PacketType.BasicFrame)
					{
						SendingBasicDataFiles(client, items);
					}
					else if(client.SendType == PacketType.BasicSecurity)
					{
						SendingBasicSecurityFiles(client, items);
					}
					else if(client.SendType == PacketType.ExpertSecurity)
					{
						SendingExpertDataFiles(client, items);
					}
				}
				catch(Exception excpt)
				{
					throw;// notifyIcon1.ShowBalloonTip(notifyShowTime, sendingFailure, excpt.Message, ToolTipIcon.Error);
				}
			});
			clientSendlingHandler.Start(listView1.Items);
		}

		private void SendingExpertDataFiles(Client client, ListView.ListViewItemCollection items)
		{
			List<ExpertSecurityDataPacket> files = new List<ExpertSecurityDataPacket>();

			foreach(ListViewItem value in items)
			{
				FileInfo fileInfo = new FileInfo(value.Text);
				files.Add(new ExpertSecurityDataPacket(new BasicDataPacket(fileInfo.Name, File.Open(fileInfo.FullName, FileMode.Open))));
			}

			client.SendFile(files.ToArray());
		}

		private void SendingBasicDataFiles(Client client, ListView.ListViewItemCollection items)
		{
			List<BasicDataPacket> files = new List<BasicDataPacket>();

			foreach(ListViewItem value in items)
			{
				FileInfo fileInfo = new FileInfo(value.Text);
				files.Add(new BasicDataPacket(fileInfo.Name, File.Open(fileInfo.FullName, FileMode.Open)));
			}

			client.SendFile(files.ToArray());
		}

		private void SendingBasicSecurityFiles(Client client, ListView.ListViewItemCollection items)
		{
			List<BasicSecurityDataPacket> files = new List<BasicSecurityDataPacket>();

			foreach(ListViewItem value in items)
			{
				FileInfo fileInfo = new FileInfo(value.Text);
				files.Add(new BasicSecurityDataPacket(fileInfo.Name, File.Open(fileInfo.FullName, FileMode.Open)));
			}

			client.SendFile(files.ToArray());
		}

		private void 송신설정ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			SenderConfigs senderConfigForm = new SenderConfigs(ref sendConfig);
			senderConfigForm.ShowDialog();
		}

		private void 프로그램설정ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			ProgramConfigs programCfg = new ProgramConfigs(ref programConfig);
			programCfg.ShowDialog();
		}
	}
}
