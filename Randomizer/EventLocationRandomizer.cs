using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BinarySerializer.Ray1;

namespace Rayman1Randomizer {
   public static class EventLocationRandomizer {

      public static void RandomizeEventLocations(PS1_LevFile level, int wi, int li, int? seed, List<ObjType> typeWhitelist = null)
      {
         var random = seed != null ? new Random(seed.Value) : new Random();

         var events = level.ObjData.Objects.Where(o => o.IsAlways());
      }

   }
}
