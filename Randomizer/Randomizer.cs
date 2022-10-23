using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using BinarySerializer;
using BinarySerializer.Ray1;

namespace Rayman1Randomizer;

/// <summary>
/// Randomizer for PS1
/// </summary>
public class Randomizer
{
    #region Constructor

    public Randomizer(RandomizerSettings settings)
    {
        Settings = settings;
    }

    #endregion

    #region Public Constant Fields
    public const string ExeFileName = "SLUS-000.05";
    #endregion

    #region Private Constant Fields

    private const double MaxGenDoorFromObjectDist = 200;
    private const double MaxObjectFromOriginalSpotDist = 1000;
    private const int TriesLimit = 100;

    #endregion

    #region Private Properties

    // Banned areas where no objects may be placed due to platforming
    private Dictionary<(int World, int Level), Rect[]> BannedAreas { get; } = new()
    {
        [(2, 8)] = new[] { Rect.FromCoordinates(15, 638, 838, 2208) }, // Allegro 2 slippery trumpet platforms
    };

    private ObjType[] BannedObjTypes { get; } = 
    {
        ObjType.MS_nougat,
        ObjType.MS_poing_plate_forme,
        ObjType.MS_porte,
        ObjType.TYPE_ANNULE_SORT_DARK,
        ObjType.TYPE_AUTOJUMP_PLAT,
        ObjType.TYPE_BAG3,
        ObjType.TYPE_BB1_PLAT,
        ObjType.TYPE_BIGSTONE,
        ObjType.TYPE_BIG_BOING_PLAT,
        ObjType.TYPE_BOING_PLAT,
        ObjType.TYPE_BON3,
        ObjType.TYPE_BONBON_PLAT,
        ObjType.TYPE_BOUEE_JOE,
        ObjType.TYPE_BOUT_TOTEM,
        ObjType.TYPE_BTBPLAT,
        ObjType.TYPE_CAISSE_CLAIRE,
        ObjType.TYPE_CFUMEE,
        ObjType.TYPE_CORDE,
        ObjType.TYPE_CORDEBAS,
        ObjType.TYPE_COUTEAU_SUISSE,
        ObjType.TYPE_CRAYON_BAS,
        ObjType.TYPE_CRAYON_HAUT,
        ObjType.TYPE_CRUMBLE_PLAT,
        ObjType.TYPE_CYMBAL1,
        ObjType.TYPE_CYMBAL2,
        ObjType.TYPE_CYMBALE,
        ObjType.TYPE_DARK,
        ObjType.TYPE_DARK_SORT,
        ObjType.TYPE_DUNE,
        ObjType.TYPE_EAU,
        ObjType.TYPE_ENS,
        ObjType.TYPE_FALLING_CRAYON,
        ObjType.TYPE_FALLING_OBJ,
        ObjType.TYPE_FALLING_OBJ2,
        ObjType.TYPE_FALLING_YING,
        ObjType.TYPE_FALLING_YING_OUYE,
        ObjType.TYPE_FALLPLAT,
        ObjType.TYPE_FEE,
        ObjType.TYPE_GOMME,
        ObjType.TYPE_GRAINE,
        ObjType.TYPE_HERSE_HAUT,
        ObjType.TYPE_HERSE_HAUT_NEXT,
        ObjType.TYPE_INDICATOR,
        ObjType.TYPE_INST_PLAT,
        ObjType.TYPE_JOE,
        ObjType.TYPE_LAVE,
        ObjType.TYPE_LEVIER,
        ObjType.TYPE_LIFTPLAT,
        ObjType.TYPE_MARACAS,
        ObjType.TYPE_MARACAS_BAS,
        ObjType.TYPE_MARK_AUTOJUMP_PLAT,
        ObjType.TYPE_MARTEAU,
        ObjType.TYPE_MOVE_AUTOJUMP_PLAT,
        ObjType.TYPE_MOVE_MARTEAU,
        ObjType.TYPE_MOVE_PLAT,
        ObjType.TYPE_MOVE_RUBIS,
        ObjType.TYPE_MOVE_START_NUA,
        ObjType.TYPE_MOVE_START_PLAT,
        ObjType.TYPE_MUS_WAIT,
        ObjType.TYPE_ONOFF_PLAT,
        ObjType.TYPE_PALETTE_SWAPPER,
        ObjType.TYPE_PETIT_COUTEAU,
        ObjType.TYPE_PI,
        ObjType.TYPE_PIERREACORDE,
        ObjType.TYPE_PI_BOUM,
        ObjType.TYPE_PI_MUS,
        ObjType.TYPE_PLATFORM,
        ObjType.TYPE_POELLE,
        ObjType.TYPE_PRI,
        ObjType.TYPE_PT_GRAPPIN,
        ObjType.TYPE_PUNAISE1,
        ObjType.TYPE_RAYMAN,
        ObjType.TYPE_RAY_ETOILES,
        ObjType.TYPE_REDUCTEUR,
        ObjType.TYPE_ROULETTE,
        ObjType.TYPE_ROULETTE2,
        ObjType.TYPE_ROULETTE3,
        ObjType.TYPE_RUBIS,
        ObjType.TYPE_SCROLL,
        ObjType.TYPE_SCROLL_SAX,
        ObjType.TYPE_SIGNPOST,
        ObjType.TYPE_SLOPEY_PLAT,
        ObjType.TYPE_SUPERHELICO,
        ObjType.TYPE_SWING_PLAT,
        ObjType.TYPE_TAMBOUR1,
        ObjType.TYPE_TAMBOUR2,
        ObjType.TYPE_TARZAN,
        ObjType.TYPE_TIBETAIN,
        ObjType.TYPE_TIBETAIN_2,
        ObjType.TYPE_TIBETAIN_6,
        ObjType.TYPE_TOTEM,
        ObjType.TYPE_TROMPETTE,
        ObjType.TYPE_UFO_IDC,
        ObjType.TYPE_PANCARTE,
        ObjType.TYPE_RAYMAN,
        ObjType.TYPE_RAY_POS,
        ObjType.TYPE_MST_SCROLL,
        ObjType.TYPE_MOSKITO,
        ObjType.TYPE_MOSKITO2,
        ObjType.TYPE_DARK,
        ObjType.TYPE_DARK_SORT,
        ObjType.TYPE_ANNULE_SORT_DARK,
        ObjType.TYPE_DARK2_PINK_FLY,
        ObjType.TYPE_DARK_PHASE2,
        ObjType.TYPE_CORDE_DARK,
        ObjType.TYPE_SCORPION,
        ObjType.TYPE_SAXO,
        ObjType.TYPE_SAXO2,
        ObjType.TYPE_SAXO3,
        ObjType.TYPE_HYBRIDE_MOSAMS,
        ObjType.TYPE_HYBRIDE_STOSKO,
        ObjType.TYPE_HYB_BBF2_D,
        ObjType.TYPE_HYB_BBF2_G,
        ObjType.TYPE_HYB_BBF2_LAS,
        ObjType.TYPE_SKO_PINCE,
        ObjType.TYPE_BB1,
        ObjType.TYPE_BB1_PLAT,
        ObjType.TYPE_BB12,
        ObjType.TYPE_BB1_VIT,
        ObjType.TYPE_MAMA_PIRATE,
        ObjType.TYPE_SPACE_MAMA,
        ObjType.TYPE_SPACE_MAMA2,
        ObjType.TYPE_GENERATING_DOOR
    };

