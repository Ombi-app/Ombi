using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Ombi.Api.Telegram
{
    public class TelegramRequestAnswer<TMovieEngine, TTvEngine> : ITelegramRequestAnswer
    {
        public TelegramRequestAnswer(
            IServiceProvider rootProvider, 
            Func<IServiceProvider, TMovieEngine> initMovie,
            Func<IServiceProvider, TTvEngine> initTv
        )
        {
            _rootProvider = rootProvider;
            _initMovie = initMovie;
            _initTv = initTv;
        }


        private readonly IServiceProvider _rootProvider;
        private readonly Func<IServiceProvider, TMovieEngine> _initMovie;
        private readonly Func<IServiceProvider, TTvEngine> _initTv;

        public Func<TMovieEngine, int, Task<string>> OnApproveMovie { get; set; }
        public Func<TMovieEngine, int, Task<string>> OnDenyMovie { get; set; }
        public Func<TTvEngine, int, Task<string>> OnApproveTv { get; set; }
        public Func<TTvEngine, int, Task<string>> OnDenyTv { get; set; }


        public async Task<string> ApproveMovie(int id) => await Execute(_initMovie, OnApproveMovie, id);
        public async Task<string> DenyMovie(int id) => await Execute(_initMovie, OnDenyMovie, id);
        public async Task<string> ApproveTv(int id) => await Execute(_initTv, OnApproveTv, id);
        public async Task<string> DenyTv(int id) => await Execute(_initTv, OnDenyTv, id);


        private async Task<string> Execute<TEngine>(Func<IServiceProvider, TEngine> init, Func<TEngine, int, Task<string>> action, int id)
        {
            using (var scope = _rootProvider.CreateScope())
            {
                var engine = init(scope.ServiceProvider);
                return await action(engine, id);
            }
        }
    }
}
