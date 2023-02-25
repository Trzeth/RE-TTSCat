using BilibiliDM_PluginFramework;
using Re_TTSCat.Data;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Re_TTSCat
{
    public static partial class TTSPlayer
    {
        public static async Task<bool> PlayVoiceReply(DanmakuModel e, VoiceReplyRule rule, bool alwaysMatch = false, bool overrideReadInQueue = false)
        {
            if (alwaysMatch || rule.Matches(e))
            {
                if ((VoiceReplyRule.ReplyMode)rule.ReplyingMode == VoiceReplyRule.ReplyMode.PrerecordedMessage)
                {
                    // play specific file:
                    try
                    {
                        if (Vars.CurrentConf.InstantVoiceReply || overrideReadInQueue)
                        {
                            Play(rule.ReplyContent, false, true);
                        }
                        else
                        {
                            // add the file to queue
                            fileList.Add(new TTSEntry(rule.ReplyContent, true));
                        }
                    }
                    catch (Exception ex)
                    {
                        Bridge.ALog($"无法读出语音答复: {ex.Message}");
                    }
                }
                else if ((VoiceReplyRule.ReplyMode)rule.ReplyingMode == VoiceReplyRule.ReplyMode.PrerecordedMessageFolder)
                {
                    // play file in folder randomly:
                    try
                    {
                        var files = Directory.GetFiles(rule.ReplyContent);
                        var randomList = files.ToList();
                        foreach (var file in files)
                        {
                            int weight = 1;
                            if (file.Count(c => c == '.') >= 2)
                            {
                                int lastDotIndex = file.LastIndexOf('.');
                                int secondLastDotIndex = file.LastIndexOf('.', lastDotIndex - 1);
                                string numberString = file.Substring(secondLastDotIndex + 1, lastDotIndex - secondLastDotIndex - 1);
                                weight = int.Parse(numberString);

                                if (weight < 1)
                                {
                                    randomList.Remove(file);
                                }
                                else
                                {
                                    for (int i = 0; i != weight - 1; i++)
                                        randomList.Add(file);
                                }
                            }
                        }

                        Random random = new Random();

                        string filePath = randomList[random.Next(randomList.Count)];

                        if (Vars.CurrentConf.InstantVoiceReply || overrideReadInQueue)
                        {
                            Play(filePath, false, true);
                        }
                        else
                        {
                            // add the file to queue
                            fileList.Add(new TTSEntry(filePath, true));
                        }
                    }
                    catch (Exception ex)
                    {
                        Bridge.ALog($"无法读出语音答复: {ex.Message}");
                    }
                }
                else
                {
                    // play by voice generation (and by default)
                    // caution: different types of voice reply have different variables available
                    await UnifiedPlay(e.CommentText,
                        Main.ProcessVoiceReply(e, rule),
                        true,
                        Vars.CurrentConf.InstantVoiceReply || overrideReadInQueue
                    );
                }
                return true;
            }
            else return false;
        }

        public static async Task<bool> PlayVoiceReply(DanmakuModel e)
        {
            if (!Vars.CurrentConf.EnableVoiceReply) return false;
            // danmaku blocking rules have been processed, just process what's left for us
            // go through all rules to see if there's a match
            // (this is the master cycle)
            var hitAny = false;
            foreach (var rule in Vars.CurrentConf.VoiceReplyRules)
            {
                if (await PlayVoiceReply(e, rule)) hitAny = true;
            }
            return hitAny;
        }
    }
}