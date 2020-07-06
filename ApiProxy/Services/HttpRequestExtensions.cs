using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ApiProxy.Services
{
    public static class HttpRequestExtensions
    {
        /// <summary>
        /// Retrieve the raw body as a string from the Request.Body stream
        /// </summary>
        /// <param name="request">Request instance to apply to</param>
        /// <param name="encoding">Optional - Encoding, defaults to UTF8</param>
        public static async Task<string> GetRawBodyStringAsync( this HttpRequest request, Encoding encoding = null )
        {
            if( encoding == null )
                encoding = Encoding.UTF8;

            using( StreamReader reader = new StreamReader( request.Body, encoding ) )
                return await reader.ReadToEndAsync();
        }

        /// <summary>
        /// Retrieves the raw body as a byte array from the Request.Body stream
        /// </summary>
        public static async Task<byte[]> GetRawBodyBytesAsync( this HttpRequest request )
        {
            using( var ms = new MemoryStream( 2048 ) )
            {
                await request.Body.CopyToAsync( ms );
                return ms.ToArray();
            }
        }
    }
}
