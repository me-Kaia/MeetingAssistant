using NAudio.Wave;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MeetingAssistant
{
    public class Recorder
    {
        private WaveInEvent waveIn;
        private WaveFileWriter writer;
        public string currentFilePath;

        private ManualResetEventSlim stopSignal = new ManualResetEventSlim(false);

        public void StartRecording()
        {
            stopSignal.Reset(); // 每次开始录音前重置信号

            string recordDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Recordings");
            Directory.CreateDirectory(recordDir);
            currentFilePath = Path.Combine(recordDir, $"Meeting_{DateTime.Now:yyyyMMdd_HHmmss}.wav");

            waveIn = new WaveInEvent();
            waveIn.WaveFormat = new WaveFormat(16000, 1);

            writer = new WaveFileWriter(currentFilePath, waveIn.WaveFormat);
            waveIn.DataAvailable += WaveIn_DataAvailable;
            waveIn.RecordingStopped += WaveIn_RecordingStopped;
            waveIn.StartRecording();
        }

        public async Task StopRecordingAsync()
        {
            waveIn.StopRecording();
            await Task.Run(() => stopSignal.Wait());
        }

        private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        {
            writer.Write(e.Buffer, 0, e.BytesRecorded);
            writer.Flush();
        }

        private void WaveIn_RecordingStopped(object sender, StoppedEventArgs e)
        {
            writer.Dispose();
            waveIn.Dispose();
            stopSignal.Set(); // 文件真正写完、关闭了，这时候才发出"可以了"的信号
        }
    }
}