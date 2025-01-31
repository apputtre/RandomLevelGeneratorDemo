using RandomLevelGeneratorDemo;
using System;
using CSGraph;
public class RandomLevelGenerator : LevelGenerator
{
    private class Region
    {
        public HashSet<Vec2i> Tiles { get; set; }

        public Region()
        {
            Tiles = new();
        }

        public bool Contains(Vec2i tile)
        {
            return Tiles.Contains(tile);
        }

        public void Clear()
        {
            Tiles.Clear();
        }

        public void Subsume(Region other)
        {
            Tiles.UnionWith(other.Tiles);
            other.Clear();
        }

        public void Add(Region other)
        {
            Tiles.UnionWith(other.Tiles);
        }

        public void Add(Vec2i tile)
        {
            Tiles.Add(tile);
        }

        public void Remove(Region other)
        {
            Tiles.ExceptWith(other.Tiles);
        }

        public void Remove(Vec2i tile)
        {
            Tiles.Remove(tile);
        }
    }

    private Random rand;
    private Region hall_tiles = new();
    private Region room_tiles = new();
    private List<Region> halls = new();
    private List<Region> rooms = new();

    private Graph<Vec2i> cells = new();
    private Graph<Region> regions = new();
    private List<Vec2i> roomLeaders = new();
    private LevelParameters _levelParameters;

    private int _seed;
    private int? _nextSeed;

    /*
    The seed used to generate the most recently generated level.
     */
    public int Seed
    {
        get => _seed;
    }

    /*
    The seed that will be used to generate the last level.
     */
    public int? NextSeed
    {
        get => _nextSeed;
        set { _nextSeed = value; }
    }

    public RandomLevelGenerator(LevelBuilder builder) : base(builder)
    {
        _levelParameters = new(50, 50, 12);
    }

