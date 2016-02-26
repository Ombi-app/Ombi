var searchSource = $("#search-template").html();
var searchTemplate = Handlebars.compile(searchSource);
var movieTimer = 0;
var tvimer = 0;

$("#movieSearchContent").on("keyup", function (e) {
    if (movieTimer) {
        clearTimeout(movieTimer);
    }
    movieTimer = setTimeout(movieSearch, 400);
});

$("#tvSearchContent").on("keyup", function (e) {
    if (tvimer) {
        clearTimeout(tvimer);
    }
    tvimer = setTimeout(tvSearch(), 400);
});

function movieSearch() {
    $("#movieList").html("");
    var query = $("#movieSearchContent").val();

    $.ajax("/search/movie/" + query).success(function (results) {
        results.forEach(function (result) {
            var context = buildMovieContext(result);

            var html = searchTemplate(context);
            $("#movieList").append(html);
        });
    });
};

function tvSearch() {
    $("#tvList").html("");
    var query = $("#tvSearchContent").val();

    $.ajax("/search/tv/" + query).success(function (results) {
        results.forEach(function (result) {
            var context = buildTvShowContext(result);
            var html = searchTemplate(context);
            $("#tvList").append(html);
        });
    });
};

function buildMovieContext(result) {
    var date = new Date(result.releaseDate);
    var year = date.getFullYear();
    var context = {
        posterPath: result.posterPath,
        id: result.id,
        title: result.title,
        overview: result.overview,
        voteCount: result.voteCount,
        voteAverage: result.voteAverage,
        year: year,
        type : "movie"
    };

    return context;
}

function buildTvShowContext(result) {
    var date = new Date(result.firstAirDate);
    var year = date.getFullYear();
    var context = {
        posterPath: result.posterPath,
        id: result.id,
        title: result.name,
        overview: result.overview,
        voteCount: result.voteCount,
        voteAverage: result.voteAverage,
        year: year,
        type: "tv"
    };
    return context;
}

function generateNotify(message, type) {
    // type = danger, warning, info, successs
    $.notify({
        // options
        message: message
    }, {
        // settings
        type: type
    });
}