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
using SimFTP.Net;
using SimFTP.Net.Client;
using SimFTP.Net.DataPackets;
using SimFTP.Net.MetadataPackets;

using SimFTPV.Forms;
using SimFTPV.Configs;

using SimFTP.Enums;

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

		private const string SendingCompleted = "송신 완료";
		private const string SuccessfulSending = "파일이 성공적으로 전송되었습니다.";
		private const string IsReceiveClient = "{0} 으로 부터 데이터를 수신할까요?";
		private const string ReceiveNotify = "수신 알림";
		private const string CreditMessageBoxDesc = "이 프로그램은 MIT 라이센스 조건 하에 모든 사용이 허가됩니다.\n아이콘 출처 www.iconfinder.com/icons/103291/arrow_down_full_icon";
		private const string CreditMessageBoxName = "프로그램 정보";

		private const int notifyShowTime = 1;

		#endregion

		// ICON LINK : https://www.iconfinder.com/icons/103291/arrow_down_full_icon //

		#region Private Field 

		private SendConfig sendConfig = new SendConfig();
		private ProgramConfig programConfig = new ProgramConfig();
		private CacheConfigs cacheConfig = new CacheConfigs();

		private Server server = new Server();
        private IOQueue IOQueueForm = new IOQueue();
		private ManualResetEvent LastClientEvent;

		private ComboBox overrideTextBox;

		#endregion

		public MainForm()
		{
			InitializeComponent();
			server.ReceivedBasicPacket += Server_ReceivedBasicPacket;
			server.ConnectedClient += Server_ConnectedCallback;
			server.ReceiveEnd += Server_ReceiveEndCallback;

			if (programConfig.GetConfigTable(ProgramConfig.UsingCacheBox) == bool.TrueString)
			{
				textBox1.Visible = false;
				overrideTextBox = new ComboBox();
				overrideTextBox.Size = textBox1.Size;
				overrideTextBox.Location = textBox1.Location;
				overrideTextBox.Anchor = textBox1.Anchor;
				overrideTextBox.TextChanged += (a, b) =>
				{
					// Override //
					textBox1.Text = ((ComboBox)a).Text;
				};

				foreach(var item in cacheConfig.GetAllCacheValue())
					overrideTextBox.Items.Add(item);

				this.Controls.Add(overrideTextBox);
			}
		}

        private void Server_ClientInvalid(ServerEventArgs args)
        {
            Invoke((MethodInvoker)delegate () { IOQueueForm.RemoveServerQueue(); });
        }

		private void Server_ConnectedCallback(ServerEventArgs args)
		{
			Invoke((MethodInvoker)delegate () { IOQueueForm.AddServerQueue(args.ClientAddress); });
		}

		private void Server_ReceiveEndCallback(ServerEventArgs args)
		{
			Invoke((MethodInvoker)delegate () { IOQueueForm.RemoveServerQueue(); });
		}

		private void Server_ReceivedBasicPacket(ServerEventArgs args)
		{
			if(programConfig.GetConfigTable(ProgramConfig.NotifyAcceptEvent) == bool.FalseString)
				return;

			if(this.Visible)
			{
				if(MessageBox.Show(string.Format(IsReceiveClient, args.ClientAddress), ReceiveNotify, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
					args.Cancel = true;
			}
			else
			{
				EventHandler ballonTipEvent = (a, b) => 
				{
					if(MessageBox.Show(string.Format(IsReceiveClient, args.ClientAddress), ReceiveNotify, MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
						args.Cancel = true;
				};

				EventHandler ballonTipTimeout = (a, b) =>
				{
					args.Cancel = true;
				};

				notifyIcon1.BalloonTipClicked += ballonTipEvent;
				notifyIcon1.BalloonTipClosed += ballonTipTimeout;

				notifyIcon1.ShowBalloonTip(notifyShowTime, ReceiveNotify, string.Format(IsReceiveClient, args.ClientAddress), ToolTipIcon.Info);

				notifyIcon1.BalloonTipClicked -= ballonTipEvent;
				notifyIcon1.BalloonTipClosed -= ballonTipTimeout;
			}
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
			if(openFileDialog1.ShowDialog() == DialogResult.OK)
			{
				foreach(var file in openFileDialog1.FileNames)
				{
					FileInfo fileInfo = new FileInfo(file);
					ListViewItem item = new ListViewItem(fileInfo.FullName);

					item.SubItems.Add(GetAutoChangedFileSize(fileInfo.Length));
					item.SubItems.Add(fileInfo.CreationTime.ToString());

					listView1.Items.Add(item);
				}

				listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
				listView1.Update();
			}
		}

		private string GetAutoChangedFileSize(long fileSize)
		{
			if(fileSize > (int.MaxValue / 2))
				return Math.Round(((double)fileSize / 1024 / 1024 / 1024), 2).ToString() + " GB";
			else if(fileSize > (int.MaxValue / 4))
				return Math.Round(((double)fileSize / 1024 / 1024), 2).ToString() + " MB";
			else
				return Math.Round(((double)fileSize / 1024), 2).ToString() + " KB";
		}

		private void button4_Click(object sender, EventArgs e)
		{
			if(listView1.SelectedItems.Count > 0)
				listView1.Items.RemoveAt(listView1.SelectedItems[0].Index);
			else
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
			if(e.CloseReason == CloseReason.ApplicationExitCall || programConfig.GetConfigTable(ProgramConfig.UsingProgramTray) == bool.FalseString)
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
				MessageBox.Show(inputWrongAddress_desc, inputWrongAddress, MessageBoxButtons.OK, MessageBoxIcon.Error);
			else if(listView1.Items.Count <= 0)
				MessageBox.Show(noAddedFiles, noFiles, MessageBoxButtons.OK, MessageBoxIcon.Error);
			else
			{
				notifyIcon1.ShowBalloonTip(notifyShowTime, startSendingFiles, sendingFiles, ToolTipIcon.Info);
				Thread clientSendlingHandler = new Thread(SendingThreadRoutine);
				List<string> parameter = new List<string>();

				foreach(ListViewItem value in listView1.Items)
					parameter.Add(value.Text);

				clientSendlingHandler.Start(parameter.ToArray());
			}
		}

		private void SendingCallback(ClientEventArgs args)
		{
			notifyIcon1.ShowBalloonTip(notifyShowTime, SendingCompleted, args.ClientAddress + " " + SuccessfulSending, ToolTipIcon.Info);
			Invoke((MethodInvoker)delegate { IOQueueForm.RemoveClientQueue(); });
		}

		private void SendingThreadRoutine(object a)
		{
			try
			{
				string[] items = (string[])a;
				List<string> temp = new List<string>();
				Invoke((MethodInvoker)delegate { temp.Add(textBox1.Text); });

				Client client = new Client(temp.ToArray(), (PacketType)Enum.Parse(typeof(PacketType), sendConfig.GetConfigTable("Using_Mode")), LastClientEvent);
				client.SendingCompleted += SendingCallback;

				Invoke((MethodInvoker)delegate { IOQueueForm.AddClientQueue(textBox1.Text); });
				LastClientEvent = client.ClientIOCompleteEvent;

				if(client.WaitingEvent != null)
					client.WaitingEvent.WaitOne();

				SendingDataFiles(client, items, client.SendType);
			}
			catch(Exception excpt)
			{
				notifyIcon1.ShowBalloonTip(notifyShowTime, sendingFailure, excpt.Message, ToolTipIcon.Error);

				// Invalid Check Require //
				Invoke((MethodInvoker)delegate () { IOQueueForm.RemoveClientQueue(); });
			}
		}

		private void SendingDataFiles(Client client, string[] files, PacketType packetType)
		{
			List<BasicDataPacket> fileList = new List<BasicDataPacket>();

			foreach (var value in files)
			{
				if (File.Exists(value))
				{
					FileInfo fileInfo = new FileInfo(value);

					switch (packetType)
					{
						case PacketType.BasicFrame:
							fileList.Add(new BasicDataPacket(value, File.Open(fileInfo.Name, FileMode.Open)));
							break;
						case PacketType.BasicSecurity:
							fileList.Add(new BasicSecurityDataPacket(value, File.Open(fileInfo.Name, FileMode.Open)));
							break;
						case PacketType.ExpertSecurity:
							fileList.Add(new BasicSecurityDataPacket(fileInfo.Name, File.Open(fileInfo.Name, FileMode.Open)));
							break;
						default:
							return;
					}
				}
			}

			switch(packetType)
			{
				case PacketType.BasicFrame:
					client.SendFile(fileList.ToArray());
					break;
				case PacketType.BasicSecurity:
					client.SendFile((BasicSecurityDataPacket[])fileList.ToArray());
					break;
				case PacketType.ExpertSecurity:
					client.SendFile((ExpertSecurityDataPacket[])fileList.ToArray());
					break;
				default:
					break;
			}
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

		private void 전송상황ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			IOQueueForm.Show();
		}

		private void 프로그램정보ToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MessageBox.Show(CreditMessageBoxDesc, CreditMessageBoxName, MessageBoxButtons.OK, MessageBoxIcon.Information);
		}
	}
}
