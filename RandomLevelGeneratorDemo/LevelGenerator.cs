
namespace RandomLevelGeneratorDemo;

public abstract class LevelGenerator
{
    protected LevelBuilder builder;

    public LevelGenerator(LevelBuilder builder)
    {
        this.builder = builder;
    }

    public abstract void Generate();
    public abstract void SetParameters(LevelParameters lvlParams);
    public abstract LevelParameters GetParameters();
}
