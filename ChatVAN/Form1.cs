using System;
using BattleshipGameFORMS;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace ChatVAN
{
    public partial class Form1 : Form
    {
        private TcpListener tcpListener;
        private TcpClient tcpClient;
        private NetworkStream stream;
        private Thread listenThread;
        private Thread clientThread;

        public Form1()
        {
            InitializeComponent();
            button1.Click += Button1_Click;
            button2.Click += Button2_Click;
            textBox1.KeyDown += TextBox1_KeyDown;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            int port;
            if (int.TryParse(textBox2.Text, out port))
            {
                tcpListener = new TcpListener(IPAddress.Any, port);
                tcpListener.Start();
                listenThread = new Thread(new ThreadStart(ListenForClients));
                listenThread.IsBackground = true;
                listenThread.Start();
                tabControl1.Visible = false;
                DisplayMessage("Hosting on port " + port);
            }
            else
            {
                MessageBox.Show("Invalid port number.");
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            string ipAddress = textBox4.Text;
            int port;
            if (IPAddress.TryParse(ipAddress, out IPAddress ip) && int.TryParse(textBox3.Text, out port))
            {
                tcpClient = new TcpClient();
                clientThread = new Thread(() => ConnectToServer(ip, port));
                clientThread.IsBackground = true;
                clientThread.Start();
                tabControl1.Visible = false;
            }
            else
            {
                MessageBox.Show("Invalid IP address or port number.");
            }
        }

        private void TextBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                SendMessage();
                e.SuppressKeyPress = true; // Prevents the beep sound on enter
            }
        }

        private void ListenForClients()
        {
            while (true)
            {
                TcpClient client = tcpListener.AcceptTcpClient();
                stream = client.GetStream();
                Thread clientThread = new Thread(() => HandleClientComm(client));
                clientThread.IsBackground = true;
                clientThread.Start();
            }
        }

        private void HandleClientComm(TcpClient client)
        {
            NetworkStream clientStream = client.GetStream();
            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;
                try
                {
                    bytesRead = clientStream.Read(message, 0, 4096);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }

                string msg = Encoding.ASCII.GetString(message, 0, bytesRead);
                DisplayMessage(msg);
            }

            client.Close();
        }

        private void ConnectToServer(IPAddress ip, int port)
        {
            tcpClient.Connect(ip, port);
            stream = tcpClient.GetStream();
            Thread readThread = new Thread(ReadMessages);
            readThread.IsBackground = true;
            readThread.Start();
        }

        private void ReadMessages()
        {
            byte[] message = new byte[4096];
            int bytesRead;

            while (true)
            {
                bytesRead = 0;
                try
                {
                    bytesRead = stream.Read(message, 0, 4096);
                }
                catch
                {
                    break;
                }

                if (bytesRead == 0)
                {
                    break;
                }

                string msg = Encoding.ASCII.GetString(message, 0, bytesRead);
                DisplayMessage(msg);
            }
        }

        private void SendMessage()
        {
            string msg = textBox1.Text;
            if (!string.IsNullOrEmpty(msg) && stream != null)
            {
                byte[] buffer = Encoding.ASCII.GetBytes(msg);
                stream.Write(buffer, 0, buffer.Length);
                DisplayMessage("Me: " + msg);
                textBox1.Clear();
            }
        }

        private void DisplayMessage(string message)
        {
            if (flowLayoutPanel1.InvokeRequired)
            {
                flowLayoutPanel1.Invoke(new Action<string>(DisplayMessage), new object[] { message });
            }
            else
            {
                Label msgLabel = new Label();
                msgLabel.Text = message;
                msgLabel.AutoSize = true;
                flowLayoutPanel1.Controls.Add(msgLabel);
                flowLayoutPanel1.ScrollControlIntoView(msgLabel);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            Start startScene = new Start();
            startScene.Show();
            this.Hide();
        }
    }
}
