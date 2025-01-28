using System.Diagnostics.CodeAnalysis;

namespace RandomLevelGeneratorDemo;

public struct Vec2d
{
    public double X { get; set; }
    public double Y { get; set; }

    public Vec2d(double x, double y)
    {
        X = x;
        Y = y;
    }

    public Vec2d(Vec2d other)
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

    public static Vec2d operator+(Vec2d v1, Vec2d v2)
    {
        return new(v1.X + v2.X, v1.Y + v2.Y);
    }
    public static Vec2d operator-(Vec2d v1, Vec2d v2)
    {
        return new(v1.X - v2.X, v1.Y - v2.Y);
    }

    public static bool operator==(Vec2d v1, Vec2d v2)
    {
        return v1.Equals(v2);
    }
    public static bool operator!=(Vec2d v1, Vec2d v2)
    {
        return !v1.Equals(v2);
    }
}
