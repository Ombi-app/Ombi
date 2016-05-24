using System;
using FluentValidation;
using PlexRequests.Core.SettingModels;

namespace PlexRequests.UI
{
	public class PlexRequestsValidator : AbstractValidator<PlexRequestSettings>
	{
		public PlexRequestsValidator ()
		{
			RuleFor (x => x.BaseUrl).NotEqual ("requests").WithMessage ("You cannot use 'requests' as this is reserved by the application.");
			RuleFor (x => x.BaseUrl).NotEqual ("admin").WithMessage ("You cannot use 'admin' as this is reserved by the application.");
			RuleFor (x => x.BaseUrl).NotEqual ("search").WithMessage ("You cannot use 'search' as this is reserved by the application.");
			RuleFor (x => x.BaseUrl).NotEqual ("issues").WithMessage ("You cannot use 'issues' as this is reserved by the application.");
			RuleFor (x => x.BaseUrl).NotEqual ("userlogin").WithMessage ("You cannot use 'userlogin' as this is reserved by the application.");
			RuleFor (x => x.BaseUrl).NotEqual ("login").WithMessage ("You cannot use 'login' as this is reserved by the application.");
			RuleFor (x => x.BaseUrl).NotEqual ("test").WithMessage ("You cannot use 'test' as this is reserved by the application.");
			RuleFor (x => x.BaseUrl).NotEqual ("approval").WithMessage ("You cannot use 'approval' as this is reserved by the application.");
			RuleFor (x => x.BaseUrl).NotEqual ("updatechecker").WithMessage ("You cannot use 'updatechecker' as this is reserved by the application.");
			RuleFor (x => x.BaseUrl).NotEqual ("usermanagement").WithMessage ("You cannot use 'usermanagement' as this is reserved by the application.");
			RuleFor (x => x.BaseUrl).NotEqual ("api").WithMessage ("You cannot use 'api' as this is reserved by the application.");

		
		
		}
	}
}

