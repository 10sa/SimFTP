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

		public ProgramConfigs(ref ProgramConfig config)
		{
			InitializeComponent();
			this.config = config;
		}

		private void ProgramConfigs_Load(object sender, EventArgs e)
		{
			foreach(var data in config.ConfigTable)
			{
				checkedListBox1.Items.Add(data.Key, bool.Parse(data.Value));
			}

			_isLoadedConfigs_ = false;
		}

		private void checkedListBox1_ItemCheck(object sender, ItemCheckEventArgs e)
		{	
			if(!_isLoadedConfigs_)
				config.SetConfigTable((string)checkedListBox1.Items[e.Index], e.NewValue == CheckState.Checked ? bool.TrueString : bool.FalseString);
		}
	}
}
