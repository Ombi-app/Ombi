


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