    public override void Generate()
    {
        if (_nextSeed == null)
        {
            Random r = new();
            _seed = r.Next();
            rand = new(_seed);
        }
        else
        {
            _seed = (int) _nextSeed;
            rand = new((int) _nextSeed);
        }

        _nextSeed = rand.Next();

        builder.Clear();

        hall_tiles.Clear();
        room_tiles.Clear();
        halls.Clear();
        rooms.Clear();

        cells.Clear();
        regions.Clear();
        roomLeaders.Clear();

        int level_width = _levelParameters.Width;     // width of the level, in tiles, not including perimeter walls
        int level_height = _levelParameters.Height;    // height of the level, in tiles, not including perimeter walls

        // Coordinates of the top-left tile of the perimeter wall enclosing the level
        Vec2i top_left = new(0, 0);
        // Coordinates of the bottom-right tile of the perimeter wall enclosing the level
        Vec2i bottom_right = new Vec2i(top_left.X + level_width + 1, top_left.Y + level_height + 1);

        if (bottom_right.X - top_left.X <= 0 || bottom_right.Y - top_left.Y <= 0)
            return;

        // initialize the graph and set up the connections between adjacent vertices
        {
            List<Vec2i> prev_row = null;
            List<Vec2i> curr_row = null;
            for (int row = top_left.Y + 1; row <= bottom_right.Y - 1; ++row)
            {
                curr_row = new List<Vec2i>();
                Vec2i? prev_vertex = null;
                for (int col = top_left.X + 1; col <= bottom_right.X - 1; ++col)
                {
                    // get the current tile
                    Vec2i tile = top_left + new Vec2i(col, row);
                    // add this tile to the graph
                    cells.AddVertex(tile);
                    curr_row.Add(tile);

                    if (prev_vertex != null)
                        cells.AddEdge(tile, prev_vertex.GetValueOrDefault());

                    prev_vertex = tile;
                }

                if (prev_row != null)
                    for (int i = 0; i < prev_row.Count; ++i)
                        cells.AddEdge(prev_row[i], curr_row[i]);

                prev_row = curr_row;
            }
        }

        // generate the rooms
        const int attempts = 50;         // how many times to place a room if initial placement fails
        const int min_room_width = 7;    // minimum width of the room, including walls
        const int min_room_height = 7;   // minimum height of the room, including walls
        const int max_room_width = 12;   // maximum width of the room, including walls
        const int max_room_height = 12;  // maximum height of the room, including walls

        // fill the level with walls and floors
        FillRectangle(top_left, level_width, level_height);

        int num_rooms = _levelParameters.NumRooms;

        for (int room = 0; room < num_rooms; ++room)
        {
            for (int attempt = 0; attempt < attempts; ++attempt)
            {
                int w = rand.Next(min_room_width, max_room_width + 1);
                int h = rand.Next(min_room_height, max_room_height + 1);
                int x = rand.Next(top_left.X + 1, bottom_right.X);
                int y = rand.Next(top_left.Y + 1, bottom_right.Y);

                if (x + w > level_width || y + h > level_height)
                    continue;

                if (!DoesOverlap(new Vec2i(x, y), w, h))
                {
                    Region r = CreateRectangularRoom(new Vec2i(x, y), w, h);
                    rooms.Add(r);
                    regions.AddVertex(r);

                    break;
                }
            }
        }

        if (rooms.Count <= 1)
            return;

        foreach (Region room in rooms)
        {
            // the vertex which represents the room. used for connecting rooms with hallways.
            Vec2i leader;

            {
                Vec2i? candidateLeader = null;

                // choose an eligible leader
                foreach (Vec2i tile in room.Tiles)
                    if (!builder.HasWall(tile))
                    {
                        candidateLeader = tile;
                        break;
                    }

                if (candidateLeader == null)
                    throw new Exception("No candidate leader found");

                leader = candidateLeader.GetValueOrDefault();
                roomLeaders.Add(leader);
            }

            foreach (Vec2i tile in cells.GetNeighbors(leader))
                cells.RemoveEdge(leader, tile);

            // update the edges involving the vertices which fall within the room
            foreach (Vec2i tile in room.Tiles)
            {
                if (tile == leader)
                    continue;

                if (!cells.ContainsVertex(tile))
                    continue;

                if (builder.HasWall(tile))
                {
                    Vec2i[] neighbors = cells.GetNeighbors(tile);

                    bool hasAdjacentFloor = false;
                    foreach (Vec2i neighbor in neighbors)
                        if (!builder.HasWall(neighbor) && room.Contains(neighbor))
                            hasAdjacentFloor = true;

                    if (!hasAdjacentFloor)
                    {
                        foreach (Vec2i n in cells.GetNeighbors(tile))
                            cells.RemoveEdge(tile, n);

                        continue;
                    }

                    foreach (Edge<Vec2i> edge in cells.GetEdges(tile))
                    {
                        // if the connection leads to a vertex outside of the room, keep it
                        if (!room.Contains(edge.To))
                            continue;
                        // if the connection leads to a floor vertex inside of the room, reassign it to the leader
                        else if (!builder.HasWall(edge.To) && room.Contains(edge.To))
                        {
                            cells.RemoveVertex(edge.To);
                            cells.AddEdge(tile, leader);
                        }
                        // otherwise, sever the edge
                        else
                            cells.RemoveEdge(edge.From, edge.To);
                    }
                }
                else
                {
                    // is this floor tile adjacent to a wall?
                    bool adjacentToWall = false;
                    foreach (Vec2i neighbor in cells.GetNeighbors(tile))
                        if (builder.HasWall(neighbor))
                            adjacentToWall = true;

                    if (!adjacentToWall)
                        cells.RemoveVertex(tile);
                }
            }
        }
        
        // generate the hallways
        {
            WeightedGraph<List<Vec2i>, Vec2i> gRoutes = new();
            WeightedGraph<int, Vec2i> gRouteCosts = new();

            foreach (Vec2i vertex in cells.Vertices)
            {
                gRouteCosts.AddVertex(vertex);
                gRoutes.AddVertex(vertex);
            }

            foreach (Vec2i roomLeader1 in roomLeaders)
            {
                foreach (Vec2i roomLeader2 in roomLeaders)
                {
                    if (roomLeader1 != roomLeader2 && !gRouteCosts.ContainsEdge(roomLeader1, roomLeader2))
                    {
                        //CSGraph.Algorithms.Dijkstra(cells, roomLeader1, out var costs, out var routes);
                        List<Vec2i> path = GetPath(cells, roomLeader1, roomLeader2);

                        if (path.Count == 0)
                            throw new Exception();

                        gRouteCosts.AddEdge(roomLeader1, roomLeader2, path.Count);
                        //List<Vec2i> path = CSGraph.Algorithms.ShortestPath(routes, roomLeader2);
                        gRoutes.AddEdge(roomLeader1, roomLeader2, path);
                    }
                }
            }

            WeightedGraph<int, Vec2i> mstRouteCosts = CSGraph.Algorithms.MST(gRouteCosts, gRouteCosts.Vertices.First());
            WeightedGraph<int, Vec2i> steiner = new();

            foreach (Edge<Vec2i, int> route in mstRouteCosts.Edges)
            {
                Vec2i? prev = null;
                Vec2i next;

                foreach (Vec2i t in gRoutes.GetEdgeData(route.From, route.To))
                {
                    next = t;

                    if (!steiner.ContainsVertex(next))
                        steiner.AddVertex(next);

                    if (prev != null)
                        steiner.AddEdge(prev.GetValueOrDefault(), next, 1);

                    prev = next;
                }
            }

            foreach (Vec2i t in steiner.Vertices)
            {
                builder.PlaceFloor(t);
                hall_tiles.Add(t);
            }
        }
    }
    public override LevelParameters GetParameters()
    {
        return _levelParameters;
    }

