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


$('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
    var target = $(e.target).attr('href');
    var activeState = "";
    if (target === "#TvShowTab") {
        if ($('#movieList').mixItUp('isLoaded')) {
            activeState = $('#movieList').mixItUp('getState');
            $('#movieList').mixItUp('destroy');
        }
        if (!$('#tvList').mixItUp('isLoaded')) {
            $('#tvList').mixItUp({
                load: {
                    filter: activeState.activeFilter || 'all',
                    sort: activeState.activeSort || 'default:asc'
                },
                layout: {
                display: 'block'
            }

            });
        }
    }
    if (target === "#MoviesTab") {
        if ($('#tvList').mixItUp('isLoaded')) {
            activeState = $('#tvList').mixItUp('getState');
            $('#tvList').mixItUp('destroy');
        }
        if (!$('#movieList').mixItUp('isLoaded')) {
            $('#movieList').mixItUp({
                load: {
                    filter: activeState.activeFilter || 'all',
                    sort: activeState.activeSort || 'default:asc'
                },
                layout: {
                    display: 'block'
                }
            });
        }
    }
});

// Approve all
$('#approveAll').click(function () {
    $.ajax({
        type: 'post',
        url: '/approval/approveall',
        dataType: "json",
        success: function (response) {
            if (checkJsonResponse(response)) {
                generateNotify("Success! All requests approved!", "success");
            }
        },
        error: function (e) {
            console.log(e);
            generateNotify("Something went wrong!", "danger");
        }
    });
});


// Report Issue
$(document).on("click", ".dropdownIssue", function (e) {
    var issue = $(this).attr("issue-select");
    var id = e.target.id;
    // Other issue so the modal is opening
    if (issue == 4) {
        return;
    }
    e.preventDefault();

    var $form = $('#report' + id);
    var data = $form.serialize();
    data = data + "&issue=" + issue;

    $.ajax({
        type: $form.prop('method'),
        url: $form.prop('action'),
        data: data,
        dataType: "json",
        success: function (response) {
            if (checkJsonResponse(response)) {
                generateNotify("Success! Added Issue.", "success");
            }
        },
        error: function (e) {
            console.log(e);
            generateNotify("Something went wrong!", "danger");
        }
    });
});

// Modal click
$(".theSaveButton").click(function (e) {
    var comment = $("#commentArea").val();
    e.preventDefault();

    var $form = $("#commentForm");
    var data = $form.serialize();
    data = data + "&issue=" + 4 + "&comment=" + comment;

    $.ajax({
        type: $form.prop("method"),
        url: $form.prop("action"),
        data: data,
        dataType: "json",
        success: function (response) {
            if (checkJsonResponse(response)) {
                generateNotify("Success! Added Issue.", "success");
                $("#myModal").modal("hide");
            }
        },
        error: function (e) {
            console.log(e);
            generateNotify("Something went wrong!", "danger");
        }
    });
});

// Note Modal click
$(".theNoteSaveButton").click(function (e) {
    var comment = $("#noteArea").val();
    e.preventDefault();

    var $form = $("#noteForm");
    var data = $form.serialize();


    $.ajax({
        type: $form.prop("method"),
        url: $form.prop("action"),
        data: data,
        dataType: "json",
        success: function (response) {
            if (checkJsonResponse(response)) {
                generateNotify("Success! Added Note.", "success");
                $("#myModal").modal("hide");
                $('#adminNotesArea' + e.target.value).html("<div>Note from Admin: " + comment + "</div>");
            }
        },
        error: function (e) {
            console.log(e);
            generateNotify("Something went wrong!", "danger");
        }
    });
});

// Update the modal
$('#myModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget); // Button that triggered the modal
    var id = button.data('identifier'); // Extract info from data-* attributes

    var modal = $(this);
    modal.find('.theSaveButton').val(id); // Add ID to the button
    var requestField = modal.find('input');
    requestField.val(id);  // Add ID to the hidden field
});

// Update the note modal
$('#noteModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget); // Button that triggered the modal
    var id = button.data('identifier'); // Extract info from data-* attributes

    var modal = $(this);
    modal.find('.theNoteSaveButton').val(id); // Add ID to the button
    var requestField = modal.find('.noteId');
    requestField.val(id);  // Add ID to the hidden field
});

