using EventPro.API.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;

namespace EventPro.API.Controllers
{
    public static class EventProFun
    {
        public static void DecodeToken(Microsoft.AspNetCore.Http.IHeaderDictionary headers, UserToken userToken)
        {
            string token = Convert.ToString(headers.Where(p => p.Key == "Authorization").Select(p => p.Value).FirstOrDefault());
            token = token.Replace("Bearer ", "");
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(token);
            var tokenS = handler.ReadToken(token) as JwtSecurityToken;

            userToken.UserId = Convert.ToInt32(tokenS.Claims.Where(p => p.Type.EndsWith(@"/claims/sid")).Select(p => p.Value).FirstOrDefault());
            userToken.UserName = tokenS.Claims.Where(p => p.Type.EndsWith(@"/claims/name")).Select(p => p.Value).FirstOrDefault();
            userToken.Role = tokenS.Claims.Where(p => p.Type.EndsWith(@"/claims/role")).Select(p => p.Value).FirstOrDefault();
            userToken.Issuer = tokenS.Issuer;
            userToken.ValidTo = tokenS.ValidTo;
        }
    }
}
