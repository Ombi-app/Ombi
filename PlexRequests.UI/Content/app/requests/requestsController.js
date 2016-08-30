(function () {
    var controller = function($scope, requestsService) {

        $scope.requests = [];
        $scope.selectedTab = {};
        $scope.currentPage = 1;
        $scope.tabs = [];

        $scope.plexSettings = {};
        $scope.requestSettings = {};

        // Search
        $scope.searchTerm = "";


        // Called on page load
        $scope.init = function() {
            // Get the settings
            $scope.plexSettings = requestsService.getPlexRequestSettings(getBaseUrl());
            $scope.requestSettings = requestsService.getRequestSettings(getBaseUrl());

            if ($scope.plexSettings.SearchForMovies) {
                $scope.selectedTab = "movies";

                // Load the movie Requests
                $scope.requests = requestsService.getRequests("movie", getBaseUrl());
            }
        };


        $scope.changeTab = function(tab) {
            // load the data from the tab
            switch (tab) {
                // Set the selected tab and load the appropriate data
            }

        };

        $scope.search = function() {
            $scope.requests = requestsService.getRequests
        };

    function getBaseUrl() {
            return $('#baseUrl').val();
        }


    }

    angular.module('PlexRequests').controller('requestsController', ["$scope", "requestsService", controller]);
}());