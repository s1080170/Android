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

        readonly Int32 mSamplingRate;
        readonly Int32 mFrameSize;
        readonly Encoding mFormat;
        readonly String mRecFolder;

        /*********************************************************************************
        *
        * 
        *********************************************************************************/
        public AudioInterface()
        {
            mSamplingRate = 8000;
            //mSamplingRate = 16000;
            //mSamplingRate = 44100;
            mFrameSize = 256;
            mFormat = Encoding.Pcm8bit;
            //mFormat = Encoding.Pcm16bit;
            mRecFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryMusic).Path;

        }

        /*********************************************************************************
        *
        * 
        *********************************************************************************/
        public void TestPlay()
        {
            String recPath = mRecFolder + "/sample_rec.wav";

            System.Diagnostics.Debug.WriteLine(recPath);

            File file = new File(recPath);
            FileInputStream inputStream = new FileInputStream(file);

            // Streamモードで再生を行うので、リングバッファサイズを取得
            //TrackBuffer.Instance.Frames = AudioTrack.GetMinBufferSize(8000, ChannelOut.Mono, mFormat);
            // Xperia Z4 = 820
            //TrackBuffer.Instance.Frames = 1024;

            // AudioTrackを生成する
            mAudioTrack = new AudioTrack(
                                            Stream.Music,
                                            //Stream.VoiceCall,
                                            mSamplingRate,
                                            ChannelOut.Mono,
                                            mFormat,
                                            AudioTrack.GetMinBufferSize(mSamplingRate, ChannelOut.Mono, mFormat),
                                            AudioTrackMode.Stream);

            // コールバックを指定
            //mAudioTrack.SetPlaybackPositionUpdateListener(new OnPlaybackPositionUpdateListener());

            //通知の発生するフレーム数を指定
            //mAudioTrack.SetPositionNotificationPeriod(TrackBuffer.Instance.Frames);

            Task.Run(() =>
            {

                System.Diagnostics.Debug.WriteLine("AudioTrack play a recording file");
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
            //String musicFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryMusic).Path;
            String filePath = mRecFolder + "/sample_mono_8k8bit.wav";
            //String filePath = musicFolder + "/sample_stereo_44k16bit.wav";

            System.Diagnostics.Debug.WriteLine(filePath);

            File file = new File(filePath);
            FileInputStream inputStream = new FileInputStream(file);

            // Streamモードで再生を行うので、リングバッファサイズを取得
            Int32 bufferSize = AudioTrack.GetMinBufferSize(mSamplingRate, ChannelOut.Mono, mFormat);
            System.Diagnostics.Debug.WriteLine("AudioTrack : GetMinBufferSize={0}", bufferSize);

            // Frame size
            TrackBuffer.Instance.Frames = mFrameSize;

            // AudioTrackを生成する
            mAudioTrack = new AudioTrack(
                                            Stream.Music,
                                            //Stream.VoiceCall,
                                            mSamplingRate,
                                            ChannelOut.Mono,
                                            mFormat,
                                            bufferSize,
                                            AudioTrackMode.Stream);

            // コールバックを指定
            mAudioTrack.SetPlaybackPositionUpdateListener(new OnPlaybackPositionUpdateListener());

            //通知の発生するフレーム数を指定
            mAudioTrack.SetPositionNotificationPeriod(TrackBuffer.Instance.Frames);
            
            TrackBuffer.Instance.Clear();

            Task.Run(() => {

                while (true)
                {
                    if (TrackBuffer.Instance.Count > 5)
                    {
                        break;
                    }
                }

                System.Diagnostics.Debug.WriteLine("AudioTrack play streaming data");
                mAudioTrack.Play();

                Byte[] wav = null;
                wav = TrackBuffer.Instance.Dequeue(); mAudioTrack.Write(wav, 0, wav.Length);
                wav = TrackBuffer.Instance.Dequeue(); mAudioTrack.Write(wav, 0, wav.Length);
                wav = TrackBuffer.Instance.Dequeue(); mAudioTrack.Write(wav, 0, wav.Length);
                wav = TrackBuffer.Instance.Dequeue(); mAudioTrack.Write(wav, 0, wav.Length);

            });

        }

        /*********************************************************************************
         *
         * 
         *********************************************************************************/
        public void PushData(Byte[] dat)
        {
            TrackBuffer.Instance.Enqueue(dat);

            if(TrackBuffer.Instance.Count < 2)
            {
                TrackBuffer.Instance.Enqueue(dat);
            }

            Byte[] d = TrackBuffer.Instance.Dequeue();

            mAudioTrack.Write(d, 0, d.Length);
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
            Int32 bufferSize = AudioRecord.GetMinBufferSize(mSamplingRate, ChannelIn.Mono, mFormat);
            System.Diagnostics.Debug.WriteLine("AudioRecord : GetMinBufferSize={0}", bufferSize);

            RecordBuffer.Instance.Frames = mFrameSize;

            mAudioRecord = new AudioRecord(
                                            //AudioSource.Default,
                                            //AudioSource.Camcorder,
                                            AudioSource.Mic,
                                            //AudioSource.VoiceCommunication,
                                            //AudioSource.VoiceRecognition,
                                            //AudioSource.VoiceUplink,
                                            mSamplingRate,
                                            ChannelIn.Mono,
                                            mFormat,
                                            bufferSize);

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

                WaveBuffer.Instance.Enqueue(buff);
            }

            public void OnPeriodicNotification(AudioRecord recorder)
            {
                Byte[] buff = new Byte[RecordBuffer.Instance.Frames];

                recorder.Read(buff, 0, buff.Length);
                RecordBuffer.Instance.Enqueue(buff);

                WaveBuffer.Instance.Enqueue(buff);
            }
        }

        /*********************************************************************************
        *
        * 
        *********************************************************************************/
        public class RecordBuffer
        {
            public static RecordBuffer Instance = new RecordBuffer();
            readonly System.Collections.Generic.Queue<Byte[]> Buffer = new System.Collections.Generic.Queue<Byte[]>();
            public int Frames { get; set; }
            public int Count { get { return this.Buffer.Count; } }

            /*********************************************************************************
             *
             * 
             *********************************************************************************/
            public void Enqueue(Byte[] data)
            {
                this.Buffer.Enqueue(data);
                //System.Diagnostics.Debug.WriteLine("Recording Enqueue... D1={0}", data[0].ToString());
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
            public void Clear()
            {
                this.Buffer.Clear();
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
                //System.Diagnostics.Debug.WriteLine("AudioTrack OnMarkerReached : Coun={0}", TrackBuffer.Instance.Count);

                //if (TrackBuffer.Instance.Count > 0)
                //{
                    Byte[] data = TrackBuffer.Instance.Dequeue();
                    track.Write(data, 0, data.Length);

                    WaveBuffer.Instance.Enqueue(data);
                //}
            }

            public void OnPeriodicNotification(AudioTrack track)
            {
                //System.Diagnostics.Debug.WriteLine("AudioTrack OnPeriodicNotification : Count={0}", TrackBuffer.Instance.Count);

                //if (TrackBuffer.Instance.Count > 0)
                //{
                    Byte[] data = TrackBuffer.Instance.Dequeue();
                    track.Write(data, 0, data.Length);

                    WaveBuffer.Instance.Enqueue(data);
                //}
            }
        }

        /*********************************************************************************
        *
        * 
        *********************************************************************************/
        public class TrackBuffer
        {
            public static TrackBuffer Instance = new TrackBuffer();
            readonly System.Collections.Generic.Queue<Byte[]> Buffer = new System.Collections.Generic.Queue<Byte[]>();
            private Byte[] DummyData = null;

            public int Frames { get; set; }
            public int Count { get { return this.Buffer.Count; } }

            /*********************************************************************************
             *
             * 
             *********************************************************************************/
            public void Enqueue(Byte[] data)
            {
                if ( this.Buffer.Count < 100 )
                {
                    this.Buffer.Enqueue(data);
                }
            }

            /*********************************************************************************
             *
             * 
             *********************************************************************************/
            public Byte[] Dequeue()
            {
                if ( this.Buffer.Count == 0 )
                {
                    return DummyData;
                }
                else
                {
                    DummyData = this.Buffer.Dequeue();
                    return DummyData;
                }
            }
            
            /*********************************************************************************
             *
             * 
             *********************************************************************************/
            public void Clear()
            {
                this.Buffer.Clear();
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
            public void Clear()
            {
                this.Buffer.Clear();
            }

            /*********************************************************************************
             *
             * 
             *********************************************************************************/
            public void WriteFile()
            {
                String recFolder = Android.OS.Environment.GetExternalStoragePublicDirectory(Android.OS.Environment.DirectoryMusic).Path;
                String recPath = recFolder + "/sample_rec.wav";
                //String recPath = recFolder + "/sample_rec" + DateTime.Now.ToString("yyyymmddhhmmss") + ".wav";

                System.IO.FileStream recFs = System.IO.File.Open(recPath, System.IO.FileMode.Create, System.IO.FileAccess.Write);

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

                Int32 tmpCount = Buffer.Count;
                while( Buffer.Count != 0 )
                {
                    Byte[] tmp = Buffer.Dequeue();
                    recFs.Write(tmp, 0, tmp.Length);
                }

                recFs.Close();
                System.Diagnostics.Debug.WriteLine("Write wave file...");
                System.Diagnostics.Debug.WriteLine("Buffer count" + tmpCount.ToString());
            }
        }
    }
}

