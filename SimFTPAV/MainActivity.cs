using Android.App;
using Android.Widget;
using Android.OS;

namespace SimFTPAV
{
    [Activity(Label = "SimFTPAV", MainLauncher = true, Icon = "@drawable/icon")]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Set our view from the "main" layout resource
            SetContentView (Resource.Layout.MainForm);

			Button btnMain = FindViewById<Button>(Resource.Id.button1);
			TextView lblText = FindViewById<TextView>(Resource.Id.textView1);
			
			btnMain.Click += delegate { 
				btnMain.Text = "Clicked!";
				lblText.Text = "Clicked!";
			};

			return;
        }
    }
}

