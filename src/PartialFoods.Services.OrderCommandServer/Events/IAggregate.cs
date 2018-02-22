using System.Collections.Generic;

namespace PartialFoods.Services.OrderCommandServer.Events
{
    public interface IAggregate<T>
    {
        void Apply(T evt);

        void ApplyAll(IList<T> evts);

        ulong Version { get; }
    }
}