(function () {

    var userManagementService = function ($http) {

        $http.defaults.headers.common['Content-Type'] = 'application/json'; // Set default headers

        var getUsers = function () {
            return $http.get('/usermanagement/users');
        };

        var addUser = function (user, claims) {
            if (!user || claims.length === 0) {
                return null;
            }

            return $http({
                url: '/usermanagement/createuser',
                method: "POST",
                data: { username: user.username, password: user.password, claims: claims, email: user.email }
            });
        }

        var getClaims = function () {
            return $http.get('/usermanagement/claims');
        }

        var updateUser = function (id, claims, alias, email) {
            return $http({
                url: '/usermanagement/updateUser',
                method: "POST",
                data: { id: id, claims: claims, alias: alias, emailAddress: email }
            });
        }

        var deleteUser = function (id) {
            return $http({
                url: '/usermanagement/deleteUser',
                method: "POST",
                data: { id: id }
            });
        }

        return {
            getUsers: getUsers,
            addUser: addUser,
            getClaims: getClaims,
            updateUser: updateUser,
            deleteUser: deleteUser
        };
    }

    angular.module('PlexRequests').factory('userManagementService', ["$http", userManagementService]);

}());