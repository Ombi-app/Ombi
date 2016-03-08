function generateNotify(message, type) {
    // type = danger, warning, info, successs
    $.notify({
        // options
        message: message
    }, {
        // settings
        type: type
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
