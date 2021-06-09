using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.Management;
using System.Dynamic;

namespace Task_Manager
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public void renderProcessesOnListView()
        {
            Process[] processList = Process.GetProcesses();
            ImageList Imagelist = new ImageList();

            foreach (Process process in processList)
            {

                string status = process.Responding == true ? "Responding" : "Not responding";
                dynamic extraProcessInfo = GetProcessExtraInformation(process.Id);
                string[] row = {
                    process.ProcessName,
                    process.Id.ToString(),
                    status,
                    extraProcessInfo.Username,
                    BytesToReadableValue(process.PrivateMemorySize64),
                    extraProcessInfo.Description
                };

                try
                {
                    Imagelist.Images.Add(
                        process.Id.ToString(),
                        Icon.ExtractAssociatedIcon(process.MainModule.FileName).ToBitmap()
                    );
                }
                catch { }

                ListViewItem item = new ListViewItem(row)
                {
                    ImageIndex = Imagelist.Images.IndexOfKey(process.Id.ToString())
                };

                listView1.BeginInvoke(new Action(() =>
                {
                    listView1.Items.Add(item);
                }));

            }
            listView1.BeginInvoke(new Action(() =>
            {
                listView1.LargeImageList = Imagelist;
                listView1.SmallImageList = Imagelist;
            }));


        }

        public string BytesToReadableValue(long number)
        {
            string[] suffixes = new string[] { " B", " KB", " MB", " GB", " TB", " PB" };

            for (int i = 0; i < suffixes.Length; i++)
            {
                long temp = number / (int)Math.Pow(1024, i + 1);

                if (temp == 0)
                {
                    return number / (int)Math.Pow(1024, i) + suffixes[i];
                }
            }

            return number.ToString() + suffixes[suffixes.Length];
        }

        public ExpandoObject GetProcessExtraInformation(int processId)
        {
            string query = "Select * From Win32_Process Where ProcessID = " + processId;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher(query);
            ManagementObjectCollection processList = searcher.Get();

            dynamic response = new ExpandoObject();
            response.Description = "";
            response.Username = "Unknown";

            foreach (ManagementObject obj in processList)
            {

                string[] argList = new string[] { string.Empty, string.Empty };
                int returnVal = Convert.ToInt32(obj.InvokeMethod("GetOwner", argList));
                if (returnVal == 0)
                {
                    response.Username = argList[0];
                }

                if (obj["ExecutablePath"] != null)
                {
                    try
                    {
                        FileVersionInfo info = FileVersionInfo.GetVersionInfo(obj["ExecutablePath"].ToString());
                        response.Description = info.FileDescription;
                    }
                    catch { }
                }
            }

            return response;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Task.Factory.StartNew(() =>
            {
                renderProcessesOnListView();
            });
        }
    }
}
