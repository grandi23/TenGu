using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Platform.StartUp
{
    public class AggregateStartUp : IAggregateStartUp
    {
        public AggregateStartUp(IEnumerable<IStartUp> startUps)
        {
            this.StartUps = startUps;
        }

        public IEnumerable<IStartUp> StartUps { get; private set; }

        public void StartUp()
        {
            var tasks = new List<Task>();
            foreach (var startUp in this.StartUps)
            {
                tasks.Add(Task.Run(() =>
                {
                    startUp.StartUp();
                }));
            }

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException excetion)
            {
                throw excetion.InnerExceptions[0];
            }
        }
    }
}
