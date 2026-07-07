window.workplanConnectivity = {
    isOnline: function () {
        return navigator.onLine;
    },
    register: function (dotnetRef) {
        var notify = function () {
            dotnetRef.invokeMethodAsync('SetOnlineState', navigator.onLine);
        };

        window.addEventListener('online', notify);
        window.addEventListener('offline', notify);
        notify();
    }
};
