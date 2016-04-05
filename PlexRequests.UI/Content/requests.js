Handlebars.registerHelper('if_eq', function (a, b, opts) {
    if (a == b)
        return !opts ? null : opts.fn(this);
    else
        return !opts ? null : opts.inverse(this);
});

var searchSource = $("#search-template").html();
var albumSource = $("#album-template").html();
var searchTemplate = Handlebars.compile(searchSource);
var albumTemplate = Handlebars.compile(albumSource);
var movieTimer = 0;
var tvimer = 0;

var mixItUpDefault = {
    animation: { enable: true },
    load: {
        filter: 'all',
        sort: 'requestorder:desc'
    },
    layout: {
        display: 'block'
    },
    callbacks: {
        onMixStart: function (state, futureState) {
            $('.mix', this).removeAttr('data-bound').removeData('bound'); // fix for animation issues in other tabs
        }
    }
};

initLoad();


$('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
    var target = $(e.target).attr('href');
    var activeState = "";

    var $ml = $('#movieList');
    var $tvl = $('#tvList');
    var $musicL = $('#musicList');

    $('.approve-category').hide();
    if (target === "#TvShowTab") {
        $('#approveTVShows').show();
        if ($ml.mixItUp('isLoaded')) {
            activeState = $ml.mixItUp('getState');
            $ml.mixItUp('destroy');
        }
        if ($musicL.mixItUp('isLoaded')) {
            activeState = $musicL.mixItUp('getState');
            $musicL.mixItUp('destroy');
        }
        if ($tvl.mixItUp('isLoaded')) $tvl.mixItUp('destroy');
        $tvl.mixItUp(mixItUpConfig(activeState)); // init or reinit
    }
    if (target === "#MoviesTab") {
        $('#approveMovies').show();
        if ($tvl.mixItUp('isLoaded')) {
            activeState = $tvl.mixItUp('getState');
            $tvl.mixItUp('destroy');
        }
        if ($musicL.mixItUp('isLoaded')) {
            activeState = $musicL.mixItUp('getState');
            $musicL.mixItUp('destroy');
        }
        if ($ml.mixItUp('isLoaded')) $ml.mixItUp('destroy');
        $ml.mixItUp(mixItUpConfig(activeState)); // init or reinit
    }

    if (target === "#MusicTab") {
        $('#approveMusic').show();
        if ($tvl.mixItUp('isLoaded')) {
            activeState = $tvl.mixItUp('getState');
            $tvl.mixItUp('destroy');
        }
        if ($ml.mixItUp('isLoaded')) {
            activeState = $ml.mixItUp('getState');
            $ml.mixItUp('destroy');
        }
        if ($musicL.mixItUp('isLoaded')) $musicL.mixItUp('destroy');
        $musicL.mixItUp(mixItUpConfig(activeState)); // init or reinit
    }
});

// Approve all
$('#approveMovies').click(function (e) {
    e.preventDefault();
    var buttonId = e.target.id;
    var origHtml = $(this).html();

    if ($('#' + buttonId).text() === " Loading...") {
        return;
    }

    loadingButton(buttonId, "success");

    $.ajax({
        type: 'post',
        url: '/approval/approveallmovies',
        dataType: "json",
        success: function (response) {
            if (checkJsonResponse(response)) {
                generateNotify("Success! All Movie requests approved!", "success");
                movieLoad();
            }
        },
        error: function (e) {
            console.log(e);
            generateNotify("Something went wrong!", "danger");
        },
        complete: function (e) {
            finishLoading(buttonId, "success", origHtml);
        }
    });
});
$('#approveTVShows').click(function (e) {
    e.preventDefault();
    var buttonId = e.target.id;
    var origHtml = $(this).html();

    if ($('#' + buttonId).text() === " Loading...") {
        return;
    }

    loadingButton(buttonId, "success");

    $.ajax({
        type: 'post',
        url: '/approval/approvealltvshows',
        dataType: "json",
        success: function (response) {
            if (checkJsonResponse(response)) {
                generateNotify("Success! All TV Show requests approved!", "success");
                tvLoad();
            }
        },
        error: function (e) {
            console.log(e);
            generateNotify("Something went wrong!", "danger");
        },
        complete: function (e) {
            finishLoading(buttonId, "success", origHtml);
        }
    });
});

// filtering/sorting
$('.filter,.sort', '.dropdown-menu').click(function (e) {
    var $this = $(this);
    $('.fa-check-square', $this.parents('.dropdown-menu:first')).removeClass('fa-check-square').addClass('fa-square-o');
    $this.children('.fa').first().removeClass('fa-square-o').addClass('fa-check-square');
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
    var $this = $(this);
    var $form = $this.parents('form').first();

    if ($this.text() === " Loading...") {
        return;
    }

    loadingButton($this.attr('id'), "success");

    approveRequest($form, null, function () {
        $("#" + $this.attr('id') + "notapproved").prop("class", "fa fa-check");
        
        var $group = $this.parent('.btn-split');
        if ($group.length > 0) {
            $group.remove();
        }
        else {
            $this.remove();
        }              
    });
});

