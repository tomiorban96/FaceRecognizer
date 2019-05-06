using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http.Headers;
using Windows.Graphics.Imaging;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml.Media.Imaging;

namespace FaceRecognizer.ApiHelpers
{
    public class CognitiveServiceHelper
    {
        static string uriBase = "https://westeurope.api.cognitive.microsoft.com/face/v1.0/detect";
        static string ApiKey1 = "90c5e8bc2b8043f4a09cd5c5c7ba2514";
        static string ApiKey2 = "8e45afaa73c747f2b395b509af2a5a10";

        

        public static async void FetchApi(SoftwareBitmap bitmap)
        {
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", ApiKey1);

            var requestParameters = "returnFaceId=false";

            var uri = $"{uriBase}?{requestParameters}";

            HttpResponseMessage response;
            string contentString;

            var bytes = await GetImageAsByteArray(bitmap);


            using (var content = new ByteArrayContent(bytes))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                response = await client.PostAsync(uri, content);

                contentString = await response.Content.ReadAsStringAsync();
            }
 
            
        }

        private static async Task<byte[]> GetImageAsByteArray(SoftwareBitmap softwareBitmap)
        {
            byte[] bytes;
            WriteableBitmap newBitmap = new WriteableBitmap(softwareBitmap.PixelWidth, softwareBitmap.PixelHeight);
            softwareBitmap.CopyToBuffer(newBitmap.PixelBuffer);

            using (var stream = newBitmap.PixelBuffer.AsStream())
            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }



            using (var stream = newBitmap.PixelBuffer.AsStream())
            {
                await stream.WriteAsync(bytes, 0, bytes.Length);
            }

            return bytes;
        }
    }
}
