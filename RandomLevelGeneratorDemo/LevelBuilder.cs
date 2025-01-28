using RandomLevelGeneratorDemo;

public class LevelBuilder
{
    protected Level level;

    public Level Level
    {
        get => level;
        set
        {
            this.level = value;
        }
    }

    public LevelBuilder()
    {
        level = new();
    }

    public virtual void Clear()
    {
        level.Clear();
    }

    public virtual void PlaceWall(Vec2i pos)
    {
        level.SetTile(pos, TileType.Wall);
    }

    public virtual void PlaceFloor(Vec2i pos)
    {
        level.SetTile(pos, TileType.Floor);
    }

    public virtual void Remove(Vec2i pos)
    {
        level.SetTile(pos, TileType.Empty);
    }

    public virtual bool HasWall(Vec2i pos)
    {
        return level.Tiles.ContainsKey(pos) && level.Tiles[pos] == TileType.Wall;
    }
    public virtual bool HasFloor(Vec2i pos)
    {
        return level.Tiles.ContainsKey(pos) && level.Tiles[pos] == TileType.Floor;
    }

    public virtual bool IsEmpty(Vec2i pos)
    {
        if (level.Tiles.ContainsKey(pos))
            return level.Tiles[pos] == TileType.Empty;
        else
            return true;
    }
}
