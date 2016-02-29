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

$("#movieSearchContent").keypress(function (e) {
    if (movieTimer) {
        clearTimeout(movieTimer);
    }
    movieTimer = setTimeout(movieSearch, 400);
});

$("#tvSearchContent").keypress(function (e) {
    if (tvimer) {
        clearTimeout(tvimer);
    }
    tvimer = setTimeout(tvSearch(), 400);
});

$(document).on("click", ".dropdownTv", function (e) {
    var formData = [];
    e.preventDefault();
    console.log(e.target.id);
    var $form = $('#form'+e.target.id);
    var data = $form.serialize();
    var seasons = $(this).attr("season-select");
    console.log(data);
    formData.push(data);
    if (seasons === "1") {
        formData.push("latest=true");
    } else {
        data.latest = false;
    }

    $.ajax({
        type: $form.prop('method'),
        url: $form.prop('action'),
        data: data,
        dataType: "json",
        success: function (response) {
            console.log(response);
            if (response.result === true) {
                generateNotify("Success!", "success");
            } else {
                generateNotify(response.message, "warning");
            }
        },
        error: function (e) {
            console.log(e);
            generateNotify("Something went wrong!", "danger");
        }
    });

});

$(document).on("click", ".requesttv", function (e) {
    e.preventDefault();
    console.log(e.target.id);
    var $form = $('#form' + e.target.id);

    $.ajax({
        type: $form.prop('method'),
        url: $form.prop('action'),
        data: $form.serialize(),
        dataType: "json",
        success: function (response) {
            console.log(response);
            if (response.result === true) {
                generateNotify("Success!", "success");
            } else {
                generateNotify(response.message, "warning");
            }
        },
        error: function (e) {
            console.log(e);
            generateNotify("Something went wrong!", "danger");
        }
    });

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
