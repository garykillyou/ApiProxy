using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiProxy.Services
{
    public class HttpClientHelpers
    {
        private readonly IHttpClientFactory _clientFactory;

        public HttpClientHelpers( IHttpClientFactory clientFactory )
        {
            _clientFactory = clientFactory ?? throw new ArgumentNullException();
        }

        public async Task<string> SendAsync( HttpMethod method, string url, List<KeyValuePair<string, string>> headers, object body )
        {
            HttpRequestMessage request = new HttpRequestMessage( method, url );

            foreach( var i in headers )
            {
                request.Headers.Add( i.Key, i.Value );
            }

            string JsonString = JsonSerializer.Serialize( body );
            request.Content = new StringContent( JsonString, Encoding.UTF8, "application/json" );

            var client = _clientFactory.CreateClient();

            var response = await client.SendAsync( request );

            if( response.IsSuccessStatusCode )
            {
                string responseResult = await response.Content.ReadAsStringAsync();
                return responseResult;
            }
            else
            {
                return null;
            }
        }
        public async Task<string> SendAsync( HttpMethod method, string url, List<KeyValuePair<string, string>> headers, string bodyJson )
        {
            HttpRequestMessage request = new HttpRequestMessage( method, url );

            foreach( var i in headers )
            {
                request.Headers.Add( i.Key, i.Value );
            }

            request.Content = new StringContent( bodyJson, Encoding.UTF8, "application/json" );

            var client = _clientFactory.CreateClient();

            var response = await client.SendAsync( request );

            string responseResult = await response.Content.ReadAsStringAsync();
            return responseResult;
        }

        public async Task<ApiProxyResponse> SendWithStatusCodeAsync( HttpMethod method, string url, List<KeyValuePair<string, string>> headers, string bodyJson )
        {
            HttpRequestMessage request = new HttpRequestMessage( method, url );

            foreach( var i in headers )
            {
                request.Headers.Add( i.Key, i.Value );
            }

            request.Content = new StringContent( bodyJson, Encoding.UTF8, "application/json" );

            var client = _clientFactory.CreateClient();

            var response = await client.SendAsync( request );

            ApiProxyResponse apiProxyResponse = new ApiProxyResponse
            {
                HttpStatusCode = response.StatusCode,
                ResponseResult = await response.Content.ReadAsStringAsync()
            };

            return apiProxyResponse;
        }
    }

    public class ApiProxyResponse
    {
        public HttpStatusCode HttpStatusCode { get; set; }

        public string ResponseResult { get; set; }
    }
}
