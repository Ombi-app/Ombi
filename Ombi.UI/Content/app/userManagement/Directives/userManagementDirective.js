(function () {

    module.directive('tableComponent',
            function () {
                return {
                    templateUrl: createBaseUrl(getBaseUrl(), 'Content/app/userManagement/Directives/table.html')
                };
            })
        .directive('addUser',
            function () {
                return {
                    templateUrl: createBaseUrl(getBaseUrl(), 'Content/app/userManagement/Directives/addUser.html')
                };
            })
        .directive('sidebar',
            function () {
                return {
                    templateUrl: createBaseUrl(getBaseUrl(), 'Content/app/userManagement/Directives/sidebar.html')
                };
            });

    function getBaseUrl() {
        return $('#baseUrl').text();
    }
})();