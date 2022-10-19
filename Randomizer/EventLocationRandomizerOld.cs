using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Windows;
using Random = System.Random;

namespace Rayman1Randomizer {
   /// <summary>
   /// Robin's Rayman 1 cage location randomizer
   /// </summary>
   public static class EventLocationRandomizerOld {
      public const float MaxGenDoorFromObjectDist = 200;
      public const float MaxObjectFromOriginalSpotDist = 1000;
      private const int triesLimit = 100;

      private struct BannedRect {
         public readonly Rect Rect;

         public BannedRect(float xFrom, float yFrom, float xTo, float yTo)
         {
            Rect = new Rect(xFrom, yFrom, xTo - xFrom, yTo - yFrom);
         }
      }

      // Banned areas where no objects may be placed due to platforming
      private static readonly Dictionary<(int, int), List<BannedRect>> BannedTargetAreas = new Dictionary<(int, int), List<BannedRect>>()
        {
            {(2,8), new List<BannedRect>{new BannedRect(15,638, 838,2208)}}, // Allegro 2 slippery trumpet platforms
        };

      private static List<R1_EventType> BannedTargets = new List<R1_EventType>()
        {
            R1_EventType.MS_nougat,
            R1_EventType.MS_poing_plate_forme,
            R1_EventType.MS_porte,
            R1_EventType.TYPE_ANNULE_SORT_DARK,
            R1_EventType.TYPE_AUTOJUMP_PLAT,
            R1_EventType.TYPE_BAG3,
            R1_EventType.TYPE_BB1_PLAT,
            R1_EventType.TYPE_BIGSTONE,
            R1_EventType.TYPE_BIG_BOING_PLAT,
            R1_EventType.TYPE_BOING_PLAT,
            R1_EventType.TYPE_BON3,
            R1_EventType.TYPE_BONBON_PLAT,
            R1_EventType.TYPE_BOUEE_JOE,
            R1_EventType.TYPE_BOUT_TOTEM,
            R1_EventType.TYPE_BTBPLAT,
            R1_EventType.TYPE_CAISSE_CLAIRE,
            R1_EventType.TYPE_CFUMEE,
            R1_EventType.TYPE_CORDE,
            R1_EventType.TYPE_CORDEBAS,
            R1_EventType.TYPE_COUTEAU_SUISSE,
            R1_EventType.TYPE_CRAYON_BAS,
            R1_EventType.TYPE_CRAYON_HAUT,
            R1_EventType.TYPE_CRUMBLE_PLAT,
            R1_EventType.TYPE_CYMBAL1,
            R1_EventType.TYPE_CYMBAL2,
            R1_EventType.TYPE_CYMBALE,
            R1_EventType.TYPE_DARK,
            R1_EventType.TYPE_DARK_SORT,
            R1_EventType.TYPE_DUNE,
            R1_EventType.TYPE_EAU,
            R1_EventType.TYPE_ENS,
            R1_EventType.TYPE_FALLING_CRAYON,
            R1_EventType.TYPE_FALLING_OBJ,
            R1_EventType.TYPE_FALLING_OBJ2,
            R1_EventType.TYPE_FALLING_YING,
            R1_EventType.TYPE_FALLING_YING_OUYE,
            R1_EventType.TYPE_FALLPLAT,
            R1_EventType.TYPE_FEE,
            R1_EventType.TYPE_GOMME,
            R1_EventType.TYPE_GRAINE,
            R1_EventType.TYPE_HERSE_HAUT,
            R1_EventType.TYPE_HERSE_HAUT_NEXT,
            R1_EventType.TYPE_INDICATOR,
            R1_EventType.TYPE_INST_PLAT,
            R1_EventType.TYPE_JOE,
            R1_EventType.TYPE_LAVE,
            R1_EventType.TYPE_LEVIER,
            R1_EventType.TYPE_LIFTPLAT,
            R1_EventType.TYPE_MARACAS,
            R1_EventType.TYPE_MARACAS_BAS,
            R1_EventType.TYPE_MARK_AUTOJUMP_PLAT,
            R1_EventType.TYPE_MARTEAU,
            R1_EventType.TYPE_MOVE_AUTOJUMP_PLAT,
            R1_EventType.TYPE_MOVE_MARTEAU,
            R1_EventType.TYPE_MOVE_PLAT,
            R1_EventType.TYPE_MOVE_RUBIS,
            R1_EventType.TYPE_MOVE_START_NUA,
            R1_EventType.TYPE_MOVE_START_PLAT,
            R1_EventType.TYPE_MUS_WAIT,
            R1_EventType.TYPE_ONOFF_PLAT,
            R1_EventType.TYPE_PALETTE_SWAPPER,
            R1_EventType.TYPE_PETIT_COUTEAU,
            R1_EventType.TYPE_PI,
            R1_EventType.TYPE_PIERREACORDE,
            R1_EventType.TYPE_PI_BOUM,
            R1_EventType.TYPE_PI_MUS,
            R1_EventType.TYPE_PLATFORM,
            R1_EventType.TYPE_POELLE,
            R1_EventType.TYPE_PRI,
            R1_EventType.TYPE_PT_GRAPPIN,
            R1_EventType.TYPE_PUNAISE1,
            R1_EventType.TYPE_RAYMAN,
            R1_EventType.TYPE_RAY_ETOILES,
            R1_EventType.TYPE_REDUCTEUR,
            R1_EventType.TYPE_ROULETTE,
            R1_EventType.TYPE_ROULETTE2,
            R1_EventType.TYPE_ROULETTE3,
            R1_EventType.TYPE_RUBIS,
            R1_EventType.TYPE_SCROLL,
            R1_EventType.TYPE_SCROLL_SAX,
            R1_EventType.TYPE_SIGNPOST,
            R1_EventType.TYPE_SLOPEY_PLAT,
            R1_EventType.TYPE_SUPERHELICO,
            R1_EventType.TYPE_SWING_PLAT,
            R1_EventType.TYPE_TAMBOUR1,
            R1_EventType.TYPE_TAMBOUR2,
            R1_EventType.TYPE_TARZAN,
            R1_EventType.TYPE_TIBETAIN,
            R1_EventType.TYPE_TIBETAIN_2,
            R1_EventType.TYPE_TIBETAIN_6,
            R1_EventType.TYPE_TOTEM,
            R1_EventType.TYPE_TROMPETTE,
            R1_EventType.TYPE_UFO_IDC,
            R1_EventType.TYPE_PANCARTE,
            R1_EventType.TYPE_RAYMAN,
            R1_EventType.TYPE_RAY_POS,
            R1_EventType.TYPE_MST_SCROLL,
            R1_EventType.TYPE_MOSKITO,
            R1_EventType.TYPE_MOSKITO2,
            R1_EventType.TYPE_DARK,
            R1_EventType.TYPE_DARK_SORT,
            R1_EventType.TYPE_ANNULE_SORT_DARK,
            R1_EventType.TYPE_DARK2_PINK_FLY,
            R1_EventType.TYPE_DARK_PHASE2,
            R1_EventType.TYPE_CORDE_DARK,
            R1_EventType.TYPE_SCORPION,
            R1_EventType.TYPE_SAXO,
            R1_EventType.TYPE_SAXO2,
            R1_EventType.TYPE_SAXO3,
            R1_EventType.TYPE_HYBRIDE_MOSAMS,
            R1_EventType.TYPE_HYBRIDE_STOSKO,
            R1_EventType.TYPE_HYB_BBF2_D,
            R1_EventType.TYPE_HYB_BBF2_G,
            R1_EventType.TYPE_HYB_BBF2_LAS,
            R1_EventType.TYPE_SKO_PINCE,
            R1_EventType.TYPE_BB1,
            R1_EventType.TYPE_BB1_PLAT,
            R1_EventType.TYPE_BB12,
            R1_EventType.TYPE_BB1_VIT,
            R1_EventType.TYPE_MAMA_PIRATE,
            R1_EventType.TYPE_SPACE_MAMA,
            R1_EventType.TYPE_SPACE_MAMA2,
            R1_EventType.TYPE_GENERATING_DOOR
        };

