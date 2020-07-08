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

        public string Description { get; set; }

        public List<ApiWithUrl> ApiWithUrls { get; set; }
    }

    public class AskUrlReference
    {
        [Key]
        public int ID { get; set; }

        public string UserEmail { get; set; }

        public string ApiKey { get; set; }

        public int UrlReferenceID { get; set; }

        public string UrlReferenceDescription { get; set; }
    }
}