    #endregion

    #region Public Properties

    public RandomizerSettings Settings { get; }

    #endregion

    #region Private Methods

    private void Randomize(ObjData[] objects, byte[] linkTable, MapData map, int world, int level)
    {
        Random random = new(Settings.Seed);

        LoadedObject[] loadedObjects = objects.
            Select((x, i) => new LoadedObject(x, i)).
            ToArray();

        LoadedObject[] normalObjects = loadedObjects.
            Where(x => !x.Obj.IsAlways() && !x.Obj.IsEditor() && !BannedObjTypes.Contains(x.Obj.Type)).
            ToArray();

        foreach (LoadedObject loadedObj in normalObjects)
        {
            if (Settings.Flags.HasFlag(RandomizerFlags.CagesOnly) && loadedObj.Obj.Type != ObjType.TYPE_CAGE)
                continue;

            LoadedObject? linkedGendoor = null;

            int link = loadedObj.Index;

            while ((link = linkTable[link]) != loadedObj.Index)
            {
                if (objects[link].Type == ObjType.TYPE_GENERATING_DOOR)
                {
                    linkedGendoor = loadedObjects[link];
                    break;
                }
            }

            (LoadedObject LocAndObj, float Weight)[] targetSpots = linkedGendoor != null ?
                GetWeighedList(GetCloseSpots(linkedGendoor, normalObjects, MaxGenDoorFromObjectDist), normalObjects) :
                GetWeighedList(GetCloseSpots(loadedObj, normalObjects, MaxObjectFromOriginalSpotDist), normalObjects);

            if (!targetSpots.Any())
                continue;

            for (int i = 0; i < TriesLimit; i++)
            {
                LoadedObject spawningSpot = random.NextWeighedItem(targetSpots.ToList());

                if (IsPositionSafe(map, world, level, spawningSpot.CenterPosition, RequiresFloor(loadedObj, linkTable)))
                {
                    loadedObj.Obj.XPosition = spawningSpot.CenterPosition.X - loadedObj.CenterOffset.X;
                    loadedObj.Obj.YPosition = spawningSpot.CenterPosition.Y - loadedObj.CenterOffset.Y;
                    break;
                }
            }
        }
    }

