using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Linq;
using System.Windows.Forms;
using System.Net;   //匯入網路通訊相關協定函數
using System.Net.Sockets;  //匯入網路插座功能函數
using System.Threading;  //匯入多執行緒功能函數
using System.Collections;  //匯入集合物件

namespace WindowsFormsApp1
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

        TcpListener Server;  //總機
        Socket Client;   //分機
        Thread Th_Svr;   //總機監聽用執行緒
        Thread Th_Clt;   //分機通話用執行緒
        Hashtable HT = new Hashtable();   //客戶連線物件集合(類似陣列)
                                          //key:name, value:socket


        //開啟server:用server thread監聽client連線請求
        private void button1_click(object sender, EventArgs e)
        {
            //允許跨執行緒存取變數
            CheckForIllegalCrossThreadCalls = false;
            Thread Th_srv = new Thread(ServerSub);
            Th_srv.IsBackground = true;   //設定為背景執行緒
            Th_srv.Start();
            button1.Enabled = false;  //不能重複啟用伺服器
        }

        //接收客戶連線要求的程式，每個客戶有獨立執行緒
        private void ServerSub()
        {
            //Server IP和Port
            IPEndPoint EP = new IPEndPoint(IPAddress.Parse(textBox1.Text),
                                                                      int.Parse(textBox2.Text));
            Server = new TcpListener(EP);   //建立伺服器端監聽器(總機)
            Server.Start(100);                       //允許最多連線100人
            while (true)
            {
                Client = Server.AcceptSocket();     //建立此客戶的連線物件Client
                Thread Thr_Clt = new Thread(Listen);       //建立獨立監聽執行緒
                Thr_Clt.IsBackground = true;   //設定為背景執行緒
                Thr_Clt.Start();
            }
        }

        //監聽客戶一般訊息的程式
        private void Listen()
        {
            Socket Sck = Client;    //複製Client通訊物件
            Thread Th = Th_Clt;    //複製執行緒到區域變數Th
            while (true)                    //持續監聽
            {
                try                          //用Sck接收此客戶端訊息
                {
                    byte[] B = new Byte[1023];   //建立一個位元組陣列用來當做容器，   
                                                 //去承接客戶端傳送過來的資料
                    int length = Sck.Receive(B);  //length是接受訊息的長度(byte數) 
                    string Msg = Encoding.UTF8.GetString(B, 0, length);
                    //把位元組資料翻譯成一個字串
                    //UTF-8也可改Default
                    string Cmd = Msg.Substring(0, 1); //取出命令碼第一個字
                    string Str = Msg.Substring(1);    //取出命令碼後的訊息

                    switch (Cmd)
                    {
                        case "0":                     //有使用者上線，加入名單
                            HT.Add(Str, Sck);         //連線加入雜湊表，key:user(使用者), value:socket(連線物件)
                            listBox1.Items.Add(Str);
                            break;

                        case "9":
                            HT.Remove(Str);           //移除使用者名稱為name的連線物件
                            listBox1.Items.Remove(Str);  //自上線者名單移除
                            Th.Abort();                  //關閉此客戶執行緒
                            break;
                    }
                }
                catch (Exception)
                {
                    //有錯誤時忽略
                }
            }
        }

        //關閉視窗時
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.ExitThread();   //關閉所有執行緒
        }
    }
}



