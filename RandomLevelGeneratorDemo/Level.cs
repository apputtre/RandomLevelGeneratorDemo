namespace RandomLevelGeneratorDemo;

public class Level
{
    private Dictionary<Vec2i, TileType> _tiles = new();

    public Dictionary<Vec2i, TileType> Tiles => _tiles;
    public Level()
    {
    
    }

    public void SetTile(Vec2i pos, TileType type)
    {
        _tiles[pos] = type;
    }

    public void Clear()
    {
        _tiles.Clear();
    }
}
