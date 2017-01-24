using System.Collections.Generic;

namespace Platform.StartUp
{
    public interface IAggregateStartUp : IStartUp
    {
        IEnumerable<IStartUp> StartUps { get; }
    }
}
