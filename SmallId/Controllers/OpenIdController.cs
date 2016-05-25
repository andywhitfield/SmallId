using DotNetOpenAuth.Messaging;
using DotNetOpenAuth.OpenId.Extensions.ProviderAuthenticationPolicy;
using DotNetOpenAuth.OpenId.Provider;
using DotNetOpenAuth.OpenId.Provider.Behaviors;
using SmallId.Code;
using System;
using System.Web.Mvc;

namespace SmallId.Controllers
{
    public class OpenIdController : Controller
    {
        private static readonly OpenIdProvider OpenIdProvider = new OpenIdProvider();

        private readonly IFormsAuthentication formsAuth;

        public OpenIdController()
        {
            formsAuth = new FormsAuthenticationService();
        }

        [ValidateInput(false)]
        public ActionResult Provider()
        {
            IRequest request = OpenIdProvider.GetRequest();
            if (request != null)
            {
                // Some requests are automatically handled by DotNetOpenAuth.  If this is one, go ahead and let it go.
                if (request.IsResponseReady)
                {
                    return OpenIdProvider.PrepareResponse(request).AsActionResult();
                }

                // This is apparently one that the host (the web site itself) has to respond to.
                ProviderEndpoint.PendingRequest = (IHostProcessedRequest)request;

                // If PAPE requires that the user has logged in recently, we may be required to challenge the user to log in.
                var papeRequest = ProviderEndpoint.PendingRequest.GetExtension<PolicyRequest>();
                if (papeRequest != null && papeRequest.MaximumAuthenticationAge.HasValue)
                {
                    TimeSpan timeSinceLogin = DateTime.UtcNow - formsAuth.SignedInTimestampUtc.Value;
                    if (timeSinceLogin > papeRequest.MaximumAuthenticationAge.Value)
                    {
                        // The RP wants the user to have logged in more recently than he has.  
                        // We'll have to redirect the user to a login screen.
                        return RedirectToAction("LogOn", "Account", new { returnUrl = Url.Action("ProcessAuthRequest") });
                    }
                }

                return ProcessAuthRequest();
            }
            else
            {
                // No OpenID request was recognized.  This may be a user that stumbled on the OP Endpoint.  
                return View();
            }
        }

        public ActionResult ProcessAuthRequest()
        {
            if (ProviderEndpoint.PendingRequest == null)
            {
                return RedirectToAction("Index", "Home");
            }

            // Try responding immediately if possible.
            ActionResult response;
            if (AutoRespondIfPossible(out response))
            {
                return response;
            }

            // We can't respond immediately with a positive result.  But if we still have to respond immediately...
            if (ProviderEndpoint.PendingRequest.Immediate)
            {
                // We can't stop to prompt the user -- we must just return a negative response.
                return SendAssertion();
            }

            if (ProviderEndpoint.PendingAuthenticationRequest != null &&
                Uri.IsWellFormedUriString(ProviderEndpoint.PendingAuthenticationRequest.ClaimedIdentifier, UriKind.Absolute))
            {
                TempData["username"] = Util.GetUserFromClaimedIdentifier(new Uri(
                    ProviderEndpoint.PendingAuthenticationRequest.ClaimedIdentifier));
            }

            return RedirectToAction("AskUser");
        }

        /// <summary>
        /// Displays a confirmation page.
        /// </summary>
        /// <returns>The response for the user agent.</returns>
        [Authorize]
        public ActionResult AskUser()
        {
            if (ProviderEndpoint.PendingRequest == null)
            {
                // Oops... precious little we can confirm without a pending OpenID request.
                return RedirectToAction("Index", "Home");
            }

            // The user MAY have just logged in.  Try again to respond automatically to the RP if appropriate.
            ActionResult response;
            if (AutoRespondIfPossible(out response))
            {
                return response;
            }

            ViewData["Realm"] = ProviderEndpoint.PendingRequest.Realm;

            return View();
        }

        [HttpPost, Authorize, ValidateAntiForgeryToken]
        public ActionResult AskUserResponse(bool confirmed)
        {
            if (ProviderEndpoint.PendingAnonymousRequest != null)
            {
                ProviderEndpoint.PendingAnonymousRequest.IsApproved = confirmed;
            }
            else if (ProviderEndpoint.PendingAuthenticationRequest != null)
            {
                ProviderEndpoint.PendingAuthenticationRequest.IsAuthenticated = confirmed;
            }
            else
            {
                throw new InvalidOperationException("There's no pending authentication request!");
            }

            return SendAssertion();
        }

