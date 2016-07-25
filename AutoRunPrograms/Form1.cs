using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;
using System.Diagnostics;
using System.IO;
using System.Web.Script.Serialization;//引入命名空间,提供用于自定义序列化行为的扩展性功能


namespace AutoRunPragrams
{
    public partial class Form1 : Form
    {
        string ConfigFile;
        public Form1()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Interval = 1000;
            int i = 0;
            timer1.Tick += (ss,ee) => {
                label1.Text = "当前启动" + i + "秒，180秒之后启动添加程序";
            };           

            ConfigFile = Environment.GetFolderPath(Environment.SpecialFolder.CommonDocuments) + "/AutoRunPrograms.cfg";
            List<FileItem> list= Read();
            try
            {
                //添加启动
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Run", true);
                key.SetValue("自动启动", Application.ExecutablePath.ToString());

                if (list != null && list.Count > 0)
                {
                    Thread th = new Thread(delegate ()
                    {
                        Thread.Sleep(1000 * 60 * 3);
                        
                        foreach (FileItem item in list)
                        {
                            Process.Start(item.name);
                        }

                        Thread.Sleep(1000 * 60 * 2);
                        Environment.Exit(0);
                    });
                    th.Start();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }           
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Multiselect = false;
            if(dialog.ShowDialog()== DialogResult.OK)
            {
                listboxPrograms.Items.Add(dialog.FileName);
            }
            Save();
        }



        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (listboxPrograms.SelectedIndex >= 0)
            {
                listboxPrograms.Items.RemoveAt(listboxPrograms.SelectedIndex);
            }
            Save();
        }

        //保存列表
        void Save() {
            List<FileItem> list = new List<FileItem>();
            foreach(string s in listboxPrograms.Items)
            {
                list.Add(new FileItem { name = s });
            }

            //AJAX的应用程序提供序列化和反序列化功能
            JavaScriptSerializer js = new JavaScriptSerializer();
            //将对象转换为JSON格式字符串 
            var json = js.Serialize(list);
            File.WriteAllText(ConfigFile, json);
        }

        //读取
        List<FileItem> Read() {
            try
            {
                string cfg = File.ReadAllText(ConfigFile);
                JavaScriptSerializer js = new JavaScriptSerializer();
                //将对象转换为JSON格式字符串 
                List<FileItem> list = js.Deserialize<List<FileItem>>(cfg);
                foreach (FileItem item in list)
                {
                    listboxPrograms.Items.Add(item.name);
                }
                return list;
            }catch(Exception ex)
            {
                return null;
            }
            
        }

        private void btnStart_Click(object sender, EventArgs e)
        {
            foreach (string item in listboxPrograms.Items)
            {
                Process.Start(item);
            }
        }
    }
}
