String.prototype.format = String.prototype.f = function () {
    var s = this,
        i = arguments.length;

    while (i--) {
        s = s.replace(new RegExp('\\{' + i + '\\}', 'gm'), arguments[i]);
    }
    return s;
}

function generateNotify(message, type) {
    // type = danger, warning, info, successs
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

function loadingButton(elementId, originalCss) {
    $('#' + elementId).removeClass("btn-" + originalCss + "-outline");
    $('#' + elementId).addClass("btn-primary-outline");
    $('#' + elementId).html("<i class='fa fa-spinner fa-spin'></i> Loading...");
}

function finishLoading(elementId, originalCss, html) {
    $('#' + elementId).removeClass("btn-primary-outline");
    $('#' + elementId).addClass("btn-" + originalCss + "-outline");
    $('#' + elementId).html(html);
}

var noResultsHtml = "<div class='no-search-results'>" +
    "<i class='fa fa-film no-search-results-icon'></i><div class='no-search-results-text'>Sorry, we didn't find any results!</div></div>";
var noResultsMusic = "<div class='no-search-results'>" +
    "<i class='fa fa-headphones no-search-results-icon'></i><div class='no-search-results-text'>Sorry, we didn't find any results!</div></div>";