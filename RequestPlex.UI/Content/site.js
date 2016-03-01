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
        type: "movie"
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
