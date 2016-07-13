(function () {

    var userManagementService = function ($http) {

        var getUsers = function () {
            return $http.get('/usermanagement/users');
        };

        var addUser = function (user) {

           return $http({
                url: '/usermanagement/createuser',
                method: "POST",
                data: $.param(user),
                headers: {
                    'Content-Type': 'application/x-www-form-urlencoded; charset=UTF-8'
                }
            });
        }

        return {
            getUsers: getUsers,
            addUser: addUser
        };
    }

    angular.module('PlexRequests').factory('userManagementService', ["$http", userManagementService]);

}());