using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace cb_viewer
{
    public partial class StreamLinker : Form
    {
        BindingList<Task> _tasks = new BindingList<Task>();

        private class CbItem
        {
            public string DisplayName;
            public string ArgumentName;
            public int Value;
            public CbItem(string displayName, string argumentName, int value)
            {
                DisplayName = displayName;
                ArgumentName = argumentName;
                Value = value;
            }

            public override string ToString()
            {
                return DisplayName;
            }
        }

        public StreamLinker()
        {
            InitializeComponent();

            comboBox1.Items.Add(new CbItem("Chaturbate", "chaturbate.com/", 1));
            comboBox1.SelectedIndex = 0;

            dataGridView1.DataSource = _tasks;
            dataGridView1.Columns[0].HeaderText = "Task ID";
            dataGridView1.AutoResizeColumn(0); 
        }

        private void button1_Click(object sender, EventArgs e)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.CreateNoWindow = true;
            startInfo.UseShellExecute = false;
            startInfo.FileName = "D:/my-work/streamlink/Streamlink.exe";
            startInfo.WindowStyle = ProcessWindowStyle.Hidden;
            CbItem plugin = (CbItem)comboBox1.SelectedItem;
            string arguments = plugin.ArgumentName + textBox1.Text + " 720p";
            Debug.WriteLine(">>> " + arguments);
            startInfo.Arguments = arguments;

            //startSync();
            Task task = startAsync(startInfo);            
            _tasks.Add(task);
            task.ContinueWith((antecedent) => {                
                dataGridView1.Invoke((MethodInvoker)delegate
                {
                    _tasks.Remove(task);
                    dataGridView1.Update();
                    dataGridView1.Refresh();
                    dataGridView1.Parent.Refresh();
                });
            });
        }

        private async Task startAsync(ProcessStartInfo startInfo)
        {
            await Task.Run(() => startSync(startInfo));
        }

        private void startSync(ProcessStartInfo startInfo)
        {
            try
            {
                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                }
            }
            catch (Win32Exception ex)
            {
                if (ex.NativeErrorCode == 2) //  ERROR_FILE_NOT_FOUND 
                {
                    String msg = ex.Message + ". Check the path.";
                    MessageBox.Show(msg, "Validation",
                        MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }
    }
}
