using HtmlAgilityPack;
using Sitecore;
using Sitecore.Abstractions;
using Sitecore.Analytics;
using Sitecore.Analytics.Core;
using Sitecore.Analytics.Tracking;
using Sitecore.Analytics.Web;
using Sitecore.Buckets.Extensions;
using Sitecore.Data;
using Sitecore.Diagnostics;
using Sitecore.FXM.Abstractions;
using Sitecore.FXM.Extensions;
using Sitecore.FXM.Matchers;
using Sitecore.FXM.Pipelines.Tracking.BeforeEvent;
using Sitecore.FXM.Pipelines.Tracking.TrackPageVisit;
using Sitecore.FXM.Service.Abstractions;
using Sitecore.FXM.Service.ActionFilters;
using Sitecore.FXM.Service.Data;
using Sitecore.FXM.Service.Model;
using Sitecore.FXM.Service.Utils;
using Sitecore.FXM.Tracking;
using Sitecore.FXM.Utilities;
using Sitecore.Globalization;
using Sitecore.Services.Core;
using Sitecore.Services.Infrastructure.Sitecore.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.Http.Cors;

namespace Volvo.Fxm.Service.Controllers
{
    [ConfiguredP3PHeader, RobotDetectionFilter, ServicesController("Beacon.Service"), EnableCors("*", "*", "GET,POST", SupportsCredentials = true)]
    public class BeaconController : EntityService<BeaconEntity>
    {
        private readonly ILog logger;

        private readonly ICorePipeline pipeline;

        private readonly ITrackerProvider trackerProvider;

        private readonly ITrackingManager trackingManager;

        private readonly ISitecoreContext sitecoreContext;

        private readonly ISettings sitecoreSettings;

        private readonly IRequestHelper requestHelper;

        private readonly HttpContextBase httpContextBase;

        private readonly IWebClient webClient;

        public BeaconController() : this(new BeaconEntityRepository())
        {
        }

        public BeaconController(IRepository<BeaconEntity> repository, ILog logger, ICorePipeline pipeline, ITrackerProvider trackerProvider, ISitecoreContext sitecoreContext, ISettings settings, IRequestHelper request, HttpContextBase httpContext, IWebClient client, ITrackingManager trackingManager) : base(repository)
        {
            Assert.ArgumentNotNull(logger, "logger");
            Assert.ArgumentNotNull(pipeline, "pipeline");
            Assert.ArgumentNotNull(settings, "settings");
            Assert.ArgumentNotNull(trackerProvider, "TrackerProvider");
            Assert.ArgumentNotNull(client, "client");
            Assert.ArgumentNotNull(trackingManager, "trackingManager");
            this.logger = logger;
            this.pipeline = pipeline;
            this.trackerProvider = trackerProvider;
            this.sitecoreSettings = settings;
            this.requestHelper = request;
            this.sitecoreContext = sitecoreContext;
            this.trackingManager = trackingManager;
            this.httpContextBase = httpContext;
            this.webClient = client;
        }

        public BeaconController(IRepository<BeaconEntity> repository) : this(repository, new LogWrapper(), new CorePipelineWrapper(), new TrackerProviderWrapper(), new SitecoreContextWrapper(), new SettingsWrapper(), new RequestHelper(), new HttpContextWrapper(HttpContext.Current), new WebClientWrapper(new WebClient()), new TrackingManager(new CorePipelineWrapper(), new TrackerProviderWrapper(), new SitecoreContextWrapper()))
        {
        }

        [HttpPost]
        public BeaconEntity TrackPageVisit(string page, string referrer, string contactId = "")
        {
            SpoofedHttpRequestBase spoofedRequest = this.GetSpoofedRequest(page, referrer);
            string resolvedContactIdentifier = this.ResolveContactIdentifier(contactId, this.requestHelper.IsRequestInternal(spoofedRequest));
            return this.ExecuteTrackingRequest(string.Format("[PageVisit] : Page = {0} : Referrer = {1} : CID = {2}", page, referrer, resolvedContactIdentifier), delegate
            {
                PageVisitParameters parameters = new PageVisitParameters(spoofedRequest.Url, spoofedRequest.UrlReferrer, resolvedContactIdentifier);
                return this.trackingManager.TrackPageVisit(this.httpContextBase.Request, spoofedRequest, this.httpContextBase.Response, parameters);
            });
        }

        [HttpPost]
        public BeaconEntity TrackGoal(string id, string page, string contactId = "", string data = null, string dataKey = null)
        {
            SpoofedHttpRequestBase spoofedRequest = this.GetSpoofedRequest(page, string.Empty);
            IDictionary<string, string> extras = null;
            if (HttpContext.Current != null && base.Request != null)
            {
                extras = WebUtil.ExtractPageEventParameters(base.Request.GetQueryNameValuePairs());
            }
            return this.TrackPageEvent(id, spoofedRequest, PageEventType.Goal, page, this.ResolveContactIdentifier(contactId, this.requestHelper.IsRequestInternal(spoofedRequest)), data, dataKey, extras, "0");
        }

