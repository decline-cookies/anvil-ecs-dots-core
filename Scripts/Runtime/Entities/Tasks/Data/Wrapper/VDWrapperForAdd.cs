using Anvil.Unity.DOTS.Data;
using Anvil.Unity.DOTS.Jobs;
using System;
using Unity.Jobs;

namespace Anvil.Unity.DOTS.Entities
{
    internal class VDWrapperForAdd<TKey> : AbstractVDWrapper<TKey>
        where TKey : unmanaged, IEquatable<TKey>
    {
        public VDWrapperForAdd(AbstractVirtualData<TKey> data) : base(data)
        {
        }
        
        public override JobHandle AcquireAsync()
        {
            return Data.AccessController.AcquireAsync(AccessType.SharedWrite);
        }

        public override void ReleaseAsync(JobHandle releaseAccessDependency)
        {
            Data.AccessController.ReleaseAsync(releaseAccessDependency);
        }

        public override void Acquire()
        {
            Data.AccessController.Acquire(AccessType.SharedWrite);
        }

        public override void Release()
        {
            Data.AccessController.Release();
        }
    }
}
