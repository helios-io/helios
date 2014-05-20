using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Helios.Exceptions;
using Helios.Net;
using Helios.Net.Bootstrap;
using Helios.Topology;

namespace Helios.RawSocket
{
    public partial class SocketClient : Form
    {
        public INode RemoteHost;
        public IConnection Connection;

        public SocketClient()
        {
            InitializeComponent();
        }

        #region UI handlers

        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbHost.Text) || string.IsNullOrEmpty(tbPort.Text))
            {
                AppendStatusText("Must supply valid host and port before attempting to connect");
            }
            else
            {
                var didConnect = AttemptConnect(tbHost.Text, tbPort.Text);
                if (!didConnect)
                {
                    AppendStatusText("Invalid host and port - unable to attempt connection");
                }
            }
        }

        private void AppendStatusText(string text)
        {
            txtOutput.Text += Environment.NewLine + text;
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(tbSend.Text))
            {
                AppendStatusText(string.Format("Attempting to send: {0}", tbSend.Text));
                var bytes = Encoding.UTF8.GetBytes(tbSend.Text);
                var networkData = NetworkData.Create(Connection.RemoteHost, bytes, bytes.Length);
                Connection.Send(networkData);
            }
        }

        private void btnDisconnect_Click(object sender, EventArgs e)
        {
            Connection.Close();
        }

        #endregion 

        #region Network Handlers

        public bool AttemptConnect(string host, string portStr)
        {
            try
            {
                var port = Int32.Parse(portStr);
                RemoteHost = NodeBuilder.BuildNode().Host(host).WithPort(port).WithTransportType(TransportType.Tcp);
                Connection =
                    new ClientBootstrap()
                        .SetTransport(TransportType.Tcp)
                        .RemoteAddress(RemoteHost)
                        .OnConnect(ConnectionEstablishedCallback)
                        .OnReceive(ReceivedDataCallback)
                        .OnDisconnect(ConnectionTerminatedCallback)
                        .Build().NewConnection(NodeBuilder.BuildNode().Host(IPAddress.Any).WithPort(10001), RemoteHost);
                Connection.Open();
                return true;
            }
            catch (Exception ex)
            {
                AppendStatusText(ex.Message);
                AppendStatusText(ex.StackTrace);
                AppendStatusText(ex.Source);
                return false;
            }
        }

        private void ConnectionTerminatedCallback(HeliosConnectionException reason, IConnection closedChannel)
        {
            Invoke((Action) (() =>
            {
                AppendStatusText(string.Format("Disconnected from {0}", closedChannel.RemoteHost));
                AppendStatusText(string.Format("Reason: {0}", reason.Message));
                tsStatusLabel.Text = string.Format("Disconnected from {0}", closedChannel.RemoteHost);
                btnConnect.Enabled = true;
                btnDisconnect.Enabled = false;
                btnSend.Enabled = false;
                tbSend.Enabled = false;
            }));
        }

        private void ReceivedDataCallback(NetworkData incomingData, IConnection responseChannel)
        {
            Invoke((Action) (() =>
            {
                AppendStatusText(string.Format("Received {0} bytes from {1}", incomingData.Length,
                    incomingData.RemoteHost));
            }));
        }

        private void ConnectionEstablishedCallback(INode remoteAddress, IConnection responseChannel)
        {
            Invoke((Action) (() =>
            {
                AppendStatusText(string.Format("Connected to {0}", remoteAddress));
                tsStatusLabel.Text = string.Format("Connected to {0}", remoteAddress);
                btnConnect.Enabled = false;
                btnDisconnect.Enabled = true;
                btnSend.Enabled = true;
                tbSend.Enabled = true;
            }));
        }

        #endregion

       

        
    }
}
