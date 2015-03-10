using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading.Tasks;
using WCFRESTImage.WCFRESTImageWP;
using Windows.ApplicationModel.Activation;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace WCFRESTImageWP
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private CoreApplicationView view = CoreApplication.GetCurrentView();
        private int userId = 1;
        private UserProfile userProfile;

        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            try
            {
                //For demo purpose we fix userId = 1
                string base64String = await WCFRESTServiceCall("GET", "GetAvatarBase64?userId=" + userId);
                userProfile = JsonConvert.DeserializeObject<UserProfile>(base64String);
                if (userProfile.AvatarBase64String != null)
                {
                    SetAvatarFromBase64String(userProfile.AvatarBase64String);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private void Grid_Tapped(object sender, TappedRoutedEventArgs e)
        {
            try
            {
                var openPicker = new FileOpenPicker
                {
                    SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                    ViewMode = PickerViewMode.Thumbnail
                };

                //Filter to include a sample subset of file types.
                //For demo purpose we only use jpg image
                openPicker.FileTypeFilter.Clear();
                //openPicker.FileTypeFilter.Add(".jpeg");
                //openPicker.FileTypeFilter.Add(".bmp");
                //openPicker.FileTypeFilter.Add(".png");
                openPicker.FileTypeFilter.Add(".jpg");

                //Handle Activated event for SetAvatar and POST to service after choose image 
                view.Activated += viewActivated;
                openPicker.PickSingleFileAndContinue();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async void viewActivated(CoreApplicationView sender, IActivatedEventArgs args)
        {
            view.Activated -= viewActivated;
            FileOpenPickerContinuationEventArgs fileOpenPickerEventArgs = args as FileOpenPickerContinuationEventArgs;
            if (fileOpenPickerEventArgs != null)
            {
                if (fileOpenPickerEventArgs.Files.Count == 0) return;
                StorageFile file = fileOpenPickerEventArgs.Files[0];
                if (file != null)
                {
                    IRandomAccessStream fileStream = await file.OpenAsync(FileAccessMode.Read);
                    string errorMessage = null;
                    try
                    {
                        bool setAvatarResult = await PostAvatarToService(fileStream);
                        if (setAvatarResult)
                        {
                            SetAvatarFromBase64String(userProfile.AvatarBase64String);
                        }
                    }
                    catch (Exception exception)
                    {
                        errorMessage = exception.Message;
                    }

                    if (!string.IsNullOrEmpty(errorMessage))
                    {
                        var dialog = new MessageDialog(errorMessage);
                        await dialog.ShowAsync();
                    }
                }
            }
        }

        /// <summary>
        /// Post avatar to wcf rest service from RandomAccessStream
        /// </summary>
        /// <param name="fileStream"></param>
        /// <returns></returns>
        /// @created by tungnt.net - 2/2015
        private async Task<bool> PostAvatarToService(IRandomAccessStream fileStream)
        {
            bool setAvatarResult = false;
            using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
            {
                using (DataReader reader = new DataReader(ms.GetInputStreamAt(0)))
                {
                    byte[] imageBytes = new byte[fileStream.Size];
                    await fileStream.ReadAsync(imageBytes.AsBuffer(), (uint)fileStream.Size, Windows.Storage.Streams.InputStreamOptions.None);
                    string avatarBase64String = Convert.ToBase64String(imageBytes);
                    userProfile.AvatarBase64String = avatarBase64String;
                }
            }
            setAvatarResult = JsonConvert.DeserializeObject<bool>(await WCFRESTServiceCall("POST", "SetAvatarBase64", JsonConvert.SerializeObject(userProfile)));
            return setAvatarResult;
        }

        /// <summary>
        /// Set Avatar Image From Base64String use InMemoryRandomAccessStream
        /// </summary>
        /// <param name="base64String"></param>
        /// @created by tungnt.net - 2/2015
        private void SetAvatarFromBase64String(string base64String)
        {
            using (InMemoryRandomAccessStream ms = new InMemoryRandomAccessStream())
            {
                using (DataWriter writer = new DataWriter(ms.GetOutputStreamAt(0)))
                {
                    byte[] imageBytes = Convert.FromBase64String(base64String);
                    writer.WriteBytes((byte[])imageBytes);
                    writer.StoreAsync().GetResults();
                }
                var image = new BitmapImage();
                image.SetSource(ms);
                imgAvatar.Source = image;
            }
        }

        /// <summary>
        /// Utility function to get/post WCFRESTService
        /// </summary>
        /// <param name="methodRequestType">RequestType:GET/POST</param>
        /// <param name="methodName">WCFREST Method Name To GET/POST</param>
        /// <param name="bodyParam">Parameter of POST Method (Need serialize to JSON before passed in)</param>
        /// <returns></returns>
        /// Created by tungnt.net - 1/2015
        private async Task<string> WCFRESTServiceCall(string methodRequestType, string methodName, string bodyParam = "")
        {
            string ServiceURI = "http://localhost:5550/Services/ImageService.svc/rest/" + methodName;
            HttpClient httpClient = new HttpClient();
            HttpRequestMessage request = new HttpRequestMessage(methodRequestType == "GET" ? HttpMethod.Get : HttpMethod.Post, ServiceURI);
            if (!string.IsNullOrEmpty(bodyParam))
            {
                request.Content = new StringContent(bodyParam, Encoding.UTF8, "application/json");
            }
            HttpResponseMessage response = await httpClient.SendAsync(request);
            string returnString = await response.Content.ReadAsStringAsync();
            return returnString;
        }
    }
}