$(document).on("click", ".approve-with-quality", function (e) {
    e.preventDefault();
    var $this = $(this);
    var $button = $this.parents('.btn-split').children('.approve').first();
    var qualityId = e.target.id
    var $form = $this.parents('form').first();
    
    if ($button.text() === " Loading...") {
        return;
    }

    loadingButton($button.attr('id'), "success");

    approveRequest($form, qualityId, function () {
        $("#" + $button.attr('id') + "notapproved").prop("class", "fa fa-check");
        $this.parents('.btn-split').remove();
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

function approveRequest($form, qualityId, successCallback) {

    var formData = $form.serialize();
    if (qualityId) formData += ("&qualityId=" + qualityId);

    $.ajax({
        type: $form.prop('method'),
        url: $form.prop('action'),
        data: formData,
        dataType: "json",
        success: function (response) {

            if (checkJsonResponse(response)) {
                if (response.message) {
                    generateNotify(response.message, "success");
                } else {
                    generateNotify("Success! Request Approved.", "success");
                }

                if (successCallback) {
                    successCallback();
                }
            }
        },
        error: function (e) {
            console.log(e);
            generateNotify("Something went wrong!", "danger");
        }
    });
}

function mixItUpConfig(activeState) {
    var conf = mixItUpDefault;

    if (activeState) {
        if (activeState.activeFilter) conf['load']['filter'] = activeState.activeFilter;
        if (activeState.activeSort) conf['load']['sort'] = activeState.activeSort;
    }
    return conf;
};

function initLoad() {
    movieLoad();
    tvLoad();
    albumLoad();
}

function movieLoad() {
    var $ml = $('#movieList');
    if ($ml.mixItUp('isLoaded')) {
        activeState = $ml.mixItUp('getState');
        $ml.mixItUp('destroy');
    }
    $ml.html("");

    $.ajax("/requests/movies/").success(function (results) {
        if (results.length > 0) {
            results.forEach(function (result) {
                var context = buildRequestContext(result, "movie");
                var html = searchTemplate(context);
                $ml.append(html);
            });
        }
        else {
            $ml.html(noResultsHtml.format("movie"));
        }
        $ml.mixItUp(mixItUpConfig());
    });
};

function tvLoad() {
    var $tvl = $('#tvList');
    if ($tvl.mixItUp('isLoaded')) {
        activeState = $tvl.mixItUp('getState');
        $tvl.mixItUp('destroy');
    }
    $tvl.html("");

    $.ajax("/requests/tvshows/").success(function (results) {
        if (results.length > 0) {
            results.forEach(function (result) {
                var context = buildRequestContext(result, "tv");
                var html = searchTemplate(context);
                $tvl.append(html);
            });
        }
        else {
            $tvl.html(noResultsHtml.format("tv show"));
        }
        $tvl.mixItUp(mixItUpConfig());
    });
};

function albumLoad() {
    var $albumL = $('#musicList');
    if ($albumL.mixItUp('isLoaded')) {
        activeState = $albumL.mixItUp('getState');
        $albumL.mixItUp('destroy');
    }
    $albumL.html("");

    $.ajax("/requests/albums/").success(function (results) {
        if (results.length > 0) {
            results.forEach(function (result) {
                var context = buildRequestContext(result, "album");
                var html = albumTemplate(context);
                $albumL.append(html);
            });
        }
        else {
            $albumL.html(noResultsMusic.format("albums"));
        }
        $albumL.mixItUp(mixItUpConfig());
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
        releaseDate: moment.duration(moment() - moment(result.releaseDate).local()).humanize() + ' ago',
        releaseDateTicks: result.releaseDateTicks,
        approved: result.approved,
        requestedUsers: result.requestedUsers ? result.requestedUsers.join(', ') : '',
        requestedDate: moment.duration(moment() - moment(result.requestedDate).local()).humanize() + ' ago',
        requestedDateTicks: result.requestedDateTicks,
        available: result.available,
        admin: result.admin,
        issues: result.issues,
        otherMessage: result.otherMessage,
        requestId: result.id,
        adminNote: result.adminNotes,
        imdb: result.imdbId,
        seriesRequested: result.tvSeriesRequestType,
        coverArtUrl: result.coverArtUrl,
        qualities: result.qualities,
        hasQualities: result.qualities && result.qualities.length > 0,
        artist: result.artistName
    };

    return context;
}

