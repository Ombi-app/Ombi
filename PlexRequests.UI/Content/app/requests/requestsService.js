(function () {

    var requestsService = function ($http) {

        $http.defaults.headers.common['Content-Type'] = 'application/json'; // Set default headers

        var getRequests = function (type, baseUrl) {
            switch (type) {
                case "movie":
                    return $http.get(createBaseUrl(baseUrl, "/requestsbeta/movies"));
                case "tv":
                    return $http.get(createBaseUrl(baseUrl, "/requestsbeta/tvshows"));
                case "album":
                    return $http.get(createBaseUrl(baseUrl, "/requestsbeta/albums"));
            }
            return null;
        };

        var getPlexRequestSettings = function (baseUrl) {
            return $http.get(createBaseUrl(baseUrl, "/requestsbeta/plexrequestsettings"));
        }

        var getRequestsSettings = function (baseUrl) {
            return $http.get(createBaseUrl(baseUrl, "/requestsbeta/requestsettings"));
        }

        var getRequestsSearch = function (type, baseUrl, searchTerm) {
            switch (type) {
                case "movie":
                    return $http.get(createBaseUrl(baseUrl, "/requestsbeta/movies/"+ searchTerm));
                case "tv":
                    return $http.get(createBaseUrl(baseUrl, "/requestsbeta/tvshows/" + searchTerm));
                case "album":
                    return $http.get(createBaseUrl(baseUrl, "/requestsbeta/albums/" + searchTerm));
            }
            return null;
        };

        return {
            getRequests: getRequests,
            getRequestsSearch: getRequestsSearch,
            getPlexRequestSettings: getPlexRequestSettings,
            getRequestSettings: getRequestsSettings
        };
    }

    angular.module('PlexRequests').factory('requestsService', ["$http", requestsService]);

}());