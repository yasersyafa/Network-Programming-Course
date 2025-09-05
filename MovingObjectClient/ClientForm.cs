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

namespace MovingObjectClient
{
    public partial class ClientForm : Form
    {
        #region Drawing Rectangle Section
        Pen red = new Pen(Color.Red);
        Rectangle rect = new Rectangle(20, 20, 30, 30);
        SolidBrush fillBlue = new SolidBrush(Color.Blue);
        #endregion

        private Socket m_clientSocket;
        private byte[] m_buffer;
        public ClientForm()
        {
            InitializeComponent();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            ConnectToServer();
        }

        private void ConnectToServer()
        {
            try
            {
                m_clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                var endPoint = new IPEndPoint(IPAddress.Loopback, 3333);
                m_clientSocket.BeginConnect(endPoint, ConnectCallback, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to connect: {ex.Message}");
            }
        }

        private void ConnectCallback(IAsyncResult AR)
        {
            try
            {
                m_clientSocket.EndConnect(AR);
                m_buffer = new byte[m_clientSocket.ReceiveBufferSize];
                m_clientSocket.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"failed to connect callback: {ex.Message}");
            }
        }

        private void ReceiveCallback(IAsyncResult AR)
        {
            try
            {
                int received = m_clientSocket.EndReceive(AR);
                if (received == 0) return;

                string message = Encoding.ASCII.GetString(m_buffer, 0, received);
                if(message.StartsWith("Hello"))
                {
                    this.Invoke((Action)(() => this.Text = message));
                }
                else
                {
                    ObjectPackage obj = new ObjectPackage(m_buffer);
                    rect.X = obj.X;
                    rect.Y = obj.Y;
                    this.Invoke((Action)((() => this.Invalidate())));
                }

                m_clientSocket.BeginReceive(m_buffer, 0, m_buffer.Length, SocketFlags.None, ReceiveCallback, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Receive error: {ex.Message}");
            }
        }

        private void ClientForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.DrawRectangle(red, rect);
            g.FillRectangle(fillBlue, rect);
        }
    }
}
