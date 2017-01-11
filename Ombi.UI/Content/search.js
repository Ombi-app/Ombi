Handlebars.registerHelper('if_eq', function (a, b, opts) {
    if (a == b)
        return opts.fn(this);
    else
        return opts.inverse(this);
});

Function.prototype.bind = function (parent) {
    var f = this;
    var args = [];

    for (var a = 1; a < arguments.length; a++) {
        args[args.length] = arguments[a];
    }

    var temp = function () {
        return f.apply(parent, args);
    }

    return (temp);
}



$(function () {

    var searchSource = $("#search-template").html();
    var seasonsSource = $("#seasons-template").html();
    var musicSource = $("#music-template").html();
    var seasonsNumberSource = $("#seasonNumber-template").html();
    var episodeSource = $("#episode-template").html();

    var searchTemplate = Handlebars.compile(searchSource);
    var musicTemplate = Handlebars.compile(musicSource);
    var seasonsTemplate = Handlebars.compile(seasonsSource);
    var seasonsNumberTemplate = Handlebars.compile(seasonsNumberSource);
    var episodesTemplate = Handlebars.compile(episodeSource);

    var base = $('#baseUrl').text();

    var searchTimer = 0;

    // fix for selecting a default tab
    var $tabs = $('#nav-tabs').children('li');
    if ($tabs.filter(function (li) { return $(li).hasClass('active') }).length <= 0) {
        $tabs.first().children('a:first-child').tab('show');
    }

    $('a[data-toggle="tab"]').on('shown.bs.tab', function (e) {
        focusSearch($($(e.target).attr('href')));
    });
    focusSearch($('li.active a', '#nav-tabs').first().attr('href'));

    // Type in movie search
    $("#movieSearchContent").on("input", function () {
        if (searchTimer) {
            clearTimeout(searchTimer);
        }
        searchTimer = setTimeout(function () {
            movieSearch();
        }.bind(this), 800);

    });

    $('#moviesComingSoon').on('click', function (e) {
        e.preventDefault();
        moviesComingSoon();
    });

    $('#moviesInTheaters').on('click', function (e) {
        e.preventDefault();
        moviesInTheaters();
    });

    // Type in TV search
    $("#tvSearchContent").on("input", function () {
        if (searchTimer) {
            clearTimeout(searchTimer);
        }
        searchTimer = setTimeout(function () {
            tvSearch();
        }.bind(this), 800);
    });

    // Click TV dropdown option
    $(document).on("click", ".requestTv", function (e) {
        e.preventDefault();
        var buttonId = e.target.id;
        if ($("#" + buttonId).attr('disabled')) {
            return;
        }

        $("#" + buttonId).prop("disabled", true);
        loadingButton(buttonId, "primary");


        var $form = $('#form' + buttonId);
        var data = $form.serialize();
        var seasons = $(this).attr("season-select");
        if (seasons === "2") {
            // Send over the latest
            data = data + "&seasons=latest";
        }
        if (seasons === "1") {
            // Send over the first season
            data = data + "&seasons=first";
        } if (seasons === "0") {
            // Send over the first season
            data = data + "&seasons=all";
        }

        var type = $form.prop('method');
        var url = $form.prop('action');

        sendRequestAjax(data, type, url, buttonId);
    });

    // Search Music
    $("#musicSearchContent").on("input", function () {
        if (searchTimer) {
            clearTimeout(searchTimer);
        }
        searchTimer = setTimeout(function () {
            musicSearch();
        }.bind(this), 800);

    });

    // Click Request for movie
    $(document).on("click", ".requestMovie", function (e) {
        e.preventDefault();
        var buttonId = e.target.id;
        if ($("#" + buttonId).attr('disabled')) {
            return;
        }

        $("#" + buttonId).prop("disabled", true);
        loadingButton(buttonId, "primary");


        var $form = $('#form' + buttonId);

        var type = $form.prop('method');
        var url = $form.prop('action');
        var data = $form.serialize();

        sendRequestAjax(data, type, url, buttonId);

    });

    // Click Request for album
    $(document).on("click", ".requestAlbum", function (e) {
        e.preventDefault();
        var buttonId = e.target.id;
        if ($("#" + buttonId).attr('disabled')) {
            return;
        }

        $("#" + buttonId).prop("disabled", true);
        loadingButton(buttonId, "primary");


        var $form = $('#form' + buttonId);

        var type = $form.prop('method');
        var url = $form.prop('action');
        var data = $form.serialize();

        sendRequestAjax(data, type, url, buttonId);
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
                    generateNotify("Successfully Reported Issue.", "success");
                }
            },
            error: function (e) {
                console.log(e);
                generateNotify("Something went wrong!", "danger");
            }
        });
    });

    // Save Modal click
    $(".theSaveButton").click(function (e) {
        var comment = $("#commentArea").val();
        e.preventDefault();

        var $form = $("#commentForm");
        var data = $form.serialize();
        data = data + "&comment=" + comment;

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

    // Update the modal
    $('#issuesModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget); // Button that triggered the modal
        var id = button.data('identifier'); // Extract info from data-* attributes
        var type = button.data('type'); // Extract info from data-* attributes

        var modal = $(this);
        modal.find('.theSaveButton').val(id); // Add ID to the button


        $('#providerIdModal').val(id);
        $('#typeModal').val(type);
    });


    function focusSearch($content) {
        if ($content.length > 0) {
            $('input[type=text].form-control', $content).first().focus();
        }
    }

    function sendRequestAjax(data, type, url, buttonId) {
        $.ajax({
            type: type,
            url: url,
            data: data,
            dataType: "json",
            success: function (response) {
                console.log(response);
                if (response.result === true) {
                    generateNotify(response.message || "Success!", "success");

                    $('#' + buttonId).html("<i class='fa fa-check'></i> Requested");
                    $('#' + buttonId).removeClass("btn-primary-outline");
                    $('#' + buttonId).removeAttr("data-toggle");
                    $('#' + buttonId).addClass("btn-success-outline");
                } else {
                    generateNotify(response.message, "warning");
                    $('#' + buttonId).html("<i class='fa fa-plus'></i> Request");
                    $('#' + buttonId).attr("data-toggle", "dropdown");
                    $("#" + buttonId).removeAttr("disabled");
                }
            },
            error: function (e) {
                console.log(e);
                generateNotify("Something went wrong!", "danger");
            }
        });
    }

    function movieSearch() {
        var query = $("#movieSearchContent").val();
        var url = createBaseUrl(base, '/search/movie/');
        query ? getMovies(url + query) : resetMovies();
    }

    function moviesComingSoon() {
        var url = createBaseUrl(base, '/search/movie/upcoming');
        getMovies(url);
    }

    function moviesInTheaters() {
        var url = createBaseUrl(base, '/search/movie/playing');
        getMovies(url);
    }

    function getMovies(url) {
        resetMovies();

        $('#movieSearchButton').attr("class", "fa fa-spinner fa-spin");
        $.ajax(url).success(function (results) {
            if (results.length > 0) {
                results.forEach(function (result) {
                    var context = buildMovieContext(result);

                    var html = searchTemplate(context);
                    $("#movieList").append(html);
                });
            }
            else {
                $("#movieList").html(noResultsHtml);
            }
            $('#movieSearchButton').attr("class", "fa fa-search");
        });
    };

    function resetMovies() {
        $("#movieList").html("");
    }

    function tvSearch() {
        var query = $("#tvSearchContent").val();

        var url = createBaseUrl(base, '/search/tv/');
        query ? getTvShows(url + query) : resetTvShows();
    }

    function getTvShows(url) {
        resetTvShows();

        $('#tvSearchButton').attr("class", "fa fa-spinner fa-spin");
        $.ajax(url).success(function (results) {
            if (results.length > 0) {
                results.forEach(function (result) {
                    var context = buildTvShowContext(result);
                    var html = searchTemplate(context);
                    $("#tvList").append(html);
                });
            }
            else {
                $("#tvList").html(noResultsHtml);
            }
            $('#tvSearchButton').attr("class", "fa fa-search");
        });
    };

    function resetTvShows() {
        $("#tvList").html("");
    }

    function musicSearch() {
        var url = createBaseUrl(base, '/search/music/');
        var query = $("#musicSearchContent").val();
        query ? getMusic(url + query) : resetMusic();
    }

    function getMusic(url) {
        resetMusic();

        $('#musicSearchButton').attr("class", "fa fa-spinner fa-spin");
        $.ajax(url).success(function (results) {
            if (results.length > 0) {
                results.forEach(function (result) {
                    var context = buildMusicContext(result);

                    var html = musicTemplate(context);
                    $("#musicList").append(html);
                    getCoverArt(context.id);
                });
            }
            else {
                $("#musicList").html(noResultsMusic);
            }
            $('#musicSearchButton').attr("class", "fa fa-search");
        });
    };

    function resetMusic() {
        $("#musicList").html("");
    }

    function getCoverArt(artistId) {

        var url = createBaseUrl(base, '/search/music/coverart/');
        $.ajax(url + artistId).success(function (result) {
            if (result) {
                $('#' + artistId + "imageDiv").html(" <img class='img-responsive' src='" + result + "' width='150' alt='poster'>");
            }
        });
    };

    function buildMovieContext(result) {
        var date = new Date(result.releaseDate);
        var year = date.getFullYear();
        var context = {
            posterPath: result.posterPath,
            id: result.id,
            title: result.title,
            overview: result.overview,
            voteCount: result.voteCount,
            voteAverage: result.voteAverage,
            year: year,
            type: "movie",
            imdb: result.imdbId,
            requested: result.requested,
            approved: result.approved,
            available: result.available,
            url: result.plexUrl
        };

        return context;
    }

    function buildTvShowContext(result) {
        var date = new Date(result.firstAired);
        var year = date.getFullYear();
        var context = {
            posterPath: result.banner,
            id: result.id,
            title: result.seriesName,
            overview: result.overview,
            year: year,
            type: "tv",
            imdb: result.imdbId,
            requested: result.requested,
            approved: result.approved,
            available: result.available,
            episodes: result.episodes,
            tvFullyAvailable: result.tvFullyAvailable,
            url: result.plexUrl,
            tvPartialAvailable: result.tvPartialAvailable,
            disableTvRequestsByEpisode: result.disableTvRequestsByEpisode,
            disableTvRequestsBySeason: result.disableTvRequestsBySeason,
            enableTvRequestsForOnlySeries: result.enableTvRequestsForOnlySeries
        };

        return context;
    }

    function buildMusicContext(result) {

        var context = {
            id: result.id,
            title: result.title,
            overview: result.overview,
            year: result.releaseDate,
            type: "album",
            trackCount: result.trackCount,
            coverArtUrl: result.coverArtUrl,
            artist: result.artist,
            releaseType: result.releaseType,
            country: result.country,
            requested: result.requested,
            approved: result.approved,
            available: result.available,
            url: result.plexUrl
        };

        return context;
    }

    $('#seasonsModal').on('show.bs.modal', function (event) {
        var button = $(event.relatedTarget); // Button that triggered the modal
        var id = button.data('identifier'); // Extract info from data-* attributes
        var url = createBaseUrl(base, '/search/seasons/');

        $.ajax({
            type: "get",
            url: url,
            data: { tvId: id },
            dataType: "json",
            success: function (results) {
                var $content = $("#seasonsBody");
                $content.html("");
                $('#selectedSeasonsId').val(id);
                results.forEach(function (result) {
                    var context = buildSeasonsContext(result);
                    $content.append(seasonsTemplate(context));
                });
            },
            error: function (e) {
                console.log(e);
                generateNotify("Something went wrong!", "danger");
            }
        });

        function buildSeasonsContext(result) {
            var context = {
                id: result
            };
            return context;
        };
    });

    $('#seasonsRequest').click(function (e) {
        e.preventDefault();
        var tvId = $('#selectedSeasonsId').val();
        var url = createBaseUrl(base, '/search/seasons/');

        if ($("#" + tvId).attr('disabled')) {
            return;
        }

        $("#" + tvId).prop("disabled", true);
        loadingButton(tvId, "primary");


        var $form = $('#form' + tvId);
        var data = $form.serialize();
        var seasonsParam = "&seasons=";

        var $checkedSeasons = $('.selectedSeasons:checkbox:checked');
        $checkedSeasons.each(function (index, element) {
            if (index < $checkedSeasons.length - 1) {
                seasonsParam = seasonsParam + element.id + ",";
            } else {
                seasonsParam = seasonsParam + element.id;
            }
        });


        data = data + seasonsParam;

        var type = $form.prop('method');
        var url = $form.prop('action');

        sendRequestAjax(data, type, url, tvId);

    });

    $('#episodesModal').on('show.bs.modal', function (event) {
        $("#episodesBody").html(""); // Clear out the modal body
        $('#episodeModalLoading').removeAttr('hidden');
        finishLoading("episodesRequest", "primary");
        var button = $(event.relatedTarget); // Button that triggered the modal
        var id = button.data('identifier'); // Extract info from data-* attributes
        var url = createBaseUrl(base, '/search/episodes/');
        var seenSeasons = [];
        $.ajax({
            type: "get",
            url: url,
            data: { tvId: id },
            dataType: "json",
            success: function (results) {
                $('#episodeModalLoading').attr('hidden', "hidden");
                var $content = $("#episodesBody");
                $content.html("");
                $('#selectedEpisodeId').val(id);
                results.forEach(function (result) {
                    var episodes = buildEpisodesView(result);

                    if (!seenSeasons.find(function(x) {
                         return x === episodes.season
                    })) {
                        // Create the seasons heading
                        seenSeasons.push(episodes.season);
                        var context = buildSeasonsCount(result);
                        $content.append(seasonsNumberTemplate(context));
                    }
                    var episodesResult = episodesTemplate(episodes);
                    $content.append(episodesResult);
                });
            },
            error: function (e) {
                console.log(e);
                generateNotify("Something went wrong!", "danger");
            }
        });

        // Save Modal click
        $("#episodesRequest").click(function (e) {
            e.preventDefault();
            $("#episodesRequest").unbind();
            var origHtml = $('#episodesRequest').html();

            disableElement($('#episodeRequest'));
            loadingButton("episodesRequest", "primary");
            var tvId = $('#selectedEpisodeId').val();


            var $form = $('#form' + tvId);
            var model = [];

            var $checkedEpisodes = $('.selectedEpisodes:checkbox:checked:not(:disabled)');
            $checkedEpisodes.each(function (index, element) {
                var $element = $('#' + element.id);
                var tempObj = {};
                tempObj.episodeNumber = $element.attr("epNumber");
                tempObj.seasonNumber = $element.attr("epSeason");
                model.push(tempObj);
            });

            var finalObj = {
                ShowId: tvId,
                Episodes: model
            }

            var methodUrl = createBaseUrl(mainBaseUrl, "search/request/tvEpisodes");
            var methodType = $form.prop('method');
            $('#episodesModal').modal('toggle');

            $.ajax({
                type: methodType,
                url: methodUrl,
                data: JSON.stringify(finalObj),
                dataType: "json",
                success: function (response) {
                    finishLoading("episodesRequest", "primary", origHtml);
                    enableElement($('#episodeRequest'));
                    if (response.result === true) {
                        generateNotify(response.message, "success");
                    } else {
                        generateNotify(response.message, "warning");
                    }

                },
                error: function (e) {
                    console.log(e);
                    generateNotify("Something went wrong!", "danger");
                }
            });
        });

        function buildSeasonsContext(result) {
            var context = {
                id: result
            };
            return context;
        };

        function buildSeasonsCount(result) {
            return {
                seasonNumber: result.seasonNumber
            }
        }


        function buildEpisodesView(result) {
            return {
                id: result.id,
                name: result.name,
                season: result.seasonNumber,
                number: result.episodeNumber,
                requested: result.requested,
                episodeId: result.episodeId
            }
        }
    });

});
