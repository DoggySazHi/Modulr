using System;

namespace Modulr.Models;

public class UserLogin
{
    public string Password { get; set; }
    public string Salt { get; set; }
    public string LoginCookie { get; set; }
    public DateTimeOffset LoginExpiration { get; set; }
}