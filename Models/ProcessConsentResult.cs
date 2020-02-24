using System.Collections.Generic;

namespace ProjectIds.Models
{
    public class ProcessConsentResult
    {
        public bool IsRedirect => RedirectUri != null;
        public string RedirectUri { get; set; }
        public string ClientId { get; set; }
        public bool RememberConsent { get; set; }
        public List<string> ScopesConsented { get; set; } = new List<string>();
        public IEnumerable<ScopeViewModel> IdentityScopes { get; set; }
        public IEnumerable<ScopeViewModel> ResourceScopes { get; set; }
        public bool HasValidationError => ValidationError != null;
        public string ValidationError { get; set; }
    }

    public class ScopeViewModel
    {
        public string Name { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public bool Emphasize { get; set; }
        public bool Required { get; set; }
        public bool Checked { get; set; }
    }
}
