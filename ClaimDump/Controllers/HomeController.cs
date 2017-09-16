using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Web.Mvc;

namespace ClaimDump.Controllers
{
    public class PageRef
    {
        public string PartitionKey { get; set; }  // "registered_user"
        public string RowKey { get; set; }        // slug = twitter handle
        public string Name { get; set; }          // user's name
        public string Description { get; set; }   // description of user
        public string UserAvatarUrl { get; set; } // url to user's profile photo
    }


    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            var isAuth = ((System.Security.Claims.ClaimsPrincipal)(System.Threading.Thread.CurrentPrincipal)).Identity.IsAuthenticated;
            if (isAuth)
                return Redirect("http://pageofphotos.com");
            else
                return View();
        }

        public ActionResult About()
        {
            var isAuth = ((System.Security.Claims.ClaimsPrincipal)(System.Threading.Thread.CurrentPrincipal)).Identity.IsAuthenticated;
            if (isAuth)
            {
                var claims = ((System.Security.Claims.ClaimsPrincipal)(System.Threading.Thread.CurrentPrincipal)).Claims;

                var SlugClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn";
                var NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
                var DescriptionClaimType = "urn:twitter:description";
                var ProfileImageUrlClaimType = "urn:twitter:profile_image_url_https";

                var slug = claims.FirstOrDefault(c => c.Type == SlugClaimType).Value;
                var name = claims.FirstOrDefault(c => c.Type == NameClaimType).Value;
                var desc = claims.FirstOrDefault(c => c.Type == DescriptionClaimType).Value;
                var url = claims.FirstOrDefault(c => c.Type == ProfileImageUrlClaimType).Value;

                var pageRef =
                     new PageRef()
                     {
                         PartitionKey = "registered_user",
                         RowKey = slug,
                         Name = name,
                         Description = $"{Guid.NewGuid()} - {desc}",
                         UserAvatarUrl = url
                     };

                // TODO: Do something with this claim data that's already packaged up!
                string registrationUrl = ConfigurationManager.AppSettings["RegistrationUrl"];
                // TODO: MOVE RegistrationUrl TO CONFIG
//                registrationUrl = "";
                if (!String.IsNullOrEmpty(registrationUrl))
                {
                    var client = new HttpClient();
                    string json = JsonConvert.SerializeObject(pageRef);
                    var requestData = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                    var uri = new Uri(registrationUrl);
                    var response = client.PostAsync(uri, requestData).Result;
                    var result = response.Content.ReadAsStringAsync().Result;
                }
                // TODO: fix redirect back to PoP
                // return RedirectToAction("Index", "Home");             
                ViewBag.Message = $"Congrats! @{slug} [{name}] has just signed up for a pageofphotos.com account! (or maybe already had one, but we'll double-check either way!)";
            }
            else
            {
                ViewBag.Message = "Nobody is logged in";
            }
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
    }
}



#if false
using System.Net;

public static void Run(HttpRequestMessage req, out PageRef pageRef, TraceWriter log)
{
    log.Info("BEGIN - RegisterTwitterUser");

    var claims = ((System.Security.Claims.ClaimsPrincipal)(System.Threading.Thread.CurrentPrincipal)).Claims;
    if (claims == null)
    {
        return req.CreateResponse(HttpStatusCode.BadRequest, "Please authenticate with Twitter before calling this endpoint. Probably this means that the Authentication from Azure Functions or Web Apps is not configured - or that the Twitter App configuration is not aligned.");
    }

    var isAuth = ((System.Security.Claims.ClaimsPrincipal)(System.Threading.Thread.CurrentPrincipal)).Identity.IsAuthenticated;

    log.Info($"Is Authenticated? {isAuth}");

#if DEBUG
    foreach (var claim in claims)
    {
        log.Info($"Claim '{claim.Type}' = '{claim.Value}'");
    }
#endif

    var SlugClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/upn";
    var NameClaimType = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name";
    var DescriptionClaimType = "urn:twitter:description";
    var ProfileImageUrlClaimType = "urn:twitter:profile_image_url_https";

    var slug = claims.FirstOrDefault(c => c.Type == SlugClaimType).Value;
    var name = claims.FirstOrDefault(c => c.Type == NameClaimType).Value;
    var desc = claims.FirstOrDefault(c => c.Type == DescriptionClaimType).Value;
    var url = claims.FirstOrDefault(c => c.Type == ProfileImageUrlClaimType).Value;

    pageRef =
         new PageRef()
         {
             PartitionKey = "registered_user",
             RowKey = slug,
             Name = name,
             Description = $"{Guid.NewGuid()} - {desc}",
             UserAvatarUrl = url
         };

    log.Info("END - RegisterTwitterUser");
}

public class PageRef
{
    public string PartitionKey { get; set; }  // "registered_user"
    public string RowKey { get; set; }        // slug = twitter handle
    public string Name { get; set; }          // user's name
    public string Description { get; set; }   // description of user
    public string UserAvatarUrl { get; set; } // url to user's profile photo
}
#endif