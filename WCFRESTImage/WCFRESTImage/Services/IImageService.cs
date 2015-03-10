using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace WCFRESTImage.Services
{
    [ServiceContract]
    public interface IImageService
    {
        [OperationContract]
        [WebGet( RequestFormat=WebMessageFormat.Json, ResponseFormat=WebMessageFormat.Json)]
        UserProfile GetAvatarBase64(int userId);

        [OperationContract]
        [WebInvoke(Method="POST" ,RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        bool SetAvatarBase64(UserProfile userProfile);
    }
}
