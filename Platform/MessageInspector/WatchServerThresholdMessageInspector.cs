using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Dispatcher;
using System.Threading;

using Tuhu.Component.Framework;

namespace Platform.MessageInspector
{
    public class WatchServerThresholdMessageInspector : IDispatchMessageInspector, IClientMessageInspector
    {
        #region Private Fields

        private static readonly ILog logger = LoggerFactory.GetLogger("WatchServerThreshold");

        private static readonly object DumpObj = new object();

        private static DateTime dumpTime = DateTime.MinValue;

        private const int DumpTimeout = 5 * 1000;

        #endregion

        #region Ctor

        private readonly WatchServerThresholdConfig config = null;

        public WatchServerThresholdMessageInspector(WatchServerThresholdConfig config)
        {
            this.config = config;
        }

        #endregion

        #region IDispatchMessageInspector

        public object AfterReceiveRequest(ref Message request, IClientChannel channel, InstanceContext instanceContext)
        { 
            return CreateMessageWatcher(ref request);
        }

        public void BeforeSendReply(ref Message reply, object correlationState)
        {
            WatchMessage(ref reply, correlationState);
        }

        #endregion

        #region IClientMessageInspector

        public object BeforeSendRequest(ref Message request, IClientChannel channel)
        {
            return CreateMessageWatcher(ref request);
        }

        public void AfterReceiveReply(ref Message reply, object correlationState)
        {
            WatchMessage(ref reply, correlationState);
        }

        #endregion

        #region Private Methods

        private MessageWatcher CreateMessageWatcher(ref Message request)
        {
            return new MessageWatcher(request.Headers.Action, DateTime.Now.Ticks, request.ToString());
        }

        private void WatchMessage(ref Message message, object correlationState)
        {
            if (this.config == null || config.Threshold <= 0)
            {
                return;
            }

            var messageWatcher = correlationState as MessageWatcher;
            if (messageWatcher == null)
            {
                return;
            }

           var performanceResult = messageWatcher.PerformanceWatcher.StopWatcher();

            var elapsedTime = new TimeSpan(DateTime.Now.Ticks - messageWatcher.StartTicks).TotalMilliseconds;
            if (elapsedTime < config.Threshold)
            {
                return;
            }

            ThreadPool.QueueUserWorkItem(responseMessage =>
               {
                   try
                   {
                       logger.Log(Level.Warning, string.Format("Action: {0}, Request: {1}, Response: {2}, TotalMilliseconds: {3}",
                                                   messageWatcher.Action, messageWatcher.Message,
                                                   responseMessage.ToString(), elapsedTime));
                       if(performanceResult != null)
                       {
                           logger.Log(Level.Warning, performanceResult.ToString());
                       }

                       if (this.config.EnableDump && !string.IsNullOrEmpty(this.config.DumpCmd))
                       {
                           lock (DumpObj)
                           {
                               if (dumpTime.AddMilliseconds(this.config.DumpInterval) <= DateTime.Now)
                               {
                                   var dumpCmd = this.config.DumpCmd.Split(new char[] { '*' }, StringSplitOptions.RemoveEmptyEntries);
                                   var startInfo = new ProcessStartInfo
                                   {
                                       FileName = dumpCmd[0],
                                       Arguments = string.Format(dumpCmd[1], Process.GetCurrentProcess().Id),
                                       CreateNoWindow = true,
                                       UseShellExecute = false,
                                   };
                                   var process = Process.Start(startInfo);
                                   process.WaitForExit(DumpTimeout);
                                   dumpTime = DateTime.Now;
                               }
                           }
                       }
                   }
                   catch (Exception ex)
                   {
                       logger.Log(Level.Error, ex, "WatchServerThreshold");
                   }
               }, message.ToString());

            using (var messageBuffer = message.CreateBufferedCopy(int.MaxValue))
            {
                message = messageBuffer.CreateMessage();
            }
        }

        private class MessageWatcher
        {
            public string Action { get; private set; }
            public long StartTicks { get; private set; }
            public string Message { get; private set; }
            public PerformanceWatcher PerformanceWatcher { get; private set; }

            public MessageWatcher(string action, long startTicks, string message)
            {
                this.Action = action;
                this.StartTicks = startTicks;
                this.Message = message;

                this.PerformanceWatcher = PerformanceWatcher.StartNewWatcher();
            }
        }

        #endregion
    }
}
