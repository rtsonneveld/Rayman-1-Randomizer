namespace Rayman1Randomizer;

public readonly record struct Rect(int X, int Y, int Width, int Height)
{
    public static Rect FromCoordinates(int minX, int minY, int maxX, int maxY) =>
        new(minX, minY, maxX - minX, maxY - minY);

    public bool Contains(Vec2 point) => point.X >= X && point.X < X + Width && point.Y >= Y && point.Y < Y + Height;
}