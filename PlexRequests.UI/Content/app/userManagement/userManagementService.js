(function () {

    var userManagementService = function ($http) {

        //$http.defaults.headers.common['Content-Type'] = 'application/x-www-form-urlencoded'; // Set default headers

        var getUsers = function () {
            return $http.get('/usermanagement/users');
        };

        var addUser = function (user, claims) {
            if (!user || claims.length === 0) {
                return null;
            }

            var claimJson = angular.toJson(claims);
            var objectToSerialize = { 'claims': claimJson };
            var data = $.param(user) +"&"+ $.param(objectToSerialize);
            return $http({
                url: '/usermanagement/createuser',
                method: "POST",
                data: data
            });
        }

        var getClaims = function() {
            return $http.get('/usermanagement/claims');
        }

        return {
            getUsers: getUsers,
            addUser: addUser,
            getClaims: getClaims
        };
    }

    angular.module('PlexRequests').factory('userManagementService', ["$http", userManagementService]);

}());