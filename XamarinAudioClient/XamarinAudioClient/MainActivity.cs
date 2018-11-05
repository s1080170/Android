using System;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;

namespace XamarinAudioClient
{
    [Activity(Label = "@string/app_name", MainLauncher = true, LaunchMode = Android.Content.PM.LaunchMode.SingleTop, Icon = "@drawable/icon")]
    public class MainActivity : AppCompatActivity
    {
        TcpClient tcpClient; // Creates a TCP Client
        UdpClient udpClient; // Creates a UDP Client
        NetworkStream tcpStream; //Creats a NetworkStream (used for sending and receiving data)
        //NetworkStream udpStream; //Creats a NetworkStream (used for sending and receiving data)
        Thread audioControlThread;
        Thread audioRecordThread;
        Thread audioTrackThread;

        bool isAudioTxAvailable;
        bool isAudioRxAvailable;
        bool isAudioControlAvailable;

        AudioInterface audioInterface;

        Button buttonConnect;
        EditText editTextIpAddress;
        EditText editTextTcpPort;
        EditText editTextUdpPort;

        Button buttonSendData;
        EditText editTextSendData;

        Button buttonLogClear;
        TextView textViewTcpLog;
        TextView textViewUdpLog;
        TextView textViewLog;
        ScrollView scrollView;

        Button buttonTx;
        Button buttonRx;
        Button buttonPlayRec;

        /*********************************************************************************
         *
         * 
         *********************************************************************************/
        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.main);

            // Get our button from the layout resource,
            // and attach an event to it
            buttonConnect = FindViewById<Button>(Resource.Id.buttonConnect);
            editTextIpAddress = FindViewById<EditText>(Resource.Id.editTextIpAddress);
            editTextTcpPort = FindViewById<EditText>(Resource.Id.editTextTcpPort);
            editTextUdpPort = FindViewById<EditText>(Resource.Id.editTextUdpPort);

            buttonSendData = FindViewById<Button>(Resource.Id.buttonSendData);
            editTextSendData = FindViewById<EditText>(Resource.Id.editTextSendData);

            buttonLogClear = FindViewById<Button>(Resource.Id.buttonLogClear);
            textViewTcpLog = FindViewById<TextView>(Resource.Id.textViewTcpLog);
            textViewUdpLog = FindViewById<TextView>(Resource.Id.textViewUdpLog);
            textViewLog = FindViewById<TextView>(Resource.Id.textViewLog);
            scrollView = FindViewById<ScrollView>(Resource.Id.scrollViewLog);
            
            buttonConnect.Click += ButtonConnect_Click;
            buttonSendData.Click += ButtonSendData_Click;
            buttonLogClear.Click += ButtonLogClear_Click;

            audioInterface = new AudioInterface();

            buttonTx = FindViewById<Button>(Resource.Id.buttonTx);
            buttonRx = FindViewById<Button>(Resource.Id.buttonRx);
            buttonPlayRec = FindViewById<Button>(Resource.Id.buttonPlayRec);

            buttonTx.Click += ButtonTx_Click;
            buttonRx.Click += ButtonRx_Click;
            buttonPlayRec.Click += ButtonPlayRec_Click;

