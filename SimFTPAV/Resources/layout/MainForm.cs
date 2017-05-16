using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace SimFTPAV.Resources.layout
{
	class MainForm
	{
		public Button button;
		public TextView label;

		public MainForm()
		{
			Activity act = new Activity();
			button = act.FindViewById<Button>(Resource.Id.button1);
			label = act.FindViewById<TextView>(Resource.Id.textView1);
		}
	}
}