//TODO: RE-ENABLE IF NEEDED
// using Anvil.Unity.DOTS.Data;
// using Anvil.Unity.DOTS.Jobs;
// using System;
// using System.Collections.Generic;
// using Unity.Core;
// using Unity.Entities;
//
// namespace Anvil.Unity.DOTS.Entities
// {
//     /// <summary>
//     /// Job information object to aid with scheduling and populating a job instance.
//     /// Data acquired from this object is guaranteed to have the proper access.
//     /// </summary>
//     public class TaskWorkData
//     {
//         //We don't have to be on the main thread, but it makes sense as a good default
//         private static readonly int SYNCHRONOUS_THREAD_INDEX = ParallelAccessUtil.CollectionIndexForMainThread();
//
//         private readonly Dictionary<Type, AbstractVDWrapper> m_WrappedDataLookup;
//         private readonly byte m_Context;
// #if ENABLE_UNITY_COLLECTIONS_CHECKS
//         private readonly Dictionary<Type, AbstractTaskWorkConfig.DataUsage> m_DataUsageByType;
// #endif
//
//         /// <summary>
//         /// The <see cref="AbstractTaskSystem{TTaskDriver}"/> this job is being scheduled during.
//         /// Calls to <see cref="SystemBase.GetComponentDataFromEntity{T}"/> and similar will attribute dependencies
//         /// correctly.
//         /// </summary>
//         public AbstractTaskSystem<> System
//         {
//             get;
//         }
//
//         /// <summary>
//         /// The <see cref="World"/> this job is being scheduled under.
//         /// </summary>
//         public World World
//         {
//             get;
//         }
//
//         /// <summary>
//         /// Helper function for accessing the <see cref="TimeData"/> normally found on <see cref="SystemBase.Time"/>
//         /// </summary>
//         public ref readonly TimeData Time
//         {
//             get => ref World.Time;
//         }
//
//         internal TaskWorkData(AbstractTaskSystem<> system, byte context)
//         {
//             System = system;
//             m_Context = context;
//             World = System.World;
//             m_WrappedDataLookup = new Dictionary<Type, AbstractVDWrapper>();
// #if ENABLE_UNITY_COLLECTIONS_CHECKS
//             m_DataUsageByType = new Dictionary<Type, AbstractTaskWorkConfig.DataUsage>();
// #endif
//         }
//
//         internal void AddDataWrapper(AbstractVDWrapper dataWrapper)
//         {
// #if ENABLE_UNITY_COLLECTIONS_CHECKS
//             if (m_WrappedDataLookup.ContainsKey(dataWrapper.Type))
//             {
//                 throw new InvalidOperationException($"{this} already contains data registered for {dataWrapper.Type}. Please ensure that data is not registered more than once.");
//             }
// #endif
//             m_WrappedDataLookup.Add(dataWrapper.Type, dataWrapper);
//         }
//
//         private ProxyDataStream<TInstance> GetVirtualData<TInstance>()
//             where TInstance : unmanaged, IProxyData
//         {
//             Type type = typeof(ProxyDataStream<TInstance>);
// #if ENABLE_UNITY_COLLECTIONS_CHECKS
//             if (!m_WrappedDataLookup.ContainsKey(type))
//             {
//                 throw new InvalidOperationException($"Tried to get {nameof(ProxyDataStream<TInstance>)} but it doesn't exist on {this}. Please ensure a \"RequireData\" function was called on the corresponding config.");
//             }
// #endif
//             AbstractVDWrapper wrapper = m_WrappedDataLookup[type];
//             return (ProxyDataStream<TInstance>)wrapper.Data;
//         }
//
//         /// <summary>
//         /// Returns a <see cref="PDSReader{TInstance}"/> for use in a job.
//         /// </summary>
//         /// <typeparam name="TKey">The type of the key</typeparam>
//         /// <typeparam name="TInstance">The type of the data</typeparam>
//         /// <returns>The <see cref="PDSReader{TInstance}"/></returns>
//         public PDSReader<TInstance> GetVDReaderAsync<TInstance>()
//             where TInstance : unmanaged, IProxyData
//         {
//             ProxyDataStream<TInstance> virtualData = GetVirtualData<TInstance>();
//
// #if ENABLE_UNITY_COLLECTIONS_CHECKS
//             CheckUsage(virtualData.Type, AbstractTaskWorkConfig.DataUsage.IterateAsync);
// #endif
//
//             PDSReader<TInstance> reader = virtualData.CreatePDSReader();
//             return reader;
//         }
//
//         /// <summary>
//         /// Returns a <see cref="PDSReader{TInstance}"/> for synchronous use.
//         /// </summary>
//         /// <typeparam name="TKey">The type of the key</typeparam>
//         /// <typeparam name="TInstance">The type of the data</typeparam>
//         /// <returns>The <see cref="PDSReader{TInstance}"/></returns>
//         public PDSReader<TInstance> GetVDReader<TInstance>()
//             where TInstance : unmanaged, IProxyData
//         {
//             ProxyDataStream<TInstance> virtualData = GetVirtualData<TInstance>();
//
// #if ENABLE_UNITY_COLLECTIONS_CHECKS
//             CheckUsage(virtualData.Type, AbstractTaskWorkConfig.DataUsage.Iterate);
// #endif
//
//             PDSReader<TInstance> reader = virtualData.CreatePDSReader();
//             return reader;
//         }
//
//         //         /// <summary>
//         //         /// Returns a <see cref="VDResultsDestination{TResult}"/> for use in a job.
//         //         /// </summary>
//         //         /// <typeparam name="TKey">The type of the key</typeparam>
//         //         /// <typeparam name="TResult">The type of the data</typeparam>
//         //         /// <returns>The <see cref="VDResultsDestination{TResult}"/></returns>
//         //         public VDResultsDestination<TResult> GetVDResultsDestinationAsync<TKey, TResult>()
//         //             where TKey : unmanaged, IEquatable<TKey>
//         //             where TResult : unmanaged, IKeyedData<TKey>
//         //         {
//         //             VirtualData<TKey, TResult> virtualData = GetVirtualData<TKey, TResult>();
//         //             
//         // #if ENABLE_UNITY_COLLECTIONS_CHECKS
//         //             CheckUsage(virtualData.Type, AbstractTaskWorkConfig.DataUsage.ResultsDestinationAsync);
//         // #endif
//         //             
//         //             VDResultsDestination<TResult> resultsDestination = virtualData.CreateVDResultsDestination();
//         //             return resultsDestination;
//         //         }
//
//         //         /// <summary>
//         //         /// Returns a <see cref="VDResultsDestination{TResult}"/> for synchronous use.
//         //         /// </summary>
//         //         /// <typeparam name="TKey">The type of the key</typeparam>
//         //         /// <typeparam name="TResult">The type of the data</typeparam>
//         //         /// <returns>The <see cref="VDResultsDestination{TResult}"/></returns>
//         //         public VDResultsDestination<TResult> GetVDResultsDestination<TKey, TResult>()
//         //             where TKey : unmanaged, IEquatable<TKey>
//         //             where TResult : unmanaged, IKeyedData<TKey>
//         //         {
//         //             VirtualData<TKey, TResult> virtualData = GetVirtualData<TKey, TResult>();
//         //
//         // #if ENABLE_UNITY_COLLECTIONS_CHECKS
//         //             CheckUsage(virtualData.Type, AbstractTaskWorkConfig.DataUsage.ResultsDestination);
//         // #endif
//         //
//         //             VDResultsDestination<TResult> resultsDestination = virtualData.CreateVDResultsDestination();
//         //             return resultsDestination;
//         //         }
//
//         /// <summary>
//         /// Returns a <see cref="PDSUpdater{TInstance}"/> for use in a job.
//         /// </summary>
//         /// <typeparam name="TKey">The type of the key</typeparam>
//         /// <typeparam name="TInstance">The type of the data</typeparam>
//         /// <returns>The <see cref="PDSUpdater{TInstance}"/></returns>
//         public virtual PDSUpdater<TInstance> GetVDUpdaterAsync<TInstance>()
//             where TInstance : unmanaged, IProxyData
//         {
//             ProxyDataStream<TInstance> virtualData = GetVirtualData<TInstance>();
//
// #if ENABLE_UNITY_COLLECTIONS_CHECKS
//             CheckUsage(virtualData.Type, AbstractTaskWorkConfig.DataUsage.UpdateAsync);
// #endif
//
//             PDSUpdater<TInstance> updater = virtualData.CreateVDUpdater(m_Context);
//             return updater;
//         }
//
//         /// <summary>
//         /// Returns a <see cref="PDSUpdater{TInstance}"/> for synchronous use.
//         /// </summary>
//         /// <typeparam name="TKey">The type of the key</typeparam>
//         /// <typeparam name="TInstance">The type of the data</typeparam>
//         /// <returns>The <see cref="PDSUpdater{TInstance}"/></returns>
//         public virtual PDSUpdater<TInstance> GetVDUpdater<TInstance>()
//             where TInstance : unmanaged, IProxyData
//         {
//             ProxyDataStream<TInstance> virtualData = GetVirtualData<TInstance>();
//
// #if ENABLE_UNITY_COLLECTIONS_CHECKS
//             CheckUsage(virtualData.Type, AbstractTaskWorkConfig.DataUsage.Update);
// #endif
//
//             PDSUpdater<TInstance> updater = virtualData.CreateVDUpdater(m_Context);
//             updater.InitForThread(SYNCHRONOUS_THREAD_INDEX);
//             return updater;
//         }
//
//         /// <summary>
//         /// Returns a <see cref="PDSWriter{TInstance}"/> for use in a job.
//         /// </summary>
//         /// <typeparam name="TKey">The type of the key</typeparam>
//         /// <typeparam name="TInstance">The type of the data</typeparam>
//         /// <returns>The <see cref="PDSWriter{TInstance}"/></returns>
//         public virtual PDSWriter<TInstance> GetVDWriterAsync<TInstance>()
//             where TInstance : unmanaged, IProxyData
//         {
//             ProxyDataStream<TInstance> virtualData = GetVirtualData<TInstance>();
//
// #if ENABLE_UNITY_COLLECTIONS_CHECKS
//             CheckUsage(virtualData.Type, AbstractTaskWorkConfig.DataUsage.AddAsync);
// #endif
//
//             PDSWriter<TInstance> writer = virtualData.CreateVDWriter(m_Context);
//             return writer;
//         }
//
//         /// <summary>
//         /// Returns a <see cref="PDSWriter{TInstance}"/> for synchronous use.
//         /// </summary>
//         /// <typeparam name="TKey">The type of the key</typeparam>
//         /// <typeparam name="TInstance">The type of the data</typeparam>
//         /// <returns>The <see cref="PDSWriter{TInstance}"/></returns>
//         public virtual PDSWriter<TInstance> GetVDWriter<TInstance>()
//             where TInstance : unmanaged, IProxyData
//         {
//             ProxyDataStream<TInstance> virtualData = GetVirtualData<TInstance>();
//
// #if ENABLE_UNITY_COLLECTIONS_CHECKS
//             CheckUsage(virtualData.Type, AbstractTaskWorkConfig.DataUsage.Add);
// #endif
//
//             PDSWriter<TInstance> writer = virtualData.CreateVDWriter(m_Context);
//             writer.InitForThread(SYNCHRONOUS_THREAD_INDEX);
//             return writer;
//         }
//
//         public VDResultsDestinationLookup GetVDResultsDestinationLookup<TInstance>()
//             where TInstance : unmanaged, IProxyData
//         {
//             ProxyDataStream<TInstance> virtualData = GetVirtualData<TInstance>();
//             
// #if ENABLE_UNITY_COLLECTIONS_CHECKS
//             //TODO: Fix this, we want to make sure we're added for this usage but type conflict
//             // CheckUsage(virtualData.Type, AbstractTaskWorkConfig.DataUsage.Add);
// #endif
//             VDResultsDestinationLookup resultsDestinationLookup = virtualData.GetOrCreateVDResultsDestinationLookup();
//             return resultsDestinationLookup;
//         }
//
// #if ENABLE_UNITY_COLLECTIONS_CHECKS
//         internal void Debug_NotifyWorkDataOfUsage(Type type, AbstractTaskWorkConfig.DataUsage usage)
//         {
//             m_DataUsageByType.Add(type, usage);
//         }
//
//         private void CheckUsage(Type type, AbstractTaskWorkConfig.DataUsage expectedUsage)
//         {
//             AbstractTaskWorkConfig.DataUsage dataUsage = m_DataUsageByType[type];
//             if (dataUsage != expectedUsage)
//             {
//                 throw new InvalidOperationException($"Trying to get data of {type} with usage of {expectedUsage} but data was required with {dataUsage}. Check the configuration for the right \"Require\" calls.");
//             }
//         }
// #endif
//     }
// }