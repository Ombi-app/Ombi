Handlebars.registerHelper('if_eq', function (a, b, opts) {
    if (a == b)
        return opts.fn(this);
    else
        return opts.inverse(this);
});

var searchSource = $("#search-template").html();
var searchTemplate = Handlebars.compile(searchSource);
var movieTimer = 0;
var tvimer = 0;

movieLoad();
tvLoad();


function movieLoad() {
    $("#movieList").html("");

    $.ajax("/requests/movies/").success(function (results) {
        results.forEach(function (result) {
            var context = buildMovieRequestContext(result);

            var html = searchTemplate(context);
            $("#movieList").append(html);
        });
    });
};

function tvLoad() {
    $("#tvList").html("");

    $.ajax("/requests/tvshows/").success(function (results) {
        results.forEach(function (result) {
            var context = buildTvShowRequestContext(result);
            var html = searchTemplate(context);
            $("#tvList").append(html);
        });
    });
};

function buildMovieRequestContext(result) {
    var date = new Date(result.releaseDate);
    var year = date.getFullYear();
    var context = {
        posterPath: result.posterPath,
        id: result.tmdbid,
        title: result.title,
        overview: result.overview,
        year: year,
        type: "movie",
        status: result.status
    };

    return context;
}

function buildTvShowRequestContext(result) {
    var date = new Date(result.releaseDate);
    var year = date.getFullYear();
    var context = {
        posterPath: result.posterPath,
        id: result.tmdbid,
        title: result.title,
        overview: result.overview,
        year: year,
        type: "tv",
        status: result.status
    };
    return context;
}
