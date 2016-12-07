$(function () {

    // Step 1
    $('#firstNext')
        .click(function () {
            loadArea("plexAuthArea");
        });

    // Step 2 - Get the auth token
    $('#contentBody').on('click', '#requestToken', function (e) {
        e.preventDefault();

        var $form = $("#plexAuthForm");
        $.post($form.prop("action"), $form.serialize(), function (response) {
            if (response.result === true) {
                loadArea("plexArea");

                if (response.port) {
                    $('#portNumber').val(response.port);
                }
                if (response.ip) {
                    $('#Ip').val(response.ip);
                }
                if (response.scheme) {
                    response.scheme === "http" ? $('#Ssl').prop('checked', false) : $('#Ssl').prop('checked', true);
                }
            } else {
                generateNotify(response.message, "warning");
            }
        });
    });

    // Step 3 - Submit the Plex Details
    $('#contentBody').on('click', '#submitPlex', function (e) {
        e.preventDefault();
        var $form = $("#plexForm");
        $.ajax({
            type: $form.prop("method"),
            url: $form.prop("action"),
            data: $form.serialize(),
            dataType: "json",
            success: function (response) {
                if (response.result === true) {
                    //Next
                    loadArea("plexRequestArea");
                } else {
                    generateNotify(response.message, "warning");
                }
            },
            error: function (e) {
                console.log(e);
            }
        });
    });

    // Step 4 - Submit the Plex Request Settings
    $('#contentBody').on('click', '#submitPlexRequest', function (e) {
        e.preventDefault();
        var $form = $("#plexRequestForm");
        $.ajax({
            type: $form.prop("method"),
            url: $form.prop("action"),
            data: $form.serialize(),
            dataType: "json",
            success: function (response) {
                if (response.result === true) {
                    //Next
                    loadArea("authArea");
                    $('.userAuthTooltip').tooltipster({
                        theme: 'borderless'
                    });
                    $('.passwordAuthTooltip').tooltipster({
                        theme: 'borderless'
                    });
                } else {
                    generateNotify(response.message, "warning");
                }
            },
            error: function (e) {
                console.log(e);
            }
        });
    });

    // Step 5 - Plex Requests Authentication Settings
    $('#contentBody').on('click', '#submitAuth', function (e) {
        e.preventDefault();
        var $form = $("#authForm");
        $.ajax({
            type: $form.prop("method"),
            url: $form.prop("action"),
            data: $form.serialize(),
            dataType: "json",
            success: function (response) {
                if (response.result === true) {
                    //Next
                    loadArea("adminArea");
                } else {
                    generateNotify(response.message, "warning");
                }
            },
            error: function (e) {
                console.log(e);
            }
        });
    });

    
    function loadArea(templateId) {
        var $body = $('#contentBody');

        var templateSource = $("#" + templateId).html();
        // Do some sliding?
        $body.html(templateSource);
    }
});