            buttonSendData.Enabled = false;
            //buttonTx.Enabled = false;
            //buttonRx.Enabled = false;

        }

        /*********************************************************************************
         *
         * 
         *********************************************************************************/
        void ButtonLogClear_Click(object sender, EventArgs e)
        {
            textViewLog.Text = "";
        }

        /*********************************************************************************
         *
         * 
         *********************************************************************************/
        bool isAudioControlThreadRun;
        void ButtonConnect_Click(object sender, EventArgs e)
        {
            try
            {
                if (buttonConnect.Text == "Connect")
                {

                    tcpClient = new TcpClient();

                    Task tcpConnectTask = tcpClient.ConnectAsync(editTextIpAddress.Text, Convert.ToInt32(editTextTcpPort.Text));
                    if (!tcpConnectTask.Wait(5000))
                    {
                        tcpClient.Close();
                        throw new SocketException(10060);
                    }

                    //Toast.MakeText(this, "Connected", ToastLength.Short).Show();
                    buttonConnect.Text = "Dis-Connect";

                    tcpStream = tcpClient.GetStream();

                    textViewTcpLog.Text = "Audio control connect audio server.";
                    buttonSendData.Enabled = true;
                    //buttonTx.Enabled = true;
                    //buttonRx.Enabled = true;

                    isAudioControlThreadRun = true;
                    ClientTcpReceive(); //Starts Receiving When Connected
                }
                else
                {
                    //Toast.MakeText(this, "Dis-Connected", ToastLength.Short).Show();
                    buttonConnect.Text = "Connect";
                    textViewTcpLog.Text = "Audio control disconnect audio server.";

                    isAudioControlThreadRun = false;
                    audioControlThread.Join();

                    buttonSendData.Enabled = false;

                    tcpStream.Close();
                    tcpClient.Close();
                }
            }
            catch (SocketException ex)
            {
                System.Console.WriteLine("buttonConnect_Click SocketException:" + ex.ToString());
            }
            catch (AggregateException ex)
            {
                System.Console.WriteLine("buttonConnect_Click AggregateException:" + ex.ToString());
            }
            catch (Exception ex)
            {
                System.Console.Write("buttonConnect_Click Exception:" + ex.ToString());
            }
        }

        /*********************************************************************************
         *
         * 
         *********************************************************************************/
        private void ClientTcpReceive()
        {
            Int32 resSize = 0;
            Byte[] resBytes = new Byte[1];

            audioControlThread = new Thread(() => // Thread (like Timer)
            {
                try
                {
                    while (true)
                    {
                        if ( isAudioControlThreadRun == false )
                        {
                            System.Console.WriteLine("AudioControlTask get CancellationRequested");
                            break;
                        }

                        if ( tcpStream.DataAvailable )
                        {
                            resSize = tcpStream.Read(resBytes, 0, resBytes.Length);

                            if (resSize == 1)
                            {
                                RunOnUiThread(() => textViewLog.Text = textViewLog.Text 
                                     + String.Format("TCP RX n={0} d=0x{1}\n", resSize, resBytes[0].ToString("X2")));
                                System.Console.Write("TCP RX n={0} d=0x{1}\n", resSize, resBytes[0].ToString("X2"));

                                if (resBytes[0] == 0x31)
                                {
                                    isAudioTxAvailable = true;
                                    RunOnUiThread(() => textViewUdpLog.Text = "Get audio record start ack");
                                    RunOnUiThread(() => textViewLog.Text    = "Get audio record start ack");
                                }
                                else if (resBytes[0] == 0x32)
                                {
                                    isAudioRxAvailable = true;
                                    RunOnUiThread(() => textViewUdpLog.Text = "Get audio track start ack");
                                    RunOnUiThread(() => textViewLog.Text    = "Get audio track start ack");
                                }
                                else if (resBytes[0] == 0x33)
                                {
                                    RunOnUiThread(() => textViewLog.Text = textViewLog.Text + "Get audio record/track stop ack\n");
                                }
                                else if (resBytes[0] == 0x34)
                                {
                                    isAudioControlAvailable = true;
                                    RunOnUiThread(() => textViewLog.Text = textViewLog.Text + "Get audio control live check ack\n");
                                }
                                else
                                {
                                    RunOnUiThread(() => textViewLog.Text = textViewLog.Text + "Get non command\n");
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Console.WriteLine(ex.Message);
                }
            });
            audioControlThread.Start(); // Start the Thread
        }

        /*********************************************************************************
         *
         * 
         *********************************************************************************/
        private void ClientUdpReceive()
        {
        }

        /*********************************************************************************
         *
         * 
         *********************************************************************************/
        void ButtonSendData_Click(object sender, EventArgs e)
        {
            Byte[] data = Encoding.Default.GetBytes(editTextSendData.Text);
            if (data == null)
            {
                return;
            }

            SendTcpData(data);

        }

        void SendTcpData(Byte[] data)
        {
            if (data == null)
            {
                return;
            }

            try
            {
                if (tcpClient.Connected) // if the client is connected
                {
                    tcpStream.Write(data, 0, data.Length); //Sends the real data
                }
            }
            catch (Exception ex)
            {
                System.Console.WriteLine(ex.Message);
            }
        }

        /*********************************************************************************
         *
         * 
         *********************************************************************************/
        void ButtonPlayRec_Click(object sender, EventArgs e)
        {
            if (buttonPlayRec.Text == "再生")
            {
                buttonPlayRec.Text = "再生中";

                audioInterface.TestPlay();
            }
            else
            {
                buttonPlayRec.Text = "再生";

                audioInterface.TestPlayStop();
            }
        }

        /*********************************************************************************
         *
         * 
         *********************************************************************************/
        bool isAudioRecordThreadRun;
        void ButtonTx_Click(object sender, EventArgs e)
        {
            if ( buttonTx.Text == "送話" )
            {
                Byte[] cmd = { 0x31 };
                SendTcpData(cmd);

                buttonTx.Text = "送話中";
                isAudioRecordThreadRun = true;

                audioInterface.ButtonRec_Click(sender, e);

                audioRecordThread = new Thread(() =>
                {
                    IPAddress remoteIp = IPAddress.Parse(editTextIpAddress.Text);
                    Int32 port = Convert.ToInt32(editTextUdpPort.Text);
                    IPEndPoint remoteEp = new IPEndPoint(remoteIp, port);

                    udpClient = new UdpClient();
                    udpClient.Connect(remoteEp);

                    while (true)
                    {
                        if ( isAudioRecordThreadRun == false )
                        {
                            break;
                        }

                        try
                        {
                            Byte[] data = AudioInterface.RecordBuffer.Instance.Dequeue();
                            if (data != null)
                            {
                                udpClient.Send(data, data.Length);

                                //RunOnUiThread(() => textViewLog.Text = textViewLog.Text 
                                //     + String.Format("UDP TX n={0} d={1}\n", data.Length.ToString(), data[0].ToString()));
                                System.Console.Write("UDP TX n={0} d={1}\n", data.Length.ToString(), data[0].ToString());
                            }
                        }
                        catch (Exception ex)
                        {
                            System.Console.WriteLine(ex.Message);
                        }
                    }
                });
                audioRecordThread.Start();

            }
            else
            {
                buttonTx.Text = "送話";
                isAudioRecordThreadRun = false;

                audioInterface.ButtonRecStop_Click(sender, e);

                audioRecordThread.Join();

                udpClient.Close();
            }
        }

        /*********************************************************************************
         *
         * 
         *********************************************************************************/
        bool isAudioTrackThreadRun;
        void ButtonRx_Click(object sender, EventArgs e)
        {
            if(buttonRx.Text == "受話")
            {

                Byte[] cmd = { 0x32 };
                SendTcpData(cmd);

                buttonRx.Text = "受話中";
                isAudioTrackThreadRun = true;

                audioInterface.ButtonPlay_Click(sender, e);

                audioTrackThread = new Thread(() =>
                {
                    try
                    {
                        IPEndPoint localEp = new IPEndPoint(IPAddress.Any, 4001);
                        udpClient = new UdpClient(localEp);

                        while (true)
                        {
                            if (isAudioTrackThreadRun == false)
                            {
                                break;
                            }

                            if (udpClient.Available > 0)
                            {
                                IPEndPoint remoteEp = null;
                                Byte[] resBytes = udpClient.Receive(ref remoteEp);

                                RunOnUiThread(() => textViewLog.Text = textViewLog.Text 
                                     + String.Format("UDP RX n={0} d={1}\n", resBytes.Length.ToString(), resBytes[0].ToString()));
                                System.Console.Write("UDP RX n={0} d={1}\n", resBytes.Length.ToString(), resBytes[0].ToString());

                                audioInterface.PushData(resBytes);
                                //AudioInterface.TrackBuffer.Instance.Enqueue(resBytes);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Console.WriteLine(ex.Message);
                    }
                });
                audioTrackThread.Start();

            }
            else
            {
                buttonRx.Text = "受話";
                isAudioTrackThreadRun = false;

                audioInterface.ButtonPlayStop_Click(sender, e);

                audioTrackThread.Join();

                udpClient.Close();

                //AudioInterface.TrackBuffer.Instance.WriteFile();

            }
        }
    }
}


