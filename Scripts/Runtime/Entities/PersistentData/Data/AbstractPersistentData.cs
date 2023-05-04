using Anvil.CSharp.Core;
using Anvil.CSharp.Logging;
using Anvil.Unity.DOTS.Jobs;
using System.Runtime.CompilerServices;
using Unity.Jobs;
using UnityEngine;

namespace Anvil.Unity.DOTS.Entities
{
    internal abstract class AbstractPersistentData : AbstractAnvilBase
    {
        private readonly AccessController m_AccessController;
        private readonly string m_UniqueContextIdentifier;
        private readonly IDataOwner m_DataOwner;
        private DataTargetID m_DataTargetID;
        
        public DataTargetID DataTargetID
        {
            get
            {
                Debug.Assert(m_DataTargetID.IsValid);
                return m_DataTargetID;
            }
        }

        protected AbstractPersistentData(IDataOwner dataOwner, string uniqueContextIdentifier)
        {
            m_DataOwner = dataOwner;
            m_UniqueContextIdentifier = uniqueContextIdentifier;
            m_AccessController = new AccessController();
        }

        protected sealed override void DisposeSelf()
        {
            m_AccessController.Acquire(AccessType.Disposal);
            DisposeData();
            m_AccessController.Dispose();
            base.DisposeSelf();
        }

        protected abstract void DisposeData();

        public override string ToString()
        {
            return $"{GetType().GetReadableName()}";
        }

        public void GenerateWorldUniqueID()
        {
            Debug.Assert(m_DataOwner == null || m_DataOwner.WorldUniqueID.IsValid);
            string idPath = $"{(m_DataOwner != null ? m_DataOwner.WorldUniqueID : string.Empty)}/{GetType().AssemblyQualifiedName}{m_UniqueContextIdentifier}";
            m_DataTargetID = new DataTargetID(idPath.GetBurstHashCode32());
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public JobHandle AcquireAsync(AccessType accessType)
        {
            return m_AccessController.AcquireAsync(accessType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ReleaseAsync(JobHandle dependsOn)
        {
            m_AccessController.ReleaseAsync(dependsOn);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Acquire(AccessType accessType)
        {
            m_AccessController.Acquire(accessType);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Release()
        {
            m_AccessController.Release();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public AccessController.AccessHandle AcquireWithHandle(AccessType accessType)
        {
            return m_AccessController.AcquireWithHandle(accessType);
        }
    }
}