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
    tvimer = setTimeout(tvSearch, 400);
});

// Click TV dropdown option
$(document).on("click", ".dropdownTv", function (e) {
    e.preventDefault();
    var buttonId = e.target.id;
    var $form = $('#form' + buttonId);
    var data = $form.serialize();
    var seasons = $(this).attr("season-select");
    if (seasons === "1") {
        data = data + "&latest=true";
    }

    var type = $form.prop('method');
    var url = $form.prop('action');

    sendRequestAjax(data, type, url, buttonId);
});

// Click Request for movie
$(document).on("click", ".requestMovie", function (e) {
    $(".requestMovie").prop("disabled", true);
    e.preventDefault();

    var buttonId = e.target.id;
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
                generateNotify("Success!", "success");

                $('#' + buttonId).html("<i class='fa fa-check'></i> Requested");
                $('#' + buttonId).removeClass("btn-primary");
                $('#' + buttonId).removeAttr("data-toggle");
                $('#' + buttonId).addClass("btn-success");
            } else {
                generateNotify(response.message, "warning");
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
    });
};

