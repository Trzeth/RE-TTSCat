﻿using BilibiliDM_PluginFramework;
using Re_TTSCat.Data;

namespace Re_TTSCat
{
    public partial class Main : DMPlugin
    {
        public async void OnDisconnected(object sender, DisconnectEvtArgs e)
        {
            if (!IsNAudioReady) return;
            if (!IsEnabled) return;
            if (!Vars.CurrentConf.AllowConnectEvents) return;
            else if (Vars.CurrentConf.ClearQueueAfterDisconnect)
            {
                TTSPlayer.fileList.Clear();
            }
            if (e == null)
            {
                await TTSPlayer.UnifiedPlay("",
                    Vars.CurrentConf.OnDisconnected.Replace(
                        "$ERROR", ""
                    ), true
                );
            }
            else
            {
                await TTSPlayer.UnifiedPlay("",
                    Vars.CurrentConf.OnDisconnected.Replace(
                        "$ERROR", e.Error.Message
                    )
                );
            }
        }
    }
}
