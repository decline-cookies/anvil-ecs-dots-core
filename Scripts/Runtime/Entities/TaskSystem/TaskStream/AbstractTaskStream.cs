using System;

namespace Anvil.Unity.DOTS.Entities.Tasks
{
    /// <summary>
    /// Represents a stream of data for use in the task system via <see cref="AbstractTaskDriver"/> and/or
    /// <see cref="AbstractTaskSystem"/>
    /// </summary>
    public abstract class AbstractTaskStream
    {
        private readonly string m_TypeString;
        
        internal abstract bool IsCancellable { get; }
        internal abstract AbstractEntityProxyDataStream GetDataStream();
        internal abstract AbstractEntityProxyDataStream GetPendingCancelDataStream();

        protected AbstractTaskStream()
        {
            Type type = GetType();
            
            //TODO: Extract to Anvil-CSharp Util method -Used in AbstractJobConfig as well
            m_TypeString = type.IsGenericType
                ? $"{type.Name[..^2]}<{type.GenericTypeArguments[0].Name}>"
                : type.Name;
        }

        public override string ToString()
        {
            return m_TypeString;
        }
    }
}