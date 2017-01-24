
namespace Platform.MessageInspector
{
    public class WatchServerThresholdConfig
    {
        public int Threshold { get; private set; }

        public bool EnableDump { get; private set; }

        public string DumpCmd { get; private set; }

        public int DumpInterval { get; set; }

        public WatchServerThresholdConfig(int threshold, bool enableDump, string dumpCmd, int dumpInterval)
        {
            this.Threshold = threshold;
            this.EnableDump = enableDump;
            this.DumpCmd = dumpCmd;
            this.DumpInterval = dumpInterval;
        }
    }
}
