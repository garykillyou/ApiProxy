using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using ApiProxy.Areas.DB.Models;
using ApiProxy.Areas.Identity.Data;
using ApiProxy.Data;
using ApiProxy.Models;
using ApiProxy.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ApiProxy.Areas.DB.Controllers
{
    [Area( "DB" )]
    [Authorize]
    public class MyApiWithUrlController : Controller
    {
        private readonly ApiProxyContext _apiProxyContext;
        private readonly ILogger _logger;
        private readonly UserManager<ApiProxyUser> _userManager;

        public MyApiWithUrlController( ApiProxyContext apiProxyContext, ILogger<MyApiWithUrlController> logger, UserManager<ApiProxyUser> userManager )
        {
            _apiProxyContext = apiProxyContext;
            _logger = logger;
            _userManager = userManager;
        }

        // GET: DB/MyApiWithUrl
        public async Task<IActionResult> Index()
        {
            ApiProxyUser user = await _userManager.GetUserAsync( User );

            ViewBag.Email = user.Email;

            ApiKeyInfo ApiKeyInfo = _apiProxyContext.ApiKeyInfos.AsNoTracking().Include( a => a.ApiWithUrls )
                .SingleOrDefault( a => a.UserEmail == user.Email );
            ViewBag.ApiKeyInfo = ApiKeyInfo;

            ViewBag.UrlReferences = ApiKeyInfo?.ApiWithUrls.Select( a => a.UrlReference ).OrderBy( u => u.ID ).ToList();

            return View();
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Test()
        {
            try
            {
                return Ok( new Status { Result = true, Message = "MyApiWithUrlController Test 成功" } );
            }
            catch( Exception ex )
            {
                _logger.LogError( "ERROR", ex );
                return BadRequest( new Status { Result = false, Message = ex.InnerException.Message } );
            }
        }

        // POST: DB/MyApiWithUrl/AskApiKey
        [HttpPost]
        public async Task<IActionResult> AskApiKey( [FromBody] AskApiKey askApiKey )
        {
            try
            {
                if( string.IsNullOrEmpty( askApiKey.UserEmail ) )
                    return BadRequest( new Status { Result = false, Message = "沒有電子信箱的值" } );

                if( _apiProxyContext.AskApiKeys.AsNoTracking().SingleOrDefault( a => a.UserEmail == askApiKey.UserEmail ) != null ||
                    _apiProxyContext.ApiKeyInfos.AsNoTracking().SingleOrDefault( a => a.UserEmail == askApiKey.UserEmail ) != null )
                    return BadRequest( new Status { Result = false, Message = "ApiKey 請求中..." } );

                _apiProxyContext.AskApiKeys.Add( askApiKey );
                await _apiProxyContext.SaveChangesAsync();
                return Ok( new Status { Result = true, Message = "AskApiKey 成功" } );
            }
            catch( Exception ex )
            {
                _logger.LogError( "ERROR", ex );
                return BadRequest( new Status { Result = false, Message = ex?.InnerException.Message } );
            }
        }

        // POST: DB/MyApiWithUrl/AddUrlReference
        [HttpPost]
        public async Task<IActionResult> AddUrlReference( [FromBody] ApiWithUrl apiWithUrl )
        {
            try
            {
                if( string.IsNullOrEmpty( apiWithUrl.UrlReference.FromUrl ) || string.IsNullOrEmpty( apiWithUrl.UrlReference.ToUrl ) )
                    return BadRequest( new Status { Result = false, Message = "網址沒有值" } );

                _apiProxyContext.ApiWithUrls.Add( apiWithUrl );
                await _apiProxyContext.SaveChangesAsync();
                return Ok( new Status { Result = true, Message = "AddUrlReference 成功" } );
            }
            catch( Exception ex )
            {
                _logger.LogError( "ERROR", ex );
                return BadRequest( new Status { Result = false, Message = ex?.InnerException.Message } );
            }
        }

        // POST: DB/MyApiWithUrl/RemoveUrlReference
        [HttpPost]
        public async Task<IActionResult> RemoveUrlReference()
        {
            try
            {
                string requestData = await Request.GetRawBodyStringAsync( Encoding.UTF8 );
                List<int> list_ID = JsonSerializer.Deserialize<List<int>>( requestData );
                var tmp = _apiProxyContext.UrlReferences.Where( a => list_ID.Contains( a.ID ) );
                if( tmp != null )
                {
                    _apiProxyContext.UrlReferences.RemoveRange( tmp );
                    await _apiProxyContext.SaveChangesAsync();
                }
                return Ok( new Status { Result = true, Message = "RemoveUrlReference 成功" } );
            }
            catch( Exception ex )
            {
                _logger.LogError( "ERROR", ex );
                return BadRequest( new Status { Result = false, Message = ex?.InnerException.Message } );
            }
        }
    }
}
