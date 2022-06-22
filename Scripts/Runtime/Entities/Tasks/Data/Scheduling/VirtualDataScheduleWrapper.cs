using Anvil.Unity.DOTS.Data;
using System;

namespace Anvil.Unity.DOTS.Entities
{
    internal class VirtualDataScheduleWrapper<TKey, TInstance> : IScheduleWrapper
        where TKey : struct, IEquatable<TKey>
        where TInstance : struct, ILookupData<TKey>
    {
        public int BatchSize
        {
            get;
        }

        public int Length
        {
            get => throw new NotSupportedException();
        }

        public DeferredNativeArrayScheduleInfo DeferredNativeArrayScheduleInfo
        {
            get;
        }

        public VirtualDataScheduleWrapper(VirtualData<TKey, TInstance> data, BatchStrategy batchStrategy)
        {
            DeferredNativeArrayScheduleInfo = data.ScheduleInfo;

            BatchSize = batchStrategy == BatchStrategy.MaximizeChunk
                ? VirtualData<TKey, TInstance>.MAX_ELEMENTS_PER_CHUNK
                : 1;
        }
    }
}
