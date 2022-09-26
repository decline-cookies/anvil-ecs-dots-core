using System;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;

namespace Anvil.Unity.DOTS.Entities
{
    internal class DataStreamNode : AbstractNode
    {
        private readonly Dictionary<Type, byte> m_ResolveTargetLookup;
        private readonly DataStreamNodeLookup m_Lookup;

        public AbstractProxyDataStream DataStream
        {
            get;
        }
        
        public AbstractTaskStream TaskStream
        {
            get;
        }

        public DataStreamNode(DataStreamNodeLookup lookup,
                              AbstractProxyDataStream dataStream,
                              TaskFlowGraph taskFlowGraph,
                              ITaskSystem taskSystem,
                              ITaskDriver taskDriver,
                              AbstractTaskStream taskStream) : base(taskFlowGraph, taskSystem, taskDriver)
        {
            m_Lookup = lookup;
            DataStream = dataStream;
            TaskStream = taskStream;

            m_ResolveTargetLookup = new Dictionary<Type, byte>();
        }

        protected override void DisposeSelf()
        {
            DataStream.Dispose();
            m_ResolveTargetLookup.Clear();

            base.DisposeSelf();
        }

        public override string ToString()
        {
            return $"{DataStream} as part of {TaskStream} located in {TaskDebugUtil.GetLocation(TaskSystem, TaskDriver)}";
        }

        public void RegisterAsResolveTarget(ResolveTargetAttribute resolveTargetAttribute)
        {
            Type type = resolveTargetAttribute.ResolveTarget.GetType();
            byte value = (byte)resolveTargetAttribute.ResolveTarget;

            m_ResolveTargetLookup.Add(type, value);
        }

        public bool IsResolveTarget<TResolveTarget>(TResolveTarget resolveTarget)
            where TResolveTarget : Enum
        {
            Type type = typeof(TResolveTarget);
            if (!m_ResolveTargetLookup.TryGetValue(type, out byte value))
            {
                return false;
            }
            
            ResolveTargetUtil.Debug_EnsureEnumValidity(resolveTarget);
            byte storedResolveTarget = UnsafeUtility.As<TResolveTarget, byte>(ref resolveTarget);
            return value == storedResolveTarget;
        }
    }
}