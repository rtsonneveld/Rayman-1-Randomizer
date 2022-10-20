using System;

namespace Rayman1Randomizer;

[Flags]
public enum RandomizerFlags
{
    None = 0,

    CagesOnly = 1 << 0,

    All = ~0,
}