// Copyright (c) Petabridge <https://petabridge.com/>. All rights reserved.
// Licensed under the Apache 2.0 license. See LICENSE file in the project root for full license information.
// See ThirdPartyNotices.txt for references to third party code used inside Helios.

using System;
using System.Net;
using System.Text;
using System.Windows.Forms;
using Helios.Exceptions;
using Helios.Net;
using Helios.Net.Bootstrap;
using Helios.Topology;

namespace Helios.RawUdpSocket
{
    public partial class SocketClient : Form
    {
        public IConnection Connection;
        public INode RemoteHost;

        public SocketClient()
        {
            InitializeComponent();
            Connection =
                new ClientBootstrap()
                    .SetTransport(TransportType.Udp)
                    .RemoteAddress(Node.Loopback())
                    .OnConnect(ConnectionEstablishedCallback)
                    .OnReceive(ReceivedDataCallback)
                    .OnDisconnect(ConnectionTerminatedCallback)
                    .Build().NewConnection(NodeBuilder.BuildNode().Host(IPAddress.Any).WithPort(10001), RemoteHost);
            Connection.OnError += ConnectionOnOnError;
            Connection.Open();
        }


        private void AppendStatusText(string text)
        {
            txtOutput.Text += Environment.NewLine + text;
        }

        public bool AttemptConnect(string host, string portStr)
        {
            try
            {
                var port = int.Parse(portStr);
                RemoteHost = NodeBuilder.BuildNode().Host(host).WithPort(port).WithTransportType(TransportType.Udp);

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
                AppendStatusText(Encoding.UTF8.GetString(incomingData.Buffer));
            }));
        }

        private void ConnectionOnOnError(Exception exception, IConnection connection)
        {
            Invoke((Action) (() =>
            {
                AppendStatusText(string.Format("Exception {0} sending data to {1}", exception.Message,
                    connection.RemoteHost));
                AppendStatusText(exception.StackTrace);
            }));
        }

        private void ConnectionEstablishedCallback(INode remoteAddress, IConnection responseChannel)
        {
            Invoke((Action) (() =>
            {
                AppendStatusText(string.Format("Connected to {0}", remoteAddress));
                responseChannel.BeginReceive();
                tsStatusLabel.Text = string.Format("Connected to {0}", remoteAddress);
                btnSend.Enabled = true;
                tbSend.Enabled = true;
            }));
        }

        private void btnSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(tbHost.Text) || string.IsNullOrEmpty(tbPort.Text))
            {
                AppendStatusText("Must supply valid host and port before attempting to connect");
                return;
            }
            var didConnect = AttemptConnect(tbHost.Text, tbPort.Text);
            if (!didConnect)
            {
                AppendStatusText("Invalid host and port - unable to attempt connection");
                return;
            }

            if (!string.IsNullOrEmpty(tbSend.Text))
            {
                AppendStatusText(string.Format("Attempting to send: {0}", tbSend.Text));
                var bytes = Encoding.ASCII.GetBytes(tbSend.Text);
                var networkData = NetworkData.Create(RemoteHost, bytes, bytes.Length);
                Connection.Send(networkData);
            }
        }
    }
}