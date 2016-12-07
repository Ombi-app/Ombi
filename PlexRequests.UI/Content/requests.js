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
var base = $('#baseUrl').text();
var tvLoaded = false;
var albumLoaded = false;

var isAdmin = $('#isAdmin').val();
var defaultFiler = isAdmin == 'True' ? '.approved-fase' : 'all';

var mixItUpDefault = {
    animation: { enable: true },
    load: {
        filter: defaultFiler,
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

    $('.approve-category,.delete-category').hide();
    if (target === "#TvShowTab") {
        if (!tvLoaded) {
            tvLoaded = true;
            tvLoad();
        }

        $('#approveTVShows,#deleteTVShows').show();
        if ($ml.mixItUp('isLoaded')) {
            activeState = $ml.mixItUp('getState');
            $ml.mixItUp('destroy');
        }
        if ($musicL.mixItUp('isLoaded')) {
            activeState = $musicL.mixItUp('getState');
            $musicL.mixItUp('destroy');
        }
        //if ($tvl.mixItUp('isLoaded')) $tvl.mixItUp('destroy');
        //$tvl.mixItUp(mixItUpConfig(activeState)); // init or reinit
    }
    if (target === "#MoviesTab") {
        $('#approveMovies,#deleteMovies').show();
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
        if (!albumLoaded) {
            albumLoaded = true;
            albumLoad();
        }
        $('#approveMusic,#deleteMusic').show();
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

    var url = createBaseUrl(base, '/approval/approveallmovies');
    $.ajax({
        type: 'post',
        url: url,
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

    loadingButton(buttonId, "warning");
    var url = createBaseUrl(base, '/approval/approvealltvshows');
    $.ajax({
        type: 'post',
        url: url,
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
            finishLoading(buttonId, "warning", origHtml);
        }
    });
});

$('#deleteMovies').click(function (e) {
    e.preventDefault();
    if (!confirm("Are you sure you want to delete all Movie requests?")) return;

    var buttonId = e.target.id;
    var origHtml = $(this).html();

    if ($('#' + buttonId).text() === " Loading...") {
        return;
    }

    loadingButton(buttonId, "warning");

    var url = createBaseUrl(base, '/approval/deleteallmovies');
    $.ajax({
        type: 'post',
        url: url,
        dataType: "json",
        success: function (response) {
            if (checkJsonResponse(response)) {
                generateNotify("Success! All Movie requests deleted!", "success");
                movieLoad();
            }
        },
        error: function (e) {
            console.log(e);
            generateNotify("Something went wrong!", "danger");
        },
        complete: function (e) {
            finishLoading(buttonId, "warning", origHtml);
        }
    });
});

$('#deleteTVShows').click(function (e) {
    e.preventDefault();
    if (!confirm("Are you sure you want to delete all TV show requests?")) return;

    var buttonId = e.target.id;
    var origHtml = $(this).html();

    if ($('#' + buttonId).text() === " Loading...") {
        return;
    }

    loadingButton(buttonId, "warning");
    var url = createBaseUrl(base, '/approval/deletealltvshows');
    $.ajax({
        type: 'post',
        url: url,
        dataType: "json",
        success: function (response) {
            if (checkJsonResponse(response)) {
                generateNotify("Success! All TV Show requests deleted!", "success");
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

$('#deleteMusic').click(function (e) {
    e.preventDefault();
    if (!confirm("Are you sure you want to delete all album requests?")) return;

    var buttonId = e.target.id;
    var origHtml = $(this).html();

    if ($('#' + buttonId).text() === " Loading...") {
        return;
    }

    loadingButton(buttonId, "warning");
    var url = createBaseUrl(base, '/approval/deleteallalbums');
    $.ajax({
        type: 'post',
        url: url,
        dataType: "json",
        success: function (response) {
            if (checkJsonResponse(response)) {
                generateNotify("Success! All TV Show requests deleted!", "success");
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
$('.filter', '.dropdown-menu').click(function (e) {
    var $this = $(this);
    $('.fa-check-square', $this.parents('.dropdown-menu:first')).removeClass('fa-check-square').addClass('fa-square-o');
    $this.children('.fa').first().removeClass('fa-square-o').addClass('fa-check-square');
    $("#filterText").fadeOut(function () {
        $(this).text($this.text().trim());
    }).fadeIn();
});

$('.sort', '.dropdown-menu').click(function (e) {
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

// Update deny reason modal
$('#denyReasonModal').on('show.bs.modal', function (event) {
    var button = $(event.relatedTarget); // Button that triggered the modal
    var id = button.data('identifier'); // Extract info from data-* attributes

    var modal = $(this);
    modal.find('.denySaveReason').val(id); // Add ID to the button
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
    var $self = $(this);
    var $form = $self.parents('form').first();

    if ($self.text() === " Loading...") {
        return;
    }

    loadingButton($self.attr('id'), "success");

    approveRequest($form, null, function () {
        $("#" + $self.attr('id') + "notapproved").prop("class", "fa fa-check");

        
        var $group = $self.parent('.btn-split');
        if ($group.length > 0) {
            $group.remove();
        }
        else {
            $self.remove();
        }              
    });
});

// Deny single request
$(document).on("click", ".deny", function (e) {
    e.preventDefault();
    var $self = $(this);
    var $form = $self.parents('form').first();

    if ($self.text() === " Loading...") {
        return;
    }
    loadingButton($self.attr('id')+"deny", "success");

    denyRequest($form, function () {
        // Remove the form
        $("#" + "deny" + $self.attr('id')).remove();
        // remove the approve button
        var id = $self.attr("custom-button");
        $("#" + id).remove();

        var $group = $self.parent('.btn-split');
        if ($group.length > 0) {
            $group.remove();
        }
        else {
            $self.remove();
        }
    });
});

// Deny single request with reason (modal)
$(document).on("click", ".denySaveReason", function (e) {
    var comment = $("#denyReason").val();
    e.preventDefault();

    var $form = $("#denyReasonForm");
    var data = $form.serialize();
    data = data + "&reason=" + comment;

    $.ajax({
        type: $form.prop("method"),
        url: $form.prop("action"),
        data: data,
        dataType: "json",
        success: function (response) {
            if (checkJsonResponse(response)) {
                generateNotify(response.message, "success");
                $("#denyReasonModal").modal("hide");
            }
        },
        error: function (e) {
            console.log(e);
            generateNotify("Something went wrong!", "danger");
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
                var approvedIcon = $("#"+buttonId + "notapproved");

                if (response.available) {
                    button.text("Mark Unavailable");
                    button.val("false");
                    button.prop("class", "btn btn-sm btn-info-outline change");
                    icon.prop("class", "fa fa-check");
                    approvedIcon.prop("class", "fa fa-check");

                } else {
                    button.text("Mark Available");
                    button.prop("class", "btn btn-sm btn-success-outline change");
                    icon.prop("class", "fa fa-times");
                    approvedIcon.prop("class", "fa fa-times");
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

function denyRequest($form, successCallback) {

    var formData = $form.serialize();
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

var position = 0;
function initLoad() {
    movieLoad();

}

function movieLoad() {
    var $ml = $('#movieList');
    if ($ml.mixItUp('isLoaded')) {
        activeState = $ml.mixItUp('getState');
        $ml.mixItUp('destroy');
    }
    $ml.html("");

    var url = createBaseUrl(base, '/requests/movies');
    $.ajax(url).success(function (results) {
        if (results.length > 0) {
            results.forEach(function (result) {
                var context = buildRequestContext(result, "movie");
                var html = searchTemplate(context);
                $ml.append(html);
            });


            $('.customTooltip').tooltipster({
                contentCloning: true
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
    var url = createBaseUrl(base, '/requests/tvshows');
    $.ajax(url).success(function (results) {
        if (results.length > 0) {
            var tvObject = new Array();
            results.forEach(function (result) {
                var ep = result.episodes;
                ep.forEach(function (episode) {
                    var foundItem = tvObject.find(function(x) { return x.seasonNumber === episode.seasonNumber });
                    if (!foundItem) {
                        var obj = { seasonNumber: episode.seasonNumber, episodes: [] }
                        tvObject.push(obj);
                        tvObject[tvObject.length - 1].episodes.push(episode.episodeNumber);
                    } else {
                        foundItem.episodes.push(episode.episodeNumber);
                    }
                });

                var context = buildRequestContext(result, "tv");
                context.episodes = tvObject;
                var html = searchTemplate(context);
                $tvl.append(html);
                tvObject = new Array();

            });

            $('.customTooltip').tooltipster({
                contentCloning: true
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
    var url = createBaseUrl(base, '/requests/albums');
    $.ajax(url).success(function (results) {
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
        releaseDate: Humanize(result.releaseDate),
        releaseDateTicks: result.releaseDateTicks,
        approved: result.approved,
        requestedUsers: result.requestedUsers ? result.requestedUsers.join(', ') : '',
        requestedDate: Humanize(result.requestedDate),
        requestedDateTicks: result.requestedDateTicks,
        released: result.released,
        available: result.available,
        admin: result.admin,
        issueId: result.issueId,
        requestId: result.id,
        imdb: result.imdbId,
        seriesRequested: result.tvSeriesRequestType,
        coverArtUrl: result.coverArtUrl,
        qualities: result.qualities,
        hasQualities: result.qualities && result.qualities.length > 0,
        artist: result.artistName,
        musicBrainzId: result.musicBrainzId,
        denied: result.denied,
        deniedReason: result.deniedReason,
    };

    return context;
}

