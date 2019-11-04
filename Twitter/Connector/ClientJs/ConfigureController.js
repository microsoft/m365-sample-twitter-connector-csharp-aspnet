angular.module('MainApp').controller('ConfigureCtrl', ['$scope', '$location', '$http', '$cookies', function ($scope, $location, $http, $cookies) {
    $scope.isLoginComplete = false;
    $scope.isDisabledSaveButton = false;

    var checkSharedSecret = "setup/Validate";
    $scope.login = () => {
        if ($scope.sharedSecretKey != null) {
            $cookies.put("sharedSecret", $scope.sharedSecretKey);
            $cookies.put("tenantId", $scope.tenantId);
            $http.get(checkSharedSecret).then((response) => {
                $scope.isLoginComplete = response.data.Status;
            });
        }
    }

    $scope.SaveConfigSettings = () => {
        $scope.isDisabledSaveButton = true;
        $scope.configurationSavedMsg = "Saving Configuration ...";
        var configureUrl = "api/Configuration";
        var settings = {
            AADAppIdValue: (typeof $scope.AADAppIdValue !== 'undefined') ? $scope.AADAppIdValue : "",
            AADAppSecretValue: (typeof $scope.AADAppSecretValue !== 'undefined') ? $scope.AADAppSecretValue : "",
            TwitterApiKeyValue: (typeof $scope.TwitterApiKeyValue !== 'undefined') ? $scope.TwitterApiKeyValue : "",
            TwitterApiSecretKeyValue: (typeof $scope.TwitterApiSecretKeyValue !== 'undefined') ? $scope.TwitterApiSecretKeyValue : "",
            TwitterAccessTokenValue: (typeof $scope.TwitterAccessTokenValue !== 'undefined') ? $scope.TwitterAccessTokenValue : "",
            TwitterAccessTokenSecretValue: (typeof $scope.TwitterAccessTokenSecretValue !== 'undefined') ? $scope.TwitterAccessTokenSecretValue : "",
            AADAppUriValue: (typeof $scope.AADAppUriValue !== 'undefined') ? $scope.AADAppUriValue : "",
            InstrumentationKeyValue: (typeof $scope.InstrumentationKeyValue !== 'undefined') ? $scope.InstrumentationKeyValue : ""
        };
        $http.post(configureUrl, settings).then((response) => {
            var res = response.data;
            setTimeout(function () {
            }, 500);
            $scope.configurationSavedMsg = "Configuration Saved Successfully.";
        }).catch((error) => { });
    }

    $scope.Home = () => {
        window.location.href = "/index.cshtml";
    }

}]);