    private bool RequiresFloor(LoadedObject obj, byte[] linkTable)
    {
        // Cages that are spawned by GenDoors will fall, so they require a solid floor below them
        // Photographers also require a floor
        return (obj.Obj.Type == ObjType.TYPE_CAGE && linkTable[obj.Index] != obj.Index) ||
               obj.Obj.Type == ObjType.TYPE_PHOTOGRAPHE;
    }

    private bool IsPositionSafe(MapData map, int world, int level, Vec2 pos, bool requiresFloor)
    {
        if (BannedAreas.TryGetValue((world, level), out Rect[]? bannedAreas) &&
            bannedAreas.Any(rect => rect.Contains(pos)))
            return false;

        int tileX = pos.X / 16;
        int tileY = pos.Y / 16;

        for (int y = -1; y <= 0; y++)
        {
            for (int x = -1; x <= 1; x++)
            {
                if (map.GetTileAt(tileX + x, tileY + y).BlockType != 0)
                    return false;
            }
        }

        if (requiresFloor)
        {
            for (int y = -1; y <= 20; y++)
            {
                for (int x = -1; x <= 1; x++)
                {
                    MapTile tile = map.GetTileAt(tileX + x, tileY + y);

                    var collisionType = (TileCollisionType)tile.BlockType;

                    if (collisionType.IsSolid())
                        return true;
                }
            }

            return false;
        }

        // Safe
        return true;
    }

    private (LoadedObject LocAndObj, float Weight)[] GetWeighedList(IEnumerable<LoadedObject> objects, LoadedObject[] allObjects)
    {
        return objects.Select(x => (x, 1.0f / allObjects.Count(e => e.Obj.Type == x.Obj.Type))).ToArray();
    }

    private IEnumerable<LoadedObject> GetCloseSpots(LoadedObject obj, IEnumerable<LoadedObject> spawningSpots, double maxDist)
    {
        double maxDistSquare = maxDist * maxDist;

        return spawningSpots.Where(spot => spot != obj && (spot.CenterPosition - obj.CenterPosition).SquareMagnitude < maxDistSquare);
    }

