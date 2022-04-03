using System;
using System.Collections.Generic;
using System.Text;

namespace Ray1ModRandomizerUI {
    [Flags]
    public enum RandomizerFlags {
        Pos = 1 << 1,
        Des = 1 << 2,
        Eta = 1 << 3,
        CommandOrder = 1 << 4,
        Follow = 1 << 5,
        States = 1 << 6,
        Type = 1 << 7,
        EventLocationRandomizerCagesOnly = 1 << 8,
        EventLocationRandomizer = 1 << 9,
    }
}
