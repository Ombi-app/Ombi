(function () {

    var controller = function ($scope, userManagementService, moment) {

        $scope.user = {}; // The local user
        $scope.users = []; // list of users

        $scope.features = []; // List of features
        $scope.permissions = []; // List of permissions

        $scope.selectedUser = {}; // User on the right side

        $scope.selectedFeatures = {};
        $scope.selectedPermissions = {};

        $scope.minDate = "0001-01-01T00:00:00.0000000+00:00";

        $scope.sortType = "username";
        $scope.sortReverse = false;
        $scope.searchTerm = "";

        $scope.hideColumns = false;
        
        var ReadOnlyPermission = "Read Only User";
        var open = false;

        // Select a user to populate on the right side
        $scope.selectUser = function (id) {
            var user = $scope.users.filter(function (item) {
                return item.id === id;
            });
            $scope.selectedUser = user[0];

            $scope.selectedUser.emailDisabled = $scope.selectedUser.type === 0 && $scope.selectedUser.managedUser == '';
            openSidebar();
        }

        // Get all users in the system
        $scope.getUsers = function () {
            $scope.users = userManagementService.getUsers()
                .then(function (data) {
                    $scope.users = data.data;
                });
        };

        // Get the permissions and features and populate the create dropdown
        $scope.getFeaturesPermissions = function () {
            userManagementService.getFeatures()
                .then(function (data) {
                    $scope.features = data.data;
                });

            userManagementService.getPermissions()
                .then(function (data) {
                    $scope.permissions = data.data;
                });
        }

        // Create a user, do some validation too    
        $scope.addUser = function () {
            if (!$scope.user.username || !$scope.user.password) {
                generateNotify("Please provide a username and password", 'warning');
                return;
            }
            if ($scope.selectedPermissions.length === 0) {
                generateNotify("Please select a permission", 'warning');
                return;
            }

            var hasReadOnly = $scope.selectedPermissions.indexOf(ReadOnlyPermission) !== -1;
            if (hasReadOnly) {
                if ($scope.selectedPermissions.length > 1) {
                    generateNotify("Cannot have the " + ReadOnlyPermission + " permission with other permissions.", 'danger');
                    return;
                }
            }

            if(Array.isArray($scope.users)){
           
            var existingUsername = $scope.users.some(function (u) {
                return u.username === $scope.user.username;
            });

            if (existingUsername) {
                return generateNotify("A user with the username " + $scope.user.username + " already exists!", 'danger');
            }
             }

            userManagementService.addUser($scope.user, $scope.selectedPermissions, $scope.selectedFeatures)
                .then(function (data) {
                    if (data.message) {
                        generateNotify(data.message, 'warning');
                    } else {
                        $scope.users.push(data.data); // Push the new user into the array to update the DOM
                        $scope.user = {};
                        clearCheckboxes();
                    };
                });

               
        };

        // Watch the checkboxes for updates (Creating a user)
        $scope.$watch('features|filter:{selected:true}',
            function (nv) {
                $scope.selectedFeatures = nv.map(function (f) {
                    return f.name;
                });
            },
            true);

        $scope.$watch('permissions|filter:{selected:true}',
            function (nv) {
                $scope.selectedPermissions = nv.map(function (f) {
                    return f.name;
                });
            },
            true);


        $scope.updateUser = function () {
            var u = $scope.selectedUser;
            userManagementService.updateUser(u.id, u.permissions, u.features, u.alias, u.emailAddress)
                .then(function success(data) {
                    if (data.data) {
                        $scope.selectedUser = data.data;

                        closeSidebar();
                        return successCallback("Updated User", "success");
                    }
                }, function errorCallback(response) {
                    successCallback(response, "danger");
                });
        }

        $scope.deleteUser = function () {
            var u = $scope.selectedUser;
            userManagementService.deleteUser(u.id)
                .then(function sucess(data) {
                    if (data.data.result) {
                        removeUser(u.id, true);
                        closeSidebar();
                        return successCallback("Deleted User", "success");
                    }
                }, function errorCallback(response) {
                    successCallback(response, "danger");
                });
        }

        $scope.formatDate = function (utcDate) {
            return moment.utc(utcDate).local().format('lll');
        }


        // On page load
        $scope.init = function () {
            $scope.getUsers();
            $scope.getFeaturesPermissions();
            return;
        }

        $scope.closeSidebarClick = function () {
            return closeSidebar();
        }

        $scope.redirectToSettings = function() {
            var url = createBaseUrl(getBaseUrl(), '/admin/usermanagementsettings');
            window.location.href = url;
        }

        function removeUser(id, current) {
            $scope.users = $scope.users.filter(function (user) {
                return user.id !== id;
            });
            if (current) {
                $scope.selectedUser = null;
            }
        }

        function closeSidebar() {
            if (open) {
                open = false;
                $("#wrapper").toggleClass("toggled");
                $scope.hideColumns = false;
            }
        }

        function openSidebar() {
            if (!open) {
                $("#wrapper").toggleClass("toggled");
                open = true;
                $scope.hideColumns = true;
            }
        }

        function clearCheckboxes() {
            $scope.selectedPermissions = {}; // Clear the checkboxes
            $scope.selectedFeatures = {};
            $scope.features.forEach(function (entry) {
                entry.selected = false;
            });
            $scope.permissions.forEach(function (entry) {
                entry.selected = false;
            });
        }


        function getBaseUrl() {
            return $('#baseUrl').text();
        }

    }

    function successCallback(message, type) {
        generateNotify(message, type);
    };

    angular.module('PlexRequests').controller('userManagementController', ["$scope", "userManagementService", "moment", controller]);
}());