      private struct LocationAndObject {
         public Vector2 location;
         public Unity_Object_R1 obj;

         public LocationAndObject(Vector2 location, Unity_Object_R1 obj)
         {
            this.location = location;
            this.obj = obj;
         }
      }

      // Cages that are spawned by GenDoors will fall, so they require a solid floor below them
      // Photographers also require a floor
      private static bool RequiresFloor(Unity_Object_R1 obj)
      {
         return (obj.EventData.Type == R1_EventType.TYPE_CAGE && obj.EditorLinkGroup != 0) || (obj.EventData.Type == R1_EventType.TYPE_PHOTOGRAPHE);
      }

      public static void Randomize(Unity_Level level, int wi, int li, int? seed, List<R1_EventType> typeWhitelist = null)
      {
         var random = seed != null ? new CrossPlatformRandom(seed.Value) : new CrossPlatformRandom();

         List<Unity_Object_R1> events = level.EventData.Select(o => o as Unity_Object_R1).Where(o => !(o.IsAlways || o.IsEditor)).ToList();

         var spawningSpots = events
             .Where(x => !x.IsAlways && !x.IsEditor && !BannedTargets.Contains(x.EventData.Type))
             .Select(x => x).ToList();

         List<LocationAndObject> targetLocations = new List<LocationAndObject>();

         foreach (var obj in events) {

            if (BannedTargets.Contains(obj.EventData.Type)) continue;
            if (typeWhitelist != null && !typeWhitelist.Contains(obj.EventData.Type)) continue;

            bool isGendoored = obj.EditorLinkGroup != 0;
            var genDoor = isGendoored ? level.EventData.Select(e => e as Unity_Object_R1)
                .FirstOrDefault(o => o.EditorLinkGroup == obj.EditorLinkGroup && o.EventData.Type == R1_EventType.TYPE_GENERATING_DOOR) : null;

            var targetSpots = genDoor != null ?
                GetWeighedList(GetCloseSpots(genDoor, spawningSpots, MaxGenDoorFromObjectDist), events) :
                GetWeighedList(GetCloseSpots(obj, spawningSpots, MaxObjectFromOriginalSpotDist), events);

            if (!targetSpots.Any()) continue;

            int tries = 0;

            var originalObjPos = GetCenteredPos(obj);
            var objCenterOffset = GetCenterOffset(obj);

            LocationAndObject spawningSpot;
            Vector2 newPos;

            do {
               spawningSpot = random.PickWeighedItem(targetSpots);

               newPos.x = (spawningSpot.location.x - objCenterOffset.x);
               newPos.y = (spawningSpot.location.y - objCenterOffset.y);

               tries++;

            } while ( // If the target object was already moved once or the position isn't safe, retry
                (!PositionSafe(level, wi, li, newPos + GetCenterOffset(obj), RequiresFloor(obj)))
                && tries < triesLimit);

            if (tries >= triesLimit - 1) {
               Debug.Log($"Failed to find position for {obj.EventData.Type} at {originalObjPos} after {tries} tries");
               continue;
            } else {
               Debug.Log(
                   $"Moved object {obj.EventData.Type} at {originalObjPos} to {spawningSpot.obj.EventData.Type} at {spawningSpot.location}");
               targetLocations.Add(new LocationAndObject(newPos, obj));

            }


         }

         foreach (var t in targetLocations) {
            t.obj.XPosition = (short)t.location.x;
            t.obj.YPosition = (short)t.location.y;
         }

         Debug.Log("Finished");
      }

