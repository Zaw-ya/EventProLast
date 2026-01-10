namespace EventPro.Web.Services
{
    using Microsoft.AspNetCore.DataProtection;

    public class UrlProtector
    {
        private readonly IDataProtector _protector;

        public UrlProtector(IDataProtectionProvider provider)
        {
            _protector = provider.CreateProtector("UrlIdProtector");
        }

        public string Protect(string plainTextId)
        {
            return _protector.Protect(plainTextId);
        }

        public string Unprotect(string protectedId)
        {
            return _protector.Unprotect(protectedId);
        }
    }
}
