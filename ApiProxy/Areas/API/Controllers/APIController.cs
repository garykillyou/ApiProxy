using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ApiProxy.Areas.API.Models;
using ApiProxy.Areas.DB.Models;
using ApiProxy.Data;
using ApiProxy.Models;
using ApiProxy.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApiProxy.Areas.API.Controllers
{
    [ApiController]
    public class APIController : ControllerBase
    {
        private readonly ApiProxyContext _apiProxyContext;
        private readonly HttpClientHelpers _httpClientHelpers;
        private readonly ILogger _logger;

        public APIController( ApiProxyContext apiProxyContext, HttpClientHelpers httpClientHelpers, ILogger<APIController> logger )
        {
            _apiProxyContext = apiProxyContext;
            _httpClientHelpers = httpClientHelpers;
            _logger = logger;
        }

        [Route( "API/{**slug}" )]
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            try
            {
                string FromUrl = Request.Path.ToString();
                Console.WriteLine( $"FromUrl : {FromUrl}" );

                string requestData = await Request.GetRawBodyStringAsync( Encoding.UTF8 );
                ApiData apiData = JsonSerializer.Deserialize<ApiData>( requestData );

                ApiKeyInfo apiKeyInfo = _apiProxyContext.ApiKeyInfos.AsNoTracking().SingleOrDefault( a => a.ApiKey == apiData.ApiKey );
                if( apiKeyInfo == null )
                    return BadRequest( new Status { Result = false, Message = "沒有對應的 ApiKey" } );

                UrlReference urlReference = _apiProxyContext.UrlReferences.AsNoTracking().SingleOrDefault( u => u.FromUrl == FromUrl );
                if( urlReference == null )
                    return BadRequest( new Status { Result = false, Message = "沒有對應的 UrlReference" } );

                ApiWithUrl apiWithUrl = _apiProxyContext.ApiWithUrls.AsNoTracking()
                    .SingleOrDefault( a => a.UserEmail == apiKeyInfo.UserEmail && a.UrlReferenceID == urlReference.ID );
                if( apiWithUrl == null )
                    return BadRequest( new Status { Result = false, Message = "ApiKey 沒有對應的 UrlReference" } );

                ApiProxyResponse res = await Send( urlReference.ToUrl, apiData.BodyJson );
                return StatusCode( (int)res.HttpStatusCode, res.ResponseResult );
            }
            catch( Exception ex )
            {
                _logger.LogError( "ERROR", ex );
                return BadRequest( new Status { Result = false, Message = ex.InnerException.Message } );
            }
        }

        private async Task<ApiProxyResponse> Send( string url, string bodyJson )
        {
            ApiProxyResponse apiProxyResponse = await _httpClientHelpers.SendWithStatusCodeAsync(
                HttpMethod.Post,
                url,
                new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("User-Agent", "ApiProxy"),
                },
                bodyJson
            );
            return apiProxyResponse;
        }
    }
}
