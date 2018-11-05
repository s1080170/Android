using System;
using System.Threading;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using Android.Support.V7.App;
using Android.Media;
using Java.IO;

namespace XamarinAudioClient
{
    public class AudioInterface
    {
        private AudioTrack mAudioTrack;
        private AudioRecord mAudioRecord;

        /*********************************************************************************
        *
        * 
        *********************************************************************************/
        public void TestPlay()
        {
            String recFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryMusic).Path;
            String recPath = recFolder + "/sample_rec.wav";

            System.Console.WriteLine(recPath);

            File file = new File(recPath);
            FileInputStream inputStream = new FileInputStream(file);

            // Streamモードで再生を行うので、リングバッファサイズを取得
            //TrackBuffer.Instance.Frames = AudioTrack.GetMinBufferSize(8000, ChannelOut.Mono, Encoding.Pcm8bit);
            // Xperia Z4 = 820
            TrackBuffer.Instance.Frames = 1024;

            // AudioTrackを生成する
            mAudioTrack = new AudioTrack(
                                            Stream.Music,
                                            8000,
                                            ChannelOut.Mono,
                                            Encoding.Pcm8bit,
                                            TrackBuffer.Instance.Frames,
                                            AudioTrackMode.Stream);

            // コールバックを指定
            //mAudioTrack.SetPlaybackPositionUpdateListener(new OnPlaybackPositionUpdateListener());

            //通知の発生するフレーム数を指定
            //mAudioTrack.SetPositionNotificationPeriod(TrackBuffer.Instance.Frames);

            Task.Run(() =>
            {

                System.Console.WriteLine("AudioTrack play a recording file");
                mAudioTrack.Play();

                Byte[] dat = new Byte[file.Length()];
                inputStream.Read(dat);
                mAudioTrack.Write(dat, 0, dat.Length);

            });
        }

        /*********************************************************************************
         *
         * 
         *********************************************************************************/
        public void TestPlayStop()
        {
            if (mAudioTrack == null)
            {
                return;
            }

            mAudioTrack.Stop();
        }

        /*********************************************************************************
        *
        * 
        *********************************************************************************/
        public void ButtonPlay_Click(object sender, EventArgs e)
        {
            String musicFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryMusic).Path;
            String filePath = musicFolder + "/sample_mono_8k8bit.wav";
            //String filePath = musicFolder + "/sample_stereo_44k16bit.wav";

            System.Console.WriteLine(filePath);

            File file = new File(filePath);
            FileInputStream inputStream = new FileInputStream(file);

            // Streamモードで再生を行うので、リングバッファサイズを取得
            //TrackBuffer.Instance.Frames = AudioTrack.GetMinBufferSize(8000, ChannelOut.Mono, Encoding.Pcm8bit);
            // Xperia Z4 = 820
            TrackBuffer.Instance.Frames = 1024;

            // AudioTrackを生成する
            mAudioTrack = new AudioTrack(
                                            Stream.Music,
                                            8000,
                                            ChannelOut.Mono,
                                            Encoding.Pcm8bit,
                                            TrackBuffer.Instance.Frames,
                                            AudioTrackMode.Stream);

            // コールバックを指定
            //mAudioTrack.SetPlaybackPositionUpdateListener(new OnPlaybackPositionUpdateListener());

            //通知の発生するフレーム数を指定
            //mAudioTrack.SetPositionNotificationPeriod(TrackBuffer.Instance.Frames);

