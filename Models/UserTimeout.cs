using System;

namespace Modulr.Models
{
    public class UserTimeout
    {
        public int TestsRemaining { get; set; }
        public DateTimeOffset TestsTimeout { get; set; }
        public long Milliseconds { get; set; }
    }
}