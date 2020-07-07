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
    [Authorize( Roles = "Admin" )]
    public class ApiKeyInfoController : Controller
    {
        private readonly ApiProxyContext _apiProxyContext;
        private readonly ILogger _logger;

        public ApiKeyInfoController( ApiProxyContext apiProxyContext, ILogger<ApiKeyInfoController> logger )
        {
            _apiProxyContext = apiProxyContext;
            _logger = logger;
        }

        // GET: DB/ApiKeyInfo
        public ActionResult Index()
        {
            var Model = _apiProxyContext.ApiKeyInfos.AsNoTracking().OrderBy( a => a.UserEmail ).ToList();
            ViewBag.AskApiKeys = _apiProxyContext.AskApiKeys.AsNoTracking().ToList();
            return View( Model );
        }

        [AllowAnonymous]
        [HttpPost]
        public async Task<IActionResult> Test()
        {
            try
            {
                string requestData = await Request.GetRawBodyStringAsync( Encoding.UTF8 );
                if( requestData != "requestData" ) return BadRequest( new Status { Result = false, Message = "密碼錯誤" } );
                return Ok( new Status { Result = true, Message = "ApiKeyInfoController Test 成功" } );
            }
            catch( Exception ex )
            {
                _logger.LogError( "ERROR", ex );
                return BadRequest( new Status { Result = false, Message = ex.InnerException.Message } );
            }
        }

        // POST: DB/ApiKeyInfo/AddUserEmail
        [HttpPost]
        public async Task<IActionResult> AddApiKeyInfo( [FromBody] ApiKeyInfo apiKeyInfo )
        {
            try
            {
                apiKeyInfo.ApiKey = ApiProxyContext.CreateKey();
                _apiProxyContext.ApiKeyInfos.Add( apiKeyInfo );
                await _apiProxyContext.SaveChangesAsync();
                return Ok( new Status { Result = true, Message = "AddApiKeyInfo 成功" } );
            }
            catch( Exception ex )
            {
                _logger.LogError( "ERROR", ex );
                return BadRequest( new Status { Result = false, Message = ex.InnerException.Message } );
            }
        }

        // POST: DB/ApiKeyInfo/RemoveApiKeyInfo
        [HttpPost]
        public async Task<IActionResult> RemoveApiKeyInfo()
        {
            try
            {
                string requestData = await Request.GetRawBodyStringAsync( Encoding.UTF8 );
                List<string> list_UserEmail = JsonSerializer.Deserialize<List<string>>( requestData );
                var tmp = _apiProxyContext.ApiKeyInfos.Where( a => list_UserEmail.Contains( a.UserEmail ) );
                if( tmp != null )
                {
                    _apiProxyContext.ApiKeyInfos.RemoveRange( tmp );
                    await _apiProxyContext.SaveChangesAsync();
                }
                return Ok( new Status { Result = true, Message = "RemoveApiKeyInfo 成功" } );
            }
            catch( Exception ex )
            {
                _logger.LogError( "ERROR", ex );
                return BadRequest( new Status { Result = false, Message = ex.InnerException.Message } );
            }
        }

        // POST: DB/ApiKeyInfo/AcceptAskApiKey
        [HttpPost]
        public async Task<IActionResult> AcceptAskApiKey()
        {
            try
            {
                string requestData = await Request.GetRawBodyStringAsync( Encoding.UTF8 );
                List<AskApiKey> askApiKeys = JsonSerializer.Deserialize<List<AskApiKey>>( requestData );

                List<ApiKeyInfo> apiKeyInfos = new List<ApiKeyInfo>();
                askApiKeys.ForEach( u =>
                {
                    apiKeyInfos.Add( new ApiKeyInfo
                    {
                        UserEmail = u.UserEmail,
                        ApiKey = ApiProxyContext.CreateKey()
                    } );
                } );
                _apiProxyContext.ApiKeyInfos.AddRange( apiKeyInfos );

                _apiProxyContext.AskApiKeys.RemoveRange( askApiKeys );
                await _apiProxyContext.SaveChangesAsync();

                return Ok( new Status { Result = true, Message = "AcceptAskApiKey 成功" } );
            }
            catch( Exception ex )
            {
                _logger.LogError( "AcceptAskApiKey()", ex );
                return BadRequest( new Status { Result = false, Message = ex.InnerException.Message } );
            }
        }

        // POST: DB/ApiKeyInfo/RefuseAskApiKey
        [HttpPost]
        public async Task<IActionResult> RefuseAskApiKey()
        {
            try
            {
                string requestData = await Request.GetRawBodyStringAsync( Encoding.UTF8 );
                List<AskApiKey> askApiKeys = JsonSerializer.Deserialize<List<AskApiKey>>( requestData );

                _apiProxyContext.AskApiKeys.RemoveRange( askApiKeys );
                await _apiProxyContext.SaveChangesAsync();

                return Ok( new Status { Result = true, Message = "RefuseAskApiKey 成功" } );
            }
            catch( Exception ex )
            {
                _logger.LogError( "RefuseAskApiKey()", ex );
                return BadRequest( new Status { Result = false, Message = ex.InnerException.Message } );
            }
        }
    }
}