using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio.Wave;
using System.Diagnostics;
using System.Threading.Tasks;

namespace MeetingAssistant
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Recorder recorder;

        private WhisperService whisperService = new WhisperService();

        private SummaryService summaryService = new SummaryService();

        private bool isRecording = false;
        

        public MainWindow()
        {
            InitializeComponent();
            recorder = new Recorder();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show("按钮点击成功");
           // RecordButton.Content = "停止录音";
            isRecording = !isRecording;
            if (isRecording)
            {
                RecordButton.Content = "停止录音";
                recorder.StartRecording();
                //waveIn = new WaveInEvent();

                // 配置录音参数
                // 开始录音
                // TODO：开始录音
                // waveIn.WaveFormat = new WaveFormat(44100, 1);

                // writer = new WaveFileWriter(
                //    @"D:\meeting.wav",//录音保存路径
                //    waveIn.WaveFormat //录音的保存格式
                //    );
                //waveIn.DataAvailable += WaveIn_DataAvailable;

                //// 新增
                //waveIn.RecordingStopped += WaveIn_RecordingStopped;
                //waveIn.StartRecording();

            }
            else
            {
                RecordButton.Content = "开始录音";
                await recorder.StopRecordingAsync();
                //System.Threading.Thread.Sleep(500);

                string text = whisperService.Transcribe(recorder.currentFilePath);
                
                MeetingContentBox.Text = text;

                SummaryBox.Text = "总结中...";
                string summary = await summaryService.SummarizeAsync(text);
                SummaryBox.Text = summary;

                //waveIn.StopRecording();
                ////writer.Dispose();//释放资源
                ////waveIn.Dispose();//释放资源
                //// TODO：停止录音
                //waveIn.StopRecording();
            }


        }

        //private string RunWhisper(string wavFilePath)
        //{
        //    Process process = new Process();
        //    process.StartInfo.FileName = "D:\\whisper\\Release\\whisper-cli.exe";
        //    process.StartInfo.Arguments = "-m D:\\whisper\\Release\\ggml-base.bin -f " + wavFilePath + " -l zh -nt";
        //    process.StartInfo.UseShellExecute = false;
        //    process.StartInfo.RedirectStandardOutput = true;
        //    process.StartInfo.CreateNoWindow = true;

        //    process.Start();

        //    string result = process.StandardOutput.ReadToEnd();

        //    process.WaitForExit();

        //    return result;
        //}

        //private void WaveIn_RecordingStopped(object? sender,StoppedEventArgs e)
        //{
        //    writer.Dispose();
        //    waveIn.Dispose();
        //}
        //private void WaveIn_DataAvailable(object sender, WaveInEventArgs e)
        //{
        //    e.BytesRecorded = 
        //    // 处理录音数据
        //    // TODO：处理录音数据
        //}
    }
}