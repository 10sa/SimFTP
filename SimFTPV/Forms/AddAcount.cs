using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SimFTP.Config;

namespace SimFTPV.Forms
{
	public partial class AddAcount : Form
	{
		AccountConfig accounts;

		public AddAcount(ref AccountConfig account)
		{
			accounts = account;
			InitializeComponent();
		}

		private void button2_Click(object sender, EventArgs e)
		{
			string username = textBox1.Text;
			string password = textBox2.Text;

			if(username == string.Empty || password == string.Empty)
			{
				MessageBox.Show("사용자 계정이나 비밀번호는 공백일 수 없습니다.", "에러", MessageBoxButtons.OK);
				return;
			}

			if(accounts.GetConfigTable(username) != null)
			{
				if(MessageBox.Show("이미 존재하는 계정입니다. 덮어 씌우시겠습니까?", "알림", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
					accounts.SetConfigTable(username, password);
			}
			else
				accounts.AddConfigTable(username, password);

			MessageBox.Show("성공적으로 계정을 추가하였습니다.", "알림", MessageBoxButtons.OK);
			this.Close();
		}

		private void button1_Click(object sender, EventArgs e)
		{
			this.Close();
		}
	}
}
