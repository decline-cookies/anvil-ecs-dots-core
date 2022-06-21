using Anvil.Unity.DOTS.Jobs;
using Unity.Collections;
using UnityEngine;

namespace Anvil.Unity.DOTS.Data
{
    /// <summary>
    /// A struct to be used in jobs that is for writing new <typeparamref name="TInstance"/> to
    /// <see cref="VirtualData{TKey,TInstance}"/>.
    ///
    /// Commonly used to add new instances. 
    /// </summary>
    /// <typeparam name="TInstance">The type of instance to add</typeparam>
    [BurstCompatible]
    public struct VDJobWriter<TInstance>
        where TInstance : struct
    {
        private const int DEFAULT_LANE_INDEX = -1;

        [ReadOnly] private readonly UnsafeTypedStream<TInstance>.Writer m_InstanceWriter;

        private UnsafeTypedStream<TInstance>.LaneWriter m_InstanceLaneWriter;
        private int m_LaneIndex;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
        private enum WriterState
        {
            Uninitialized,
            Ready
        }

        private WriterState m_State;
#endif


        internal VDJobWriter(UnsafeTypedStream<TInstance>.Writer instanceWriter) : this()
        {
            m_InstanceWriter = instanceWriter;

            m_InstanceLaneWriter = default;
            m_LaneIndex = DEFAULT_LANE_INDEX;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            m_State = WriterState.Uninitialized;
#endif
        }
        
        /// <summary>
        /// Initializes the struct based on the thread it's being used on.
        /// This must be called before doing anything else with the struct.
        /// </summary>
        /// <param name="nativeThreadIndex">The native thread index</param>
        public void InitForThread(int nativeThreadIndex)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Debug.Assert(m_State == WriterState.Uninitialized);
            m_State = WriterState.Ready;
#endif

            m_LaneIndex = ParallelAccessUtil.CollectionIndexForThread(nativeThreadIndex);
            m_InstanceLaneWriter = m_InstanceWriter.AsLaneWriter(m_LaneIndex);
        }
        
        /// <summary>
        /// Adds the instance to the <see cref="VirtualData{TKey,TInstance}"/>'s
        /// underlying pending collection to be added the next time the virtual data is
        /// consolidated.
        /// </summary>
        /// <param name="instance">The instance to add</param>
        public void Add(TInstance instance)
        {
            Add(ref instance);
        }
        
        /// <inheritdoc cref="Add(TInstance)"/>
        public void Add(ref TInstance instance)
        {
#if ENABLE_UNITY_COLLECTIONS_CHECKS
            Debug.Assert(m_State == WriterState.Ready);
#endif
            m_InstanceLaneWriter.Write(ref instance);
        }
    }
}