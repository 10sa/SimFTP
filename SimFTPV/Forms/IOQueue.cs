using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SimFTPV.Forms
{
	public partial class IOQueue : Form
	{
        Queue<QueueModel> ServerQueue = new Queue<QueueModel>();
        Queue<QueueModel> ClientQueue = new Queue<QueueModel>();


        public IOQueue()
		{
			InitializeComponent();
		}

        public void AddServerQueue(string address)
        {
            ServerQueue.Enqueue(new QueueModel(address, QueueStatus.Receiving));
			RefreshServerQueue();
		}

        public QueueModel RemoveServerQueue()
        {
			QueueModel temp = ServerQueue.Dequeue();
			RefreshServerQueue();

			return temp;
        }

		public void AddClientQueue(string address)
		{
			ClientQueue.Enqueue(new QueueModel(address, QueueStatus.Sending));
			RefreshClientQueue();
		}

		public QueueModel RemoveClientQueue()
		{
			QueueModel temp = ClientQueue.Dequeue();
			RefreshClientQueue();

			return temp;
		}

		private void IOQueue_Load(object sender, EventArgs e)
		{
			RefreshServerQueue();
			RefreshClientQueue();
		}

		private void RefreshServerQueue()
		{
			listView1.Items.Clear();
			foreach(var data in ServerQueue)
			{
				ListViewItem item = new ListViewItem(data.Address);
				item.SubItems.Add("수신 중...");

				listView1.Items.Add(item);
			}

			if(listView1.Items.Count > 0)
				listView1.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		private void RefreshClientQueue()
		{
			listView2.Items.Clear();
			foreach(var data in ClientQueue)
			{
				ListViewItem item = new ListViewItem(data.Address);
				item.SubItems.Add("송신 중...");

				listView2.Items.Add(item);
			}

			if(listView2.Items.Count > 0)
				listView2.AutoResizeColumns(ColumnHeaderAutoResizeStyle.ColumnContent);
		}

		private void IOQueue_FormClosing(object sender, FormClosingEventArgs e)
		{
			if(e.CloseReason == CloseReason.UserClosing)
			{
				e.Cancel = true;
				this.Hide();
			}
		}
	}

	public class QueueModel
	{
		public string Address { get; private set; }
		public QueueStatus Status { get; private set; }

		public QueueModel(string address, QueueStatus status)
		{
			Address = address;
			Status = status;
		}

		public void CompleteProgress()
		{
			Status = QueueStatus.End;
		}
	}

	public enum QueueStatus
	{
		Sending,
		Receiving,
		End
	}
}
