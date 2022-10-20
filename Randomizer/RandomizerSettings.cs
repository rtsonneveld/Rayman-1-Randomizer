namespace Rayman1Randomizer;

public record RandomizerSettings(
    int Seed, 
    string GameDirectory,
    string MkPsxIsoPath, 
    int RequiredCages,
    RandomizerFlags Flags);