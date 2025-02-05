using System.Collections.Generic;

namespace CSGraph.Implementation
{
    public interface IVertex<TConnection>
    {
        public List<TConnection> Connections {get;}
    }

    public interface IVertex<V, TConnection> : IVertex<TConnection>
    {
        public V Data {get;}
    }

    public readonly struct Vertex<TConnection> : IVertex<TConnection>
    {
        public List<TConnection> Connections {get;}

        public Vertex()
        {
            Connections = new();
        }
    }

    public readonly struct Vertex<VData, TConnection> : IVertex<VData, TConnection>
    {
        public List<TConnection> Connections {get;}
        public VData Data {get;}

        public Vertex(VData data)
        {
            Data = data;
            Connections = new();
        }
    }

    public interface IConnection
    {
        public int To {get; set;}
    }

    public interface IConnection<E> : IConnection
    {
        public E Data {get;}
    }

    public struct Connection : IConnection
    {
        public int To {get; set;}

        public Connection(int to)
        {
            To = to;
        }
    }

    public struct Connection<EData> : IConnection
    {
        public int To{get; set;}

        public EData? Data {get;}

        public Connection(int to, EData? data)
        {
            To = to;
            Data = data;
        }
    }

    public class AdjacencyList<TVertex, TConnection>
        where TVertex : struct, IVertex<TConnection>
        where TConnection : IConnection
    {
        public List<TVertex> Vertices {get;}

        public AdjacencyList()
        {
            Vertices = new();
        }
    }
}