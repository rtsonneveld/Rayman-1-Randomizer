using System;
using System.Collections.Generic;
using System.Text;
using BinarySerializer;
using BinarySerializer.Ray1;

namespace Rayman1Randomizer {

   /// <summary>
   /// Randomize Process for PS1
   /// </summary>
   public class RandomizeProcess {

      public RandomizeSettings Settings
      {
         get; private set;
      }

      public RandomizeProcess(RandomizeSettings settings)
      {
         Settings = settings;
      }

      public void Start()
      {
         // TODO: Change this depending on regional release
         string exeFileName = "SLUS-000.05";
         long exeAddress = 0x80125000 - 0x800;
         var definedPointers = PS1_DefinedPointers.PS1_US;
         var exeConfig = PS1_ExecutableConfig.PS1_US;

         // Create the context
         using Context context = new Context(Settings.RaymanFilesPath);

         // Add the settings
         Ray1Settings settings = context.AddSettings(new Ray1Settings(Ray1EngineVersion.PS1));

         // Add the exe file
         context.AddFile(new MemoryMappedFile(context, exeFileName, exeAddress));

         // Add the pointers
         context.AddPreDefinedPointers(definedPointers);

         // Read the exe file
         var exe = FileFactory.Read<PS1_Executable>(context, exeFileName, onPreSerialize: (_, o) => o.Pre_PS1_Config = exeConfig);

         // Get fix file entry
         PS1_FileTableEntry fileEntryFix = exe.PS1_FileTable[exe.GetFileTypeIndex(exeConfig, PS1_FileType.filefxs)];

         // Add allfix
         Utils.LoadFile(context, fileEntryFix);

         int[] levelCounts = { 21, 18, 13, 13, 12, 4 };

         // Enumerate every world and every level
         for (int worldIndex = 0; worldIndex < 6; worldIndex++) {
            settings.World = (World)(worldIndex + 1);

            // Get the world file entry
            PS1_FileTableEntry fileEntryworld = exe.PS1_FileTable[exe.GetFileTypeIndex(exeConfig, PS1_FileType.wld_file) + worldIndex];

            // Add world
            Utils.LoadFile(context, fileEntryworld);

            for (int levelIndex = 0; levelIndex < levelCounts[worldIndex]; levelIndex++) {
               settings.Level = levelIndex + 1;

               // Get the level file entry
               PS1_FileTableEntry fileEntrylevel = exe.PS1_FileTable[exe.GetFileTypeIndex(exeConfig, PS1_FileType.map_file) + (worldIndex * 21 + levelIndex)];

               // Add and read level
               Utils.LoadFile(context, fileEntrylevel);
               PS1_LevFile levFile = FileFactory.Read<PS1_LevFile>(context, fileEntrylevel.ProcessedFilePath);

               EventLocationRandomizer.RandomizeEventLocations(level);

               // TODO: Modify level data
               // TODO: Save level data
            }
         }
      }
   }
}
