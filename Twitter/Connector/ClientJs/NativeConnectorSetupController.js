angular.module('MainApp').controller('NativeConnectorSetupCtrl', ['$scope', '$location', '$http', '$cookies', function ($scope, $location, $http, $cookies) {
    $scope.jobType = "Twitter";
    $scope.isLoginComplete = false;
    $scope.sharedSecretKey = "";
    $scope.isSetupComplete = false;
    $scope.pageSaveMessage = "";
    $scope.entities = [];
    $scope.isbusy = true;
    $scope.isTokenDeleted = false;
    $scope.isAuthenticationComplete = false;
    $scope.authenticationUrl = null;
    $scope.selectedPageName = "";
    $scope.twitterRedirectUrl = $location.protocol() + "://" + location.host + "/Views/TwitterOAuth";
    $scope.twitterBaseUrl = $location.protocol() + "://" + location.host + "/Views/TwitterOAuth";

    var jobId = getParameter("jobId");
    var tenantId = getParameter("tenantId");

    var getOAuthUrl = "api/ConnectorSetup/OAuthUrl" + "?jobType=" + $scope.jobType + "&redirectUrl=" + $scope.twitterRedirectUrl;
    var deleteTokenUrl = "api/ConnectorSetup/DeleteToken" + "?jobType=" + $scope.jobType;
    var getEntitiesUrl = "api/ConnectorSetup/GetEntities" + "?jobType=" + $scope.jobType;
    var savePageurl = "api/ConnectorSetup/SavePage" + "?jobId=" + jobId + "&tenantId=" + tenantId;

    $scope.login = () => {
        if ($scope.sharedSecretKey !== null) {
            $cookies.put("sharedSecret", $scope.sharedSecretKey);
            $cookies.put("jobId", jobId);
            $cookies.put("tenantId", tenantId);

            getEntitiesUrl = getEntitiesUrl + "&jobId=" + jobId;
            deleteTokenUrl = deleteTokenUrl + "&jobId=" + jobId;
            $scope.isLoginComplete = true;

            $http.get(getOAuthUrl).then((response) => {
                $scope.authenticationUrl = response.data;
                $scope.isbusy = false;
            });
        }
    }

    $scope.openPopop = () => {
        var encodedAuthUrl = encodeURIComponent($scope.authenticationUrl);
        var url = $scope.twitterBaseUrl + "?loginUrl=" + encodedAuthUrl;
        $scope.isbusy = true;
        openPopup(this, url, function authenticationCallback() {
            $http.get(getEntitiesUrl).then((response) => {
                $scope.isbusy = false;
                if (response === 'undefined' || response.status !== 200) {
                    $scope.isSetupComplete = true;
                    $scope.pageSaveMessage = "Unable to setup Twitter Job due to Authorization Error";
                }

                if (response.data) {
                    $scope.isAuthenticationComplete = true;
                    $scope.entities = response.data;
                }               
            }).then(
                () => {
                    var selectedPage = $scope.entities[0];
                    $scope.selectedPageName = selectedPage.Name;

                    if (selectedPage) {
                        var pageToBeSaved = {
                            Name: selectedPage.Name,
                            Id: selectedPage.Id
                        };
                        $http.post(savePageurl, pageToBeSaved).then((response) => {
                            var res = response.data;
                            setTimeout(function () {
                            }, 500);

                            if (res === true) {
                                $scope.pageSaveMessage = "Twitter Connector Job Successfully set up.";
                            }
                            else {
                                $scope.pageSaveMessage = "Twitter Connector Job Successfully set up. Webhook Subscription failed for this page. Please get your app reviewed by Twitter"
                            }

                            $scope.isSetupComplete = true;
                        }).catch((error) => {
                            $scope.pageSaveMessage = "Twitter Connector Job  Setup Failed. Please retry again";
                        });
                    }
                }
            ).catch((error) => {
                $scope.isSetupComplete = true;
                $scope.isAuthenticationComplete = true;
                $scope.pageSaveMessage = "Twitter Connector Job  Setup Failed.";
            });
        });
    }

    $scope.finishSetup = () => {
        $http.get(deleteTokenUrl).then((response) => {
            $scope.isTokenDeleted = response.data;
        });
        window.close();
    }
}]);

function openPopup(context, path, callback) {
    var windowName = 'AuthenticationPopup';
    var windowOptions = 'location=0,status=0,width=800,height=400';
    var popupCallback = callback || function () { window.location.reload(); };
    var _oauthWindow = window.open(path, windowName, windowOptions);
    var _oauthInterval = window.setInterval(function () {
        if (_oauthWindow.closed) {
            window.clearInterval(_oauthInterval);
            popupCallback.call(context);
        }
    }, 1000);
}

function getParameter(paramName) {
    var searchString = window.location.search.substring(1),
        i, val, params = searchString.split("&");

    for (i = 0; i < params.length; i++) {
        val = params[i].split("=");
        if (val[0] === paramName) {
            return val[1];
        }
    }
    return null;
}