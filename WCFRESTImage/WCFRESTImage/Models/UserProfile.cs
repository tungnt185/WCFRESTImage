using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace WCFRESTImage.Services
{
    public class UserProfile
    {
        public int UserId { get; set; }

        public string AvatarBase64String { get; set; }

        //public byte[] AvatarStream { get; set; }
    }
}
