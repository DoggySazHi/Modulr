using System;

namespace Modulr.Models;

public class UserTimeout
{
    public int TestsRemaining { get; }
    public DateTimeOffset TestsTimeout { get; }
    public long Milliseconds { get; }

    public UserTimeout(DateTimeOffset testsTimeout, int testsRemaining)
    {
        TestsRemaining = testsRemaining;
        TestsTimeout = testsTimeout;
        Milliseconds = (long) (TestsTimeout - DateTimeOffset.Now).TotalMilliseconds;
    }
}