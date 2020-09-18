using System;

namespace Modulr.Models
{
    public class UserTimeout
    {
        public int TestsRemaining { get; set; }
        public DateTimeOffset ResetTime { get; set; }
    }
}