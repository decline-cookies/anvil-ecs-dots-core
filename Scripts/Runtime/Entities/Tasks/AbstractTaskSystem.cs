using Anvil.CSharp.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Entities;
using Unity.Jobs;

namespace Anvil.Unity.DOTS.Entities
{
    /// <summary>
    /// A type of System that runs <see cref="AbstractTaskDriver"/>s during its update phase.
    /// </summary>
    public abstract partial class AbstractTaskSystem<TTaskDriver, TTaskSystem> : AbstractTaskSystem,
                                                                                 ITaskSystem
        where TTaskDriver : AbstractTaskDriver<TTaskDriver, TTaskSystem>
        where TTaskSystem : AbstractTaskSystem<TTaskDriver, TTaskSystem>
    {
        
        private readonly List<TTaskDriver> m_TaskDrivers;
        private readonly ByteIDProvider m_TaskDriverIDProvider;
        private readonly TaskFlowGraph m_TaskFlowGraph;
        
        private Dictionary<TaskFlowRoute, BulkJobScheduler<IJobConfig>> m_SystemJobConfigBulkJobSchedulerLookup;
        private Dictionary<TaskFlowRoute, BulkJobScheduler<IJobConfig>> m_DriverJobConfigBulkJobSchedulerLookup;
        
        private BulkJobScheduler<IProxyDataStream> m_SystemDataStreamBulkJobScheduler;
        private BulkJobScheduler<IProxyDataStream> m_DriverDataStreamBulkJobScheduler;
        
        private bool m_IsHardened;

        public byte Context
        {
            get;
        }

        protected AbstractTaskSystem()
        {
            m_TaskDrivers = new List<TTaskDriver>();

            m_TaskDriverIDProvider = new ByteIDProvider();
            Context = m_TaskDriverIDProvider.GetNextID();

            //TODO: Talk to Mike about this. The World property is null for the default world because systems are created via Activator.CreateInstance.
            //TODO: They don't go through the GetOrCreateSystem path. Is this the case for other worlds? Can we assume a null World is the default one?
            World currentWorld = World ?? World.DefaultGameObjectInjectionWorld;
            m_TaskFlowGraph = currentWorld.GetOrCreateSystem<TaskFlowDataSystem>().TaskFlowGraph;

            CreateDataStreams();

            //TODO: 3. Custom Update Job Types
            //TODO: Create the custom Update Job so we can parse to the different result channels.
        }

        protected override void OnDestroy()
        {
            //We only want to dispose the data streams that we own, so only the system ones
            m_TaskFlowGraph.DisposeFor(this);
            
            //Clean up all the native arrays
            m_SystemDataStreamBulkJobScheduler?.Dispose();
            m_DriverDataStreamBulkJobScheduler?.Dispose();
            DisposeJobConfigBulkJobSchedulerLookup(m_SystemJobConfigBulkJobSchedulerLookup);
            DisposeJobConfigBulkJobSchedulerLookup(m_DriverJobConfigBulkJobSchedulerLookup);
            
            
            m_TaskDriverIDProvider.Dispose();

            //Note: We don't dispose TaskDrivers here because their parent or direct reference will do so. 
            m_TaskDrivers.Clear();

            base.OnDestroy();
        }

        private void DisposeJobConfigBulkJobSchedulerLookup(Dictionary<TaskFlowRoute, BulkJobScheduler<IJobConfig>> lookup)
        {
            if (lookup == null)
            {
                return;
            }
            foreach (BulkJobScheduler<IJobConfig> scheduler in lookup.Values)
            {
                scheduler.Dispose();
            }
            lookup.Clear();
        }

        protected override void OnStartRunning()
        {
            base.OnStartRunning();
            if (m_IsHardened)
            {
                return;
            }
            m_IsHardened = true;
            
            BuildOptimizedCollections();
        }

        private void CreateDataStreams()
        {
            List<IProxyDataStream> dataStreams = TaskDataStreamUtil.GenerateProxyDataStreamsOnType(this);
            foreach (IProxyDataStream dataStream in dataStreams)
            {
                RegisterDataStream(dataStream, null);
            }
        }

        //TODO: #39 - Some way to remove the update Job

        internal byte RegisterTaskDriver(TTaskDriver taskDriver)
        {
            Debug_EnsureTaskDriverSystemRelationship(taskDriver);
            m_TaskDrivers.Add(taskDriver);

            return m_TaskDriverIDProvider.GetNextID();
        }

        internal void RegisterTaskDriverDataStream(IProxyDataStream dataStream, ITaskDriver taskDriver)
        {
            RegisterDataStream(dataStream, taskDriver);
        }

        private void RegisterDataStream(IProxyDataStream dataStream, ITaskDriver taskDriver)
        {
            m_TaskFlowGraph.RegisterDataStream(dataStream, this, taskDriver);
        }

        protected override void OnUpdate()
        {
            Dependency = UpdateTaskDriverSystem(Dependency);
        }

        //TODO: Determine if we need custom configs for job types
        protected IJobConfig ConfigureUpdateJobFor<TInstance>(ProxyDataStream<TInstance> dataStream,
                                                              JobConfig<TInstance>.ScheduleJobDelegate scheduleJobFunction,
                                                              BatchStrategy batchStrategy)
            where TInstance : unmanaged, IProxyInstance
        {
            return ConfigureJobFor(dataStream,
                                   scheduleJobFunction,
                                   batchStrategy,
                                   TaskFlowRoute.Update);
        }

        //TODO: Determine if we need custom configs for job types
        internal IJobConfig ConfigurePopulateJobFor<TInstance>(ProxyDataStream<TInstance> dataStream,
                                                               JobConfig<TInstance>.ScheduleJobDelegate scheduleJobFunction,
                                                               BatchStrategy batchStrategy)
            where TInstance : unmanaged, IProxyInstance
        {
            return ConfigureJobFor(dataStream,
                                   scheduleJobFunction,
                                   batchStrategy,
                                   TaskFlowRoute.Populate);
        }


        private IJobConfig ConfigureJobFor<TInstance>(ProxyDataStream<TInstance> dataStream,
                                                      JobConfig<TInstance>.ScheduleJobDelegate scheduleJobFunction,
                                                      BatchStrategy batchStrategy,
                                                      TaskFlowRoute route)
            where TInstance : unmanaged, IProxyInstance
        {
            Debug_EnsureDataStreamIntegrity(dataStream, typeof(TInstance));
            Debug_EnsureNotHardened(dataStream, route);

            return m_TaskFlowGraph.CreateJobConfig(World,
                                                   dataStream,
                                                   scheduleJobFunction,
                                                   batchStrategy,
                                                   route);
        }


        private void BuildOptimizedCollections()
        {
            m_SystemDataStreamBulkJobScheduler = m_TaskFlowGraph.CreateDataStreamBulkJobSchedulerFor(this);
            m_DriverDataStreamBulkJobScheduler = m_TaskFlowGraph.CreateDataStreamBulkJobSchedulerFor(m_TaskDrivers);

            m_SystemJobConfigBulkJobSchedulerLookup = m_TaskFlowGraph.CreateJobConfigBulkJobSchedulerLookupFor(this);
            m_DriverJobConfigBulkJobSchedulerLookup = m_TaskFlowGraph.CreateJobConfigBulkJobSchedulerLookupFor(m_TaskDrivers);
        }

        private JobHandle UpdateTaskDriverSystem(JobHandle dependsOn)
        {
            //Run all TaskDriver populate jobs to allow them to write to data streams (TaskDrivers -> generic TaskSystem data)
            dependsOn = ScheduleJobs(dependsOn,
                                     TaskFlowRoute.Populate,
                                     m_DriverJobConfigBulkJobSchedulerLookup);
            
            //Consolidate system data so that it can be operated on. (Was populated on previous step)
            dependsOn = m_SystemDataStreamBulkJobScheduler.Schedule(dependsOn,
                                                                    IProxyDataStream.CONSOLIDATE_FOR_FRAME_SCHEDULE_FUNCTION);

            //TODO: #38 - Allow for cancels to occur

            //Schedule the Update Jobs to run on System Data
            dependsOn = ScheduleJobs(dependsOn, 
                                     TaskFlowRoute.Update,
                                     m_SystemJobConfigBulkJobSchedulerLookup);
            
            // //Have drivers consolidate their data (Generic TaskSystem Update -> TaskDriver results)
            dependsOn = m_DriverDataStreamBulkJobScheduler.Schedule(dependsOn,
                                                                    IProxyDataStream.CONSOLIDATE_FOR_FRAME_SCHEDULE_FUNCTION);

            //TODO: #38 - Allow for cancels on the drivers to occur
            
            // //Have drivers to do their own generic work if necessary
            dependsOn = ScheduleJobs(dependsOn,
                                     TaskFlowRoute.Update,
                                     m_DriverJobConfigBulkJobSchedulerLookup);

            //Ensure this system's dependency is written back
            return dependsOn;
        }

        private JobHandle ScheduleJobs(JobHandle dependsOn, 
                                       TaskFlowRoute route,
                                       Dictionary<TaskFlowRoute, BulkJobScheduler<IJobConfig>> lookup)
        {
            BulkJobScheduler<IJobConfig> scheduler = lookup[route];
            return scheduler.Schedule(dependsOn,
                                      IJobConfig.PREPARE_AND_SCHEDULE_FUNCTION);
        }


        //*************************************************************************************************************
        // SAFETY
        //*************************************************************************************************************

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void Debug_EnsureTaskDriverSystemRelationship(TTaskDriver taskDriver)
        {
            if (taskDriver.TaskSystem != this)
            {
                throw new InvalidOperationException($"{taskDriver} is part of system {taskDriver.TaskSystem} but it should be {this}!");
            }

            if (m_TaskDrivers.Contains(taskDriver))
            {
                throw new InvalidOperationException($"Trying to add {taskDriver} to {this}'s list of Task Drivers but it is already there!");
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void Debug_EnsureDataStreamIntegrity(IProxyDataStream dataStream, Type expectedType)
        {
            if (dataStream == null)
            {
                throw new InvalidOperationException($"Data Stream is null! Possible causes: "
                                                  + $"\n1. The incorrect reference to a {typeof(ProxyDataStream<>).Name}<{expectedType.Name}> was passed in such as referencing a hidden variable or something not defined on this class or one of this classes TaskDrivers. {typeof(ProxyDataStream<>)}'s are created via reflection in the constructor of this class and TaskDrivers."
                                                  + $"\n2. The {nameof(ConfigureJobFor)} function wasn't called from {nameof(OnCreate)}. The reflection to create {typeof(ProxyDataStream<>).Name}<{expectedType.Name}>'s hasn't happened yet.");
            }

            if (!m_TaskFlowGraph.IsDataStreamRegistered(dataStream))
            {
                throw new InvalidOperationException($"DataStream of {dataStream.DebugString} was not registered with the {nameof(TaskFlowGraph)}! Was it defined as a part of this class or TaskDrivers associated with this class?");
            }
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void Debug_EnsureNotHardened(IProxyDataStream dataStream, TaskFlowRoute route)
        {
            if (m_IsHardened)
            {
                throw new InvalidOperationException($"Trying to create a {route} job on {m_TaskFlowGraph.GetDebugString(dataStream)} but the create phase for systems is complete! Please ensure that you configure your jobs in the {nameof(OnCreate)} or earlier.");
            }
        }

        
    }

    //TODO: Might be able to get rid of this
    public abstract class AbstractTaskSystem : AbstractAnvilSystemBase
    {
    }
}