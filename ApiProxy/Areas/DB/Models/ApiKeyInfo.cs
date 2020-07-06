using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApiProxy.Areas.DB.Models
{
    public class ApiKeyInfo
    {
        [Key]
        public string UserEmail { get; set; }

        public string ApiKey { get; set; }

        public List<ApiWithUrl> ApiWithUrls { get; set; }
    }
}
