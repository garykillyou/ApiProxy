using System;

namespace ApiProxy.Areas.Identity.Data
{
    public class AuthenticationTicket
    {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        public ApiProxyUser User { get; set; }

        public byte[] Value { get; set; }

        public DateTimeOffset? LastActivity { get; set; }

        public DateTimeOffset? Expires { get; set; }
    }
}
