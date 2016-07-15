(function () {

    var controller = function ($scope, userManagementService) {

        $scope.user = {}; // The local user
        $scope.users = []; // list of users

        $scope.selectedUser = {};

        $scope.sortType = 'username';
        $scope.sortReverse = false;
        $scope.searchTerm = '';

        $scope.error = {
            error: false,
            errorMessage: ""
        };

        $scope.selectUser = function(id) {
            $scope.selectedUser = $scope.users.find(x => x.id === id);
        }

        $scope.getUsers = function () {
            $scope.users = userManagementService.getUsers()
            .then(function (data) {
                $scope.users = data.data;
            });
        };

        $scope.addUser = function () {
            if (!$scope.user.username || !$scope.user.password) {
                $scope.error.error = true;
                $scope.error.errorMessage = "Please provide a correct username and password";
                generateNotify($scope.error.errorMessage, 'warning');
                return;
            }
            userManagementService.addUser($scope.user).then(function (data) {
                if (data.message) {
                    $scope.error.error = true;
                    $scope.error.errorMessage = data.message;
                } else {
                    $scope.users.push(data);
                    $scope.user = {};
                }
            });
        };
    }

    angular.module('PlexRequests').controller('userManagementController', ["$scope", "userManagementService", controller]);

}());