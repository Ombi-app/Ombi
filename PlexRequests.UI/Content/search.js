Handlebars.registerHelper('if_eq', function (a, b, opts) {
    if (a == b)
        return opts.fn(this);
    else
        return opts.inverse(this);
});

var searchSource = $("#search-template").html();
var searchTemplate = Handlebars.compile(searchSource);
var noResultsHtml = "<div class='no-search-results'>" +
    "<i class='fa fa-film no-search-results-icon'></i><div class='no-search-results-text'>Sorry, we didn't find any results!</div></div>";
var movieTimer = 0;
var tvimer = 0;

// Type in movie search
$("#movieSearchContent").on("input", function () {
    if (movieTimer) {
        clearTimeout(movieTimer);
    }
    $('#movieSearchButton').attr("class","fa fa-spinner fa-spin");
    movieTimer = setTimeout(movieSearch, 400);

});

// Type in TV search
$("#tvSearchContent").on("input", function () {
    if (tvimer) {
        clearTimeout(tvimer);
    }
    $('#tvSearchButton').attr("class", "fa fa-spinner fa-spin");
    tvimer = setTimeout(tvSearch, 400);
});

// Click TV dropdown option
$(document).on("click", ".dropdownTv", function (e) {
    e.preventDefault();
    var buttonId = e.target.id;
    if ($("#" + buttonId).attr('disabled')) {
        return;
    }

    $("#" + buttonId).prop("disabled", true);
    loadingButton(buttonId, "primary");


    var $form = $('#form' + buttonId);
    var data = $form.serialize();
    var seasons = $(this).attr("season-select");
    if (seasons === "2") {
        // Send over the latest
        data = data + "&seasons=latest";
    }
    if (seasons === "1") {
        // Send over the first season
        data = data + "&seasons=first";

    }

    var type = $form.prop('method');
    var url = $form.prop('action');

    sendRequestAjax(data, type, url, buttonId);
});

// Click Request for movie
$(document).on("click", ".requestMovie", function (e) {
    e.preventDefault();
    var buttonId = e.target.id;
    if ($("#" + buttonId).attr('disabled')) {
        return;
    }

    $("#" + buttonId).prop("disabled", true);
    loadingButton(buttonId, "primary");


    var $form = $('#form' + buttonId);

    var type = $form.prop('method');
    var url = $form.prop('action');
    var data = $form.serialize();

    sendRequestAjax(data, type, url, buttonId);
    
});

function sendRequestAjax(data, type, url, buttonId) {
    $.ajax({
        type: type,
        url: url,
        data: data,
        dataType: "json",
        success: function (response) {
            console.log(response);
            if (response.result === true) {
                generateNotify(response.message || "Success!", "success");

                $('#' + buttonId).html("<i class='fa fa-check'></i> Requested");
                $('#' + buttonId).removeClass("btn-primary-outline");
                $('#' + buttonId).removeAttr("data-toggle");
                $('#' + buttonId).addClass("btn-success-outline");
            } else {
                generateNotify(response.message, "warning");
                $('#' + buttonId).html("<i class='fa fa-plus'></i> Request");
                $('#' + buttonId).attr("data-toggle", "dropdown");
                $("#" + buttonId).removeAttr("disabled");
            }
        },
        error: function (e) {
            console.log(e);
            generateNotify("Something went wrong!", "danger");
        }
    });
}

function movieSearch() {
    $("#movieList").html("");
    var query = $("#movieSearchContent").val();

    $.ajax("/search/movie/" + query).success(function (results) {
        if (results.length > 0) {
            results.forEach(function(result) {
                var context = buildMovieContext(result);

                var html = searchTemplate(context);
                $("#movieList").append(html);
            });
        }
        else {
            $("#movieList").html(noResultsHtml);
        }
        $('#movieSearchButton').attr("class","fa fa-search");
    });
};

function tvSearch() {
    $("#tvList").html("");
    var query = $("#tvSearchContent").val();

    $.ajax("/search/tv/" + query).success(function (results) {
        if (results.length > 0) {
            results.forEach(function(result) {
                var context = buildTvShowContext(result);
                var html = searchTemplate(context);
                $("#tvList").append(html);
            });
        }
        else {
            $("#tvList").html(noResultsHtml);
        }
        $('#tvSearchButton').attr("class", "fa fa-search");
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
        type: "movie",
        imdb: result.imdbId
    };

    return context;
}

function buildTvShowContext(result) {
    var date = new Date(result.firstAired);
    var year = date.getFullYear();
    var context = {
        posterPath: result.banner,
        id: result.id,
        title: result.seriesName,
        overview: result.overview,
        year: year,
        type: "tv",
        imdb: result.imdbId
    };
    return context;
}
