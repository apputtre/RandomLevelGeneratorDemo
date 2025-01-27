using System.Diagnostics.CodeAnalysis;

namespace RandomLevelGeneratorDemo;

public struct Vec2i
{
    public int X { get; set; }
    public int Y { get; set; }

    public Vec2i(int x, int y)
    {
        X = x;
        Y = y;
    }

    public Vec2i(Vec2i other)
    {
        X = other.X;
        Y = other.Y;
    }

    public override bool Equals([NotNullWhen(true)] object? obj)
    {
        if (obj == null)
            return false;

        try
        {
            Vec2i other = (Vec2i)obj;
            return X == other.X && Y == other.Y;
        }
        catch(Exception)
        {
            return false;
        }
    }

    public static Vec2i operator+(Vec2i v1, Vec2i v2)
    {
        return new(v1.X + v2.X, v1.Y + v2.Y);
    }
    public static Vec2i operator-(Vec2i v1, Vec2i v2)
    {
        return new(v1.X - v2.X, v1.Y - v2.Y);
    }

    public static bool operator==(Vec2i v1, Vec2i v2)
    {
        return v1.Equals(v2);
    }
    public static bool operator!=(Vec2i v1, Vec2i v2)
    {
        return !v1.Equals(v2);
    }
}
