using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ApiProxy.Areas.DB.Models
{
    public class UrlReference
    {
        [Key]
        public int ID { get; set; }

        public string FromUrl { get; set; }

        public string ToUrl { get; set; }

        public List<ApiWithUrl> ApiWithUrls { get; set; }
    }
}
