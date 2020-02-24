using System.Collections.Generic;
using System.Security.Claims;
using IdentityModel;
using IdentityServer4;
using IdentityServer4.Models;
using IdentityServer4.Test;

namespace ProjectIds
{
    public class Config
    {
        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResource("roles", "角色", new List<string> { JwtClaimTypes.Role }),
                new IdentityResource("locations", "地点", new List<string> { "location" })
            };
        }

        public static IEnumerable<Client> GetClients()
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "projectmanage",
                    ClientName = "系统管理端",
                    AllowedGrantTypes = GrantTypes.HybridAndClientCredentials,
                    ClientSecrets = {new Secret("managesecret".Sha256())},

                    RedirectUris = { "http://localhost:5003/signin-oidc" },
                    PostLogoutRedirectUris = { "http://localhost:5003/signout-callback-oidc" },

                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.OfflineAccess,
                        "projectapi",
                        "roles",
                        "locations"
                    },
                    AccessTokenType = AccessTokenType.Reference, // 刷新token模式
                    AlwaysIncludeUserClaimsInIdToken = true,
                    AllowOfflineAccess = true
                }
            };
        }

        public static IEnumerable<ApiResource> GetApis()
        {
            return new List<ApiResource>
            {
                new ApiResource{
                    Name="projectapi",
                    DisplayName="项目API资源",
                    // 使用introspection endpoint的密钥
                    ApiSecrets={ new Secret("projectapisecret".Sha256()) },
                    // 在访问令牌（access token）中增加除了subject id以外其他的用户声明（claim）
                    UserClaims={"roles","locations" },
                    // scope
                    Scopes =
                    {
                        new Scope()
                        {
                            Name = "projectapi",
                            DisplayName = "projectapi"
                        }
                    }
                }
            };
        }

        public static List<TestUser> Users()
        {
            return new List<TestUser>
            {
                new TestUser
                {
                    SubjectId = "1",
                    Username = "alice",
                    Password = "alice",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Alice Smith"),
                        new Claim(JwtClaimTypes.GivenName, "Alice"),
                        new Claim(JwtClaimTypes.FamilyName, "Smith"),
                        new Claim(JwtClaimTypes.Email, "AliceSmith@email.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.WebSite, "http://alice.com"),
                        new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServerConstants.ClaimValueTypes.Json),
                        new Claim(JwtClaimTypes.Role,"admin")
                    }
                },
                new TestUser
                {
                    SubjectId = "11",
                    Username = "bob",
                    Password = "bob",
                    Claims =
                    {
                        new Claim(JwtClaimTypes.Name, "Bob Smith"),
                        new Claim(JwtClaimTypes.GivenName, "Bob"),
                        new Claim(JwtClaimTypes.FamilyName, "Smith"),
                        new Claim(JwtClaimTypes.Email, "BobSmith@email.com"),
                        new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                        new Claim(JwtClaimTypes.WebSite, "http://bob.com"),
                        new Claim(JwtClaimTypes.Address, @"{ 'street_address': 'One Hacker Way', 'locality': 'Heidelberg', 'postal_code': 69118, 'country': 'Germany' }", IdentityServerConstants.ClaimValueTypes.Json),
                        new Claim("location", "somewhere"),
                        new Claim(JwtClaimTypes.Role,"user")

                    }
                }
            };
        }
    }
}
