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

            ViewBag.AskUrlReferences = _apiProxyContext.AskUrlReferences.AsNoTracking()
                .Join( _apiProxyContext.UrlReferences,
                a => a.UrlReferenceID,
                u => u.ID,
                ( a, u ) => new AskUrlReference
                {
                    ID = a.ID,
                    UserEmail = a.UserEmail,
                    UrlReferenceID = a.UrlReferenceID,
                    UrlReferenceDescription = u.Description
                } ).ToList();
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
                _logger.LogError( "Test()", ex );
                return BadRequest( new Status { Result = false, Message = ex.InnerException?.Message ?? ex.Message } );
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
                _logger.LogError( "AddUrlReference( [FromBody] UrlReference urlReference )", ex );
                return BadRequest( new Status { Result = false, Message = ex.InnerException?.Message ?? ex.Message } );
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
                _logger.LogError( "RemoveUrlReference()", ex );
                return BadRequest( new Status { Result = false, Message = ex.InnerException?.Message ?? ex.Message } );
            }
        }

        // POST: DB/UrlReference/UpdateUrlReference
        [HttpPost]
        public async Task<IActionResult> UpdateUrlReference( [FromBody] UrlReference urlReference )
        {
            try
            {
                if( string.IsNullOrEmpty( urlReference.FromUrl ) || string.IsNullOrEmpty( urlReference.ToUrl ) )
                    return BadRequest( new Status { Result = false, Message = "網址沒有值" } );

                UrlReference tmp = _apiProxyContext.UrlReferences.SingleOrDefault( u => u.FromUrl == urlReference.FromUrl && u.ToUrl == urlReference.ToUrl );
                if( tmp == null )
                    return BadRequest( new Status { Result = false, Message = "沒有對應的 UrlReference" } );

                tmp.Description = urlReference.Description;
                await _apiProxyContext.SaveChangesAsync();
                return Ok( new Status { Result = true, Message = "UpdateUrlReference 成功" } );
            }
            catch( Exception ex )
            {
                _logger.LogError( "UpdateUrlReference( [FromBody] UrlReference urlReference )", ex );
                return BadRequest( new Status { Result = false, Message = ex.InnerException?.Message ?? ex.Message } );
            }
        }

        // POST: DB/UrlReference/AcceptAskUrlReference
        [HttpPost]
        public async Task<IActionResult> AcceptAskUrlReference()
        {
            try
            {
                string requestData = await Request.GetRawBodyStringAsync( Encoding.UTF8 );
                List<int> list_ID = JsonSerializer.Deserialize<List<int>>( requestData );

                List<AskUrlReference> AskUrlReferences = _apiProxyContext.AskUrlReferences.Where( a => list_ID.Contains( a.ID ) ).ToList();
                AskUrlReferences.ForEach( aur =>
                {
                    ApiWithUrl au = _apiProxyContext.ApiWithUrls.SingleOrDefault( a => a.UserEmail == aur.UserEmail && a.UrlReferenceID == aur.UrlReferenceID );
                    if( au == null )
                    {
                        au = new ApiWithUrl
                        {
                            UserEmail = aur.UserEmail,
                            UrlReferenceID = aur.UrlReferenceID
                        };
                        _apiProxyContext.ApiWithUrls.Add( au );
                    }
                } );

                _apiProxyContext.AskUrlReferences.RemoveRange( AskUrlReferences );

                await _apiProxyContext.SaveChangesAsync();
                return Ok( new Status { Result = true, Message = "AcceptAskUrlReference 成功" } );
            }
            catch( Exception ex )
            {
                _logger.LogError( "AcceptAskUrlReference()", ex );
                return BadRequest( new Status { Result = false, Message = ex.InnerException?.Message ?? ex.Message } );
            }
        }

        // POST: DB/UrlReference/RefuseAskUrlReference
        [HttpPost]
        public async Task<IActionResult> RefuseAskUrlReference()
        {
            try
            {
                string requestData = await Request.GetRawBodyStringAsync( Encoding.UTF8 );
                List<int> list_ID = JsonSerializer.Deserialize<List<int>>( requestData );
                var tmp = _apiProxyContext.AskUrlReferences.Where( a => list_ID.Contains( a.ID ) );
                if( tmp != null )
                {
                    _apiProxyContext.AskUrlReferences.RemoveRange( tmp );
                    await _apiProxyContext.SaveChangesAsync();
                }
                return Ok( new Status { Result = true, Message = "RefuseAskUrlReference 成功" } );
            }
            catch( Exception ex )
            {
                _logger.LogError( "RefuseAskUrlReference()", ex );
                return BadRequest( new Status { Result = false, Message = ex.InnerException?.Message ?? ex.Message } );
            }
        }
    }
}
