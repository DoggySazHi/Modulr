using System;

namespace Modulr.Models
{
    public class User
    {
        public int ID { get; set; }
        public string GoogleID { get; set; }
        public string Name { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public int TestsRemaining { get; set; }
        public DateTime TestsTimeout { get; set; }
        public Role Role { get; set; }
    }
}