    private void UpdateFileTable(Context context, PS1_Executable exe, string xmlFilePath)
    {
        // Create a temporary file for the LBA log
        using TempFile lbaLogFile = new();

        // Recalculate the LBA for the files on the disc
        ProcessHelpers.RunProcess(Settings.MkPsxIsoPath, new string[]
        {
            "-lba", ProcessHelpers.GetStringAsPathArg(lbaLogFile.TempPath), // Specify LBA log path
            "-noisogen", // Don't generate an ISO now
            xmlFilePath // The xml path,
        }, workingDir: context.BasePath, logInfo: Debugger.IsAttached);

        // Read the LBA log
        using Stream lbaLogStream = lbaLogFile.OpenRead();
        using StreamReader reader = new(lbaLogStream);

        // Skip initial lines
        for (int i = 0; i < 8; i++)
            reader.ReadLine();

        List<LBALogEntry> logEntries = new();
        List<string> currentDirs = new();

        // Read all log entries
        while (!reader.EndOfStream)
        {
            string? line = reader.ReadLine();

            if (line == null)
                break;

            string[] words = line.Split(' ').Where(x => !String.IsNullOrWhiteSpace(x)).ToArray();

            if (!words.Any())
                continue;

            LBALogEntry entry = new(words, currentDirs);

            logEntries.Add(entry);

            if (entry.EntryType == LBALogEntry.Type.Dir)
                currentDirs.Add(entry.Name);

            if (entry.EntryType == LBALogEntry.Type.End)
                currentDirs.RemoveAt(currentDirs.Count - 1);
        }

        // Update every file path in the file table
        foreach (PS1_FileTableEntry fileEntry in exe.PS1_FileTable)
        {
            // Get the matching entry
            LBALogEntry? entry = logEntries.FirstOrDefault(x => x.FullPath == fileEntry.FilePath);

            if (entry == null)
                continue;

            // Update the LBA and size
            fileEntry.LBA = entry.LBA;
            fileEntry.FileSize = (uint)entry.Bytes;
        }
    }

    protected void CreateISO(Context context, string xmlFilePath, string outputDirectory)
    {
        // Close context so the exe can be accessed
        context.Close();

        // Create a new ISO
        ProcessHelpers.RunProcess(Settings.MkPsxIsoPath, new[]
        {
            "-y", // Set to always overwrite
            xmlFilePath, // The xml path,
        }, workingDir: context.BasePath, logInfo: Debugger.IsAttached);

        File.Move(Path.Combine(context.BasePath, "Rayman.bin"), Path.Combine(outputDirectory, "Rayman.bin"));
        File.Move(Path.Combine(context.BasePath, "Rayman.cue"), Path.Combine(outputDirectory, "Rayman.cue"));
    }

    #endregion

    #region Public Methods

