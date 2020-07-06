using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ApiProxy.Areas.DB.Models;
using ApiProxy.Data;
using ApiProxy.Models;
using ApiProxy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApiProxy.Areas.DB.Controllers
{
    [Area( "DB" )]
    [Authorize]
    public class ApiWithUrlController : Controller
    {
        private readonly ApiProxyContext _apiProxyContext;
        private readonly ILogger _logger;

        public ApiWithUrlController( ApiProxyContext apiProxyContext, ILogger<ApiWithUrlController> logger )
        {
            _apiProxyContext = apiProxyContext;
            _logger = logger;
        }

        // GET: DB/ApiWithUrl
        public IActionResult Index()
        {
            ViewBag.ApiKeyInfos = _apiProxyContext.ApiKeyInfos.AsNoTracking().OrderBy( u => u.UserEmail ).ToList();
            ViewBag.UrlReferences = _apiProxyContext.UrlReferences.AsNoTracking().OrderBy( u => u.ID ).ToList();

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Test()
        {
            try
            {
                return Ok( new Status { Result = true, Message = "ApiWithUrlController Test 成功" } );
            }
            catch( Exception ex )
            {
                _logger.LogError( "ERROR", ex );
                return BadRequest( new Status { Result = false, Message = ex.InnerException.Message } );
            }
        }

        // POST: DB/ApiWithUrl/GetApiWithUrl
        [HttpPost]
        public async Task<IActionResult> GetApiWithUrl()
        {
            try
            {
                string requestData = await Request.GetRawBodyStringAsync( Encoding.UTF8 );
                ApiKeyInfo apiKeyInfo = _apiProxyContext.ApiKeyInfos.AsNoTracking().Include( a => a.ApiWithUrls ).FirstOrDefault( a => a.UserEmail == requestData );
                if( apiKeyInfo != null )
                {
                    var urlIDs = apiKeyInfo.ApiWithUrls.Select( a => a.UrlReferenceID ).ToList();
                    return Json( urlIDs );
                }
                return BadRequest( new Status { Result = false, Message = "找不到相符的 UserEmail" } );
            }
            catch( Exception ex )
            {
                _logger.LogError( "ERROR", ex );
                return BadRequest( new Status { Result = false, Message = ex?.InnerException.Message } );
            }
        }

        // POST: DB/ApiWithUrl/UpdateApiWithUrl
        [HttpPost]
        public async Task<IActionResult> UpdateApiWithUrl()
        {
            try
            {
                string requestData = await Request.GetRawBodyStringAsync( Encoding.UTF8 );
                List<ApiWithUrl> list_ApiWithUrl = JsonSerializer.Deserialize<List<ApiWithUrl>>( requestData );
                ApiKeyInfo apiKeyInfo = _apiProxyContext.ApiKeyInfos.Include( a => a.ApiWithUrls ).SingleOrDefault( a => a.UserEmail == list_ApiWithUrl.First().UserEmail );
                if( apiKeyInfo != null )
                {
                    _apiProxyContext.ApiWithUrls.RemoveRange( apiKeyInfo.ApiWithUrls );
                    _apiProxyContext.ApiWithUrls.AddRange( list_ApiWithUrl );
                    await _apiProxyContext.SaveChangesAsync();
                    return Ok( new Status { Result = true, Message = "UpdateApiWithUrl 成功" } );
                }

                return BadRequest( new Status { Result = false, Message = "找不到相符的 UserEmail" } );
            }
            catch( Exception ex )
            {
                _logger.LogError( "ERROR", ex );
                return BadRequest( new Status { Result = false, Message = ex?.InnerException.Message } );
            }
        }
    }
}
