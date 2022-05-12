using Anvil.Unity.DOTS.Collections;
using System;
using System.Diagnostics.CodeAnalysis;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;

namespace Anvil.Unity.DOTS.Jobs
{
    /// <summary>
    /// A replacement for <see cref="IJobFor"/> when the number of work items is not known at Schedule time
    /// and you are using a <see cref="DeferredNativeArray{T}"/>
    /// </summary>
    [JobProducerType(typeof(JobDeferredNativeArrayForExtensions.JobDeferredNativeArrayForProducer<>))]
    public interface IJobDeferredNativeArrayFor
    {
        /// <summary>
        /// Implement this method to perform work against a specific iteration index.
        /// </summary>
        /// <param name="index">The index of the <see cref="NativeArray{T}"/> from a <see cref="DeferredNativeArray{T}"/></param>
        void Execute(int index);
    }

    public static class JobDeferredNativeArrayForExtensions
    {
        public static unsafe JobHandle ScheduleParallel<TJob, T>(this TJob jobData,
                                                                 DeferredNativeArray<T> deferredNativeArray,
                                                                 int batchSize,
                                                                 JobHandle dependsOn = default)
            where TJob : struct, IJobDeferredNativeArrayFor
            where T : struct
        {
            void* atomicSafetyHandlePtr = null;

#if ENABLE_UNITY_COLLECTIONS_CHECKS
            atomicSafetyHandlePtr = DeferredNativeArrayUnsafeUtility.GetSafetyHandlePointer(ref deferredNativeArray);
#endif

#if UNITY_2020_2_OR_NEWER
            const ScheduleMode SCHEDULE_MODE = ScheduleMode.Parallel;
#else
            const ScheduleMode SCHEDULE_MODE = ScheduleMode.Batched;
#endif

            JobsUtility.JobScheduleParameters scheduleParameters = new JobsUtility.JobScheduleParameters(UnsafeUtility.AddressOf(ref jobData),
                                                                                                         JobDeferredNativeArrayForProducer<TJob>.Initialize(),
                                                                                                         dependsOn,
                                                                                                         SCHEDULE_MODE);

            dependsOn = JobsUtility.ScheduleParallelForDeferArraySize(ref scheduleParameters,
                                                                      batchSize,
                                                                      DeferredNativeArrayUnsafeUtility.GetBufferInfoUnchecked(ref deferredNativeArray),
                                                                      atomicSafetyHandlePtr);

            return dependsOn;
        }


        internal struct JobDeferredNativeArrayForProducer<TJob>
            where TJob : struct, IJobDeferredNativeArrayFor
        {
            // ReSharper disable once StaticMemberInGenericType
            private static IntPtr s_JobReflectionData;

            public static IntPtr Initialize()
            {
                if (s_JobReflectionData == IntPtr.Zero)
                {
#if UNITY_2020_2_OR_NEWER
                    s_JobReflectionData = JobsUtility.CreateJobReflectionData(typeof(JobDeferredNativeArrayProducer<TJob, T>),
                                                                              typeof(TJob),
                                                                              (ExecuteJobFunction)Execute);
#else
                    s_JobReflectionData = JobsUtility.CreateJobReflectionData(typeof(TJob),
                                                                              typeof(TJob),
                                                                              JobType.ParallelFor,
                                                                              (ExecuteJobFunction)Execute);
#endif
                }

                return s_JobReflectionData;
            }

            private delegate void ExecuteJobFunction(ref TJob jobData,
                                                     IntPtr additionalPtr,
                                                     IntPtr bufferRangePatchData,
                                                     ref JobRanges ranges,
                                                     int jobIndex);


            [SuppressMessage("ReSharper", "MemberCanBePrivate.Global", Justification = "Required by Burst.")]
            public static unsafe void Execute(ref TJob jobData,
                                              IntPtr additionalPtr,
                                              IntPtr bufferRangePatchData,
                                              ref JobRanges ranges,
                                              int jobIndex)
            {
                while (true)
                {
                    if (!JobsUtility.GetWorkStealingRange(ref ranges, jobIndex, out int beginIndex, out int endIndex))
                    {
                        return;
                    }

#if ENABLE_UNITY_COLLECTIONS_CHECKS
                    JobsUtility.PatchBufferMinMaxRanges(bufferRangePatchData, UnsafeUtility.AddressOf(ref jobData), beginIndex, endIndex - beginIndex);
#endif

                    for (int index = beginIndex; index < endIndex; ++index)
                    {
                        jobData.Execute(index);
                    }
                }
            }
        }
    }
}