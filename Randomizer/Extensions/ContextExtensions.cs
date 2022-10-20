using BinarySerializer;
using BinarySerializer.PS1;
using BinarySerializer.Ray1;

namespace Rayman1Randomizer;

public static class ContextExtensions
{
    public static void AddFile(this Context context, PS1_FileTableEntry file)
    {
        context.AddFile(new PS1_MemoryMappedFile(
            context: context, 
            filePath: file.ProcessedFilePath, 
            baseAddress: file.MemoryAddress, 
            currentInvalidPointerMode: PS1_MemoryMappedFile.InvalidPointerMode.DevPointerXOR, 
            fileLength: file.FileSize)
        {
            RecreateOnWrite = false
        });
    }
}