        [HttpPost]
        public BeaconEntity TrackOutcome(string id, string page, string contactId = "", string monetaryValue = "0")
        {
            SpoofedHttpRequestBase spoofedRequest = this.GetSpoofedRequest(page, string.Empty);
            return this.TrackPageEvent(id, spoofedRequest, PageEventType.Outcome, page, this.ResolveContactIdentifier(contactId, this.requestHelper.IsRequestInternal(spoofedRequest)), null, null, WebUtil.ExtractPageEventParameters(base.Request.GetQueryNameValuePairs()), monetaryValue);
        }

        [HttpPost]
        public BeaconEntity TrackCampaign(string id, string page, string contactId = "")
        {
            SpoofedHttpRequestBase spoofedRequest = this.GetSpoofedRequest(page, string.Empty);
            return this.TrackPageEvent(id, spoofedRequest, PageEventType.Campaign, page, this.ResolveContactIdentifier(contactId, this.requestHelper.IsRequestInternal(spoofedRequest)), null, null, null, "0");
        }

        [HttpPost]
        public BeaconEntity TrackEvent(string id, string page, string contactId = "", string data = null, string dataKey = null)
        {
            SpoofedHttpRequestBase spoofedRequest = this.GetSpoofedRequest(page, string.Empty);
            IDictionary<string, string> extras = null;
            if (HttpContext.Current != null && base.Request != null)
            {
                extras = WebUtil.ExtractPageEventParameters(base.Request.GetQueryNameValuePairs());
            }
            return this.TrackPageEvent(id, spoofedRequest, PageEventType.Event, page, this.ResolveContactIdentifier(contactId, this.requestHelper.IsRequestInternal(spoofedRequest)), data, dataKey, extras, "0");
        }

        [HttpPost]
        public void TriggerElementMatch(string id, string page, string referrer, string contactId = "")
        {
            SpoofedHttpRequestBase spoofedRequest = this.GetSpoofedRequest(page, referrer);
            this.TrackPageEvent(id, spoofedRequest, PageEventType.Element, page, this.ResolveContactIdentifier(contactId, this.requestHelper.IsRequestInternal(spoofedRequest)), null, null, null, "0");
        }

