using System;
using System.Runtime.CompilerServices;

namespace Rayman1Randomizer;

public readonly record struct Vec2(int X, int Y)
{
    public double Magnitude
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => Math.Sqrt(X * X + Y * Y);
    }

    public double SquareMagnitude
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => X * X + Y * Y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);
}