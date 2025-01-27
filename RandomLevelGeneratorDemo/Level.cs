namespace RandomLevelGeneratorDemo;

public class Level
{
    private Dictionary<Vec2i, TileType> _tiles = new();

    public Dictionary<Vec2i, TileType> Tiles => _tiles;
    public Level()
    {

    }
}
