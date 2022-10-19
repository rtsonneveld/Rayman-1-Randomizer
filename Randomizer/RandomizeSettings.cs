using System;
using System.Collections.Generic;
using System.Text;

namespace Rayman1Randomizer {
   
   public struct RandomizeSettings
   {
      public readonly int Seed;
      public readonly int Flags;
      public readonly string MkPsxIsoPath;
      public readonly string RaymanFilesPath;

      public RandomizeSettings(int seed, int flags, string mkPsxIsoPath, string raymanFilesPath)
      {
         Seed = seed;
         Flags = flags;
         MkPsxIsoPath = mkPsxIsoPath;
         RaymanFilesPath = raymanFilesPath;
      }
   }
}
