using System.Collections.Generic;
using Marten.AspNetIdentity;
using Microsoft.AspNetCore.Identity;

namespace Roadkill.Api
{
    public class RoadkillUser : IdentityUser, IClaimsUser
    {
        public IList<byte[]> Claims { get; set; }
    }
}