using System;
using PlexRequests.UI.Modules;
using Nancy;
using Nancy.Extensions;
using Nancy.ModelBinding;
using Nancy.Responses.Negotiation;
using Nancy.Validation;
using PlexRequests.Core;
using System.Collections.Generic;
using PlexRequests.Store;
using PlexRequests.Core.SettingModels;

namespace PlexRequests.UI.Modules
{
	public class ApiModule : BaseModule
	{
		public ApiModule (IRequestService service, ISettingsService<PlexRequestSettings> settings) : base("api")
		{
			Get ["/requests"] = x => GetRequests ();

			RequestService = service;
			Settings = settings;
		}

		private IRequestService RequestService{ get; }
		private ISettingsService<PlexRequestSettings> Settings{get;}

		public Response GetRequests()
		{
			var apiModel = new ApiModel<List<RequestedModel>>{Data = new List<RequestedModel>()};
			if (!Authenticated ()) {
				apiModel.Error = true;
				apiModel.ErrorMessage = "ApiKey is invalid or not present, Please use 'apikey' in the querystring.";
				return ReturnReponse (apiModel);
			}
			var requests = RequestService.GetAll ();
			apiModel.Data.AddRange (requests);

			return ReturnReponse (apiModel);
		}
			
		private Response ReturnReponse(object result)
		{
			var queryString = (DynamicDictionary)Context.Request.Query;
			dynamic value;
			if (queryString.TryGetValue("xml", out value)) {
				if ((bool)value) {
					return Response.AsXml (result);
				}
			}
			return Response.AsJson (result);
		}

		private bool Authenticated(){

			var query = (DynamicDictionary)Context.Request.Query;
			dynamic key;
			if (!query.TryGetValue ("apikey", out key)) {
				return false;
			}

			var settings = Settings.GetSettings ();
			if ((string)key == settings.ApiKey) {
				return true;
			}
			return false;

		}
	}
}

