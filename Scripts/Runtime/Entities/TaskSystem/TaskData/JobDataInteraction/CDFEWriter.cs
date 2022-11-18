using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Entities;

namespace Anvil.Unity.DOTS.Entities.Tasks
{
    /// <summary>
    /// Represents a <see cref="ComponentDataFromEntity{T}"/> that can be read from and written to in
    /// parallel. It sets the <see cref="NativeDisableParallelForRestrictionAttribute"/> on the CDFE.
    /// To be used in jobs that allow for updating a specific instance in the CDFE.
    /// </summary>
    /// <remarks>
    /// NOTE: The <see cref="ComponentDataFromEntity{T}"/> has the
    /// <see cref="NativeDisableContainerSafetyRestrictionAttribute"/> applied meaning that Unity will not issue
    /// safety warnings when using it in jobs. This is because there might be many jobs of the same type but
    /// representing different <see cref="AbstractTaskDriver"/>s and Unity's safety system gets upset if you straddle
    /// across the jobs. 
    /// </remarks>
    /// <typeparam name="T">The type of <see cref="IComponentData"/> to update.</typeparam>
    [BurstCompatible]
    public struct CDFEWriter<T>
        where T : struct, IComponentData
    {
        [NativeDisableContainerSafetyRestriction][NativeDisableParallelForRestriction] private ComponentDataFromEntity<T> m_CDFE;

        internal CDFEWriter(SystemBase system)
        {
            m_CDFE = system.GetComponentDataFromEntity<T>(false);
        }

        /// <summary>
        /// Gets/Sets the <typeparamref name="T"/> that corresponds to the passed <see cref="Entity"/>
        /// </summary>
        /// <param name="entity">The <see cref="Entity"/> to lookup the data</param>
        public T this[Entity entity]
        {
            get => m_CDFE[entity];
            set => m_CDFE[entity] = value;
        }
    }
}