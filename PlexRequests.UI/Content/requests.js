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

$(document).on("click", ".delete", function (e) {
    e.preventDefault();
    var buttonId = e.target.id;
    var $form = $('#form' + buttonId);

    $.ajax({
        type: $form.prop('method'),
        url: $form.prop('action'),
        data: $form.serialize(),
        dataType: "json",
        success: function (response) {
            console.log(response);
            if (response.result === true) {
                generateNotify("Success!", "success");

                $("#" + buttonId + "Template").slideUp();
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


function movieLoad() {
    $("#movieList").html("");

    $.ajax("/requests/movies/").success(function (results) {
        results.forEach(function (result) {
            var context = buildRequestContext(result, "movie");

            var html = searchTemplate(context);
            $("#movieList").append(html);
        });
    });
};

function tvLoad() {
    $("#tvList").html("");

    $.ajax("/requests/tvshows/").success(function (results) {
        results.forEach(function (result) {
            var context = buildRequestContext(result, "tv");
            var html = searchTemplate(context);
            $("#tvList").append(html);
        });
    });
};

function buildRequestContext(result, type) {

    var context = {
        posterPath: result.posterPath,
        id: result.tmdbid,
        title: result.title,
        overview: result.overview,
        year: result.releaseYear,
        type: type,
        status: result.status,
        releaseDate: result.releaseDate,
        approved: result.approved,
        requestedBy: result.requestedBy,
        requestedDate: result.requestedDate,
        available: result.available
    };

    return context;
}