    public override void SetParameters(LevelParameters levelParameters)
    {
        _levelParameters = levelParameters;
    }

    private Region CreateRectangularRoom(Vec2i pos, int width, int height)
    {
        Region room = new();

        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                Vec2i tile = new(pos.X + i, pos.Y + j);

                if (i == 0 || i == width - 1 || j == 0 || j == height - 1)
                    builder.PlaceWall(new Vec2i(tile.X, tile.Y));
                else
                    builder.PlaceFloor(new Vec2i(tile.X, tile.Y));

                room.Tiles.Add(tile);
            }
        }

        room_tiles.Add(room);
        hall_tiles.Remove(room);

        return room;
    }

    private void FillRectangle(Vec2i pos, int width, int height)
    {
        for (int i = 0; i < width; ++i)
        {
            for (int j = 0; j < height; ++j)
            {
                builder.PlaceWall(new Vec2i(pos.X + i, pos.Y + j));
            }
        }
    }

    // do the walls of the room described by pos, width, height overlap the floorspace of any existing rooms?
    private bool DoesOverlap(Vec2i pos, int width, int height)
    {
        for (int x = pos.X; x < pos.X + width; ++x)
        {
            for (int y = pos.Y; y < pos.Y + height; ++y)
            {
                if(!builder.HasWall(new Vec2i(x, y))) 
                    return true;
            }
        }

        return false;
    }

    private List<Vec2i> GetPath(Graph<Vec2i> g, Vec2i from, Vec2i to)
    {
        Queue<Vec2i> toVisit = new();
        HashSet<Vec2i> visited = new();
        Dictionary<Vec2i, Vec2i?> predecessors = new();

        predecessors[from] = null;
        toVisit.Enqueue(from);

        while(toVisit.Count > 0)
        {
            Vec2i curr = toVisit.Dequeue();
            visited.Add(curr);

            foreach (Vec2i neighbor in g.GetNeighbors(curr))
            {
                if (visited.Contains(neighbor))
                    continue;

                if (toVisit.Contains(neighbor))
                    continue;

                predecessors[neighbor] = curr;

                if (neighbor == to)
                {
                    Vec2i v = to;
                    List<Vec2i> path = new();
                    path.Add(to);

                    while (predecessors[v] != null)
                    {
                        path.Add(predecessors[v].GetValueOrDefault());
                        v = predecessors[v].GetValueOrDefault();
                    }

                    path.Reverse();
                    return path;
                }
                else
                {
                    toVisit.Enqueue(neighbor);
                }
            }
        }

        return new List<Vec2i>();
    }

    private bool DoesOverlap(Region room)
    {
        foreach (Vec2i tile in room.Tiles)
        {
            if (!builder.IsEmpty(tile))
                return true;
        }

        return false;
    }
}