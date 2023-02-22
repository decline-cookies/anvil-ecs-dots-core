using Anvil.CSharp.Collections;
using Anvil.CSharp.Logging;
using Anvil.Unity.DOTS.Jobs;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;

namespace Anvil.Unity.DOTS.Entities
{
    /// <summary>
    /// System that helps in spawning new <see cref="Entity"/>s and uses <see cref="IEntitySpawnDefinition"/>s
    /// to do so.
    /// </summary>
    /// <remarks>
    /// By default, this system updates in <see cref="SimulationSystemGroup"/> but can be configured by subclassing
    /// and using the <see cref="UpdateInGroupAttribute"/> to target a different group.
    /// 
    /// By default, this system uses the <see cref="EndSimulationEntityCommandBufferSystem"/> to playback the
    /// generated <see cref="EntityCommandBuffer"/>s. This can be configured by subclassing and using the
    /// <see cref="UseCommandBufferSystemAttribute"/> to target a different <see cref="EntityCommandBufferSystem"/>
    /// </remarks>
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UseCommandBufferSystem(typeof(EndSimulationEntityCommandBufferSystem))]
    public partial class EntitySpawnSystem : AbstractAnvilSystemBase
    {
        private EntityCommandBufferSystem m_CommandBufferSystem;
        private readonly AccessControlledValue<NativeParallelHashMap<long, EntityArchetype>> m_EntityArchetypes;

        private readonly Dictionary<Type, IEntitySpawner> m_EntitySpawners;
        private readonly HashSet<IEntitySpawner> m_ActiveEntitySpawners;

        public EntitySpawnSystem()
        {
            m_EntitySpawners = new Dictionary<Type, IEntitySpawner>();
            m_ActiveEntitySpawners = new HashSet<IEntitySpawner>();
            m_EntityArchetypes = new AccessControlledValue<NativeParallelHashMap<long, EntityArchetype>>(new NativeParallelHashMap<long, EntityArchetype>(ChunkUtil.MaxElementsPerChunk<EntityArchetype>(), Allocator.Persistent));
        }

        protected override void OnCreate()
        {
            base.OnCreate();

            Type type = GetType();
            Type commandBufferSystemType = type.GetCustomAttribute<UseCommandBufferSystemAttribute>().CommandBufferSystemType;
            m_CommandBufferSystem = (EntityCommandBufferSystem)World.GetOrCreateSystem(commandBufferSystemType);

            //Default to being off, a call to a SpawnDeferred function will enable it
            Enabled = false;
        }

        protected override void OnDestroy()
        {
            m_ActiveEntitySpawners.Clear();
            m_EntityArchetypes.Dispose();
            m_EntitySpawners.DisposeAllValuesAndClear();
            base.OnDestroy();
        }


        //*************************************************************************************************************
        // SPAWN DEFERRED
        //*************************************************************************************************************

        /// <summary>
        /// Spawns an <see cref="Entity"/> with the given definition later on when the associated
        /// <see cref="EntityCommandBufferSystem"/> runs.
        /// </summary>
        /// <remarks>
        /// Will enable the system to be run for at least one frame. If no more spawn requests come in, the system
        /// will disable itself until more requests come in.
        /// </remarks>
        /// <param name="spawnDefinition">
        /// The <see cref="IEntitySpawnDefinition"/> to populate the created <see cref="Entity"/> with.
        /// </param>
        /// <typeparam name="TEntitySpawnDefinition">The type of <see cref="IEntitySpawnDefinition"/></typeparam>
        public void SpawnDeferred<TEntitySpawnDefinition>(TEntitySpawnDefinition spawnDefinition)
            where TEntitySpawnDefinition : unmanaged, IEntitySpawnDefinition
        {
            EntitySpawner<TEntitySpawnDefinition> entitySpawner = GetOrCreateEntitySpawner<EntitySpawner<TEntitySpawnDefinition>, TEntitySpawnDefinition>();
            entitySpawner.SpawnDeferred(spawnDefinition);

            EnableSystem(entitySpawner);
        }

        /// <summary>
        /// Spawns multiple <see cref="Entity"/>s with the given definitions later on when the associated
        /// <see cref="EntityCommandBufferSystem"/> runs.
        /// </summary>
        /// <remarks>
        /// Will enable the system to be run for at least one frame. If no more spawn requests come in, the system
        /// will disable itself until more requests come in.
        /// </remarks>
        /// <param name="spawnDefinitions">
        /// The <see cref="IEntitySpawnDefinition"/>s to populate the created <see cref="Entity"/>s with.
        /// </param>
        /// <typeparam name="TEntitySpawnDefinition">The type of <see cref="IEntitySpawnDefinition"/></typeparam>
        public void SpawnDeferred<TEntitySpawnDefinition>(NativeArray<TEntitySpawnDefinition> spawnDefinitions)
            where TEntitySpawnDefinition : unmanaged, IEntitySpawnDefinition
        {
            EntitySpawner<TEntitySpawnDefinition> entitySpawner = GetOrCreateEntitySpawner<EntitySpawner<TEntitySpawnDefinition>, TEntitySpawnDefinition>();
            entitySpawner.SpawnDeferred(spawnDefinitions);

            EnableSystem(entitySpawner);
        }

        /// <inheritdoc cref="SpawnDeferred{TEntitySpawnDefinition}(NativeArray{TEntitySpawnDefinition})"/>
        public void SpawnDeferred<TEntitySpawnDefinition>(ICollection<TEntitySpawnDefinition> spawnDefinitions)
            where TEntitySpawnDefinition : unmanaged, IEntitySpawnDefinition
        {
            NativeArray<TEntitySpawnDefinition> nativeArraySpawnDefinitions = new NativeArray<TEntitySpawnDefinition>(spawnDefinitions.Count, Allocator.Temp);
            int index = 0;
            foreach (TEntitySpawnDefinition spawnDefinition in spawnDefinitions)
            {
                nativeArraySpawnDefinitions[index] = spawnDefinition;
                index++;
            }

            SpawnDeferred(nativeArraySpawnDefinitions);
        }


        //*************************************************************************************************************
        // SPAWN IN A JOB
        //*************************************************************************************************************

        /// <summary>
        /// Returns a <see cref="EntitySpawnWriter{TEntitySpawnDefinition}"/> to enable queueing
        /// up <see cref="IEntitySpawnDefinition"/>s to spawn during the system's update phase while in a job.
        /// </summary>
        /// <param name="entitySpawnWriter">The <see cref="EntitySpawnWriter{TEntitySpawnDefinition}"/> to use</param>
        /// <typeparam name="TEntitySpawnDefinition">The type of <see cref="IEntitySpawnDefinition"/></typeparam>
        /// <returns>
        /// A <see cref="JobHandle"/> representing when the <see cref="EntitySpawnWriter{TEntitySpawnDefinition}"/>
        /// can be used.
        /// </returns>
        public JobHandle AcquireEntitySpawnWriterAsync<TEntitySpawnDefinition>(out EntitySpawnWriter<TEntitySpawnDefinition> entitySpawnWriter)
            where TEntitySpawnDefinition : unmanaged, IEntitySpawnDefinition
        {
            EntitySpawner<TEntitySpawnDefinition> entitySpawner = GetOrCreateEntitySpawner<EntitySpawner<TEntitySpawnDefinition>, TEntitySpawnDefinition>();

            EnableSystem(entitySpawner);

            return entitySpawner.AcquireEntitySpawnWriterAsync(out entitySpawnWriter);
        }

        /// <summary>
        /// Allows the system to know when other jobs have finished trying to queue
        /// up <see cref="IEntitySpawnDefinition"/>s to be spawned.
        /// </summary>
        /// <param name="dependsOn">The <see cref="JobHandle"/> to wait on</param>
        /// <typeparam name="TEntitySpawnDefinition">The type of <see cref="IEntitySpawnDefinition"/></typeparam>
        public void ReleaseEntitySpawnWriterAsync<TEntitySpawnDefinition>(JobHandle dependsOn)
            where TEntitySpawnDefinition : unmanaged, IEntitySpawnDefinition
        {
            EntitySpawner<TEntitySpawnDefinition> entitySpawner = GetOrCreateEntitySpawner<EntitySpawner<TEntitySpawnDefinition>, TEntitySpawnDefinition>();
            entitySpawner.ReleaseEntitySpawnWriterAsync(dependsOn);
        }


        //*************************************************************************************************************
        // SPAWN DEFERRED WITH PROTOTYPE
        //*************************************************************************************************************

        /// <summary>
        /// Spawns an <see cref="Entity"/> with the given definition by cloning the passed in prototype
        /// <see cref="Entity"/> when the associated <see cref="EntityCommandBufferSystem"/> runs later on.
        /// </summary>
        /// <remarks>
        /// Will enable the system to be run for at least one frame. If no more spawn requests come in, the system
        /// will disable itself until more requests come in.
        /// </remarks>
        /// <param name="prototype">The prototype <see cref="Entity"/> to clone.</param>
        /// <param name="spawnDefinition">
        /// The <see cref="IEntitySpawnDefinition"/> to populate the created <see cref="Entity"/> with.
        /// </param>
        /// <param name="shouldDestroyPrototype">
        /// If true, will destroy the prototype <see cref="Entity"/> after creation.
        /// </param>
        /// <typeparam name="TEntitySpawnDefinition">The type of <see cref="IEntitySpawnDefinition"/></typeparam>
        public void SpawnDeferred<TEntitySpawnDefinition>(Entity prototype, TEntitySpawnDefinition spawnDefinition, bool shouldDestroyPrototype)
            where TEntitySpawnDefinition : unmanaged, IEntitySpawnDefinition
        {
            EntityPrototypeSpawner<TEntitySpawnDefinition> entitySpawner = GetOrCreateEntitySpawner<EntityPrototypeSpawner<TEntitySpawnDefinition>, TEntitySpawnDefinition>();
            entitySpawner.Spawn(prototype, spawnDefinition, shouldDestroyPrototype);

            EnableSystem(entitySpawner);
        }

        /// <summary>
        /// Spawns <see cref="Entity"/>s with the given definitions by cloning the passed in prototype
        /// <see cref="Entity"/> when the associated <see cref="EntityCommandBufferSystem"/> runs later on.
        /// </summary>
        /// <remarks>
        /// Will enable the system to be run for at least one frame. If no more spawn requests come in, the system
        /// will disable itself until more requests come in.
        /// </remarks>
        /// <param name="prototype">The prototype <see cref="Entity"/> to clone.</param>
        /// <param name="spawnDefinitions">
        /// The <see cref="IEntitySpawnDefinition"/>s to populate the created <see cref="Entity"/>s with.
        /// </param>
        /// <param name="shouldDestroyPrototype">
        /// If true, will destroy the prototype <see cref="Entity"/> after creation.
        /// </param>
        /// <typeparam name="TEntitySpawnDefinition">The type of <see cref="IEntitySpawnDefinition"/></typeparam>
        public void SpawnDeferred<TEntitySpawnDefinition>(Entity prototype, ICollection<TEntitySpawnDefinition> spawnDefinitions, bool shouldDestroyPrototype)
            where TEntitySpawnDefinition : unmanaged, IEntitySpawnDefinition
        {
            EntityPrototypeSpawner<TEntitySpawnDefinition> entitySpawner = GetOrCreateEntitySpawner<EntityPrototypeSpawner<TEntitySpawnDefinition>, TEntitySpawnDefinition>();
            entitySpawner.Spawn(prototype, spawnDefinitions, shouldDestroyPrototype);

            EnableSystem(entitySpawner);
        }


        //*************************************************************************************************************
        // SPAWN IN A JOB WITH PROTOTYPE
        //*************************************************************************************************************

        /// <summary>
        /// Returns a <see cref="EntityPrototypeSpawnWriter{TEntitySpawnDefinition}"/> to enable queueing
        /// up <see cref="IEntitySpawnDefinition"/>s to spawn during the system's update phase while in a job.
        /// </summary>
        /// <param name="entitySpawnWriter">The <see cref="EntityPrototypeSpawnWriter{TEntitySpawnDefinition}"/> to use</param>
        /// <typeparam name="TEntitySpawnDefinition">The type of <see cref="IEntitySpawnDefinition"/></typeparam>
        /// <returns>
        /// A <see cref="JobHandle"/> representing when the <see cref="EntityPrototypeSpawnWriter{TEntitySpawnDefinition}"/>
        /// can be used.
        /// </returns>
        public JobHandle AcquireEntityPrototypeSpawnWriterAsync<TEntitySpawnDefinition>(out EntityPrototypeSpawnWriter<TEntitySpawnDefinition> entitySpawnWriter)
            where TEntitySpawnDefinition : unmanaged, IEntitySpawnDefinition
        {
            EntityPrototypeSpawner<TEntitySpawnDefinition> entitySpawner = GetOrCreateEntitySpawner<EntityPrototypeSpawner<TEntitySpawnDefinition>, TEntitySpawnDefinition>();

            EnableSystem(entitySpawner);

            return entitySpawner.AcquireEntitySpawnWriterAsync(out entitySpawnWriter);
        }

        /// <summary>
        /// Allows the system to know when other jobs have finished trying to queue
        /// up <see cref="IEntitySpawnDefinition"/>s to be spawned.
        /// </summary>
        /// <param name="dependsOn">The <see cref="JobHandle"/> to wait on</param>
        /// <typeparam name="TEntitySpawnDefinition">The type of <see cref="IEntitySpawnDefinition"/></typeparam>
        public void ReleaseEntityPrototypeSpawnWriterAsync<TEntitySpawnDefinition>(JobHandle dependsOn)
            where TEntitySpawnDefinition : unmanaged, IEntitySpawnDefinition
        {
            EntityPrototypeSpawner<TEntitySpawnDefinition> entitySpawner = GetOrCreateEntitySpawner<EntityPrototypeSpawner<TEntitySpawnDefinition>, TEntitySpawnDefinition>();
            entitySpawner.ReleaseEntitySpawnWriterAsync(dependsOn);
        }

        //*************************************************************************************************************
        // SPAWN IMMEDIATE
        //*************************************************************************************************************

        /// <summary>
        /// Spawns an <see cref="Entity"/> with the given definition immediately and returns it.
        /// </summary>
        /// <remarks>
        /// This will not enable this system.
        /// </remarks>
        /// <param name="spawnDefinition">
        /// The <see cref="IEntitySpawnDefinition"/> to populate the created <see cref="Entity"/> with.
        /// </param>
        /// <typeparam name="TEntitySpawnDefinition">The type of <see cref="IEntitySpawnDefinition"/></typeparam>
        /// <returns>The created <see cref="Entity"/></returns>
        public Entity SpawnImmediate<TEntitySpawnDefinition>(TEntitySpawnDefinition spawnDefinition)
            where TEntitySpawnDefinition : unmanaged, IEntitySpawnDefinition
        {
            EntitySpawner<TEntitySpawnDefinition> entitySpawner = GetOrCreateEntitySpawner<EntitySpawner<TEntitySpawnDefinition>, TEntitySpawnDefinition>();
            return entitySpawner.SpawnImmediate(spawnDefinition);
        }

        //TODO: Implement a SpawnImmediate that takes in a NativeArray or ICollection if needed.

        //*************************************************************************************************************
        // SPAWN IMMEDIATE WITH PROTOTYPE
        //*************************************************************************************************************

        /// <summary>
        /// Spawns an <see cref="Entity"/> with the given definition immediately by cloning the passed in prototype
        /// <see cref="Entity"/> and returns it immediately. 
        /// </summary>
        /// <remarks>
        /// This will not enable this system.
        /// </remarks>
        /// <param name="prototype">The prototype <see cref="Entity"/> to clone</param>
        /// <param name="spawnDefinition">
        /// The <see cref="IEntitySpawnDefinition"/> to populate the created <see cref="Entity"/> with.
        /// </param>
        /// <param name="shouldDestroyPrototype">
        /// If true, will destroy the prototype <see cref="Entity"/> after creation.
        /// </param>
        /// <typeparam name="TEntitySpawnDefinition">The type of <see cref="IEntitySpawnDefinition"/></typeparam>
        /// <returns>The created <see cref="Entity"/></returns>
        public Entity SpawnImmediate<TEntitySpawnDefinition>(Entity prototype, TEntitySpawnDefinition spawnDefinition, bool shouldDestroyPrototype = false)
            where TEntitySpawnDefinition : unmanaged, IEntitySpawnDefinition
        {
            EntityPrototypeSpawner<TEntitySpawnDefinition> entitySpawner = GetOrCreateEntitySpawner<EntityPrototypeSpawner<TEntitySpawnDefinition>, TEntitySpawnDefinition>();
            return entitySpawner.SpawnImmediate(prototype, spawnDefinition, shouldDestroyPrototype);
        }

        //TODO: Implement a SpawnImmediate that takes in a NativeArray or ICollection if needed.

        private void EnableSystem(IEntitySpawner entitySpawner)
        {
            //By using this, we're writing immediately, but will need to execute later on when the system runs
            Enabled = true;
            m_ActiveEntitySpawners.Add(entitySpawner);
        }

        protected override void OnUpdate()
        {
            Dependency = ScheduleActiveEntitySpawners(Dependency);

            //Ensure we're turned back off
            m_ActiveEntitySpawners.Clear();
            Enabled = false;
        }

        private JobHandle ScheduleActiveEntitySpawners(JobHandle dependsOn)
        {
            NativeArray<JobHandle> dependencies = new NativeArray<JobHandle>(m_ActiveEntitySpawners.Count + 1, Allocator.Temp, NativeArrayOptions.UninitializedMemory);
            dependencies[^1] = m_EntityArchetypes.AcquireAsync(AccessType.SharedRead, out NativeParallelHashMap<long, EntityArchetype> entityArchetypesLookup);

            //All the active spawners that need to go this frame are given a chance to go ahead and run their spawn jobs
            int index = 0;
            foreach (IEntitySpawner entitySpawner in m_ActiveEntitySpawners)
            {
                //Normally for each ECB created, you want to add the job handle for producer to the command buffer
                //system. However, we know that all these handles will be combined, so we can just do one call at the
                //end of the function. Creating here and passing into the Schedule function allows us to see the 
                //creation and AddJobHandleForProducer calls close by so we know we're adhering to the "pattern".
                EntityCommandBuffer ecb = m_CommandBufferSystem.CreateCommandBuffer();
                dependencies[index] = entitySpawner.Schedule(dependsOn, ref ecb, entityArchetypesLookup);
                index++;
            }

            dependsOn = JobHandle.CombineDependencies(dependencies);
            m_EntityArchetypes.ReleaseAsync(dependsOn);
            m_CommandBufferSystem.AddJobHandleForProducer(dependsOn);
            return dependsOn;
        }

        private TEntitySpawner GetOrCreateEntitySpawner<TEntitySpawner, TEntitySpawnDefinition>()
            where TEntitySpawner : IEntitySpawner, new()
            where TEntitySpawnDefinition : unmanaged, IEntitySpawnDefinition
        {
            Type spawnerType = typeof(TEntitySpawner);
            Type definitionType = typeof(TEntitySpawnDefinition);
            // ReSharper disable once InvertIf
            if (!m_EntitySpawners.TryGetValue(spawnerType, out IEntitySpawner entitySpawner))
            {
                // ReSharper disable once SuggestVarOrType_SimpleTypes
                using var handle = m_EntityArchetypes.AcquireWithHandle(AccessType.ExclusiveWrite);
                CreateEntityArchetypeForDefinition<TEntitySpawnDefinition>(handle.Value, out EntityArchetype entityArchetype, out long entityArchetypeHash);
                entitySpawner = new TEntitySpawner();
                entitySpawner.Init(
                    EntityManager,
                    entityArchetype);
                m_EntitySpawners.Add(spawnerType, entitySpawner);
            }

            return (TEntitySpawner)entitySpawner;
        }

        private void CreateEntityArchetypeForDefinition<TEntitySpawnDefinition>(
            NativeParallelHashMap<long, EntityArchetype> entityArchetypesLookup,
            out EntityArchetype entityArchetype,
            out long entityArchetypeHash)
            where TEntitySpawnDefinition : unmanaged, IEntitySpawnDefinition
        {
            Type definitionType = typeof(TEntitySpawnDefinition);
            entityArchetypeHash = BurstRuntime.GetHashCode64(definitionType);
            if (entityArchetypesLookup.TryGetValue(entityArchetypeHash, out entityArchetype))
            {
                return;
            }

            if (!definitionType.IsValueType)
            {
                throw new InvalidOperationException($"Definition Type of {definitionType.GetReadableName()} should be a readonly struct but it is not.");
            }

            if (definitionType.GetCustomAttribute<BurstCompatibleAttribute>() == null)
            {
                throw new InvalidOperationException($"Definition Type of {definitionType.GetReadableName()} should have the {nameof(BurstCompatibleAttribute)} set but it does not.");
            }

            if (definitionType.GetCustomAttribute<IsReadOnlyAttribute>() == null)
            {
                throw new InvalidOperationException($"Definition Type of {definitionType.GetReadableName()} should be readonly but it is not.");
            }

            TEntitySpawnDefinition defaultInstance = default;
            entityArchetype = EntityManager.CreateArchetype(defaultInstance.RequiredComponents);
            entityArchetypesLookup.Add(entityArchetypeHash, entityArchetype);
        }
    }
}