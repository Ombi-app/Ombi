﻿$(function () {

    // Step 1
    $('#firstNext')
        .click(function () {
            loadArea("mediaApplicationChoice");
        });


    // Plex click
    $('#contentBody')
        .on("click", "#plexImg", function(e) {
                e.preventDefault();
                return loadArea("plexAuthArea");
        });


    $('#contentBody')
        .on("click", "#embyImg", function (e) {
            e.preventDefault();
            return loadArea("embyApiKey");
        });

    

    $('#contentBody').on('click', '#embyApiKeySave', function (e) {
        e.preventDefault();

        var port = $('#portNumber').val();
        if (!port) {
            generateNotify("Please provide a port number", "warning");
        }

        $('#spinner').attr("class", "fa fa-spinner fa-spin");

        var $form = $("#embyAuthForm");
        $.post($form.prop("action"), $form.serialize(), function (response) {
            if (response.result === true) {
                loadArea("authArea");
            } else {

                $('#spinner').attr("class", "fa fa-times");
                generateNotify(response.message, "warning");
            }
        });
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