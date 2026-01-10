using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using EventPro.API.Controllers;
using EventPro.API.Models;
using System;

namespace EventPro.API.Services
{
    public class UserFromHeaders
    {
        static public IActionResult BadToken(IHeaderDictionary headers, UserToken userToken, IConfiguration _configuration)
        {
            EventProFun.DecodeToken(headers, userToken);
            if (userToken.Issuer != _configuration["JWT:ValidIssuer"])
            {
                return new UnauthorizedObjectResult("Token is not valid for this request");
            }
            if (userToken.ValidTo < DateTime.Now)
            {
                return new UnauthorizedObjectResult("Token Expire");
            }
            else return null;
        }
    }
}
