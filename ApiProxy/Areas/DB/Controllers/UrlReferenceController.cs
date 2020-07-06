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
    public class UrlReferenceController : Controller
    {
        private readonly ApiProxyContext _apiProxyContext;
        private readonly ILogger _logger;

        public UrlReferenceController( ApiProxyContext apiProxyContext, ILogger<UrlReferenceController> logger )
        {
            _apiProxyContext = apiProxyContext;
            _logger = logger;
        }

        // GET: DB/UrlReference
        public IActionResult Index()
        {
            List<UrlReference> data = _apiProxyContext.UrlReferences.AsNoTracking().OrderBy( u => u.ID ).ToList();
            return View( data );
        }

        [AllowAnonymous]
        [HttpPost]
        public IActionResult Test()
        {
            try
            {
                return Ok( new Status { Result = true, Message = "UrlReferenceController Test 成功" } );
            }
            catch( Exception ex )
            {
                _logger.LogError( "ERROR", ex );
                return BadRequest( new Status { Result = false, Message = ex.InnerException.Message } );
            }
        }

        // POST: DB/UrlReference/AddUrlReference
        [HttpPost]
        public async Task<IActionResult> AddUrlReference( [FromBody] UrlReference urlReference )
        {
            try
            {
                if( string.IsNullOrEmpty( urlReference.FromUrl ) || string.IsNullOrEmpty( urlReference.ToUrl ) )
                    return BadRequest( new Status { Result = false, Message = "網址沒有值" } );

                _apiProxyContext.UrlReferences.Add( urlReference );
                await _apiProxyContext.SaveChangesAsync();
                return Ok( new Status { Result = true, Message = "AddUrlReference 成功" } );
            }
            catch( Exception ex )
            {
                _logger.LogError( "ERROR", ex );
                return BadRequest( new Status { Result = false, Message = ex?.InnerException.Message } );
            }
        }

        // POST: DB/UrlReference/RemoveUrlReference
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
