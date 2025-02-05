using System.Collections;
using System.Collections.Generic;
using CSGraph.Implementation;
using System.Linq;
using System;

namespace CSGraph
{
    public class Graph : GraphImpl<Vertex<Connection>, Connection, Edge<int>>, IGraph
    {
        public Graph() {}

        public int AddVertex()
        {
            return AddVertexImpl(new());
        }

        public void RemoveVertex(int vertex)
        {
            RemoveVertexByIndex(vertex);
        }

        public void AddEdge(int fromIdx, int toIdx)
        {
            AddConnection(fromIdx, new(toIdx));
            AddConnection(toIdx, new(fromIdx));
        }

        public bool ContainsVertex(int vertex)
        {
            return ContainsVertexByIndex(vertex);
        }

        public bool ContainsEdge(int from, int to)
        {
            return TryGetConnectionByIndex(from, to, out var connection);
        }

        public Edge<int>[] GetEdges(int vertex)
        {
            return Array.ConvertAll(GetNeighbors(vertex), idx => new Edge<int>(vertex, idx)).ToArray();
        }

        public int[] GetNeighbors(int vertex)
        {
            return GetNeighborsByIndex(vertex);
        }

        public void RemoveEdge(int from, int to)
        {
            RemoveEdgeByIndex(from, to);
        }

        protected override Edge<int> GetEdge(int fromIdx, int toIdx)
        {
            if (!ContainsEdge(fromIdx, toIdx))
                throw new Exception("Nonexistant edge");
            
            return new Edge<int>(fromIdx, toIdx);
        }
    }

    public class WeightedGraph<E> : GraphImpl<Vertex<Connection<E>>, Connection<E>, Edge<int, E?>>, IWeightedGraph<E>
    {
        public E? DefaultEdgeDataValue {get; set;}

        public WeightedGraph()
        {
            DefaultEdgeDataValue = default;
        }

        public void AddEdge(int from, int to, E? eData)
        {
            AddConnection(from, new(to, eData));
            AddConnection(to, new(from, eData));
        }

        public void AddEdge(int from, int to)
        {
            AddEdge(from, to, DefaultEdgeDataValue);
        }

        public int AddVertex()
        {
            return AddVertexImpl(new());
        }

        public void RemoveVertex(int vertex)
        {
            RemoveVertexByIndex(vertex);
        }

        public bool ContainsEdge(int from, int to)
        {
            return TryGetConnectionByIndex(from, to, out var connection);
        }

        public bool ContainsVertex(int vertex)
        {
            return ContainsVertexByIndex(vertex);
        }

        public E? GetEdgeData(int from, int to)
        {
            if (!TryGetConnectionByIndex(from, to, out Connection<E> c))
                throw new Exception("Nonexistant edge");
            
            return c.Data;
        }

        public Edge<int, E?>[] GetEdges(int vertex)
        {
            return GetEdgesByIndex(vertex);
        }

        public int[] GetNeighbors(int vertex)
        {
            return GetNeighborsByIndex(vertex);
        }

        public void RemoveEdge(int from, int to)
        {
            RemoveEdgeByIndex(from, to);
        }

        public void SetEdgeData(int from, int to, E? data)
        {
            RemoveEdgeByIndex(from, to);
            AddConnection(from, new Connection<E>(to, data));
            AddConnection(to, new Connection<E>(from, data));
        }

        protected override Edge<int, E?> GetEdge(int fromIdx, int toIdx)
        {
            if (!TryGetConnectionByIndex(fromIdx, toIdx, out var connection))
                throw new Exception("Nonexistant edge");
            
            return new Edge<int, E?>(fromIdx, toIdx, connection.Data);
        }
    }

