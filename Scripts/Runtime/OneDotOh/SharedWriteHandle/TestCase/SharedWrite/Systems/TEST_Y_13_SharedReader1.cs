using Unity.Burst;
using Unity.Entities;
using Unity.Profiling;

namespace Anvil.Unity.DOTS.TestCase.SharedWrite
{
#if ANVIL_TEST_CASE_SHARED_WRITE
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    [UpdateAfter(typeof(TEST_Y_12_SharedReader0))]
    [BurstCompile]
    public partial struct TEST_Y_13_SharedReader1 : ISystem
    {
        private static readonly ProfilerMarker s_ProfilerMarker = new ProfilerMarker($"{nameof(TEST_Y_13_SharedReader1)}");
        private SharedReaderSystemPart<SWTCBufferY> m_SharedReaderSystemPart;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_SharedReaderSystemPart = new SharedReaderSystemPart<SWTCBufferY>(
                                                                               ref state,
                                                                               1,
                                                                               65,
                                                                               s_ProfilerMarker);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            m_SharedReaderSystemPart.OnUpdate(ref state);
        }
    }
#endif
}