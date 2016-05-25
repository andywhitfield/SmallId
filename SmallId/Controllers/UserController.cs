using SmallId.Code;
using System;
using System.Linq;
using System.Web.Mvc;

namespace SmallId.Controllers
{
    public class UserController : Controller
    {
        /// <summary>
        /// Identities the specified id.
        /// </summary>
        /// <param name="id">The username or anonymous identifier.</param>
        /// <param name="anon">if set to <c>true</c> then <paramref name="id"/> represents an anonymous identifier rather than a username.</param>
        /// <returns>The view to display.</returns>
        public ActionResult Identity(string id, bool anon)
        {
            if (!anon)
            {
                var redirect = RedirectIfNotNormalizedRequestUri(id);
                if (redirect != null)
                {
                    return redirect;
                }
            }

            if (Request.AcceptTypes != null && Request.AcceptTypes.Contains("application/xrds+xml"))
            {
                return View("Xrds");
            }

            if (!anon)
            {
                ViewData["username"] = id;
            }

            return View();
        }

        public ActionResult Xrds(string id)
        {
            return View();
        }

        private ActionResult RedirectIfNotNormalizedRequestUri(string user)
        {
            Uri normalized = Util.GetClaimedIdentifierForUser(user);
            if (Request.Url != normalized)
            {
                return Redirect(normalized.AbsoluteUri);
            }

            return null;
        }
    }
}