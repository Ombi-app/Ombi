(function () {

    var controller = function ($scope, userManagementService) {

        $scope.user = {}; // The local user
        $scope.users = []; // list of users
        $scope.claims = []; // List of claims

        $scope.selectedUser = {}; // User on the right side
        $scope.selectedClaims = {};


        $scope.sortType = "username";
        $scope.sortReverse = false;
        $scope.searchTerm = "";


        $scope.error = {
            error: false,
            errorMessage: ""
        };

        // Select a user to populate on the right side
        $scope.selectUser = function (id) {
            var user = $scope.users.filter(function (item) {
                return item.id === id;
            });
            $scope.selectedUser = user[0];
        }

        // Get all users in the system
        $scope.getUsers = function () {
            $scope.users = userManagementService.getUsers()
                .then(function (data) {
                    $scope.users = data.data;
                });
        };

        // Get the claims and populate the create dropdown
        $scope.getClaims = function () {
            userManagementService.getClaims()
                .then(function (data) {
                    $scope.claims = data.data;
                });
        }

        // Create a user, do some validation too    
        $scope.addUser = function () {

            if (!$scope.user.username || !$scope.user.password) {
                $scope.error.error = true;
                $scope.error.errorMessage = "Please provide a correct username and password";
                generateNotify($scope.error.errorMessage, 'warning');
                return;
            }

            userManagementService.addUser($scope.user, $scope.selectedClaims)
                .then(function (data) {
                    if (data.message) {
                        $scope.error.error = true;
                        $scope.error.errorMessage = data.message;
                    } else {
                        $scope.users.push(data); // Push the new user into the array to update the DOM
                        $scope.user = {};
                        $scope.selectedClaims = {};
                    }
                });
        };

        $scope.hasClaim = function(claim) {
            var claims = $scope.selectedUser.claimsArray;

            var result = claims.some(function (item) {
                return item === claim.name;
            });
            return result;
        };

        $scope.$watch('claims|filter:{selected:true}',
            function (nv) {
                $scope.selectedClaims = nv.map(function (claim) {
                    return claim.name;
                });
            },
            true);


        $scope.updateUser = function () {
            userManagementService.updateUser($scope.selectedUser.id, $scope.selectedUser.claimsItem);
        }

        function getBaseUrl() {
            return $('#baseUrl').val();
        }


        // On page load
        $scope.init = function () {
            $scope.getUsers();
            $scope.getClaims();
            return;
        }
    }

    angular.module('PlexRequests').controller('userManagementController', ["$scope", "userManagementService", controller]);
}());