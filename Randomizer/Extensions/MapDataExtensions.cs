using BinarySerializer.Ray1;

namespace Rayman1Randomizer;

public static class MapDataExtensions
{
    public static MapTile GetTileAt(this MapData map, int x, int y)
    {
        if (x < 0)
            x = 0;

        if (x >= map.Width)
            x = map.Width - 1;

        if (y < 0)
            y = 0;

        if (y >= map.Height)
            y = map.Height - 1;

        return map.Tiles[y * map.Width + x];
    }
}