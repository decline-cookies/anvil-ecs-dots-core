using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Anvil.Unity.DOTS.Entities
{
    internal class DataStreamNodeLookup : AbstractNodeLookup
    {
        private readonly Dictionary<AbstractProxyDataStream, DataStreamNode> m_NodesByDataStream;

        public byte Context
        {
            get;
        }

        public DataStreamNodeLookup(TaskFlowGraph taskFlowGraph,
                                    ITaskSystem taskSystem,
                                    ITaskDriver taskDriver) : base(taskFlowGraph, taskSystem, taskDriver)
        {
            m_NodesByDataStream = new Dictionary<AbstractProxyDataStream, DataStreamNode>();
            Context = taskDriver?.Context ?? taskSystem.Context;
        }

        protected override void DisposeSelf()
        {
            foreach (DataStreamNode node in m_NodesByDataStream.Values)
            {
                node.Dispose();
            }

            m_NodesByDataStream.Clear();

            base.DisposeSelf();
        }

        public DataStreamNode CreateNode(AbstractTaskStream taskStream, AbstractProxyDataStream dataStream)
        {
            Debug_EnsureNoDuplicateNodes(dataStream);
            DataStreamNode node = new DataStreamNode(this,
                                                     dataStream,
                                                     TaskFlowGraph,
                                                     TaskSystem,
                                                     TaskDriver,
                                                     taskStream);
            m_NodesByDataStream.Add(dataStream, node);
            return node;
        }

        public bool IsDataStreamRegistered(AbstractProxyDataStream dataStream)
        {
            return m_NodesByDataStream.ContainsKey(dataStream);
        }

        public DataStreamNode this[AbstractProxyDataStream dataStream]
        {
            get
            {
                Debug_EnsureExists(dataStream);
                return m_NodesByDataStream[dataStream];
            }
        }

        public void PopulateWithDataStreams(List<AbstractProxyDataStream> dataStreams)
        {
            foreach (AbstractProxyDataStream dataStream in m_NodesByDataStream.Keys)
            {
                dataStreams.Add(dataStream);
            }
        }

        public void PopulateWithResolveTargetDataStreams<TResolveTarget>(JobResolveTargetMapping jobResolveTargetMapping, TResolveTarget resolveTarget)
            where TResolveTarget : Enum
        {
            foreach (DataStreamNode node in m_NodesByDataStream.Values)
            {
                if (!node.IsResolveTarget(resolveTarget))
                {
                    continue;
                }

                jobResolveTargetMapping.RegisterDataStream(resolveTarget, node.DataStream, Context);
            }
        }

        //*************************************************************************************************************
        // SAFETY
        //*************************************************************************************************************

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void Debug_EnsureNoDuplicateNodes(AbstractProxyDataStream dataStream)
        {
            if (m_NodesByDataStream.ContainsKey(dataStream))
            {
                throw new InvalidOperationException($"Trying to create a new {nameof(DataStreamNode)} with instance of {dataStream} but one already exists!");
            }
        }
        
        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void Debug_EnsureExists(AbstractProxyDataStream dataStream)
        {
            if (!m_NodesByDataStream.ContainsKey(dataStream))
            {
                throw new InvalidOperationException($"Trying to access a {nameof(DataStreamNode)} with instance of {dataStream} but it doesn't exist!");
            }
        }
    }
}