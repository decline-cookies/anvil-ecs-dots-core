using Anvil.CSharp.Logging;
using System;
using System.Collections.Generic;
using Unity.Entities;

namespace Anvil.Unity.DOTS.Entities.TaskDriver
{
    internal interface ITaskSetOwner
    {
        public TaskSet TaskSet { get; }
        public uint ID { get; }
        public World World { get; }
        public AbstractTaskDriverSystem TaskDriverSystem { get; }
        
        public List<AbstractTaskDriver> SubTaskDrivers { get; }
        
        public bool HasCancellableData { get; }
        
        public Logger TaskSetOwnerLogger { get; }

        public void AddResolvableDataStreamsTo(Type type, List<AbstractDataStream> dataStreams);
    }
}
