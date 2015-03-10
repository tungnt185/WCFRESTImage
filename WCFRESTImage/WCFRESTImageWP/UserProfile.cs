using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using Windows.UI.Xaml.Media.Imaging;

namespace WCFRESTImage.WCFRESTImageWP
{
    public class UserProfile
    {
        public int UserId { get; set; }

        public string AvatarBase64String { get; set; }

        //public byte[] AvatarStream { get; set; }

    }
}
