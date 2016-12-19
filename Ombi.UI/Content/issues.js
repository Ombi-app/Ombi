Handlebars.registerHelper('if_eq', function (a, b, opts) {
    if (a == b)
        return !opts ? null : opts.fn(this);
    else
        return !opts ? null : opts.inverse(this);
});

var issueSource = $("#issue-template").html();
var issueTemplate = Handlebars.compile(issueSource);

var base = $('#baseUrl').text();



initLoad();

$('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
    var target = $(e.target).attr('href');

    if (target === "#resolvedTab") {
        loadResolvedIssues();
    }
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


function initLoad() {
    loadCounts();
    loadPendingIssues();
}

function loadCounts() {
    var url = createBaseUrl(base, "/issues/tabCount");
    $.ajax({
        type: "get",
        url: url,
        dataType: "json",
        success: function (response) {
            if (response.length > 0) {
                response.forEach(function (result) {
                    if (result.count > 0) {

                        if (result.name == 0) {
                            $('#pendingCount').addClass("badge");
                            $('#pendingCount').html(result.count);
                        } else if (result.name == 1) {
                            $('#inProgressCount').addClass("badge");
                            $('#inProgressCount').html(result.count);
                        } else if (result.name == 2) {
                            $('#resolvedCount').addClass("badge");
                            $('#resolvedCount').html(result.count);
                        }

                    }
                });
            };
        }
    });
}

function loadPendingIssues() {
    loadIssues("pending", $('#pendingIssues'));
}

function loadResolvedIssues() {
    var $element = $('#resolvedIssues');
    $element.html("");
    loadIssues("resolved", $element);
}

function loadIssues(type, element) {
    var url = createBaseUrl(base, "/issues/" + type);
    var linkUrl = createBaseUrl(base, "/issues/");
    $.ajax({
        type: "get",
        url: url,
        dataType: "json",
        success: function (response) {
            if (response.length > 0) {
                response.forEach(function (result) {
                    var context = buildIssueContext(result);
                    var html = issueTemplate(context);
                    element.append(html);

                    $("#" + result.id + "link").attr("href", linkUrl + result.id);
                });
            };
        },
        error: function (e) {
            console.log(e);
            generateNotify("Could not load Pending issues", "danger");
        }
    });
}


// Builds the issue context.
function buildIssueContext(result) {
    var context = {
        id: result.id,
        requestId: result.requestId,
        type: result.type,
        title: result.title,
        issues: result.issues,
        admin: result.admin
};

    return context;
}

