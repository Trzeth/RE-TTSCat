using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using BilibiliDM_PluginFramework;
using NAudio.Wave;
using Re_TTSCat.Data;

namespace Re_TTSCat
{
    public static partial class TTSPlayer
    {

        static Dictionary<string, int> contenCounter = new Dictionary<string, int>();

        /// <summary>
        /// Play something, other options are defined by current configurations
        /// </summary>
        /// <param name="content">Final TTS Content</param>
        /// <param name="ignoreRandomDitch">Specify true to ignore random ditching</param>
        public static async Task UnifiedPlay(string commentText, string content, bool ignoreRandomDitch = false, bool overrideReadInQueue = false)
        {
            var rawContent = content;
            if (string.IsNullOrWhiteSpace(content))
            {
                Bridge.ALog("放弃: 内容为空");
                return;
            }
            Bridge.ALog("尝试朗读: " + content);
            //if (!Conf.GetRandomBool(Vars.CurrentConf.ReadPossibility) && !ignoreRandomDitch)
            //{
            //    Bridge.ALog("放弃: 已随机丢弃");
            //    return;
            //}

            lock (contenCounter) {
                if (commentText == null)
                {
                    commentText = "";
                }
                if (contenCounter.ContainsKey(commentText))
                {
                    contenCounter[commentText]++;
                }
                else
                {
                    contenCounter.Add(commentText,1);
                }

                Bridge.ALog($"相同弹幕累计: {contenCounter[commentText]}");

                if(contenCounter[commentText]> Vars.CurrentConf.IgnoreRepeatedDanmuNumber)
                {
                    Bridge.ALog($"相同弹幕累计太多，跳过: number:{contenCounter[commentText]} commentText:{commentText}");
                    contenCounter[commentText]--;
                    return;
                }
            }

            try { 

            string fileName;
            if (Vars.CurrentConf.EnableUrlEncode)
            {
                content = HttpUtility.UrlEncode(content);
                Bridge.ALog("URL 编码完成: " + content);
            }
            switch (Vars.CurrentConf.Engine)
            {
                default:
                    fileName = await BaiduTTS.Download(content);
                    break;
                case 1:
                    if (!Vars.SystemSpeechAvailable)
                    {
                        Bridge.Log(".NET 框架引擎在此系统上不可用，无法朗读");
                        return;
                    }
                    fileName = FrameworkTTS.Download(content);
                    break;
                case 2:
                    fileName = GoogleTTS.Download(content, "zh-CN");
                    break;
                case 3:
                    fileName = await BaiduTTS.Download(content, true);
                    break;
                case 4:
                    fileName = await YoudaoTTS.Download(content);
                    break;
                case 5:
                    fileName = await CustomTTS.Download(content);
                    break;
                case 6:
                    fileName = await BaiduTTS.AiApi.Download(content, BaiduTTS.AiApi.ParseToSpeechPerson(Vars.CurrentConf.SpeechPerson));
                    break;
                case 7:
                    {
                        var dllfn = Path.Combine(Vars.ConfDir, "Microsoft.CognitiveServices.Speech.csharp.dll");
                        Assembly.LoadFrom(dllfn);
                        fileName = await MicrosoftTTS.Download(rawContent);
                    }
                    
                    break;
            }
            if (fileName == null)
            {
                Bridge.ALog("下载失败，丢弃");
                return;
            }
            if (Vars.CurrentConf.ReadInQueue && !overrideReadInQueue)
            {
                Bridge.ALog($"正在添加下列文件到播放列表: {fileName}");
                fileList.Add(new TTSEntry(fileName));
            }
            else
            {
                Bridge.ALog($"正在直接播放: {fileName}");
                Play(fileName, false);
            }

            }catch (Exception e)
            {
                Bridge.ALog($"播放异常: {e}");
            }

            lock (contenCounter)
            {
                contenCounter[commentText]--;
                Bridge.ALog($"相同弹幕累计2: {contenCounter[commentText]}");
                if (contenCounter[commentText] == 0)
                {
                    contenCounter.Remove(commentText);
                }
                
            }
        }
    }
}
