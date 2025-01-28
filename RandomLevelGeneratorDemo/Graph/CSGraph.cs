using System.Collections.Generic;

namespace CSGraph
{
    public readonly struct Edge<V>
    {
        public readonly V From {get;}
        public readonly V To {get;}

        public Edge(V from, V to)
        {
            From = from;
            To = to;
        }
    }

    public readonly struct Edge<V, E>
    {
        public readonly V From {get;}
        public readonly V To {get;}
        public readonly E? Data {get;}

        public Edge(V from, V to, E? data = default)
        {
            From = from;
            To = to;
            Data = data;
        }
    }

    public interface IGraph
    {
        public abstract IReadOnlyCollection<int> Vertices {get;}
        public abstract IReadOnlyCollection<Edge<int>> Edges {get;}

        public abstract int AddVertex();
        public abstract bool ContainsVertex(int vertex);
        public abstract void RemoveVertex(int vertex);
        public abstract bool ContainsEdge(int from, int to);
        public abstract void AddEdge(int from, int to);
        public abstract void RemoveEdge(int from, int to);
        public abstract Edge<int>[] GetEdges(int vertex);
        public abstract int[] GetNeighbors(int vertex);
        public void Clear();
    }

    public interface IWeightedGraph<E>
    {
        public abstract IReadOnlyCollection<int> Vertices {get;}
        public abstract IReadOnlyCollection<Edge<int, E?>> Edges {get;}

        public E? DefaultEdgeDataValue {get; set;}

        public abstract int AddVertex();
        public abstract bool ContainsVertex(int vertex);
        public abstract void RemoveVertex(int vertex);
        public abstract bool ContainsEdge(int from, int to);
        public abstract void AddEdge(int from, int to);
        public abstract void RemoveEdge(int from, int to);
        public abstract Edge<int, E?>[] GetEdges(int vertex);
        public abstract int[] GetNeighbors(int vertex);
        public abstract void AddEdge(int from, int to, E? eData);
        public abstract void SetEdgeData(int from, int to, E? data);
        public abstract E? GetEdgeData(int from, int to);
        public void Clear();
    }

    public interface IGraph<V>
    {
        public abstract IReadOnlyCollection<V> Vertices {get;}
        public abstract IReadOnlyCollection<Edge<V>> Edges {get;}

        public abstract void AddVertex(V vData);
        public abstract bool ContainsVertex(V vertex);
        public abstract void RemoveVertex(V vertex);
        public abstract bool ContainsEdge(V from, V to);
        public abstract void AddEdge(V from, V to);
        public abstract void RemoveEdge(V from, V to);
        public abstract Edge<V>[] GetEdges(V vertex);
        public abstract V[] GetNeighbors(V vertex);
        public void Clear();
    }

    public interface IWeightedGraph<E, V>
    {
        public abstract IReadOnlyCollection<V> Vertices {get;}
        public abstract IReadOnlyCollection<Edge<V, E>> Edges {get;}

        public E? DefaultEdgeDataValue {get; set;}

        public abstract void AddVertex(V vData);
        public abstract bool ContainsVertex(V vertex);
        public abstract void RemoveVertex(V vertex);
        public abstract bool ContainsEdge(V from, V to);
        public abstract void RemoveEdge(V from, V to);
        public abstract Edge<V, E?>[] GetEdges(V vertex);
        public abstract V[] GetNeighbors(V vertex);
        public abstract void AddEdge(V from, V to, E eData);
        public abstract void AddEdge(V from, V to);
        public abstract E? GetEdgeData(V from, V to);
        public abstract void SetEdgeData(V from, V to, E data);
        public void Clear();
    }
}