using System;

namespace Modulr.Models
{
    [Flags]
    public enum Role
    {
        User = 0, // Redundant.
        Admin = 1,
        Banned = 2
    }
}