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