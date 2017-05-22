using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using SimFTP.Net.MetadataPackets;
using SimFTPV.Configs;
using SimFTP.Enums;

namespace SimFTPV.Forms
{
	public partial class SenderConfigs : Form
	{
		SendConfig cfg;
		public SenderConfigs(ref SendConfig cfg)
		{
			this.cfg = cfg;
			InitializeComponent();
		}

		private void SenderConfigs_Load(object sender, EventArgs e)
		{
			PacketType type = (PacketType)Enum.Parse(typeof(PacketType), cfg.GetConfigTable("Using_Mode"));

			if(type == PacketType.BasicFrame)
				radioButton1.Checked = true;
			else if(type == PacketType.BasicSecurity)
				radioButton2.Checked = true;
			else
				radioButton3.Checked = true;

			if(cfg.GetConfigTable("Auth_Anonymous") == bool.TrueString)
				checkBox1.Checked = true;
			
			textBox1.Text = cfg.GetConfigTable("Auth_Username");
			textBox2.Text = cfg.GetConfigTable("Auth_Password");
		}

		private void checkBox1_CheckedChanged(object sender, EventArgs e)
		{
			if(checkBox1.Checked == true)
			{
				textBox1.ReadOnly = true;
				textBox2.ReadOnly = true;
				cfg.SetConfigTable("Auth_Anonymous", bool.TrueString);
			}
			else
			{
				textBox1.ReadOnly = false;
				textBox2.ReadOnly = false;
				cfg.SetConfigTable("Auth_Anonymous", bool.FalseString);
			}
		}

		private void textBox1_Leave(object sender, EventArgs e)
		{
			cfg.SetConfigTable("Auth_Username", textBox1.Text);
		}

		private void textBox2_Leave(object sender, EventArgs e)
		{
			cfg.SetConfigTable("Auth_Password", textBox2.Text);
		}

		private void radioButton1_CheckedChanged(object sender, EventArgs e)
		{
			cfg.SetConfigTable("Using_Mode", PacketType.BasicFrame.ToString());
		}

		private void radioButton2_CheckedChanged(object sender, EventArgs a)
		{
			cfg.SetConfigTable("Using_Mode", PacketType.BasicSecurity.ToString());
		}

		private void radioButton3_CheckedChanged(object sender, EventArgs e)
		{
			cfg.SetConfigTable("Using_Mode", PacketType.ExpertSecurity.ToString());
		}
	}
}
