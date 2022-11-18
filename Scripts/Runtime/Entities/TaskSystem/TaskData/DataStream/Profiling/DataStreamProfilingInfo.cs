using System;

namespace Anvil.Unity.DOTS.Entities.Tasks
{
    /// <summary>
    /// Profiling information for a <see cref="AbstractDataStream"/>
    /// </summary>
    public class DataStreamProfilingInfo : IDisposable
    {
        public readonly Type DataType;
        public readonly Type InstanceType;
        public readonly long PendingBytesPerInstance;
        public readonly long LiveBytesPerInstance;
        public readonly AbstractTaskDriver TaskDriver;
        public readonly AbstractTaskSystem TaskSystem;
        
        public DataStreamProfilingDetails ProfilingDetails;

        internal DataStreamProfilingInfo(AbstractDataStream dataStream)
        {
            DataType = dataStream.Type;
            InstanceType = dataStream.Debug_InstanceType;
            PendingBytesPerInstance = dataStream.Debug_PendingBytesPerInstance;
            LiveBytesPerInstance = dataStream.Debug_LiveBytesPerInstance;
            TaskDriver = dataStream.OwningTaskDriver;
            TaskSystem = dataStream.OwningTaskSystem;
            ProfilingDetails = new DataStreamProfilingDetails(dataStream);
        }

        public void Dispose()
        {
            ProfilingDetails.Dispose();
        }
    }
}