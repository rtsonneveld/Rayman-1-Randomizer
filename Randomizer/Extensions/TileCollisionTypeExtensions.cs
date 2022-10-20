using BinarySerializer.Ray1;

namespace Rayman1Randomizer;

public static class TileCollisionTypeExtensions
{
    public static bool IsSolid(this TileCollisionType collisionType)
    {
        switch (collisionType)
        {
            case TileCollisionType.None:
            case TileCollisionType.Reactionary:
            case TileCollisionType.Damage:
            case TileCollisionType.Water:
            case TileCollisionType.Exit:
            case TileCollisionType.WaterNoSplash:
            case TileCollisionType.Spikes:
            case TileCollisionType.Cliff:
                return false;
            default:
                return true;
        }
    }
}