    public class Graph<V> : GraphImpl<V, Vertex<V, Connection>, Connection, Edge<V>>, IGraph<V>
        where V : notnull
    {
        public void AddVertex(V vData)
        {
            AddVertexImpl(new Vertex<V, Connection>(vData));
        }

        public void AddEdge(V from, V to)
        {
            if (!TryGetVertexByData(from, out var fromVertex) || !TryGetVertexByData(to, out var toVertex))
                throw new Exception("Nonexistant vertex");
            
            TryGetIndexByData(from, out int fromIdx);
            TryGetIndexByData(to, out int toIdx);
            
            AddConnection(fromIdx, new Connection(toIdx));
            AddConnection(toIdx, new Connection(fromIdx));
        }

        public bool ContainsVertex(V vData)
        {
            return ContainsVertexByData(vData);
        }

        public void RemoveVertex(V vertex)
        {
            RemoveVertexByData(vertex);
        }

        public bool ContainsEdge(V from, V to)
        {
            return TryGetConnectionByData(from, to, out Connection c);
        }

        public V[] GetNeighbors(V vData)
        {
            return GetNeighborsByData(vData);
        }

        public Edge<V>[] GetEdges(V vertex)
        {
            return GetEdgesByData(vertex);
        }

        public void RemoveEdge(V from, V to)
        {
            RemoveEdgeByData(from, to);
        }

        protected override Edge<V> GetEdge(int fromIdx, int toIdx)
        {
            if (!TryGetConnectionByIndex(fromIdx, toIdx, out var connection))
                throw new Exception("Nonexistant edge");
            
            V vData1 = adj.Vertices[fromIdx].Data;
            V vData2 = adj.Vertices[toIdx].Data;
            
            return new Edge<V>(vData1, vData2);
        }
    }

    public class WeightedGraph<E, V> : GraphImpl<V, Vertex<V, Connection<E>>, Connection<E>, Edge<V, E?>>, IWeightedGraph<E?, V>
        where V : notnull
    {
        public E? DefaultEdgeDataValue {get; set;}

        public WeightedGraph()
        {
            DefaultEdgeDataValue = default;
        }

        public void AddVertex(V vData)
        {
            AddVertexImpl(new Vertex<V, Connection<E>>(vData));
        }

        public void AddEdge(V from, V to, E? eData)
        {
            if (!TryGetVertexByData(from, out var fromVertex) || !TryGetVertexByData(to, out var toVertex))
                throw new Exception("Nonexistant vertex");
            
            TryGetIndexByData(from, out int fromIdx);
            TryGetIndexByData(to, out int toIdx);
            
            AddConnection(fromIdx, new Connection<E>(toIdx, eData));
            AddConnection(toIdx, new Connection<E>(fromIdx, eData));
        }

        public void AddEdge(V from, V to)
        {
            AddEdge(from, to, DefaultEdgeDataValue);
        }

        public bool ContainsVertex(V vData)
        {
            return ContainsVertexByData(vData);
        }

        public bool ContainsEdge(V from, V to)
        {
            return TryGetConnectionByData(from, to, out Connection<E> c);
        }

        public V[] GetNeighbors(V vData)
        {
            return GetNeighborsByData(vData);
        }

        public Edge<V, E?>[] GetEdges(V vertex)
        {
            return GetEdgesByData(vertex);
        }

        public E? GetEdgeData(V from, V to)
        {
            if (!TryGetConnectionByData(from, to, out Connection<E> c))
                throw new Exception("Nonexistant edge");
            
            return c.Data;
        }

        public void SetEdgeData(V from, V to, E? data)
        {
            if (!TryGetIndexByData(from, out var fromIdx) || !TryGetIndexByData(to, out var toIdx))
                throw new Exception("Nonexistant vertex");

            RemoveEdge(from, to);
            AddConnection(fromIdx, new Connection<E>(toIdx, data));
            AddConnection(toIdx, new Connection<E>(fromIdx, data));
        }

        public void RemoveEdge(V from, V to)
        {
            RemoveEdgeByData(from, to);
        }

        public void RemoveVertex(V vertex)
        {
            RemoveVertexByData(vertex);
        }

        protected override Edge<V, E?> GetEdge(int fromIdx, int toIdx)
        {
            if (!TryGetConnectionByIndex(fromIdx, toIdx, out var connection))
                throw new Exception("Nonexistant edge");
            
            V vData1 = adj.Vertices[fromIdx].Data;
            V vData2 = adj.Vertices[toIdx].Data;
            
            return new Edge<V, E?>(vData1, vData2, connection.Data);
        }
    }
}

