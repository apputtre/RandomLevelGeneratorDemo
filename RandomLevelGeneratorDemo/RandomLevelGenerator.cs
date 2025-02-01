using RandomLevelGeneratorDemo;
using System;
using CSGraph;
using CSGraph.Implementation;
using System.Linq;
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
        //GenerateHallwaysSteiner();
        GenerateHallwaysRST();
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
    private void GenerateHallwaysSteiner()
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

    private void GenerateHallwaysRST()
    {
        Vec2i top_left = new Vec2i(0, 0);
        Vec2i bottom_right = top_left + new Vec2i(_levelParameters.Width + 1, _levelParameters.Height + 1);

        // generate the maze(s)
        bool finished = false;

        while (!finished)
        {
            Vec2i? start_vertex = null;

            foreach (Vec2i vertex in cells.Vertices)
            {
                Vec2i tile = vertex;

                /*
                This tile is elligible to be the start of a new hallway if
                    1) It does not lie within any room
                    2) It does not lie within an existing hallway
                    3) It has at least one neighbor
                */

                if (!room_tiles.Contains(tile) && !hall_tiles.Contains(tile) && cells.GetNeighbors(vertex).Length > 0)
                {
                    start_vertex = vertex;
                    break;
                }
            }

            if (start_vertex != null)
            {
                Region hall = new();

                /*
                This algorithm requires the spacing between adjacent tiles to be 2.
                 */

                Graph<Vec2i> modifiedGraph = new();

                foreach (Vec2i tile in cells.Vertices)
                    modifiedGraph.AddVertex(tile);

                foreach (Edge<Vec2i> edge in cells.Edges)
                    modifiedGraph.AddEdge(edge.From, edge.To);

                for (int row = top_left.Y + 1; row < bottom_right.Y; row += 2)
                {
                    for (int col = top_left.X + 1; col < bottom_right.X; col += 2)
                    {
                        Vec2i tile = new(row, col);
                        Vec2i rightNeighbor = tile + new Vec2i(1, 0);

                        if (modifiedGraph.ContainsEdge(tile, rightNeighbor))
                        {
                            if ()
                        }
                    }
                }

                // get the random spanning tree
                List<Edge<Vec2i>> span = RandomSpanningTree(cells, start_vertex.GetValueOrDefault());

                // build the maze
                foreach (var edge in span)
                {
                    Vec2i tile = edge.From;

                    // clear this tile
                    builder.PlaceFloor(tile);
                    hall.Add(tile);

                    Vec2i next_tile = edge.To;

                    builder.PlaceFloor(next_tile);
                    hall.Add(next_tile);

                    Vec2i dir = (next_tile - tile) / 2;

                    builder.PlaceFloor(tile + dir);
                    hall.Add(tile + dir);
                }

                hall_tiles.Add(hall);
                halls.Add(hall);
            }
            else
                finished = true;
        }

        // place the doors

        /*
        Rules for placing doors:
            1) A room must have one (and only one) door between itself and every adjacent hallway
            2) If a room is adjacent to one other room, these rooms must be connected by a door
            3) If a room is adjacent to multiple rooms, it must be connected to one of them and has a chance to be connected to each additional room
        */

        List<(Region, Region)> connections = new();

        Region[] rooms_arr = rooms.ToArray();

        foreach (Region room in rooms_arr)
        {
            List<(Vec2i, Vec2i, Region)> door_candidates = GetDoorCandidates(room);

            if (door_candidates.Count == 0)
            {
                // fill in the room
                Vec2i[] tiles = new Vec2i[room.Tiles.Count];
                room.Tiles.CopyTo(tiles);

                // remove the room so it isn't chosen as the entrance or exit later
                rooms.Remove(room);

                foreach (Vec2i tile in tiles)
                {
                    builder.PlaceWall(tile);
                    room.Remove(tile);
                }

                // skip this room
                continue;

                /*
                // try again
                door_candidates = GetDoorCandidates(room);

                if (door_candidates.Count == 0)
                    throw new InvalidOperationException(String.Format("Could not find door location for room. Seed: {0}, State: {1}", Seed, State));
                    */
            }

            // remove any candidates which are already connected to this room
            {
                (Vec2i, Vec2i, Region)[] candidates = door_candidates.ToArray();

                foreach ((Vec2i, Vec2i, Region) candidate in candidates)
                    if (connections.Find(connection => connection.Item1 == candidate.Item3 && connection.Item2 == room) != default((Region, Region)))
                        door_candidates.Remove(candidate);
            }

            // connect each neighboring region to this room with a door
            while (door_candidates.Count > 0)
            {
                int idx = rand.Next(door_candidates.Count);

                Region region = door_candidates[idx].Item3;

                // if the region is a room, there is only a 50% chance that it will generate a door
                bool place_door = true;
                if (rooms.Contains(region))
                    if (rand.NextDouble() <= 0.5f)
                        place_door = false;

                if (place_door)
                {
                    builder.PlaceFloor(door_candidates[idx].Item2);
                    Vec2i dir = door_candidates[idx].Item2 - door_candidates[idx].Item1;
                    builder.PlaceFloor(door_candidates[idx].Item2 + dir);
                }

                // record this connection was made, or that an attempt was made to make this connection
                connections.Add((room, door_candidates[idx].Item3));

                // remove all other door candidates which connect to this region
                (Vec2i, Vec2i, Region)[] candidates = door_candidates.ToArray();
                foreach ((Vec2i, Vec2i, Region) candidate in candidates)
                    if (candidate.Item3 == region)
                        door_candidates.Remove(candidate);
            }
        }

        /*
                foreach (var tile in hall_tiles)
                    builder.Level.SetTile((int) Level.Layer.Debug, tile, "skeleton");

                foreach (var tile in room_tiles)
                    builder.Level.SetTile((int) Level.Layer.Debug, tile, "player");
                    */

        // "prune" the maze
        finished = false;
        Vec2i top_left = new Vec2i(0, 0);

        while (!finished)
        {
            bool found_tile = false;

            for (int i = 0; i < _levelParameters.Width; ++i)
            {
                for (int j = 0; j < _levelParameters.Height; ++j)
                {
                    Vec2i tile = top_left + new Vec2i(i, j);

                    if (hall_tiles.Contains(tile) && builder.HasFloor(tile))
                    {
                        // count the number of neighbors which are walls
                        int walls = 0;

                        if (builder.HasWall(tile + new Vec2i(0, 1)))
                            ++walls;
                        if (builder.HasWall(tile + new Vec2i(0, -1)))
                            ++walls;
                        if (builder.HasWall(tile + new Vec2i(1, 0)))
                            ++walls;
                        if (builder.HasWall(tile + new Vec2i(-1, 0)))
                            ++walls;

                        if (walls >= 3)
                        {
                            found_tile = true;
                            builder.PlaceWall(tile);
                            hall_tiles.Remove(tile);
                        }
                    }
                }
            }

            if (!found_tile)
                finished = true;
        }
    }

    private List<Edge<Vec2i>> RandomSpanningTree(Graph<Vec2i> g, Vec2i start)
    {
        // this algorithm uses the backtracking method

        // initialize the array containing the visited nodes
        HashSet<Vec2i> visited = new(g.Vertices.Count);
        // intiaialize the stack which represents the path to backtrack along
        Stack<Vec2i> to_visit = new();
        // create the list to store the edges which make up the spaning tree in
        List<Edge<Vec2i>> edges = new();

        to_visit.Push(start);
        while (to_visit.Count > 0)
        {
            Vec2i curr = to_visit.Peek();
            visited.Add(curr);

            List<Vec2i> candidates = new();
            foreach (Vec2i neighbor in g.GetNeighbors(curr))
                if (!visited.Contains(neighbor))
                    candidates.Add(neighbor);

            // if this node has no unvisited neighbors, it is finished
            if (candidates.Count > 0)
            {
                // choose a random unvisited neighbor
                var next = candidates[rand.Next(candidates.Count)];

                // add this edge to the list
                edges.Add(new Edge<Vec2i>(curr, next));

                // this neighbor will be visited next
                to_visit.Push(next);
            }
            else
            {
                to_visit.Pop();
            }
        }

        return edges;
    }

    // Item1: The room tile which is adjacent to the door
    // Item2: The tile containing the door
    // Item3: The region the door connects to
    private List<(Vec2i, Vec2i, Region)> GetDoorCandidates(Region room)
    {
        List<(Vec2i, Vec2i, Region)> door_candidates = new();

        foreach (Vec2i tile in room.Tiles)
        {
            List<Vec2i> neighbors = new List<Vec2i>{
                tile + new Vec2i(0, 1),
                tile + new Vec2i(1, 0),
                tile + new Vec2i(0, -1),
                tile + new Vec2i(-1, 0)
            };

            foreach (Vec2i neighbor in neighbors)
            {
                if (builder.HasWall(neighbor))
                {
                    Vec2i door_tile = neighbor;
                    Vec2i room_tile = tile;
                    Vec2i dir = door_tile - room_tile;
                    // i.e. the tile adjacent to the door that isn't in the room
                    Vec2i threshold_tile = door_tile + dir;

                    if (hall_tiles.Contains(threshold_tile) || hall_tiles.Contains(threshold_tile + dir))
                    {
                        foreach (Region hall in halls)
                            if (hall.Contains(threshold_tile) || hall.Contains(threshold_tile + dir))
                            {
                                door_candidates.Add((room_tile, door_tile, hall));
                                break;
                            }
                    }
                    else if (room_tiles.Contains(threshold_tile) || room_tiles.Contains(threshold_tile + dir))
                    {
                        foreach (Region other_room in rooms)
                            if (other_room != room && (other_room.Contains(threshold_tile) || other_room.Contains(threshold_tile + dir)))
                            {
                                door_candidates.Add((room_tile, door_tile, other_room));
                                break;
                            }
                    }
                }
            }
        }

        return door_candidates;
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