      private static bool PositionSafe(Unity_Level level, int wi, int li, Vector2 coords, bool requireFloor)
      {
         if (BannedTargetAreas.ContainsKey((wi, li))) {
            foreach (var rect in BannedTargetAreas[(wi, li)]) {
               if (rect.Rect.Contains(coords)) {
                  return false;
               }
            }
         }

         int x = (int)coords.x / level.PixelsPerUnit;
         int y = (int)coords.y / level.PixelsPerUnit;

         var cm = level.Maps[level.DefaultCollisionMap];

         var tilemapController = Controller.obj.levelController.controllerTilemap;

         for (int i = -1; i <= 1; i++) {
            for (int j = -1; j <= 0; j++) {

               if (cm.GetMapTile(x + i, y + j)?.Data?.CollisionType != 0) {

                  return false;
               }
            }
         }

         if (requireFloor) {

            for (int i = -1; i <= 1; i++) {
               for (int j = -1; j <= 20; j++) {

                  var tile = cm.GetMapTile(x + i, y + j);
                  if (tile?.Data != null) {
                     var collisionType = ((R1_TileCollisionType)(tile.Data.CollisionType));
                     if (IsSolid(collisionType)) {

                        return true;
                     }
                  }
               }
            }
            return false;
         }

         // Safe
         return true;
      }

