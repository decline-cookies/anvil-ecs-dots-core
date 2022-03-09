using Unity.Entities;


namespace Anvil.Unity.DOTS.Entities
{
    /// <summary>
    /// A container that provides access to a <see cref="DynamicBuffer{T}" from a single entitiy.
    /// </summary>
    /// <typeparam name="T">The element type of the buffer</typeparam>
    /// <remarks>Allows developers to define jobs with fewer parameters that clearly communicate intent.</remarks>
    public readonly struct BufferFromSingleEntity<T> where T : struct, IBufferElementData
    {
        private readonly BufferFromEntity<T> m_Lookup;
        private readonly Entity m_Entity;

        /// <summary>
        /// Creates a new <see cref="BufferFromSingleEntity{T}"/>.
        /// </summary>
        /// <param name="lookup">The <see cref="BufferFromEntity{T}" /> lookup to read the buffer reference from.</param>
        /// <param name="entity">The <see cref="Entity" /> that the <see cref="DynamicBuffer{T}" /> is on.</param>
        public BufferFromSingleEntity(BufferFromEntity<T> lookup, Entity entity)
        {
            m_Lookup = lookup;
            m_Entity = entity;
        }

        /// <summary>
        /// Gets the <see cref="DynamicBuffer{T}" />. 
        /// Call during job execution.
        /// </summary>
        /// <returns>The <see cref="DynamicBuffer{T}" /> instance</returns>
        public DynamicBuffer<T> GetBuffer()
        {
            return m_Lookup[m_Entity];
        }
    }

}