// Delete
$(document).on("click", ".delete", function (e) {
    e.preventDefault();
    var buttonId = e.target.id;
    var $form = $('#delete' + buttonId);

    $.ajax({
        type: $form.prop('method'),
        url: $form.prop('action'),
        data: $form.serialize(),
        dataType: "json",
        success: function (response) {

            if (checkJsonResponse(response)) {
                generateNotify("Success! Request Deleted.", "success");

                $("#" + buttonId + "Template").slideUp();
            }
        },
        error: function (e) {
            console.log(e);
            generateNotify("Something went wrong!", "danger");
        }
    });

});

// Approve single request
$(document).on("click", ".approve", function (e) {
    e.preventDefault();
    var buttonId = e.target.id;
    var $form = $('#approve' + buttonId);

    $.ajax({
        type: $form.prop('method'),
        url: $form.prop('action'),
        data: $form.serialize(),
        dataType: "json",
        success: function (response) {

            if (checkJsonResponse(response)) {
                if (response.message) {
                    generateNotify(response.message, "success");
                } else {
                    generateNotify("Success! Request Approved.", "success");
                }

                $("button[custom-button='" + buttonId + "']").remove();
                $("#" + buttonId + "notapproved").prop("class", "fa fa-check");
            }
        },
        error: function (e) {
            console.log(e);
            generateNotify("Something went wrong!", "danger");
        }
    });

});

// Clear issues
$(document).on("click", ".clear", function (e) {
    e.preventDefault();
    var buttonId = e.target.id;
    var $form = $('#clear' + buttonId);

    $.ajax({
        type: $form.prop('method'),
        url: $form.prop('action'),
        data: $form.serialize(),
        dataType: "json",
        success: function (response) {

            if (checkJsonResponse(response)) {
                generateNotify("Success! Issues Cleared.", "info");
                $('#issueArea' + buttonId).html("<div>Issue: None</div>");
            }
        },
        error: function (e) {
            console.log(e);
            generateNotify("Something went wrong!", "danger");
        }
    });

});

// Change Availability
$(document).on("click", ".change", function (e) {
    e.preventDefault();
    var buttonId = e.target.id;

    var availablity = $("button[custom-availibility='" + buttonId + "']").val();
    var $form = $('#change' + buttonId);
    var data = $form.serialize();
    data = data + "&Available=" + availablity;

    $.ajax({
        type: $form.prop('method'),
        url: $form.prop('action'),
        data: data,
        dataType: "json",
        success: function (response) {

            if (checkJsonResponse(response)) {
                generateNotify("Success! Availibility changed.", "info");
                var button = $("button[custom-availibility='" + buttonId + "']");
                var icon = $('#availableIcon' + buttonId);

                if (response.available) {
                    button.text("Mark Unavailable");
                    button.val("false");
                    button.prop("class", "btn btn-sm btn-info-outline change");
                    icon.prop("class", "fa fa-check");
                } else {
                    button.text("Mark Available");
                    button.prop("class", "btn btn-sm btn-success-outline change");
                    icon.prop("class", "fa fa-times");
                    button.val("true");
                }
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
        $('#movieList').mixItUp({
            layout: {
                display: 'block'
            },
            load: {
                filter: 'all'
            }
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

// Builds the request context.
function buildRequestContext(result, type) {
    var context = {
        posterPath: result.posterPath,
        id: result.providerId,
        title: result.title,
        overview: result.overview,
        year: result.releaseYear,
        type: type,
        status: result.status,
        releaseDate: result.releaseDate,
        approved: result.approved,
        requestedBy: result.requestedBy,
        requestedDate: result.requestedDate,
        available: result.available,
        admin: result.admin,
        issues: result.issues,
        otherMessage: result.otherMessage,
        requestId: result.id,
        adminNote: result.adminNotes,
        imdb: result.imdbId
    };

    return context;
}

function startFilter(elementId) {
    $('#'+element).mixItUp({
        load: {
            filter: activeState.activeFilter || 'all',
            sort: activeState.activeSort || 'default:asc'
        },
        layout: {
            display: 'block'
        }
    });
}