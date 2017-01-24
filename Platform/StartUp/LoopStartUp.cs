using System;
using System.Threading;

using Tuhu.Component.Framework;

namespace Platform.StartUp
{
    public abstract class LoopStartUp : IStartUp
    {
        protected readonly ILog logger = null;
        protected LoopStartUp(ILog logger)
        {
            this.logger = logger;
        }

        protected int Interval { get; set; }

        public void StartUp()
        {
            new Thread(() =>
            {
                while (true)
                {
                    try
                    {
                        Loop();
                    }
                    catch (Exception ex)
                    {
                        logger.Log(Level.Critial, ex, string.Format("启动循环线程{0}报错", this.GetType().Name));
                    }

                    Thread.Sleep(this.Interval);
                }
            }).Start();
        }

        protected abstract void Loop();
    }
}
