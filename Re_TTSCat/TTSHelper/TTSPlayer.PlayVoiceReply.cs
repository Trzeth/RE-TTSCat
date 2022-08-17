﻿using BilibiliDM_PluginFramework;
using Re_TTSCat.Data;
using System;
using System.Threading.Tasks;

namespace Re_TTSCat
{
    public static partial class TTSPlayer
    {
        public static async Task<bool> PlayVoiceReply(DanmakuModel e, VoiceReplyRule rule, bool alwaysMatch = false, bool overrideReadInQueue = false)
        {
            if (alwaysMatch || rule.Matches(e))
            {
                if ((VoiceReplyRule.ReplyMode)rule.ReplyingMode != VoiceReplyRule.ReplyMode.VoiceGeneration)
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
