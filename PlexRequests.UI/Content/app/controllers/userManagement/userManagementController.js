(function () {

    var controller = function ($scope, userManagementService) {

        $scope.user = {}; // The local user to create
        $scope.users = []; // list of users

        $scope.error = false;
        $scope.errorMessage = {};

        $scope.getUsers = function () {
            $scope.users = userManagementService.getUsers()
            .then(function (data) {
                $scope.users = data.data;
            });
        };

        $scope.addUser = function () {
            if ($scope.users.length === 0) {
                $scope.getUsers();
            }
            userManagementService.addUser($scope.user).then(function (data) {
                if (data.message) {
                    $scope.error = true;
                    $scope.errorMessage = data.message;
                } else {
                    $scope.users.push(data);
                    $scope.user = {};
                }
            });
        };
    }

    angular.module('PlexRequests').controller('userManagementController', ["$scope", "userManagementService", controller]);

}());