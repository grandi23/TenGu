using System.Collections.Generic;
using System.Threading.Tasks;

namespace Platform.StartUp
{
    public class SequentialAggregrateStartUp :IAggregateStartUp
    {
        public SequentialAggregrateStartUp(IEnumerable<IStartUp> startUps)
        {
            this.StartUps = startUps;
        }

        public IEnumerable<IStartUp> StartUps { get; private set; }

        public void StartUp()
        {
            var tasks = new List<Task>();
            foreach (var startUp in this.StartUps)
            {
                startUp.StartUp();
            }
        }
    }
}
