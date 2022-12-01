using Anvil.CSharp.Collections;
using Anvil.CSharp.Core;
using Anvil.CSharp.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Unity.Entities;
using Unity.Mathematics;

namespace Anvil.Unity.DOTS.Entities.Tasks
{
    /// <summary>
    /// Represents a context specific Task done via Jobs over a wide array of multiple instances of data.
    /// The goal of a TaskDriver is to convert specific data into general data that the corresponding
    /// <see cref="AbstractTaskDriverSystem"/> will process en mass and in parallel. The results of that general processing
    /// are then picked up by the TaskDriver to be converted to specific data again and passed on to a sub task driver
    /// or to another general system. 
    /// </summary>
    public abstract class AbstractTaskDriver : AbstractAnvilBase
    {
        private static readonly Type GENERIC_TASK_DRIVER_SYSTEM_TYPE = typeof(TaskDriverSystem<>);
        
        private readonly TaskFlowGraph m_TaskFlowGraph;
        private readonly List<AbstractJobConfig> m_JobConfigs;
        private readonly RootWorkload m_RootWorkload;
        private readonly ContextWorkload m_ContextWorkload;

        private bool m_IsHardened;

        /// <summary>
        /// Reference to the associated <see cref="World"/>
        /// </summary>
        public World World { get; }

        internal List<AbstractTaskDriver> SubTaskDrivers { get; }
        
        internal AbstractTaskDriver Parent { get; }

        protected IRootWorkload RootWorkload
        {
            get => m_RootWorkload;
        }

        protected IContextWorkload ContextWorkload
        {
            get => m_ContextWorkload;
        }

        private readonly AbstractTaskDriverSystem m_TaskDriverSystem;

        protected AbstractTaskDriver(World world) : this(world, null)
        {
        }
        
        //This constructor can also be called via Reflection
        private AbstractTaskDriver(World world, AbstractTaskDriver parent)
        {
            Type type = GetType();
            //We can't just pull this off the System because we might have triggered it's creation via
            //world.GetOrCreateSystem and it's OnCreate hasn't occured yet so it's World is still null.
            World = world;
            Parent = parent;

            Type systemType = GENERIC_TASK_DRIVER_SYSTEM_TYPE.MakeGenericType(type);
            
            //Get the shared CoreTaskWork for this type
            m_TaskDriverSystem = (AbstractTaskDriverSystem)World.GetOrCreateSystem(systemType);
            m_RootWorkload = m_TaskDriverSystem.GetOrCreateRootWorkload(this);
            //Create a new instance of ContextualTaskWork for this instance
            m_ContextWorkload = m_RootWorkload.CreateContextWorkload(this, Parent?.m_ContextWorkload);

            SubTaskDrivers = new List<AbstractTaskDriver>();
            m_JobConfigs = new List<AbstractJobConfig>();
            
            
            TaskDriverFactory.CreateSubTaskDrivers(this, SubTaskDrivers);

            HasCancellableData = TaskData.CancellableDataStreams.Count > 0
                              || SubTaskDrivers.Any(subTaskDriver => subTaskDriver.HasCancellableData)
                              || GoverningTaskSystem.HasCancellableData;

            //TODO: We can do this in hardening
            CancelFlow.BuildRequestData();
            
            m_TaskFlowGraph = world.GetOrCreateSystem<TaskFlowSystem>().TaskFlowGraph;
            //TODO: Investigate if we need this here: #66, #67, and/or #68 - https://github.com/decline-cookies/anvil-unity-dots/pull/87/files#r995032614
            m_TaskFlowGraph.RegisterTaskDriver(this);
        }

        protected override void DisposeSelf()
        {
            //We own our sub task drivers so dispose them
            SubTaskDrivers.DisposeAllAndTryClear();
            //Dispose all the data we own
            m_JobConfigs.DisposeAllAndTryClear();

            m_ContextWorkload.Dispose();

            base.DisposeSelf();
        }

        public override string ToString()
        {
            return $"{GetType().GetReadableName()}|{Context}";
        }

        internal void Harden()
        {
            Debug_EnsureNotHardened();
            m_IsHardened = true;

            foreach (AbstractJobConfig jobConfig in m_JobConfigs)
            {
                jobConfig.Harden();
            }
        }

        //*************************************************************************************************************
        // CONFIGURATION
        //*************************************************************************************************************

        internal void AddToJobConfigs(AbstractJobConfig jobConfig)
        {
            m_JobConfigs.Add(jobConfig);
        }

        //TODO: #101 - Should drivers have all the jobs or systems?
        public IJobConfigRequirements ConfigureJobTriggeredBy<TInstance>(IAbstractDataStream<TInstance> dataStream,
                                                                         in JobConfigScheduleDelegates.ScheduleDataStreamJobDelegate<TInstance> scheduleJobFunction,
                                                                         BatchStrategy batchStrategy)
            where TInstance : unmanaged, IEntityProxyInstance
        {
            return GoverningTaskSystem.ConfigureJobTriggeredBy(this,
                                                               (DataStream<TInstance>)dataStream,
                                                               scheduleJobFunction,
                                                               batchStrategy);
        }

        public IResolvableJobConfigRequirements ConfigureCancelJobFor<TInstance>(IDriverCancellableDataStream<TInstance> dataStream,
                                                                                 in JobConfigScheduleDelegates.ScheduleCancelJobDelegate<TInstance> scheduleJobFunction,
                                                                                 BatchStrategy batchStrategy)
            where TInstance : unmanaged, IEntityProxyInstance
        {
            return GoverningTaskSystem.ConfigureCancelJobFor(this,
                                                             (CancellableDataStream<TInstance>)dataStream,
                                                             scheduleJobFunction,
                                                             batchStrategy);
        }


        public IJobConfigRequirements ConfigureJobTriggeredBy(EntityQuery entityQuery,
                                                              JobConfigScheduleDelegates.ScheduleEntityQueryJobDelegate scheduleJobFunction,
                                                              BatchStrategy batchStrategy)
        {
            return GoverningTaskSystem.ConfigureJobTriggeredBy(this,
                                                               entityQuery,
                                                               scheduleJobFunction,
                                                               batchStrategy);
        }

        public IJobConfigRequirements ConfigureJobWhenCancelComplete(AbstractTaskDriver taskDriver,
                                                                     in JobConfigScheduleDelegates.ScheduleCancelCompleteJobDelegate scheduleJobFunction,
                                                                     BatchStrategy batchStrategy)
        {
            return GoverningTaskSystem.ConfigureJobWhenCancelComplete(this,
                                                                      taskDriver.TaskData.CancelCompleteDataStream,
                                                                      scheduleJobFunction,
                                                                      batchStrategy);
        }


        //TODO: #73 - Implement other job types

        //*************************************************************************************************************
        // SAFETY
        //*************************************************************************************************************

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void Debug_EnsureNotHardened()
        {
            if (m_IsHardened)
            {
                throw new InvalidOperationException($"Trying to Harden {this} but we already are!");
            }
        }
    }
}
