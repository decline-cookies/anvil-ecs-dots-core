using Anvil.CSharp.Core;
using Anvil.Unity.DOTS.Data;
using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Anvil.Unity.DOTS.Entities
{
    public abstract class AbstractTaskDriver : AbstractAnvilBase
    {
        private readonly VirtualDataLookup m_InstanceData = new VirtualDataLookup();
        private readonly List<AbstractTaskDriver> m_ChildTaskDrivers = new List<AbstractTaskDriver>();
        private readonly List<JobData> m_PopulateJobData = new List<JobData>();
        private readonly List<JobData> m_UpdateJobData = new List<JobData>();

        public World World
        {
            get;
        }

        public AbstractTaskDriverSystem System
        {
            get;
            protected set;
        }

        protected AbstractTaskDriver(World world)
        {
            World = world;
        }

        protected override void DisposeSelf()
        {
            m_InstanceData.Dispose();

            foreach (AbstractTaskDriver childTaskDriver in m_ChildTaskDrivers)
            {
                childTaskDriver.Dispose();
            }

            m_ChildTaskDrivers.Clear();
            m_PopulateJobData.Clear();
            m_UpdateJobData.Clear();

            base.DisposeSelf();
        }


        protected abstract void CreateInstanceData();
        protected abstract void CreatePopulateJobs();
        protected abstract void CreateChildTaskDrivers();

        public VirtualData<TKey, TInstance> GetInstanceData<TKey, TInstance>()
            where TKey : struct, IEquatable<TKey>
            where TInstance : struct, ILookupData<TKey>
        {
            return m_InstanceData.GetData<TKey, TInstance>();
        }

        protected VirtualData<TKey, TInstance> CreateInstanceData<TKey, TInstance>(AbstractVirtualData sourceData = null)
            where TKey : struct, IEquatable<TKey>
            where TInstance : struct, ILookupData<TKey>
        {
            VirtualData<TKey, TInstance> virtualData = new VirtualData<TKey, TInstance>(sourceData);
            m_InstanceData.AddData(virtualData);
            return virtualData;
        }

        public JobData CreatePopulateJob(JobData.JobDataDelegate jobDataDelegate, BatchStrategy batchStrategy)
        {
            JobData jobData = new JobData(jobDataDelegate, batchStrategy, System);
            m_PopulateJobData.Add(jobData);
            return jobData;
        }

        protected JobData CreateUpdateJob(JobData.JobDataDelegate jobDataDelegate, BatchStrategy batchStrategy)
        {
            JobData jobData = new JobData(jobDataDelegate, batchStrategy, System);
            m_UpdateJobData.Add(jobData);
            return jobData;
        }

        public JobHandle Populate(JobHandle dependsOn)
        {
            int len = m_PopulateJobData.Count;
            if (len == 0)
            {
                return dependsOn;
            }

            NativeArray<JobHandle> populateDependencies = new NativeArray<JobHandle>(len, Allocator.Temp);
            for (int i = 0; i < len; ++i)
            {
                populateDependencies[i] = m_PopulateJobData[i].PrepareAndSchedule(dependsOn);
            }

            return JobHandle.CombineDependencies(populateDependencies);
        }
        
        public JobHandle Update(JobHandle dependsOn)
        {
            int len = m_UpdateJobData.Count;
            if (len == 0)
            {
                return dependsOn;
            }

            NativeArray<JobHandle> populateDependencies = new NativeArray<JobHandle>(len, Allocator.Temp);
            for (int i = 0; i < len; ++i)
            {
                populateDependencies[i] = m_UpdateJobData[i].PrepareAndSchedule(dependsOn);
            }

            return JobHandle.CombineDependencies(populateDependencies);
        }
        
        public JobHandle Consolidate(JobHandle dependsOn)
        {
            JobHandle consolidateHandle = m_InstanceData.ConsolidateForFrame(dependsOn);
            //TODO: Could add a hook for user processing
            return consolidateHandle;
        }

        
    }

    public abstract class AbstractTaskDriver<TTaskDriverSystem> : AbstractTaskDriver
        where TTaskDriverSystem : AbstractTaskDriverSystem
    {
        public new TTaskDriverSystem System
        {
            get;
        }

        protected AbstractTaskDriver(World world) : base(world)
        {
            System = World.GetOrCreateSystem<TTaskDriverSystem>();
            base.System = System;
            System.AddTaskDriver(this);

            CreateInstanceData();
            CreatePopulateJobs();
            CreateChildTaskDrivers();
        }


        protected override void DisposeSelf()
        {
            base.DisposeSelf();
        }
    }
}