namespace CSGraph.Implementation
{
    public abstract class GraphImpl<TVertex, TConnection, TEdge>
        where TVertex : struct, IVertex<TConnection>
        where TConnection : struct, IConnection
    {
        public IReadOnlyCollection<int> Vertices => new IndexSet(adj);
        public IReadOnlyCollection<TEdge> Edges => new EdgeSet(this);

        protected AdjacencyList<TVertex, TConnection> adj = new();
        protected int numConnections = 0;

        protected abstract TEdge GetEdge(int fromIdx, int toIdx);

        protected virtual bool TryGetVertexByIndex(int idx, out TVertex vertex)
        {
            vertex = new();

            if (!ContainsVertexByIndex(idx))
                return false;
            
            vertex = adj.Vertices[idx];
            return true;
        }

        protected virtual bool TryGetConnectionByIndex(int fromIdx, int toIdx, out TConnection connection)
        {
            connection = new();

            if (!TryGetVertexByIndex(fromIdx, out TVertex fromVertex) || !TryGetVertexByIndex(toIdx, out TVertex toVertex))
                return false;
            
            int idx = fromVertex.Connections.FindIndex(c => c.To == toIdx);

            if (idx == -1)
                return false;
            
            connection = fromVertex.Connections[idx];

            return true;
        }

        protected virtual void AddConnection(int fromIdx, TConnection connection)
        {
            if (!TryGetVertexByIndex(fromIdx, out var fromVertex))
                throw new Exception("Nonexistant vertex");
            
            fromVertex.Connections.Add(connection);
            ++numConnections;
        }

        protected void RemoveConnectionByIndex(int fromIdx, int toIdx)
        {
            if (!TryGetVertexByIndex(fromIdx, out var fromVertex) || !TryGetVertexByIndex(fromIdx, out var toVertex))
                throw new Exception("Nonexistant vertex");
            
            int idx = fromVertex.Connections.FindIndex(c => c.To == toIdx);

            if (idx == -1)
                throw new Exception("Nonexistant connection");
            
            fromVertex.Connections[idx] = fromVertex.Connections[^1];
            fromVertex.Connections.RemoveAt(fromVertex.Connections.Count() - 1);
            --numConnections;
        }

        protected virtual int AddVertexImpl(TVertex vertex)
        {
            adj.Vertices.Add(vertex);
            return adj.Vertices.Count() - 1;
        }

        protected void RemoveEdgeByIndex(int from, int to)
        {
            if (!TryGetConnectionByIndex(from, to, out TConnection c1) || !TryGetConnectionByIndex(to, from, out TConnection c2))
                throw new Exception("Nonexistant edge");
            
            TryGetVertexByIndex(from, out var v1);
            TryGetVertexByIndex(to, out var v2);

            int idx = v1.Connections.FindIndex(c => c.To == to);
            v1.Connections[idx] = v1.Connections[^1];
            v1.Connections.RemoveAt(v1.Connections.Count() - 1);

            idx = v2.Connections.FindIndex(c => c.To == from);
            v2.Connections[idx] = v2.Connections[^1];
            v2.Connections.RemoveAt(v2.Connections.Count() - 1);
        }

        protected int[] GetNeighborsByIndex(int vertex)
        {
            if (!TryGetVertexByIndex(vertex, out var v))
                throw new Exception("Nonexistant vertex");
            
            List<int> neighbors = new();

            foreach (TConnection c in v.Connections)
                if (!ContainsVertexByIndex(c.To))
                    continue;
                else
                    neighbors.Add(c.To);

            return neighbors.ToArray();
        }

        protected void RemoveVertexByIndex(int vertex)
        {
            if (!TryGetVertexByIndex(vertex, out TVertex toRemove))
                throw new Exception("Nonexistant vertex");
            
            TConnection[] connections = new TConnection[toRemove.Connections.Count()];
            toRemove.Connections.CopyTo(connections);
            foreach (TConnection connection in connections)
                RemoveEdgeByIndex(vertex, connection.To);

            if (vertex != adj.Vertices.Count() - 1)
            {
                /*
                The last vertex in adj.Vertices is going to get moved to 'vertex'.
                Any connections to this vertex need to be updated to reflect
                the new index.
                */
                int oldIdx = adj.Vertices.Count() - 1;
                int newIdx = vertex;
                TVertex lastVertex = adj.Vertices[oldIdx];

                foreach (TConnection connection in lastVertex.Connections)
                {
                    TryGetConnectionByIndex(connection.To, oldIdx, out TConnection toUpdate);
                    RemoveConnectionByIndex(connection.To, oldIdx);
                    AddConnection(connection.To, toUpdate with {To = newIdx});
                }

                adj.Vertices[newIdx] = adj.Vertices[oldIdx];
            }

            adj.Vertices.RemoveAt(adj.Vertices.Count() - 1);
        }

        protected virtual bool ContainsVertexByIndex(int vertex)
        {
            return vertex >= 0 && vertex < adj.Vertices.Count();
        }

        protected TEdge[] GetEdgesByIndex(int vertex)
        {
            int[] neighbors = GetNeighborsByIndex(vertex);
            TEdge[] edges = new TEdge[neighbors.Length];

            for(int i = 0; i < neighbors.Length; ++i)
                edges[i] = GetEdge(vertex, neighbors[i]);
            
            return edges;
        }

        public virtual void Clear()
        {
            adj.Vertices.Clear();
            numConnections = 0;
        }

        public class IndexSet : IReadOnlyCollection<int>
        {
            protected AdjacencyList<TVertex, TConnection> adj;

            public int Count => adj.Vertices.Count();

            public IndexSet(AdjacencyList<TVertex, TConnection> adj)
            {
                this.adj = adj;
            }

            public IEnumerator<int> GetEnumerator()
            {
                return new Enumerator(adj);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public class Enumerator : IEnumerator<int>
            {
                public int Current => idx;
                object IEnumerator.Current => Current;

                protected AdjacencyList<TVertex, TConnection> adj;
                protected int idx = -1;

                public Enumerator(AdjacencyList<TVertex, TConnection> adj)
                {
                    this.adj= adj;
                }

                public bool MoveNext()
                {
                    return ++idx < adj.Vertices.Count();
                }

                public void Reset()
                {
                    idx = -1;
                }

                public void Dispose()
                {
                    return;
                }
            }
        }

        public class EdgeSet : IReadOnlyCollection<TEdge>
        {
            public int Count => graph.numConnections / 2;

            protected GraphImpl<TVertex, TConnection, TEdge> graph;

            public EdgeSet(GraphImpl<TVertex, TConnection, TEdge> graph)
            {
                this.graph = graph;
            }

            public IEnumerator<TEdge> GetEnumerator()
            {
                return new Enumerator(graph);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public class Enumerator : IEnumerator<TEdge>
            {
                public TEdge Current => graph.GetEdge(vEnum.Current, neighbors[neighborIdx]);
                object IEnumerator.Current => Current;

                protected GraphImpl<TVertex, TConnection, TEdge> graph;
                protected IEnumerator<int> vEnum;
                protected int[] neighbors = Array.Empty<int>();
                protected int neighborIdx = -1;

                public Enumerator(GraphImpl<TVertex, TConnection, TEdge> graph)
                {
                    this.graph = graph;
                    vEnum = graph.Vertices.GetEnumerator();
                }

                public bool MoveNext()
                {
                    if (neighborIdx == -1)
                    {
                        if (!vEnum.MoveNext())
                            return false;
                        
                        neighbors = graph.GetNeighborsByIndex(vEnum.Current);
                    }

                    ++neighborIdx;

                    while(neighborIdx >= neighbors.Length)
                    {
                        if (!vEnum.MoveNext())
                            return false;
                        
                        neighbors = graph.GetNeighborsByIndex(vEnum.Current);
                        neighborIdx = 0;
                    }

                    // skip any connections for which from > to
                    while(neighbors[neighborIdx] > vEnum.Current)
                        MoveNext();

                    return true;
                }

                public void Reset()
                {
                    vEnum.Reset();
                    neighborIdx = -1;
                    neighbors = Array.Empty<int>();
                }

                public void Dispose()
                {
                    vEnum.Dispose();
                }
            }
        }
    }

    public abstract class GraphImpl<V, TVertex, TConnection, TEdge> : GraphImpl<TVertex, TConnection, TEdge>
        where V : notnull
        where TVertex : struct, IVertex<V, TConnection>
        where TConnection : struct, IConnection
    {
        new public IReadOnlyCollection<V> Vertices => new VertexSet(new IndexSet(adj).GetEnumerator(), adj);

        protected Dictionary<V, int> indices = new();

        protected override int AddVertexImpl(TVertex v)
        {
            int idx = base.AddVertexImpl(v);
            indices[v.Data] = idx;
            return idx;
        }

        protected bool ContainsVertexByData(V vData)
        {
            if (!indices.ContainsKey(vData))
                return false;
            
            int idx = indices[vData];
            return ContainsVertexByIndex(idx);
        }

        protected bool ContainsEdgeByData(V from, V to)
        {
            return TryGetConnectionByData(from, to, out TConnection c);
        }

        protected V[] GetNeighborsByData(V vData)
        {
            if (!TryGetVertexByData(vData, out var v))
                throw new Exception("Nonexistant vertex");
            
            List<V> neighbors = new();

            foreach (TConnection c in v.Connections)
                if (!ContainsVertexByIndex(c.To))
                    continue;
                else
                    neighbors.Add(adj.Vertices[c.To].Data);

            return neighbors.ToArray();
        }

        protected void RemoveVertexByData(V vertex)
        {
            if (!TryGetIndexByData(vertex, out int idx))
                throw new Exception("Nonexistant vertex");
            
            /*
            If 'vertex' is not located at the end of adj.Vertices, then the last element is going
            to be moved to the index of 'vertex' to facilitate faster deletion. The dictionary needs
            to be updated with the new location of the last element.
            */

            if (idx != adj.Vertices.Count() - 1)
                indices[adj.Vertices[adj.Vertices.Count() - 1].Data] = idx;
            indices.Remove(vertex);

            RemoveVertexByIndex(idx);
        }

        protected void RemoveEdgeByData(V from, V to)
        {
            if (!TryGetConnectionByData(from, to, out TConnection c1) || !TryGetConnectionByData(to, from, out TConnection c2))
                throw new Exception("Nonexistant edge");
            
            TryGetIndexByData(from, out int fromIdx);
            TryGetIndexByData(to, out int toIdx);

            RemoveEdgeByIndex(fromIdx, toIdx);
        }

        protected bool TryGetVertexByData(V vData, out TVertex vertex)
        {
            vertex = default;

            if (!TryGetIndexByData(vData, out int idx))
                return false;
            
            vertex = adj.Vertices[idx];
            return true;
        }

        protected bool TryGetIndexByData(V vData, out int idx)
        {
            idx = -1;

            if (!indices.ContainsKey(vData))
                return false;
            
            idx = indices[vData];
            
            if (!ContainsVertexByIndex(idx))
                return false;

            return true;
        }

        protected bool TryGetConnectionByData(V from, V to, out TConnection connection)
        {
            connection = new();

            if (!TryGetIndexByData(from, out int fromIdx) || !TryGetIndexByData(to, out int toIdx))
                return false;
            
            bool found = TryGetConnectionByIndex(fromIdx, toIdx, out TConnection c);
            connection = c;
            return found;
        }

        protected TEdge[] GetEdgesByData(V vertex)
        {
            if (!TryGetIndexByData(vertex, out int idx))
                throw new Exception("Nonexistant vertex");
            
            return GetEdgesByIndex(idx);
        }

        public override void Clear()
        {
            base.Clear();
            indices.Clear();
        }

        public class VertexSet : IReadOnlyCollection<V>
        {
            public int Count => adj.Vertices.Count();

            protected IEnumerator<int> indices;
            protected AdjacencyList<TVertex, TConnection> adj;

            public VertexSet(IEnumerator<int> indices, AdjacencyList<TVertex, TConnection> adj)
            {
                this.indices = indices;
                this.adj = adj;
            }

            public IEnumerator<V> GetEnumerator()
            {
                return new Enumerator(indices, adj);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public class Enumerator : IEnumerator<V>
            {
                public V Current => adj.Vertices[indices.Current].Data;
                object IEnumerator.Current => Current;

                protected IEnumerator<int> indices;
                protected AdjacencyList<TVertex, TConnection> adj;

                public Enumerator(IEnumerator<int> indices, AdjacencyList<TVertex, TConnection> adj)
                {
                    this.indices = indices;
                    this.adj = adj;
                }

                public bool MoveNext()
                {
                    return indices.MoveNext();
                }

                public void Reset()
                {
                    indices.Reset();
                }

                public void Dispose()
                {
                    indices.Dispose();
                }
            }
        }
    }
}