            Task.Run(() => {
                /*Int32 waitCount = 0;

                while (true)
                {
                    if (TrackBuffer.Instance.Count > 1)
                    {
                        break;
                    }
                    else
                    {
                        Thread.Sleep(100);
                        waitCount++;

                        System.Console.WriteLine("Wait writing AudioTrack buffer: Wait count={0}", waitCount);

                        if ( waitCount > 5 )
                        {
                            return;
                        }
                    }
                }*/

                System.Console.WriteLine("AudioTrack play streaming data");
                mAudioTrack.Play();

                //Byte[] wav = null;
                //wav = TrackBuffer.Instance.Dequeue();
                //mAudioTrack.Write(wav, 0, wav.Length);
                //wav = TrackBuffer.Instance.Dequeue();
                //mAudioTrack.Write(wav, 0, wav.Length);

            });

        }

        /*********************************************************************************
         *
         * 
         *********************************************************************************/
        public void PushData(Byte[] dat)
        {
            mAudioTrack.Write(dat, 0, dat.Length);
        }

        /*********************************************************************************
         *
         * 
         *********************************************************************************/
        public void ButtonPlayStop_Click(object sender, EventArgs e)
        {
            if (mAudioTrack == null)
            {
                return;
            }

            mAudioTrack.Stop();
        }

        /*********************************************************************************
         *
         * 
         *********************************************************************************/
        public void ButtonRec_Click(object sender, EventArgs e)
        {
            //RecordBuffer.Instance.Frames = AudioRecord.GetMinBufferSize(8000, ChannelIn.Mono, Encoding.Pcm8bit);
            // Xperia Z4 = 640
            RecordBuffer.Instance.Frames = 1024;

            mAudioRecord = new AudioRecord(
                                            AudioSource.Mic,
                                            8000,
                                            ChannelIn.Mono,
                                            Encoding.Pcm8bit,
                                            RecordBuffer.Instance.Frames);

            // 音声データを幾つずつ処理するか( = 1フレームのデータの数)
            mAudioRecord.SetPositionNotificationPeriod(RecordBuffer.Instance.Frames);

            // コールバックを指定
            mAudioRecord.SetRecordPositionUpdateListener(new OnRecordPositionUpdateListener());

            mAudioRecord.StartRecording();

            Byte[] dummy = new Byte[1];
            mAudioRecord.Read(dummy, 0, dummy.Length);
        }

        /*********************************************************************************
         *
         * 
         *********************************************************************************/
        public void ButtonRecStop_Click(object sender, EventArgs e)
        {
            if (mAudioRecord == null)
            {
                return;
            }

            mAudioRecord.Stop();

        }

        /*********************************************************************************
         *
         * 
         *********************************************************************************/
        public Byte[] Read()
        {
            return RecordBuffer.Instance.Dequeue();
        }

        /*********************************************************************************
         *
         * 
         *********************************************************************************/
        public class OnRecordPositionUpdateListener : Java.Lang.Object, AudioRecord.IOnRecordPositionUpdateListener
        {
            public void OnMarkerReached(AudioRecord recorder)
            {
                Byte[] buff = new Byte[RecordBuffer.Instance.Frames];

                recorder.Read(buff, 0, buff.Length);
                RecordBuffer.Instance.Enqueue(buff);
            }

            public void OnPeriodicNotification(AudioRecord recorder)
            {
                Byte[] buff = new Byte[RecordBuffer.Instance.Frames];

                recorder.Read(buff, 0, buff.Length);
                RecordBuffer.Instance.Enqueue(buff);
            }
        }

        /*********************************************************************************
        *
        * 
        *********************************************************************************/
        public class RecordBuffer
        {
            public static RecordBuffer Instance = new RecordBuffer();
            private System.Collections.Generic.Queue<Byte[]> Buffer = new System.Collections.Generic.Queue<Byte[]>();
            public int Frames { get; set; }
            public int Count { get { return this.Buffer.Count; } }

            /*********************************************************************************
             *
             * 
             *********************************************************************************/
            public void Enqueue(Byte[] data)
            {
                this.Buffer.Enqueue(data);
                //System.Console.WriteLine("Recording Enqueue... D1={0}", data[0].ToString());
            }

            /*********************************************************************************
             *
             * 
             *********************************************************************************/
            public Byte[] Dequeue()
            {
                if ( this.Buffer.Count == 0 )
                {
                    return null;
                }

                return this.Buffer.Dequeue();

            }

            /*********************************************************************************
             *
             * 
             *********************************************************************************/
            public void WriteFile()
            {
            }
        }

        /*********************************************************************************
         *
         * 
         *********************************************************************************/
        public class OnPlaybackPositionUpdateListener : Java.Lang.Object, AudioTrack.IOnPlaybackPositionUpdateListener
        {

            public void OnMarkerReached(AudioTrack track)
            {
                if (TrackBuffer.Instance.Count > 0)
                {
                    Byte[] data = TrackBuffer.Instance.Dequeue();
                    track.Write(data, 0, data.Length);
                }
            }

            public void OnPeriodicNotification(AudioTrack track)
            {
                if (TrackBuffer.Instance.Count > 0)
                {
                    Byte[] data = TrackBuffer.Instance.Dequeue();
                    track.Write(data, 0, data.Length);
                }
            }
        }

        /*********************************************************************************
        *
        * 
        *********************************************************************************/
        public class TrackBuffer
        {
            public static TrackBuffer Instance = new TrackBuffer();
            private System.Collections.Generic.Queue<Byte[]> Buffer = new System.Collections.Generic.Queue<Byte[]>();
            private Byte[] DummyData = null;

            public int Frames { get; set; }
            public int Count { get { return this.Buffer.Count; } }

            /*********************************************************************************
             *
             * 
             *********************************************************************************/
            public void Enqueue(Byte[] data)
            {
                this.Buffer.Enqueue(data);
            }

            /*********************************************************************************
             *
             * 
             *********************************************************************************/
            public Byte[] Dequeue()
            {
                if ( this.Buffer.Count == 0 )
                {
                    return null;
                }
                else
                {
                    return this.Buffer.Dequeue();
                }
            }
        }

        /*********************************************************************************
        *
        * 
        *********************************************************************************/
        public class WaveBuffer
        {
            public static WaveBuffer Instance = new WaveBuffer();
            private System.Collections.Generic.Queue<Byte[]> Buffer = new System.Collections.Generic.Queue<Byte[]>();

            public int Frames { get; set; }
            public int Count { get { return this.Buffer.Count; } }

            /*********************************************************************************
             *
             * 
             *********************************************************************************/
            public void Enqueue(Byte[] data)
            {
                this.Buffer.Enqueue(data);
            }

            /*********************************************************************************
             *
             * 
             *********************************************************************************/
            public Byte[] Dequeue()
            {
                if (this.Buffer.Count == 0)
                {
                    return null;
                }
                else
                {
                    return this.Buffer.Dequeue();
                }
            }

            /*********************************************************************************
             *
             * 
             *********************************************************************************/
            public void WriteFile()
            {
                String recFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryMusic).Path;
                String recPath = recFolder + "/sample_rec.wav";

                System.IO.FileStream recFs = System.IO.File.Open(recPath, System.IO.FileMode.OpenOrCreate, System.IO.FileAccess.Write);

                Byte[] header = new Byte[] {
                    0x52,0x49, 0x46,0x46,
                    0x3a,0x3c, 0x13,0x00,
                    0x57,0x41, 0x56,0x45,
                    0x66,0x6d, 0x74,0x20,
                    0x10,0x00, 0x00,0x00,
                    0x01,0x00, 0x01,0x00,
                    0x40,0x1f, 0x00,0x00,
                    0x40,0x1f, 0x00,0x00,
                    0x01,0x00, 0x08,0x00,
                    0x64,0x61, 0x74,0x61,
                    0x16,0x3c, 0x13,0x00
                };

                Byte[] len1, len2;
                len1 = BitConverter.GetBytes(Buffer.Count * Frames - 8 + header.Length);
                header[4] = len1[3];
                header[5] = len1[2];
                header[6] = len1[1];
                header[7] = len1[0];

                len2 = BitConverter.GetBytes(Buffer.Count * Frames);
                header[40] = len2[3];
                header[41] = len2[2];
                header[42] = len2[1];
                header[43] = len2[0];

                recFs.Write(header, 0, header.Length);

                for (Int32 i = 0; i < Buffer.Count; i++)
                {
                    Byte[] tmp = Buffer.Dequeue();
                    recFs.Write(tmp, 0, tmp.Length);
                }

                recFs.Close();
                System.Console.WriteLine("Write wave file...");
            }
        }
    }
}

