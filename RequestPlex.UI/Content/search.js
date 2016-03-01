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

    e.preventDefault();
    console.log(e.target.id);
    var $form = $('#form'+e.target.id);
    var data = $form.serialize();
    var seasons = $(this).attr("season-select");
    console.log(data);
    if (seasons === "1") {
        data = data + "&latest=true";
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

$(document).on("click", ".requestMovie", function (e) {
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

