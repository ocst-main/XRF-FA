using System;  
using System.Net;  
using System.Net.Sockets;  
using System.Threading;  
using System.Text; 
using Innoit.FrameWork.Communication;
using System.Windows.Forms;

namespace XRF_FA
{


    public class SocketClient
    {

        private const int port = 701;
        private const int port_X2 = 1904;

        private static ManualResetEvent connectDone =
                new ManualResetEvent(false);
        private static ManualResetEvent sendDone =
                new ManualResetEvent(false);

        private static ManualResetEvent connectDone_X2 =
                new ManualResetEvent(false);
        private static ManualResetEvent sendDone_X2 =
                new ManualResetEvent(false);

        //public static bool ConnectFlag = false;

        public static void Connect(EndPoint remoteEP, Socket client)
        {

            client.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback), client);

            //connectDone.WaitOne();

            bool success = connectDone.WaitOne(3000);

            if (!success)
            {
                // NOTE, MUST CLOSE THE SOCKET
                client.Close();
                MessageBox.Show("SuperQ Measure 프로그램을 실행한후에 다시 진행하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                MainForm.ConnectFlag = false;
                return;
            }
            MainForm.ConnectFlag = true;
        }

        public static bool Connect_X2(EndPoint remoteEP, Socket client)
        {

            client.BeginConnect(remoteEP,
                new AsyncCallback(ConnectCallback_X2), client);

            //connectDone_X2.WaitOne();
            bool success = connectDone_X2.WaitOne(3000);

            if (!success)
            {
                // NOTE, MUST CLOSE THE SOCKET
                client.Close();
                MessageBox.Show("BRUKER Meas8m Measure 프로그램을 실행한후에 다시 진행하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //MainForm.ConnectFlag = false;
                return false;
            }
            //MainForm.ConnectFlag = true;
            return true;

        }

        private static void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete the connection.
                client.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void ConnectCallback_X2(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client_X2 = (Socket)ar.AsyncState;

                // Complete the connection.
                client_X2.EndConnect(ar);

                Console.WriteLine("Socket connected to {0}",
                    client_X2.RemoteEndPoint.ToString());

                // Signal that the connection has been made.
                connectDone_X2.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static void Send(Socket client, String data)
        {
            byte[] byteData = null;
            if (data.Length > 1)
            {
                byteData = Innoit.FrameWork.Communication.PaNalyticalXRF.SendMessageMake(data);  
            }
            else
            {
                byteData = Encoding.ASCII.GetBytes(data);
            }

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, SocketFlags.None,
                new AsyncCallback(SendCallback), client);
        }

        public static void Send_X2(Socket client, String data)
        {
            byte[] byteData = null;
            byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            client.BeginSend(byteData, 0, byteData.Length, SocketFlags.None,
                new AsyncCallback(SendCallback_X2), client);
        }

        private static void SendCallback(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private static void SendCallback_X2(IAsyncResult ar)
        {
            try
            {
                // Retrieve the socket from the state object.
                Socket client = (Socket)ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);

                // Signal that all bytes have been sent.
                sendDone_X2.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        public static bool SocketConnectCheck(Socket client)
        {
            //if (!ConnectFlag) return false;
            if (client == null) return false;

            bool blockingState = client.Blocking;
            try
            {
                byte[] tmp = new byte[1];

                client.Blocking = false;
                client.Send(tmp, 0, 0);
                Console.WriteLine("Connected!");
            }
            catch (SocketException e)
            {
                // 10035 == WSAEWOULDBLOCK
                if (e.NativeErrorCode.Equals(10035))
                {
                    MessageBox.Show("Still Connected, but the Send would block");
                    return false;
                }
                else
                {
                    MessageBox.Show("Disconnected: error code {0}!"+ e.NativeErrorCode.ToString());
                    return false;
                }
            }
            finally
            {
                if(client.Blocking == false) 
                        client.Blocking = true;
            }

            return true;
        }

    }

    public class StateObject
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 300;   //263;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder m_Response = null;

        public static bool Recv_ENQ = false;   // true면 ENQ 받은 상태
        public static bool Recv_ACK = false;   // true면 ACK 받은 상태
        public static bool Recv_NAK = false;   // true면 NAK 받은 상태
        public static bool Recv_MSG = false;   // true면 Message 받은 상태

        public static bool Send_ENQ = false;   // true면 ENQ 보낸 상태
        public static bool Send_ACK = false;   // true면 ACK 보낸 상태
        public static bool Send_NAK = false;   // true면 NAK 보낸 상태
        public static bool Send_MSG = false;   // true면 Message 보낸 상태

    }

    // for BRUKER
    public class StateObject_X2
    {
        // Client socket.
        public Socket workSocket = null;
        // Size of receive buffer.
        public const int BufferSize = 300;   //263;
        // Receive buffer.
        public byte[] buffer = new byte[BufferSize];
        // Received data string.
        public StringBuilder m_Response = null;
    }

}