        /// <summary>
        /// Sends a positive or a negative assertion, based on how the pending request is currently marked.
        /// </summary>
        /// <returns>An MVC redirect result.</returns>
        public ActionResult SendAssertion()
        {
            var pendingRequest = ProviderEndpoint.PendingRequest;
            var authReq = pendingRequest as IAuthenticationRequest;
            var anonReq = pendingRequest as IAnonymousRequest;
            ProviderEndpoint.PendingRequest = null; // clear session static so we don't do this again
            if (pendingRequest == null)
            {
                throw new InvalidOperationException("There's no pending authentication request!");
            }

            // Set safe defaults if somehow the user ended up (perhaps through XSRF) here before electing to send data to the RP.
            if (anonReq != null && !anonReq.IsApproved.HasValue)
            {
                anonReq.IsApproved = false;
            }

            if (authReq != null && !authReq.IsAuthenticated.HasValue)
            {
                authReq.IsAuthenticated = false;
            }

            if (authReq != null && authReq.IsAuthenticated.Value)
            {
                if (authReq.IsDirectedIdentity)
                {
                    authReq.LocalIdentifier = Util.GetClaimedIdentifierForUser(User.Identity.Name);
                }

                if (!authReq.IsDelegatedIdentifier)
                {
                    authReq.ClaimedIdentifier = authReq.LocalIdentifier;
                }
            }

            // Respond to AX/sreg extension requests only on a positive result.
            if ((authReq != null && authReq.IsAuthenticated.Value) ||
                (anonReq != null && anonReq.IsApproved.Value))
            {
                // Look for PAPE requests.
                var papeRequest = pendingRequest.GetExtension<PolicyRequest>();
                if (papeRequest != null)
                {
                    var papeResponse = new PolicyResponse();
                    if (papeRequest.MaximumAuthenticationAge.HasValue)
                    {
                        papeResponse.AuthenticationTimeUtc = formsAuth.SignedInTimestampUtc;
                    }

                    pendingRequest.AddResponseExtension(papeResponse);
                }
            }

            return OpenIdProvider.PrepareResponse(pendingRequest).AsActionResult();
        }

        /// <summary>
        /// Attempts to formulate an automatic response to the RP if the user's profile allows it.
        /// </summary>
        /// <param name="response">Receives the ActionResult for the caller to return, or <c>null</c> if no automatic response can be made.</param>
        /// <returns>A value indicating whether an automatic response is possible.</returns>
        private bool AutoRespondIfPossible(out ActionResult response)
        {
            // If the odds are good we can respond to this one immediately (without prompting the user)...
            if (ProviderEndpoint.PendingRequest.IsReturnUrlDiscoverable(OpenIdProvider.Channel.WebRequestHandler) == RelyingPartyDiscoveryResult.Success
                && User.Identity.IsAuthenticated)
            {
                // Is this is an identity authentication request? (as opposed to an anonymous request)...
                if (ProviderEndpoint.PendingAuthenticationRequest != null)
                {
                    // If this is directed identity, or if the claimed identifier being checked is controlled by the current user...
                    if (ProviderEndpoint.PendingAuthenticationRequest.IsDirectedIdentity
                        || UserControlsIdentifier(ProviderEndpoint.PendingAuthenticationRequest))
                    {
                        ProviderEndpoint.PendingAuthenticationRequest.IsAuthenticated = true;
                        response = SendAssertion();
                        return true;
                    }
                }

                // If this is an anonymous request, we can respond to that too.
                if (ProviderEndpoint.PendingAnonymousRequest != null)
                {
                    ProviderEndpoint.PendingAnonymousRequest.IsApproved = true;
                    response = SendAssertion();
                    return true;
                }
            }

            response = null;
            return false;
        }
        
        /// <summary>
        /// Checks whether the logged in user controls the OP local identifier in the given authentication request.
        /// </summary>
        /// <param name="authReq">The authentication request.</param>
        /// <returns><c>true</c> if the user controls the identifier; <c>false</c> otherwise.</returns>
        private bool UserControlsIdentifier(IAuthenticationRequest authReq)
        {
            if (authReq == null)
            {
                throw new ArgumentNullException("authReq");
            }

            if (User == null || User.Identity == null)
            {
                return false;
            }

            Uri userLocalIdentifier = Util.GetClaimedIdentifierForUser(User.Identity.Name);
            return authReq.LocalIdentifier == userLocalIdentifier ||
                authReq.LocalIdentifier == PpidGeneration.PpidIdentifierProvider.GetIdentifier(userLocalIdentifier, authReq.Realm);
        }
    }
}