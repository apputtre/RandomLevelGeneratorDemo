using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

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

    public static Vec2i operator*(Vec2i v1, int val)
    {
        return new Vec2i(v1.X * val, v1.Y * val);
    }
    public static Vec2i operator/(Vec2i v1, int val)
    {
        return new Vec2i(v1.X / val, v1.Y / val);
    }
    public static Vec2i operator*(Vec2i v1, double val)
    {
        return new Vec2i((int) (v1.X * val), (int) (v1.Y * val));
    }
    public static Vec2i operator/(Vec2i v1, double val)
    {
        return new Vec2i((int) (v1.X / val), (int) (v1.Y / val));
    }

    public static bool operator==(Vec2i v1, Vec2i v2)
    {
        return v1.X == v2.X && v1.Y == v2.Y;
    }
    public static bool operator!=(Vec2i v1, Vec2i v2)
    {
        return !(v1 == v2);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
}
