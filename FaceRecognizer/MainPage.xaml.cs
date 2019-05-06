using FaceRecognizer.ApiHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Graphics;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace FaceRecognizer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        MediaCapture mediaCapture;
        DisplayRequest displayRequest = new DisplayRequest();
        bool isPreviewing;

        

        public MainPage()
        {
            this.InitializeComponent();
            Application.Current.Resuming += Application_Resuming;
            Application.Current.Suspending += Application_Suspending;
            ImagePopup.IsOpen = false;

        }

        private async Task StartPreviewAsync()
        {
            try
            {
                mediaCapture = new MediaCapture();
                await mediaCapture.InitializeAsync();

                displayRequest.RequestActive();
                DisplayInformation.AutoRotationPreferences = DisplayOrientations.Landscape;
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }

            try
            {
                PreviewControl.Source = mediaCapture;
                await mediaCapture.StartPreviewAsync();
                isPreviewing = true;
            }
            catch (System.IO.FileLoadException)
            {
                //mediaCapture.CaptureDeviceExclusiveControlStatusChanged
            }
        }

        private async Task CleanupCameraAsync()
        {
            if (mediaCapture != null)
            {
                if (isPreviewing)
                {
                    await mediaCapture.StopPreviewAsync();
                }

                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    PreviewControl.Source = null;
                    if (displayRequest != null)
                    {
                        displayRequest.RequestRelease();
                    }

                    mediaCapture.Dispose();
                    mediaCapture = null;
                });
            }
        }

        private async void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            if (Frame.CurrentSourcePageType == typeof(MainPage))
            {
                var deferral = e.SuspendingOperation.GetDeferral();
                await CleanupCameraAsync();
                deferral.Complete();
            }
        }

        private async void Application_Resuming(object sender, object e)
        {
            await StartPreviewAsync();
        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            await StartPreviewAsync();
        }

        private async void CaptureButton_Click(object sender, RoutedEventArgs e)
        {
            //Capturing the photo
            var capture = await mediaCapture.PrepareLowLagPhotoCaptureAsync(ImageEncodingProperties.CreateUncompressed(MediaPixelFormat.Bgra8));
            var photo = await capture.CaptureAsync();


            //Creating the bitmap
            var softwareBitmap = photo.Frame.SoftwareBitmap;

            await capture.FinishAsync();
            
            //API call
            CognitiveServiceHelper.FetchApi(softwareBitmap);

            //Converting the bitmap
            var convertedSoftwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);

            //Creating a bitmap source to set to the image source
            var bitmapSource = new SoftwareBitmapSource();
            await bitmapSource.SetBitmapAsync(convertedSoftwareBitmap);

            //Drawing the taken picture (without drawing the face rectangle on it, due to fail in the API call)
            Photo.Source = bitmapSource;

            //Setting the size of the wrapper grid element, so the popups size is the full view
            WrapperGrid.Width = Window.Current.Bounds.Width;
            WrapperGrid.Height = Window.Current.Bounds.Height;

            //Disabling capture button
            CaptureButton.IsEnabled = false;

            //Showing the popup
            ImagePopup.IsOpen = true;
            
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            //Enabling capture button
            CaptureButton.IsEnabled = true;

            //Closing the popup
            ImagePopup.IsOpen = false;
        }


    }
}
