using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


using SimFTPV.Configs;

namespace SimFTPV.Forms
{
	public partial class ProgramConfigs : Form
	{
		private ProgramConfig config;
		private bool _isLoadedConfigs_ = true;

		private readonly Dictionary<string, string> ConfigDesc = new Dictionary<string, string>()
		{
			{ProgramConfig.UsingProgramTray, "트레이 모드 사용" },
			{ProgramConfig.NotifyAcceptEvent, "수신 허가 여부 묻기" },
			{ProgramConfig.UsingCacheBox, "주소 기록 사용" }
		};

		public ProgramConfigs(ref ProgramConfig config)
		{
			InitializeComponent();
			this.config = config;
		}

		private void ProgramConfigs_Load(object sender, EventArgs e)
		{
			foreach(var data in config.ConfigTable)
				checkedListBox1.Items.Add(ConfigDesc[data.Key], bool.Parse(data.Value));

			_isLoadedConfigs_ = false;
		}

		private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
		{	
			if(!_isLoadedConfigs_)
			{
				var data = ConfigDesc.FirstOrDefault(x => x.Value == checkedListBox1.Items[e.Index].ToString());

				if (!string.IsNullOrEmpty(data.Key))
					config.SetConfigTable(data.Key, e.NewValue == CheckState.Checked ? bool.TrueString : bool.FalseString);
			}
		}
	}
}
