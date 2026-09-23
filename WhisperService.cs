using System;
using System.Diagnostics;
using System.Text;
using System.IO;

namespace MeetingAssistant
{
    public class WhisperService
    {

        private string whisperPath =
     Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Whisper", "whisper-cli.exe");

        private string modelPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Whisper", "ggml-base.bin");


        public string Transcribe(string wavFilePath)
        {
            Process process = new Process();
            process.StartInfo.FileName = whisperPath;
            process.StartInfo.Arguments =
                $"-m \"{modelPath}\" -f \"{wavFilePath}\" -l zh -nt";
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.StartInfo.CreateNoWindow = true;

            try
            {
                process.Start();
            }
            catch (Exception ex)
            {
                return "[Whisper启动失败] " + ex.Message;
            }

            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            // 优先返回真正识别出的文字
            if (!string.IsNullOrWhiteSpace(output))
            {
                return output.Trim();
            }

            // output是空的，才把error当成报错信息返回，方便排查
            return "[识别失败] " + error;
        }
    }
}