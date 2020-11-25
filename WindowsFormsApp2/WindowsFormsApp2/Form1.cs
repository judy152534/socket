using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;   //匯入網路通訊相關協定函數
using System.Net.Sockets;  //匯入網路插座功能函數
using System.Threading;  //匯入多執行緒功能函數
using System.Collections;  //匯入集合物件


namespace WindowsFormsApp2
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
        }

        Socket T;   //通訊物件
        string User;  //使用者

        //登入伺服器
        private void button1_Click(object sender, EventArgs e)
        {
            string IP = textBox1.Text;                                  //伺服器IP
            int Port = int.Parse(textBox2.Text);                        //伺服器Port
            IPEndPoint EP = new IPEndPoint(IPAddress.Parse(IP), Port);  //伺服器連線端點資訊

            //建立雙向連線
            T = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            User = textBox3.Text;   //使用者名稱
            try
            {
                T.Connect(EP);    //連上伺服器(撥號給總機)
                send("0" + User);
            }
            catch (Exception)
            {
                MessageBox.Show("無法連上伺服器");
                return;
            }
            button1.Enabled = false;  //連線按鈕失效，避免重複連線
        }

        //傳送訊息給server
        private void send(string Str)
        {
            byte[] B = Encoding.UTF8.GetBytes(Str);
            T.Send(B, 0, B.Length, SocketFlags.None);   //使用連線物件傳送資料
        }

        //關閉視窗代表離線登出
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (button1.Enabled == false)
            {
                send("9" + User);   //傳送自己的離線訊息給伺服器
                T.Close();
            }
        }
    }
}
