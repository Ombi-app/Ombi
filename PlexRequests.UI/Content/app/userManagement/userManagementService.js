(function () {

    var userManagementService = function ($http) {

        $http.defaults.headers.common['Content-Type'] = 'application/json'; // Set default headers

        var getUsers = function () {
            return $http.get('/usermanagement/users');
        };

        var addUser = function (user, permissions, features) {
            if (!user || permissions.length === 0) {
                return null;
            }

            return $http({
                url: '/usermanagement/createuser',
                method: "POST",
                data: { username: user.username, password: user.password, permissions: permissions, features : features, email: user.email }
            });
        }

        var getFeatures = function () {
            return $http.get('/usermanagement/features');
        }

        var getPermissions = function () {
            return $http.get('/usermanagement/permissions');
        }

        var updateUser = function (id, permissions, alias, email) {
            return $http({
                url: '/usermanagement/updateUser',
                method: "POST",
                data: { id: id, permissions: permissions, alias: alias, emailAddress: email }
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
            getFeatures: getFeatures,
            getPermissions: getPermissions,
            updateUser: updateUser,
            deleteUser: deleteUser
        };
    }

    angular.module('PlexRequests').factory('userManagementService', ["$http", userManagementService]);

}());