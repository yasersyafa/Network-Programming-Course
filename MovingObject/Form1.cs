using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MovingObject
{
    public partial class Form1 : Form
    {
        private Socket _serverSocket;
        List<Socket> clients = new List<Socket>();
        Pen red = new Pen(Color.Red);
        Rectangle rect = new Rectangle(20, 20, 30, 30);
        SolidBrush fillBlue = new SolidBrush(Color.Blue);
        int slide = 10; 

        public Form1()
        {
            InitializeComponent();
            timer1.Interval = 50;
            timer1.Enabled = true;
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            StartServer();
        }

        private void StartServer()
        {
            try
            {
                _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _serverSocket.Bind(new IPEndPoint(IPAddress.Any, 3333));
                _serverSocket.Listen(10);
                _serverSocket.BeginAccept(AcceptCallback, null);

                this.Text = "Server running on 3333 port";
            }
            catch(Exception ex)
            {
                MessageBox.Show($"Failed to start server: {ex.Message}");
            }
        }

        private void AcceptCallback(IAsyncResult AR)
        {
            try
            {
                Socket client = _serverSocket.EndAccept(AR);
                clients.Add(client);

                string message = $"Hello client #{clients.Count}";
                byte[] data = Encoding.ASCII.GetBytes(message);
                client.BeginSend(data, 0, data.Length, SocketFlags.None, null, null);

                _serverSocket.BeginAccept(AcceptCallback, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to accept: {ex.Message}");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            back();

            rect.X += slide;
            Invalidate();

            ObjectPackage obj = new ObjectPackage(rect.X, rect.Y);
            byte[] data = obj.ToByteArray();

            List<Socket> deadClients = new List<Socket>();
            foreach (Socket client in clients)
            {
                try
                {
                    client.BeginSend(data, 0, data.Length, SocketFlags.None, null, null);
                } 
                catch
                {
                    deadClients.Add(client);
                }
            }

            foreach(var dead in deadClients)
            {
                clients.Remove(dead);
            }
        }

        private void back()
        {
            if (rect.X >= this.Width - rect.Width * 2)
                slide = -10;
            else
            if (rect.X <= rect.Width / 2)
                slide = 10;
        }

        private void Form1_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            g.DrawRectangle(red, rect);
            g.FillRectangle(fillBlue, rect);
        }
    }
}
