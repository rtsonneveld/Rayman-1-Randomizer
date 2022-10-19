using System;
using System.Collections.Generic;
using System.Text;
using BinarySerializer;
using BinarySerializer.PS1;
using BinarySerializer.Ray1;

namespace Rayman1Randomizer {
   public static class Utils {
      public static void LoadFile(Context context, PS1_FileTableEntry file)
      {
         context.AddFile(new PS1_MemoryMappedFile(context, file.ProcessedFilePath, file.MemoryAddress, PS1_MemoryMappedFile.InvalidPointerMode.DevPointerXOR, fileLength: file.FileSize)
         {
            RecreateOnWrite = false
         });
      }

      public static bool IsAlways(this ObjData obj)
      {
         var info = obj.Type.GetAttribute<ObjTypeInfoAttribute>();

         return info.Flag == ObjTypeFlag.Always;
      }

   }
}
