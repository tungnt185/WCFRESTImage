using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web;

namespace WCFRESTImage.Services
{
    /// <summary>
    /// Service demo handling image download and upload by WCF REST Service
    /// Created by tungnt.net 2/2015
    /// </summary>
    public class ImageService : IImageService
    {
        #region UseBaseString64

        /// <summary>
        /// Get Avatar image by userId
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        /// @Created by tungnt.net - 2/2015
        public UserProfile GetAvatarBase64(int userId)
        {
            string imagePath;
            UserProfile userProfile = new UserProfile() { UserId = userId };
            try
            {
                //For demo purpose I only use jpg file and save file name by userId integer;
                imagePath = HttpContext.Current.Server.MapPath("~/Avatar/") + userId + ".jpg";
                if (File.Exists(imagePath))
                {
                    using (Image img = Image.FromFile(imagePath))
                    {
                        if (img != null)
                        {
                            userProfile.AvatarBase64String = ImageToBase64(img, ImageFormat.Jpeg);
                        }
                    }
                }
            }
            catch (Exception)
            {
                userProfile.AvatarBase64String = null;
            }
            return userProfile;
        }

        private string ImageToBase64(Image image, ImageFormat format)
        {
            string base64String;
            using (MemoryStream ms = new MemoryStream())
            {
                // Convert Image to byte[]
                image.Save(ms, format);
                ms.Position = 0;
                byte[] imageBytes = ms.ToArray();

                // Convert byte[] to Base64 String
                base64String = Convert.ToBase64String(imageBytes);
            }
            return base64String;
        }

        /// <summary>
        /// Save image to Folder's Avatar
        /// </summary>
        /// <param name="userProfile"></param>
        /// <returns></returns>
        /// @Created by tungnt.net - 2/2015
        public bool SetAvatarBase64(UserProfile userProfile)
        {
            bool result = false;
            try
            {
                //For demo purpose I only use jpg file and save file name by userId integer
                if (!string.IsNullOrEmpty(userProfile.AvatarBase64String))
                {
                    using (Image image = Base64ToImage(userProfile.AvatarBase64String))
                    {
                        string strFileName = "~/Avatar/" + userProfile.UserId + ".jpg";
                        image.Save(HttpContext.Current.Server.MapPath(strFileName), ImageFormat.Jpeg);
                        result = true;
                    }
                }
            }
            catch (Exception)
            {
                result = false;
            }
            return result;
        }

        private Image Base64ToImage(string base64String)
        {
            // Convert Base64 String to byte[]
            byte[] imageBytes = Convert.FromBase64String(base64String);
            Bitmap tempBmp;
            using (MemoryStream ms = new MemoryStream(imageBytes, 0, imageBytes.Length))
            {
                // Convert byte[] to Image
                ms.Write(imageBytes, 0, imageBytes.Length);
                using (Image image = Image.FromStream(ms, true))
                {
                    //Create another object image for dispose old image handler
                    tempBmp = new Bitmap(image.Width, image.Height);
                    Graphics g = Graphics.FromImage(tempBmp);
                    g.DrawImage(image, 0, 0, image.Width, image.Height);
                }
            }
            return tempBmp;
        }

        #endregion
    }
}
