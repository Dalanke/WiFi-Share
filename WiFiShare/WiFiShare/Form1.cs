using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using NETCONLib;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace WiFiShare
{
    public partial class Form1 : Form
    {
        string sc = "";
        //string cs = "";
        public Form1()
        {
            InitializeComponent();
        }
        public static string CmdExecute(string command) //Cmd 执行
        {
            string output = ""; //输出字符串  
            if (command != null && !command.Equals(""))
            {
                Process process = new Process();//创建进程对象  
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = @"c:\windows\system32\cmd.exe";//设定需要执行的命令  
                startInfo.Arguments = "/C " + command;//“/C”表示执行完命令后马上退出  
                startInfo.UseShellExecute = false;//不使用系统外壳程序启动  
                startInfo.RedirectStandardInput = false;//不重定向输入  
                startInfo.RedirectStandardOutput = true; //重定向输出  
                startInfo.CreateNoWindow = true;//不创建窗口  
                startInfo.Verb = "runas";
                process.StartInfo = startInfo;

                try
                {
                    if (process.Start())//开始进程  
                    {
                        output = process.StandardOutput.ReadToEnd();//读取进程的输出  
                    }
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if (process != null)
                        process.Close();
                }
            }
            return output;
        }

        private void button1_Click(object sender, EventArgs e) //创建
        {
            if (textBox1.Text == "" || textBox2.Text == "")
            {
                MessageBox.Show("账号密码不可以为空");
                textBox1.Focus();
            }
            else if (textBox2.Text.Length < 8)
                MessageBox.Show("密码长度不小于八位");
            else
            {
                string command = "netsh wlan set hostednetwork mode=allow ssid=" + textBox1.Text + " key=" + textBox2.Text;
                string str2 = CmdExecute(command);
                MessageBox.Show(str2);
                if (((str2.IndexOf("承载网络模式已设置为允许") > -1) && (str2.IndexOf("已成功更改承载网络的 SSID。") > -1)) && (str2.IndexOf("已成功更改托管网络的用户密钥密码。") > -1))
                {
                    listBox1.Items.Add(textBox1.Text + "   " + "新建共享网络成功  " + DateTime.Now.ToString("HH:mm:ss"));
                }
                else
                {
                    listBox1.Items.Add(textBox1.Text + "   " + "搭建失败请重试  " + DateTime.Now.ToString("HH:mm:ss"));
                }
            }

        }

        private void button4_Click(object sender, EventArgs e)//禁止
        {
            string command = "netsh wlan set hostednetwork mode=disallow";
            if (CmdExecute(command).IndexOf("承载网络模式已设置为禁止") > -1)
            {
                listBox1.Items.Add(textBox1.Text + "   " + "禁止共享网络成功！  " + DateTime.Now.ToString("HH:mm:ss"));
            }
            else
            {
                listBox1.Items.Add(textBox1.Text + "   " + "禁止失败！  " + DateTime.Now.ToString("HH:mm:ss"));
            }

        }

        private void button2_Click(object sender, EventArgs e)//启动
        {
            if (CmdExecute("netsh wlan start hostednetwork").IndexOf("已启动承载网络") > -1)
            {
                listBox1.Items.Add("启动成功！  " + DateTime.Now.ToString("HH:mm:ss"));
            }
            else
            {
                listBox1.Items.Add("启动失败，请尝试新建网络  " + DateTime.Now.ToString("HH:mm:ss"));
            }
            //读取所有网络
            comboBox1.Items.Clear();
            NetSharingManager manager = new NetSharingManager();
            var connections = manager.EnumEveryConnection;
            foreach (INetConnection c in connections)
            {
                var props = manager.NetConnectionProps[c];            
                comboBox1.Items.Add(props.Name);
            }
            NetworkInterface[] Ninterface = NetworkInterface.GetAllNetworkInterfaces();//确定虚拟网络名称
            foreach (NetworkInterface  IN in Ninterface)
            {
                if (IN.Description== "Microsoft Hosted Network Virtual Adapter")
                {
                    sc = IN.Name;
                }
            }
            timer1.Enabled = true;
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (CmdExecute("netsh wlan stop hostednetwork").IndexOf("已停止承载网络") > -1)
            {
                listBox1.Items.Add("停止成功！  " + DateTime.Now.ToString("HH:mm:ss"));
            }
            else
            {
                listBox1.Items.Add("停止失败！  " + DateTime.Now.ToString("HH:mm:ss"));
            }
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                CmdExecute("netsh wlan stop hostednetwork");
                StreamWriter sw = new StreamWriter("Logs.txt", false);// textbox 记忆功能
                sw.WriteLine(textBox1.Text);
                sw.WriteLine(textBox2.Text);
                sw.Flush();
                sw.Close();
            }
            catch (Exception)
            {
                MessageBox.Show("应用程序退出但承载网络可能未停止");
            }
        }

        private void button5_Click(object sender, EventArgs e)  //网络共享设置
        {
            if (comboBox1.SelectedItem == null)
            {
                MessageBox.Show("请先选择电脑连接到的网络！");
            }
            else
            {
                string connectionToShare = comboBox1.SelectedItem.ToString(); // 被共享的网络连接
                string sharedForConnection = sc; // 共享的家庭网络连接

                var manager = new NetSharingManager();
                var connections = manager.EnumEveryConnection;

                foreach (INetConnection c in connections)
                {
                    try
                    {
                        var props = manager.NetConnectionProps[c];
                        var sharingCfg = manager.INetSharingConfigurationForINetConnection[c];
                        if (props.Name == connectionToShare)
                        {
                            sharingCfg.EnableSharing(tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PUBLIC);
                            if (sharingCfg.SharingEnabled == true)
                                listBox1.Items.Add("已设置"+props.Name+"用于共享");
                        }
                        else if (props.Name == sharedForConnection)
                        {
                            sharingCfg.EnableSharing(tagSHARINGCONNECTIONTYPE.ICSSHARINGTYPE_PRIVATE);
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("设置共享失败");
                        throw;  
                    }  
                }
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.Visible = true;
            this.WindowState = FormWindowState.Normal;
            notifyIcon1.Visible = false;
        }

        private void Form1_SizeChanged(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                this.Hide();
                this.notifyIcon1.Visible = true;
                notifyIcon1.BalloonTipText = "已经最小化到这里了哦";
                notifyIcon1.ShowBalloonTip(5);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            if (File.Exists("Logs.txt"))
            {
                StreamReader sr = new StreamReader("Logs.txt");
                textBox1.Text = sr.ReadLine();
                textBox2.Text = sr.ReadLine();
                sr.Close();
            }  
        }
        private void Clients_Number()
        {
            string s="";
            int n = CmdExecute("netsh wlan show hostednetwork").IndexOf("客户端数      :");
            if(n>-1)
            s=CmdExecute("netsh wlan show hostednetwork").Substring(n+11,2);
            s = Regex.Replace(s, @"\D", "");
            toolStripStatusLabel1.Text = "蹭网的人有："+s+"个";
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            Clients_Number();
        }
    }
}
