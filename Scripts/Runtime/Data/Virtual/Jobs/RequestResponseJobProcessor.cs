using Anvil.Unity.DOTS.Data;
using Anvil.Unity.DOTS.Jobs;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace Anvil.Unity.DOTS.Data
{
    public struct RequestResponseJobProcessor<TRequest, TResponse> : ISystemDataJobProcessor<TRequest, TResponse>
        where TRequest : struct, IRequest<TResponse>
        where TResponse : struct
    {
        private const int DEFAULT_LANE_INDEX = -1;

        private readonly UnsafeTypedStream<TRequest>.Writer m_ContinueWriter;
        private readonly NativeArray<TRequest> m_Current;

        private UnsafeTypedStream<TRequest>.LaneWriter m_ContinueLaneWriter;
        private int m_LaneIndex;

        public int Length
        {
            get => m_Current.Length;
        }

        public RequestResponseJobProcessor(UnsafeTypedStream<TRequest>.Writer continueWriter,
                                           NativeArray<TRequest> current)
        {
            m_ContinueWriter = continueWriter;
            m_Current = current;
            
            m_ContinueLaneWriter = default;
            m_LaneIndex = DEFAULT_LANE_INDEX;
        }

        public void InitForThread(int nativeThreadIndex)
        {
            if (m_ContinueLaneWriter.IsCreated)
            {
                float a = 5.0f;
            }
            
            //TODO: Collection checks - Ensure this is called before anything else is called
            m_LaneIndex = ParallelAccessUtil.CollectionIndexForThread(nativeThreadIndex);
            m_ContinueLaneWriter = m_ContinueWriter.AsLaneWriter(m_LaneIndex);
        }

        public TRequest this[int index]
        {
            get => m_Current[index];
        }

        public void Continue(ref TRequest value)
        {
            //TODO: Collection checks
            m_ContinueLaneWriter.Write(ref value);
        }

        public void Complete(ref TRequest request, ref TResponse response)
        {
            //TODO: Collection checks
            request.ResponseWriter.Add(ref response, m_LaneIndex);
        }
    }
}
