using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Ombi.Api.Telegram
{
    public static class TelegramApiExtensions
    {
        public static IServiceCollection AddTelegramCallbacks<TMovieEngine, TTvEngine, TResult>(
            this IServiceCollection services,

            Func<IServiceProvider, TMovieEngine> initMovie,
            Func<TMovieEngine, int, Task<TResult>> approveMovie,
            Func<TMovieEngine, int, Task<TResult>> denyMovie,

            Func<IServiceProvider, TTvEngine> initTvShow,
            Func<TTvEngine, int, Task<TResult>> approveTv,
            Func<TTvEngine, int, Task<TResult>> denyTv,

            Func<TResult, string> formater
        )
        {
            services.AddSingleton<ITelegramRequestAnswer>(rootProvider =>
            {
                return new TelegramRequestAnswer<TMovieEngine, TTvEngine>(rootProvider, initMovie, initTvShow)
                {
                    OnApproveMovie = async (engine, id) => formater(await approveMovie(engine, id)),
                    OnDenyMovie = async (engine, id) => formater(await denyMovie(engine, id)),
                    OnApproveTv = async (engine, id) => formater(await approveTv(engine, id)),
                    OnDenyTv = async (engine, id) => formater(await denyTv(engine, id)),
                };
            });

            return services;
        }
    }
}