        [HttpGet]
        public bool Ping(Uri address)
        {
            Uri address2;
            if (address.IsAbsoluteUri)
            {
                address2 = address;
            }
            else
            {
                address2 = new Uri(string.Format("http://{0}", address));
            }
            string text;
            try
            {
                text = this.webClient.Download(address2);
            }
            catch (WebException ex)
            {
                this.logger.Error(ex.Message, ex, this);
                throw new HttpResponseException(new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    ReasonPhrase = ex.Message
                });
            }
            if (string.IsNullOrEmpty(text))
            {
                this.logger.Warn(string.Format("[BeaconController - Ping] Response body for request {0} was empty", address), this);
                return false;
            }
            this.logger.Debug(string.Format("[Ping]: Page: {0}", address), null);
            HtmlDocument htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(text);
            HtmlNodeCollection htmlNodeCollection = htmlDocument.DocumentNode.SelectNodes("//script");
            if (htmlNodeCollection == null || !htmlNodeCollection.Any<HtmlNode>())
            {
                return false;
            }
            foreach (HtmlNode current in ((IEnumerable<HtmlNode>)htmlNodeCollection))
            {
                string attributeValue = current.GetAttributeValue("src", string.Empty);
                string beaconHostName = this.GetBeaconHostName();
                if (attributeValue.Equals(this.GetBeaconBundleAddress(beaconHostName), StringComparison.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        [HttpGet]
        public string BundleAddress()
        {
            string beaconHostName = this.GetBeaconHostName();
            return this.GetBeaconBundleAddress(beaconHostName);
        }

        [HttpGet]
        public BeaconEntity StopTracking(string contactId = "", bool killContact = false, string endTime = "")
        {
            if (this.httpContextBase != null && this.httpContextBase.Session != null)
            {
                string arg;
                if (!this.TryGetSessionId(out arg))
                {
                    HttpExceptionHelper.InternetServerError("Unexpected server error", "Session could not be initialized.");
                }
                this.logger.Debug(string.Format("[StopTracking]: CID: {0} SID: {1}", contactId, arg), null);
                HttpRequestBase httpRequestBase = this.httpContextBase.Request;
                if (!base.Request.Headers.Referrer.Host.Equals(base.Request.RequestUri.Host))
                {
                    SpoofedHttpRequestBase spoofedHttpRequestBase = new SpoofedHttpRequestBase(this.httpContextBase.Request);
                    spoofedHttpRequestBase.SetUrl(base.Request.Headers.Referrer);
                    httpRequestBase = spoofedHttpRequestBase;
                    IDomainMatcher domainMatcher;
                    Language language;
                    if (!this.trackingManager.DomainIsValid(httpRequestBase, out domainMatcher, out language))
                    {
                        throw new HttpResponseException(HttpStatusCode.BadRequest);
                    }
                    TrackPageVisitArgs args = new TrackPageVisitArgs(this.sitecoreContext, this.trackerProvider, this.httpContextBase.Request, this.httpContextBase.Response, httpRequestBase, new PageVisitParameters(this.httpContextBase.Request.Url, null, contactId), domainMatcher, false);
                    InitializeTrackingCookieProcessor initializeTrackingCookieProcessor = new InitializeTrackingCookieProcessor();
                    initializeTrackingCookieProcessor.Process(args);
                    InitializeExternalTrackingProcessor initializeExternalTrackingProcessor = new InitializeExternalTrackingProcessor();
                    initializeExternalTrackingProcessor.Process(args);
                    InitializeContextSiteProcessor initializeContextSiteProcessor = new InitializeContextSiteProcessor();
                    initializeContextSiteProcessor.Process(args);
                }
                else
                {
                    Tracker.Initialize();
                    Tracker.Current.Session.SetClassification(0, 0, false);
                }
                if (!string.IsNullOrWhiteSpace(endTime))
                {
                    DateTime dateTime = DateTime.Parse(endTime, DateTimeFormatInfo.InvariantInfo);
                    CurrentInteraction interaction = this.trackerProvider.Current.Interaction;
                    Assert.IsNotNull(interaction, "interaction");
                    TimeSpan t = DateTime.UtcNow - DateUtil.ToUniversalTime(dateTime);
                    interaction.EndDateTime = dateTime;
                    interaction.StartDateTime -= t;
                    Page[] pages = interaction.Pages;
                    for (int i = 0; i < pages.Length; i++)
                    {
                        Page page = pages[i];
                        page.DateTime -= t;
                    }
                }
                this.httpContextBase.Session.Abandon();
                this.httpContextBase.Response.Cookies.Clear();
                if (killContact)
                {
                    DateTime expires = new DateTime(1979, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    string[] allKeys = this.httpContextBase.Request.Cookies.AllKeys;
                    for (int j = 0; j < allKeys.Length; j++)
                    {
                        string text = allKeys[j];
                        HttpCookie cookie = new HttpCookie(text)
                        {
                            Expires = expires,
                            Value = string.Empty,
                            Domain = this.httpContextBase.Request.Cookies.GetSafely(text).Domain
                        };
                        this.httpContextBase.Response.Cookies.Add(cookie);
                    }
                    this.httpContextBase.Response.Cookies.Add(new HttpCookie("SC_ANALYTICS_GLOBAL_COOKIE")
                    {
                        Domain = "." + this.httpContextBase.Request.Url.Host,
                        Value = string.Empty,
                        Expires = expires
                    });
                }
            }
            return new BeaconEntity
            {
                Id = "TBC",
                ContactId = string.Empty,
                SessionId = string.Empty,
                Url = string.Empty
            };
        }

        protected SpoofedHttpRequestBase GetSpoofedRequest(string page, string referrer)
        {
            Uri url;
            if (string.IsNullOrWhiteSpace(page) || !Uri.TryCreate(page, UriKind.Absolute, out url))
            {
                throw HttpExceptionHelper.BadRequest("Bad Request", "No valid page was specified.");
            }
            Uri urlReferrer = null;
            if (!string.IsNullOrEmpty(referrer) && !Uri.TryCreate(referrer, UriKind.RelativeOrAbsolute, out urlReferrer))
            {
                throw HttpExceptionHelper.BadRequest("Bad Request", "An invalid referrer was specified.");
            }
            SpoofedHttpRequestBase spoofedHttpRequestBase = new SpoofedHttpRequestBase(this.httpContextBase.Request);
            spoofedHttpRequestBase.SetUrl(url);
            spoofedHttpRequestBase.SetUrlReferrer(urlReferrer);
            return spoofedHttpRequestBase;
        }

        protected string ResolveContactIdentifier(string contactId, bool isInternalRequest)
        {
            if (!isInternalRequest)
            {
                return contactId;
            }
            ContactKeyCookie contactKeyCookie = new ContactKeyCookie("");
            return contactKeyCookie.ContactId.ToString("N");
        }

        protected BeaconEntity ExecuteTrackingRequest(string requestInfo, Func<TrackingResult> implementation)
        {
            BeaconEntity result;
            try
            {
                string text;
                if (!this.TryGetSessionId(out text))
                {
                    throw HttpExceptionHelper.BadRequest("Bad Request", "Session could not be initialized with the details provided.");
                }
                TrackingResult trackingResult = implementation();
                if (trackingResult == null)
                {
                    throw HttpExceptionHelper.InternetServerError("Unexpected server error", "Tracking result could not be determined.");
                }
                if (trackingResult.ResultCode != TrackingResultCode.Success)
                {
                    this.logger.Debug(string.Format("[FXM Tracking] [Failed] : Session = {0} : Code : {1} : Message : {2} : {3}", new object[]
                    {
                        text,
                        trackingResult.ResultCode,
                        trackingResult.Message,
                        requestInfo
                    }), null);
                    throw HttpExceptionHelper.BadRequest(trackingResult.ResultCode.ToString(), trackingResult.Message);
                }
                this.logger.Debug((!trackingResult.DoNotTrack) ? string.Format("[FXM Tracking] : Session = {0} : {1}", text, requestInfo) : string.Format("[FXM Tracking] [DoNotTrack] : Session = {0} : {1}", text, requestInfo), null);
                BeaconEntity beaconEntity = (BeaconEntity)trackingResult;
                beaconEntity.SessionId = text;
                result = beaconEntity;
            }
            catch (HttpResponseException exception)
            {
                this.logger.Error(string.Format("[FXM Tracking] [Error] : {0}", requestInfo), exception, typeof(BeaconController));
                throw;
            }
            catch (Exception ex)
            {
                this.logger.Error(string.Format("[FXM Tracking] [Error] : {0}", requestInfo), ex, typeof(BeaconController));
                throw HttpExceptionHelper.InternetServerError("Unexpected server error", ex.Message);
            }
            return result;
        }

        private BeaconEntity TrackPageEvent(string id, SpoofedHttpRequestBase spoofedRequest, PageEventType eventType, string page, string contactId, string data = null, string dataKey = null, IDictionary<string, string> extras = null, string monetaryValue = "0")
        {
            return this.ExecuteTrackingRequest(string.Format("[PageVisit] : Page = {0} : Event = {1} : CID = {2}", page, eventType, contactId), delegate
            {
                if (string.IsNullOrWhiteSpace(id))
                {
                    throw HttpExceptionHelper.BadRequest("Bad Request", "No event identifier was specified.");
                }
                PageEventParameters pageEventParameters;
                if (id.IsGuid())
                {
                    pageEventParameters = new PageEventParameters(spoofedRequest.Url, new ID(id), eventType, contactId);
                }
                else
                {
                    pageEventParameters = new PageEventParameters(spoofedRequest.Url, id, eventType, contactId);
                }
                pageEventParameters.Data = data;
                pageEventParameters.DataKey = dataKey;
                pageEventParameters.Extras = extras;
                if (eventType == PageEventType.Outcome)
                {
                    decimal monetaryValue2;
                    if (!decimal.TryParse(monetaryValue, out monetaryValue2))
                    {
                        throw HttpExceptionHelper.BadRequest("Bad Request", "The outcome monetary value is invalid.");
                    }
                    pageEventParameters.MonetaryValue = monetaryValue2;
                }
                return this.trackingManager.TrackPageEvent(this.httpContextBase.Request, spoofedRequest, this.httpContextBase.Response, pageEventParameters);
            });
        }

        private string GetBeaconHostName()
        {
            string uriHost = FxmUtility.GetUriHost(this.httpContextBase.Request.Url);
            string text = this.sitecoreSettings.GetSetting("FXM.Hostname", uriHost);
            if (string.IsNullOrEmpty(text))
            {
                text = uriHost;
            }
            return text;
        }

        private bool TryGetSessionId(out string sessionId)
        {
            sessionId = ((this.httpContextBase.Session != null) ? this.httpContextBase.Session.SessionID : null);
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                sessionId = null;
            }
            return sessionId != null;
        }

        private string GetBeaconBundleAddress(string host)
        {
            string setting = this.sitecoreSettings.GetSetting("FXM.Protocol", string.Empty);
            string setting2 = this.sitecoreSettings.GetSetting("Bundle.BasePath", string.Empty);
            return string.Format("{0}{1}{2}/{3}", new object[]
            {
                setting,
                host.TrimEnd(new char[]
                {
                    '/'
                }),
                setting2.TrimStart(new char[]
                {
                    '~'
                }).TrimEnd(new char[]
                {
                    '/'
                }),
                "beacon"
            });
        }
    }
}