using System;

namespace DotVVM.Utils.ProjectService.Lookup
{
    [Flags]
    public enum TargetFramework
    {
        Unknown = 0x0,
        Net45 = 0x1,
        Net451 = 0x2,
        Net452 = 0x4,
        Net46 = 0x8,
        Net461 = 0x10,
        Net462 = 0x20,
        Net47 = 0x40,
        Net471 = 0x80,
        Net472 = 0x100,
        NetFramework = Net45 | Net451 | Net452 | Net46 | Net461 | Net462 | Net47 | Net471 | Net472,

        NetStandard10 = 0x200,
        NetStandard11 = 0x400,
        NetStandard12 = 0x800,
        NetStandard13 = 0x1000,
        NetStandard14 = 0x2000,
        NetStandard15 = 0x4000,
        NetStandard16 = 0x8000,
        NetStandard20 = 0x10000,
        NetStandard = NetStandard10 | NetStandard11 | NetStandard12 | NetStandard13 | NetStandard14 | NetStandard15 | NetStandard16 | NetStandard20,

        NetCoreApp10 = 0x20000,
        NetCoreApp11 = 0x40000,
        NetCoreApp20 = 0x80000,
        NetCoreApp21 = 0x100000,
        NetCoreApp22 = 0x200000,
        NetCoreApp = NetCoreApp10 | NetCoreApp11 | NetCoreApp20 | NetCoreApp21 | NetCoreApp22
    }
}
