using Anvil.Unity.DOTS.Data;
using Anvil.Unity.DOTS.Jobs;
using Unity.Collections;
using Unity.Jobs;

namespace Anvil.Unity.DOTS.Entities.Tasks
{
    internal abstract class AbstractArrayDataStream<TInstance> : AbstractDataStream<TInstance>
        where TInstance : unmanaged, IEntityProxyInstance
    {
        private readonly ActiveArrayData<TInstance> m_ActiveArrayData;

        protected uint ActiveID
        {
            get => m_ActiveArrayData.ID;
        }

        public DeferredNativeArrayScheduleInfo ScheduleInfo { get; }

        protected NativeArray<EntityProxyInstanceWrapper<TInstance>> DeferredJobArray
        {
            get => m_ActiveArrayData.DeferredJobArray;
        }

        protected AbstractArrayDataStream(ITaskSetOwner taskSetOwner) : base(taskSetOwner)
        {
            m_ActiveArrayData = DataSource.CreateActiveArrayData();
            ScheduleInfo = m_ActiveArrayData.ScheduleInfo;
        }

        protected AbstractArrayDataStream(ITaskSetOwner taskSetOwner, DataStream<TInstance> systemDataStream) : base(taskSetOwner, systemDataStream)
        {
            m_ActiveArrayData = systemDataStream.m_ActiveArrayData;
            ScheduleInfo = systemDataStream.ScheduleInfo;
        }

        public sealed override uint GetActiveID()
        {
            return ActiveID;
        }

        public JobHandle AcquireActiveAsync(AccessType accessType)
        {
            return m_ActiveArrayData.AcquireAsync(accessType);
        }

        public void ReleaseActiveAsync(JobHandle dependsOn)
        {
            m_ActiveArrayData.ReleaseAsync(dependsOn);
        }
    }
}
