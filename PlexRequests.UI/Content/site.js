String.prototype.format = String.prototype.f = function () {
    var s = this,
        i = arguments.length;

    while (i--) {
        s = s.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
    }
    return s;
}

$(function() {
    $('[data-toggle="tooltip"]').tooltip();
});

var mainBaseUrl = $('#baseUrl').text();

function Humanize(date) {
    var mNow = moment();
    var mDate = moment(date).local();
    return moment.duration(mNow - mDate).humanize() + (mNow.isBefore(mDate) ? ' from now' : ' ago');
}

function utcToLocal(date) {
    return moment(date).local();
}

function generateNotify(message, type) {
    // type = danger, warning, info, successs
    if (!type) {
        type = "success";
    }
    $.notify({
        // options
        message: message
    }, {
        // settings
        type: type,
        animate: {
            enter: 'animated bounceInDown',
            exit: 'animated bounceOutUp'
        },
        newest_on_top: true

    });
}

function checkJsonResponse(response) {
    if (response.result === true) {
        return true;
    } else {
        generateNotify(response.message, "warning");
        return false;
    }
}

function disableElement(element) {
    element.attr("disabled", "disabled");
}
function enableElement(element) {
    element.removeAttr("disabled");
}

function loadingButton(elementId, originalCss) {
    var $element = $('#' + elementId);
    $element.removeClass("btn-" + originalCss + "-outline").addClass("btn-primary-outline").addClass('disabled').html("<i class='fa fa-spinner fa-spin'></i> Loading...");

    // handle split-buttons
    var $dropdown = $element.next('.dropdown-toggle');
    if ($dropdown.length > 0) {
        $dropdown.removeClass("btn-" + originalCss + "-outline").addClass("btn-primary-outline").addClass('disabled');
    }
}

function finishLoading(elementId, originalCss, html) {
    var $element = $('#' + elementId);
    $element.removeClass("btn-primary-outline").removeClass('disabled').addClass("btn-" + originalCss + "-outline").html(html);

    // handle split-buttons
    var $dropdown = $element.next('.dropdown-toggle');
    if ($dropdown.length > 0) {
        $dropdown.removeClass("btn-primary-outline").removeClass('disabled').addClass("btn-" + originalCss + "-outline");
    }
}

function createBaseUrl(base, url) {
    if (base) {
        if (url.charAt(0) === "/") {
            url = "/" + base + url;
        } else {
            url = "/" + base + "/" + url;
        }
    }
    return url;
}

var noResultsHtml = "<div class='no-search-results'>" +
    "<i class='fa fa-film no-search-results-icon'></i><div class='no-search-results-text'>Sorry, we didn't find any results!</div></div>";
var noResultsMusic = "<div class='no-search-results'>" +
    "<i class='fa fa-headphones no-search-results-icon'></i><div class='no-search-results-text'>Sorry, we didn't find any results!</div></div>";