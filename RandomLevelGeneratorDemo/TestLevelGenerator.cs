namespace RandomLevelGeneratorDemo;

public class TestLevelGenerator : LevelGenerator
{
    public TestLevelGenerator(LevelBuilder builder) : base(builder) {}

    public override void Generate()
    {
        builder.Clear();

        for (int i = 0; i < 10; ++i)
        {
            for (int j = 0; j < 10; ++j)
            {
                if (i == 0 || i == 9)
                    builder.PlaceWall(new Vec2i(i, j));
                else if (j == 0 || j == 9)
                    builder.PlaceWall(new Vec2i(i, j));
                else
                    builder.PlaceFloor(new Vec2i(i, j));
            }
        }
    }
}
