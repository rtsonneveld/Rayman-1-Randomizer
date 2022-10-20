using System;
using System.Linq;
using BinarySerializer.Ray1;

namespace Rayman1Randomizer;

public static class ObjDataExtensions
{
    public static bool IsAlways(this ObjData obj)
    {
        ObjTypeInfoAttribute? info = obj.Type.GetAttribute<ObjTypeInfoAttribute>();
        return info?.Flag == ObjTypeFlag.Always;
    }

    public static bool IsEditor(this ObjData obj)
    {
        ObjTypeInfoAttribute? info = obj.Type.GetAttribute<ObjTypeInfoAttribute>();
        return info?.Flag == ObjTypeFlag.Editor;
    }

    public static Vec2 GetCenteredPos(this ObjData obj)
    {
        Vec2 offset = GetCenterOffset(obj);
        return new Vec2(obj.XPosition + offset.X, obj.YPosition + offset.Y);
    }

    public static Vec2 GetCenterOffset(this ObjData obj)
    {
        Animation? anim = obj.AnimationCollection.Animations.ElementAtOrDefault(obj.ETA.States[obj.Etat][obj.SubEtat].AnimationIndex);

        // This will be null for Dark Rayman
        if (anim == null)
            return default;

        int minX = -1;
        int minY = -1;
        int maxX = 0;
        int maxY = 0;

        foreach (AnimationLayer l in anim.Layers)
        {
            Sprite sprite = obj.SpriteCollection.Sprites[l.SpriteIndex];

            minX = minX == -1 ? l.XPosition : Math.Min(maxX, l.XPosition);
            minY = minY == -1 ? l.YPosition : Math.Min(minY, l.XPosition);
            maxX = Math.Max(maxX, l.XPosition + sprite.Width);
            maxY = Math.Max(maxY, l.YPosition + sprite.Height);
        }

        return new Vec2(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2);
    }
}