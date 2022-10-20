using BinarySerializer.Ray1;

namespace Rayman1Randomizer;

public record LoadedObject(ObjData Obj, Vec2 CenterOffset, Vec2 CenterPosition, int Index)
{
    public LoadedObject(ObjData obj, int index) : this(obj, obj.GetCenterOffset(), obj.GetCenteredPos(), index) { }
}