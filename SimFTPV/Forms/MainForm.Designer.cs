namespace SimFTPV.Forms
{
	partial class MainForm
	{
		/// <summary>
		/// 필수 디자이너 변수입니다.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary>
		/// 사용 중인 모든 리소스를 정리합니다.
		/// </summary>
		/// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
		protected override void Dispose(bool disposing)
		{
			sendConfig.Dispose();
			programConfig.Dispose();
			server.Dispose();
			notifyIcon1.Visible = false;

			if(disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form 디자이너에서 생성한 코드

		/// <summary>
		/// 디자이너 지원에 필요한 메서드입니다. 
		/// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
			this.menuStrip1 = new System.Windows.Forms.MenuStrip();
			this.설정ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.SettingsReceive = new System.Windows.Forms.ToolStripMenuItem();
			this.송신설정ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.프로그램설정ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.listView1 = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.textBox1 = new System.Windows.Forms.TextBox();
			this.label1 = new System.Windows.Forms.Label();
			this.button3 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
			this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
			this.종료ToolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
			this.전송상황ToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
			this.menuStrip1.SuspendLayout();
			this.contextMenuStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// menuStrip1
			// 
			this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.설정ToolStripMenuItem,
            this.전송상황ToolStripMenuItem});
			this.menuStrip1.Location = new System.Drawing.Point(0, 0);
			this.menuStrip1.Name = "menuStrip1";
			this.menuStrip1.Size = new System.Drawing.Size(624, 24);
			this.menuStrip1.TabIndex = 0;
			this.menuStrip1.Text = "menuStrip1";
			// 
			// 설정ToolStripMenuItem
			// 
			this.설정ToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.SettingsReceive,
            this.송신설정ToolStripMenuItem,
            this.프로그램설정ToolStripMenuItem});
			this.설정ToolStripMenuItem.Name = "설정ToolStripMenuItem";
			this.설정ToolStripMenuItem.Size = new System.Drawing.Size(43, 20);
			this.설정ToolStripMenuItem.Text = "설정";
			// 
			// SettingsReceive
			// 
			this.SettingsReceive.Name = "SettingsReceive";
			this.SettingsReceive.Size = new System.Drawing.Size(150, 22);
			this.SettingsReceive.Text = "수신 설정";
			this.SettingsReceive.Click += new System.EventHandler(this.SettingsReceive_Click);
			// 
			// 송신설정ToolStripMenuItem
			// 
			this.송신설정ToolStripMenuItem.Name = "송신설정ToolStripMenuItem";
			this.송신설정ToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
			this.송신설정ToolStripMenuItem.Text = "송신 설정";
			this.송신설정ToolStripMenuItem.Click += new System.EventHandler(this.송신설정ToolStripMenuItem_Click);
			// 
			// 프로그램설정ToolStripMenuItem
			// 
			this.프로그램설정ToolStripMenuItem.Name = "프로그램설정ToolStripMenuItem";
			this.프로그램설정ToolStripMenuItem.Size = new System.Drawing.Size(150, 22);
			this.프로그램설정ToolStripMenuItem.Text = "프로그램 설정";
			this.프로그램설정ToolStripMenuItem.Click += new System.EventHandler(this.프로그램설정ToolStripMenuItem_Click);
			// 
			// listView1
			// 
			this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
			this.listView1.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
			this.listView1.Location = new System.Drawing.Point(12, 27);
			this.listView1.Name = "listView1";
			this.listView1.Size = new System.Drawing.Size(600, 324);
			this.listView1.TabIndex = 1;
			this.listView1.UseCompatibleStateImageBehavior = false;
			this.listView1.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "File";
			this.columnHeader1.Width = 596;
			// 
			// button1
			// 
			this.button1.Location = new System.Drawing.Point(12, 396);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(141, 33);
			this.button1.TabIndex = 2;
			this.button1.Text = "파일 보내기";
			this.button1.UseVisualStyleBackColor = true;
			this.button1.Click += new System.EventHandler(this.button1_Click);
			// 
			// button2
			// 
			this.button2.Location = new System.Drawing.Point(12, 357);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(141, 33);
			this.button2.TabIndex = 3;
			this.button2.Text = "파일 추가";
			this.button2.UseVisualStyleBackColor = true;
			this.button2.Click += new System.EventHandler(this.button2_Click);
			// 
			// textBox1
			// 
			this.textBox1.Location = new System.Drawing.Point(161, 408);
			this.textBox1.Name = "textBox1";
			this.textBox1.Size = new System.Drawing.Size(453, 21);
			this.textBox1.TabIndex = 4;
			// 
			// label1
			// 
			this.label1.AutoSize = true;
			this.label1.Location = new System.Drawing.Point(159, 393);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(57, 12);
			this.label1.TabIndex = 5;
			this.label1.Text = "서버 주소";
			// 
			// button3
			// 
			this.button3.Location = new System.Drawing.Point(471, 357);
			this.button3.Name = "button3";
			this.button3.Size = new System.Drawing.Size(141, 33);
			this.button3.TabIndex = 6;
			this.button3.Text = "서버 켜기";
			this.button3.UseVisualStyleBackColor = true;
			this.button3.Click += new System.EventHandler(this.button3_Click);
			// 
			// button4
			// 
			this.button4.Location = new System.Drawing.Point(159, 357);
			this.button4.Name = "button4";
			this.button4.Size = new System.Drawing.Size(141, 33);
			this.button4.TabIndex = 7;
			this.button4.Text = "파일 제거";
			this.button4.UseVisualStyleBackColor = true;
			this.button4.Click += new System.EventHandler(this.button4_Click);
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.FileName = "openFileDialog1";
			this.openFileDialog1.Multiselect = true;
			// 
			// notifyIcon1
			// 
			this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
			this.notifyIcon1.Text = "SimFTPV";
			this.notifyIcon1.Visible = true;
			this.notifyIcon1.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon1_MouseDoubleClick);
			// 
			// contextMenuStrip1
			// 
			this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.종료ToolStripMenuItem1});
			this.contextMenuStrip1.Name = "contextMenuStrip1";
			this.contextMenuStrip1.Size = new System.Drawing.Size(99, 26);
			// 
			// 종료ToolStripMenuItem1
			// 
			this.종료ToolStripMenuItem1.Name = "종료ToolStripMenuItem1";
			this.종료ToolStripMenuItem1.Size = new System.Drawing.Size(98, 22);
			this.종료ToolStripMenuItem1.Text = "종료";
			this.종료ToolStripMenuItem1.Click += new System.EventHandler(this.종료ToolStripMenuItem1_Click);
			// 
			// 전송상황ToolStripMenuItem
			// 
			this.전송상황ToolStripMenuItem.Name = "전송상황ToolStripMenuItem";
			this.전송상황ToolStripMenuItem.Size = new System.Drawing.Size(71, 20);
			this.전송상황ToolStripMenuItem.Text = "전송 상황";
			this.전송상황ToolStripMenuItem.Click += new System.EventHandler(this.전송상황ToolStripMenuItem_Click);
			// 
			// MainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.ClientSize = new System.Drawing.Size(624, 441);
			this.Controls.Add(this.button4);
			this.Controls.Add(this.button3);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.textBox1);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.Controls.Add(this.listView1);
			this.Controls.Add(this.menuStrip1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.MainMenuStrip = this.menuStrip1;
			this.Name = "MainForm";
			this.Text = "SimFTP 뷰어";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
			this.Load += new System.EventHandler(this.MainForm_Load);
			this.menuStrip1.ResumeLayout(false);
			this.menuStrip1.PerformLayout();
			this.contextMenuStrip1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.MenuStrip menuStrip1;
		private System.Windows.Forms.ToolStripMenuItem 설정ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem SettingsReceive;
		private System.Windows.Forms.ListView listView1;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.TextBox textBox1;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button button3;
		private System.Windows.Forms.Button button4;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.NotifyIcon notifyIcon1;
		private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
		private System.Windows.Forms.ToolStripMenuItem 종료ToolStripMenuItem1;
		private System.Windows.Forms.ToolStripMenuItem 송신설정ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem 프로그램설정ToolStripMenuItem;
		private System.Windows.Forms.ToolStripMenuItem 전송상황ToolStripMenuItem;
	}
}

