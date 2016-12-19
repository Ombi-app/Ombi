(function() {
    module = angular.module('PlexRequests', ['ngLoadingSpinner']);
    module.constant("moment", moment);

    //module.config(['usSpinnerConfigProvider', function (usSpinnerConfigProvider) {
    //    usSpinnerConfigProvider.setDefaults({ color: 'white' });
    //}]);
}());