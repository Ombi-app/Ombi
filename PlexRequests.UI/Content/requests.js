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

// Update the modal
$('#myModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget); // Button that triggered the modal
    var id = button.data('identifier'); // Extract info from data-* attributes

    var modal = $(this);
    modal.find('.theSaveButton').val(id); // Add ID to the button
    var requestField = modal.find('input');
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
                generateNotify("Success! Request Approved.", "success");

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
                $('#issueArea').html("<p>Issue: None</p>");
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
        requestId: result.id
    };

    return context;
}
