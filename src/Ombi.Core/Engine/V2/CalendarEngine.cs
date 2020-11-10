using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Ombi.Core.Authentication;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Search.V2;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Entities;
using Ombi.Store.Entities.Requests;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Engine.V2
{
    public class CalendarEngine : BaseEngine, ICalendarEngine
    {
        public DateTime DaysAgo => DateTime.Now.AddDays(-90);
        public DateTime DaysAhead => DateTime.Now.AddDays(90);
        public CalendarEngine(IPrincipal user, OmbiUserManager um, IRuleEvaluator rules, IMovieRequestRepository movieRepo,
            ITvRequestRepository tvRequestRepo) : base(user, um, rules)
        {
            _movieRepo = movieRepo;
            _tvRepo = tvRequestRepo;
        }

        private readonly IMovieRequestRepository _movieRepo;
        private readonly ITvRequestRepository _tvRepo;

        public Task<List<CalendarViewModel>> GetCalendarData()
        {
            var viewModel = new List<CalendarViewModel>();
            var movies = _movieRepo.GetAll().Where(x =>
                x.ReleaseDate > DaysAgo && x.ReleaseDate < DaysAhead);
            var episodes = _tvRepo.GetChild().SelectMany(x => x.SeasonRequests.SelectMany(e => e.Episodes
                .Where(w => w.AirDate > DaysAgo && w.AirDate < DaysAhead)));
            foreach (var e in episodes)
            {
                viewModel.Add(new CalendarViewModel
                {
                    Title = e.Title,
                    Start = e.AirDate.Date,
                    Type = RequestType.TvShow,
                    BackgroundColor = GetBackgroundColor(e),
                    ExtraParams = new List<ExtraParams>
                    {
                        new ExtraParams
                        {
                            Overview = e.Season?.ChildRequest?.ParentRequest?.Overview ?? string.Empty,
                            ProviderId = e.Season?.ChildRequest?.ParentRequest?.TvDbId ?? 0,
                            Type = RequestType.TvShow,
                            ReleaseDate = e.AirDate,
                            RequestStatus = e.RequestStatus
                        }
                    }
                });
            }

            foreach (var m in movies)
            {
                viewModel.Add(new CalendarViewModel
                {
                    Title = m.Title,
                    Start = m.ReleaseDate.Date,
                    BackgroundColor = GetBackgroundColor(m),
                    Type = RequestType.Movie,
                    ExtraParams = new List<ExtraParams>
                     {
                     new ExtraParams
                     {
                         Overview = m.Overview,
                         ProviderId = m.TheMovieDbId,
                         Type = RequestType.Movie,
                         ReleaseDate = m.ReleaseDate,
                         RequestStatus = m.RequestStatus
                     }
                 }
                });
            }

            return Task.FromResult(viewModel);
        }

        private string GetBackgroundColor(BaseRequest req)
        {
            if (req.Available)
            {
                return "#469c83";
            }

            if (!req.Available)
            {
                if (req.Denied ?? false)
                {
                    return "red";
                }
                if (req.Approved)
                {
                    // We are approved state
                    return "blue";
                }

                if (!req.Approved)
                {
                    // Processing
                    return "teal";
                }
            }

            return "gray";
        }

        private string GetBackgroundColor(EpisodeRequests req)
        {
            if (req.Available)
            {
                return "#469c83";
            }

            if (!req.Available)
            {
                if (req.Approved)
                {
                    // We are approved state
                    return "blue";
                }

                if (!req.Approved)
                {
                    // Processing
                    return "teal";
                }

            }

            return "gray";
        }
    }
}