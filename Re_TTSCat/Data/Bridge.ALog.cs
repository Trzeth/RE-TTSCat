using System.Diagnostics;

namespace Re_TTSCat.Data
{
    public partial class Bridge
    {
        /// <summary>
        /// 仅在调试模式下输出日志
        /// </summary>
        /// <param name="content">日志内容</param>
        public static void ALog(string content)
        {
            Trace.WriteLine("TTScat:"+ content);
            MainInstance.ALog(content);
        }
    }
}
