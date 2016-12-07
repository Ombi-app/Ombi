(function () {

    var userManagementService = function ($http) {

        $http.defaults.headers.common['Content-Type'] = 'application/json'; // Set default headers

        

        var getUsers = function () {
            var url = createBaseUrl(getBaseUrl(), '/usermanagement/users');
            return $http.get(url);
        };

        var addUser = function (user, permissions, features) {
            if (!user || permissions.length === 0) {
                return null;
            }

            if (!isArray(permissions)) {
                permissions = [];
            }
            if (!isArray(features)) {
                features = [];
            }

            var url = createBaseUrl(getBaseUrl(), '/usermanagement/createuser');

            return $http({
                url: url,
                method: "POST",
                data: { username: user.username, password: user.password, permissions: permissions, features : features, email: user.email }
            });
        }

        var getFeatures = function () {
            var url = createBaseUrl(getBaseUrl(), '/usermanagement/features');
            return $http.get(url);
        }

        var getPermissions = function () {
            var url = createBaseUrl(getBaseUrl(), '/usermanagement/permissions');
            return $http.get(url);

        }

        var updateUser = function (id, permissions, features, alias, email) {

            if (!isArray(permissions)) {
                permissions = [];
            }
            if (!isArray(features)) {
                features = [];
            }


            var url = createBaseUrl(getBaseUrl(), '/usermanagement/updateUser');
            return $http({
                url: url,
                method: "POST",
                data: { id: id, permissions: permissions, features: features, alias: alias, emailAddress: email },
            });
        }

        var deleteUser = function (id) {

            var url = createBaseUrl(getBaseUrl(), '/usermanagement/deleteUser');
            return $http({
                url: url,
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
    function getBaseUrl() {
        return $('#baseUrl').text();
    }

    function isArray(obj) {
        return !!obj && Array === obj.constructor;
    }

    angular.module('PlexRequests').factory('userManagementService', ["$http", userManagementService]);

}());