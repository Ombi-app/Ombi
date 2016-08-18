(function () {

    var userManagementService = function ($http) {

        $http.defaults.headers.common['Content-Type'] = 'application/x-www-form-urlencoded'; // Set default headers

        var getUsers = function () {
            return $http.get('/usermanagement/users');
        };

        var addUser = function (user) {
            if (!user) {
                return null;
            }

            return $http({
                url: '/usermanagement/createuser',
                method: "POST",
                data: $.param(user)
            });
        }

        return {
            getUsers: getUsers,
            addUser: addUser
        };
    }

    angular.module('PlexRequests').factory('userManagementService', ["$http", userManagementService]);

}());