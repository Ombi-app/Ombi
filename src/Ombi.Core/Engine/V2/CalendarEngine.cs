using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Ombi.Core.Authentication;
using Ombi.Core.Engine.Interfaces;
using Ombi.Core.Models.Search.V2;
using Ombi.Core.Rule.Interfaces;
using Ombi.Store.Repository.Requests;

namespace Ombi.Core.Engine.V2
{
    public class CalendarEngine : BaseEngine, ICalendarEngine
    {
        public CalendarEngine(IPrincipal user, OmbiUserManager um, IRuleEvaluator rules, IMovieRequestRepository movieRepo,
            ITvRequestRepository tvRequestRepo) : base(user, um, rules)
        {
            _movieRepo = movieRepo;
            _tvRepo = tvRequestRepo;
        }

        private readonly IMovieRequestRepository _movieRepo;
        private readonly ITvRequestRepository _tvRepo;

        public async Task<List<CalendarViewModel>> GetCalendarData()
        {
            var viewModel = new List<CalendarViewModel>();
            var movies = _movieRepo.GetAll().Where(x =>
                x.ReleaseDate > DateTime.Now.AddDays(-30) && x.ReleaseDate < DateTime.Now.AddDays(30));
            var episodes = _tvRepo.GetChild().SelectMany(x => x.SeasonRequests.SelectMany(e => e.Episodes)).ToList();
            foreach (var e in episodes)
            {
                viewModel.Add(new CalendarViewModel
                {
                    Title = e.Title,
                    Start = e.AirDate.Date
                });
            }

            foreach (var m in movies)
             {
                 viewModel.Add(new CalendarViewModel
                 {
                     Title = m.Title,
                     Start = m.ReleaseDate.Date
                 });
             }

             return viewModel;
        }
    }
}