    public void Start(Action<double> progressCallback, CancellationToken cancellationToken)
    {
        // TODO: Change this depending on regional release
        const long exeAddress = 0x80125000 - 0x800;
        long[] cageRequirementAddresses = { 0x8015b4ec, 0x8018df7c };
        Dictionary<PS1_DefinedPointer, long> definedPointers = PS1_DefinedPointers.PS1_US;
        PS1_ExecutableConfig exeConfig = PS1_ExecutableConfig.PS1_US;
        const string xmlFileName = "disc.xml";

        // Create the context
        using Context context = new(Settings.GameDirectory, new SerializerSettings());

        // Add the settings
        Ray1Settings settings = context.AddSettings(new Ray1Settings(Ray1EngineVersion.PS1));

        // Add the exe file
        context.AddFile(new MemoryMappedFile(context, ExeFileName, exeAddress)
        {
            RecreateOnWrite = false
        });

        // Add the pointers
        context.AddPreDefinedPointers(definedPointers);

        // Read the exe file
        PS1_Executable exe = FileFactory.Read<PS1_Executable>(context, ExeFileName, onPreSerialize: (_, o) => o.Pre_PS1_Config = exeConfig);

        // Get fix file entry
        PS1_FileTableEntry fixFileEntry = exe.PS1_FileTable[exe.GetFileTypeIndex(exeConfig, PS1_FileType.filefxs)];

        // Add allfix
        context.AddFile(fixFileEntry);

        int[] levelCounts = { 21, 18, 13, 13, 12, 4 };
        int totalLevelsCount = levelCounts.Sum();

        int totalLevelIndex = 0;

        // Enumerate every world and every level
        for (int worldIndex = 0; worldIndex < 6; worldIndex++) {

            break; // DEBUG
            settings.World = (World)(worldIndex + 1);

            // Get the world file entry
            PS1_FileTableEntry worldFileEntry = exe.PS1_FileTable[exe.GetFileTypeIndex(exeConfig, PS1_FileType.wld_file) + worldIndex];

            // Add world
            context.AddFile(worldFileEntry);

            for (int levelIndex = 0; levelIndex < levelCounts[worldIndex]; levelIndex++)
            {
                progressCallback(totalLevelIndex / (double)totalLevelsCount);
                totalLevelIndex++;

                cancellationToken.ThrowIfCancellationRequested();

                settings.Level = levelIndex + 1;

                // Get the level file entry
                PS1_FileTableEntry levelFileEntry = exe.PS1_FileTable[exe.GetFileTypeIndex(exeConfig, PS1_FileType.map_file) + (worldIndex * 21 + levelIndex)];

                // Add and read level
                context.AddFile(levelFileEntry);
                string lvlPath = levelFileEntry.ProcessedFilePath;
                PS1_LevFile levFile = FileFactory.Read<PS1_LevFile>(context, lvlPath);

                // Perform the randomization
                Randomize(levFile.ObjData.Objects, levFile.ObjData.ObjectLinkingTable, levFile.MapData, worldIndex, levelIndex);

                // TODO: This could be optimized a bit more by preventing it from writing data from pointers, like animations etc.
                // Write back the level file
                FileFactory.Write<PS1_LevFile>(context, lvlPath);

                // Remove level
                context.RemoveFile(lvlPath);
            }

            // Remove world
            context.RemoveFile(worldFileEntry.ProcessedFilePath);
        }

        // Close the context so the files can be accessed by other processes
        context.Close();

        UpdateFileTable(context, exe, xmlFileName);

        // Edit the required cages count
        var s = context.Serializer;
        foreach (long address in cageRequirementAddresses)
            s.DoAt(new Pointer(address, context.GetRequiredFile(ExeFileName)), 
                () => s.Serialize<byte>((byte)Settings.RequiredCages, name: "RequiredCages"));

        // Save the exe
        FileFactory.Write<PS1_Executable>(context, ExeFileName);

        string outputDirectory = Path.Combine(Settings.GameDirectory, $"RandomizerOutput_{DateTime.Now:yyyyMMddTHHmmss}_{Settings.Seed}");

        Directory.CreateDirectory(outputDirectory);

        CreateISO(context, xmlFileName, Path.Combine(Settings.GameDirectory, outputDirectory));

        OpenDirectoryInExplorer(outputDirectory);

        progressCallback(1);
    }

    private static void OpenDirectoryInExplorer(string outputDirectory)
    {
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo()
        {
            FileName = outputDirectory,
            UseShellExecute = true,
            Verb = "open"
        });
    }

    #endregion

    #region Classes

    private class LBALogEntry
    {
        public LBALogEntry(IReadOnlyList<string> words, IReadOnlyList<string> dirs)
        {
            EntryType = (Type)Enum.Parse(typeof(Type), words[0], true);
            Name = words[1];

            if (EntryType == Type.End)
                return;

            Length = Int32.Parse(words[2]);
            LBA = Int32.Parse(words[3]);
            TimeCode = words[4];
            Bytes = Int32.Parse(words[5]);

            if (EntryType != Type.Dir)
                SourceFile = words[6];

            FullPath = $"\\{String.Join("\\", dirs.Append(Name))}";
        }

        public Type EntryType { get; }
        public string Name { get; }
        public int Length { get; }
        public int LBA { get; }
        public string? TimeCode { get; }
        public int Bytes { get; }
        public string? SourceFile { get; }

        public string? FullPath { get; }

        public enum Type
        {
            File,
            STR,
            XA,
            CDDA,
            Dir,
            End
        }
    }

    #endregion
}