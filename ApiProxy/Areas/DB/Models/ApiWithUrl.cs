using System.ComponentModel.DataAnnotations;

namespace ApiProxy.Areas.DB.Models
{
    public class ApiWithUrl
    {
        [Key]
        public string UserEmail { get; set; }

        public ApiKeyInfo ApiKeyInfo { get; set; }

        public int UrlReferenceID { get; set; }

        public UrlReference UrlReference { get; set; }
    }
}
