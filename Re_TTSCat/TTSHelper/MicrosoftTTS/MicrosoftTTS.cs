using Re_TTSCat.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CognitiveServices.Speech;
using System.Diagnostics;
using System.Web;

namespace Re_TTSCat
{
    internal class MicrosoftTTS
    {
        public static SpeechSynthesizer speechSynthesizer { get; set; }


        public static async Task<string> Download(string content)
        {
            Trace.WriteLine("Download "+ content);
            var errorCount = 0;
        Retry:
            try
            {
                if (speechSynthesizer == null)
                {
                    var speechConfig = SpeechConfig.FromSubscription(Vars.CurrentConf.MicrosoftTtsSecretKey, "eastasia");
                    speechSynthesizer = new SpeechSynthesizer(speechConfig, null);
                }

                var fileName = Path.Combine(Vars.CacheDir, Conf.GetRandomFileName() + "MicrosoftTTS.wav");
                Trace.WriteLine("fileName " + fileName);
                Bridge.ALog($"正在下载 TTS, 文件名: {fileName}, 方法: {Vars.CurrentConf.ReqType}");
                await Start(fileName, content);
                return fileName;
            }
            catch (Exception ex)
            {
                Bridge.ALog($"(E5) TTS 下载失败: {ex.Message}");
                errorCount += 1;
                Vars.TotalFails++;
                if (errorCount <= Vars.CurrentConf.DownloadFailRetryCount)
                {
                    goto Retry;
                }
                return null;
            }
            
        }

        static async Task Start(string fileName, string text)
        {
            var ssml = "<speak version=\"1.0\" xmlns=\"https://www.w3.org/2001/10/synthesis\" xml:lang=\"en-US\">\n" +
                 $"<voice name = \"{Vars.CurrentConf.MicrosoftTtsSound}\">" +
                 text +
                 "</voice> " +
                 "\n</speak>";

            var result = await speechSynthesizer.SpeakSsmlAsync(ssml);
            using (var stream = AudioDataStream.FromResult(result))
            {
                await stream.SaveToWaveFileAsync(fileName);
            }
        }
    }
}