      private static bool IsSolid(R1_TileCollisionType collisionType)
      {
         switch (collisionType) {
            case R1_TileCollisionType.None:
            case R1_TileCollisionType.Reactionary:
            case R1_TileCollisionType.Damage:
            case R1_TileCollisionType.Water:
            case R1_TileCollisionType.Exit:
            case R1_TileCollisionType.WaterNoSplash:
            case R1_TileCollisionType.Spikes:
            case R1_TileCollisionType.Cliff:
               return false;
            default:
               return true;
         }
      }

      /// <summary>
      /// Assigns weight to a list of LocationAndObject, where each event type has the same weight
      /// </summary>
      /// <param name="objects">LocationAndObject list</param>
      /// <param name="events">Full object list</param>
      /// <returns></returns>
      private static List<(LocationAndObject, float)> GetWeighedList(List<LocationAndObject> objects, List<Unity_Object_R1> events)
      {
         List<(LocationAndObject, float)> result = new List<(LocationAndObject, float)>();
         foreach (var li in objects) {
            float weight = 1.0f / events.Count(e => e.EventData.Type == li.obj.EventData.Type);
            result.Add((li, weight));
         }

         return result;
      }

      private static List<LocationAndObject> GetCloseSpots(Unity_Object_R1 me, List<Unity_Object_R1> spawningSpots, float maxDist)
      {
         var eventPos = GetCenteredPos(me);

         return spawningSpots.Where(spot =>
         {
            var spotPos = GetCenteredPos(spot);
            return spot != me && (spotPos - eventPos).sqrMagnitude <
                   maxDist * maxDist;
         }).Select(x => new LocationAndObject(GetCenteredPos(x), x)).ToList();
      }

      private static Vector2 GetCenteredPos(Unity_Object_R1 o)
      {
         var co = GetCenterOffset(o);
         return new Vector2(o.XPosition + co.x, o.YPosition + co.y);
      }

      private static Vector2 GetCenterOffset(Unity_Object_R1 o)
      {
         var anim = o.CurrentAnimation;

         if (anim == null) {
            return Vector2.zero;
         }

         var frame = anim.Frames[0];

         if (frame == null || frame.SpriteLayers == null) {
            return Vector2.zero;
         }

         float minX = -1;
         float minY = -1;
         float maxX = 0;
         float maxY = 0;

         foreach (var l in frame.SpriteLayers) {
            var sprite = o.Sprites[l.ImageIndex];

            if (sprite != null) {
               minX = minX == -1 ? l.XPosition : Mathf.Min(maxX, l.XPosition);
               minY = minY == -1 ? l.YPosition : Mathf.Min(minY, l.XPosition);
               maxX = Mathf.Max(maxX, l.XPosition + sprite.rect.width);
               maxY = Mathf.Max(maxY, l.YPosition + sprite.rect.height);
            }
         }

         return new Vector2(minX + (maxX - minX) * 0.5f, minY + (maxY - minY) * 0.5f);
      }
   }
}