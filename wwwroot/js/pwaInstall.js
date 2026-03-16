window.bookQuotesPwaInstall = {
    _deferredPrompt: null,
    _dotNetRef: null,
    _dismissKey: "bookquotes.pwa.dismissed",

    initialize: function (dotNetRef) {
        window.bookQuotesPwaInstall._dotNetRef = dotNetRef;

        window.addEventListener("beforeinstallprompt", window.bookQuotesPwaInstall._handleBeforeInstallPrompt);
        window.addEventListener("appinstalled", window.bookQuotesPwaInstall._handleAppInstalled);
    },

    dispose: function () {
        window.removeEventListener("beforeinstallprompt", window.bookQuotesPwaInstall._handleBeforeInstallPrompt);
        window.removeEventListener("appinstalled", window.bookQuotesPwaInstall._handleAppInstalled);
        window.bookQuotesPwaInstall._dotNetRef = null;
    },

    getState: function () {
        const isStandalone = window.matchMedia("(display-mode: standalone)").matches || window.navigator.standalone === true;
        const isDismissed = window.localStorage.getItem(window.bookQuotesPwaInstall._dismissKey) === "true";
        const isIos = /iphone|ipad|ipod/i.test(window.navigator.userAgent);
        const isSafari = /safari/i.test(window.navigator.userAgent) && !/chrome|android/i.test(window.navigator.userAgent);
        const canPromptInstall = !!window.bookQuotesPwaInstall._deferredPrompt;
        const showIosInstructions = !isStandalone && !isDismissed && isIos && isSafari;
        const isVisible = !isStandalone && !isDismissed && (canPromptInstall || showIosInstructions);

        let message = "";

        if (canPromptInstall) {
            message = "Instale o app para abrir mais rapido e usar uma experiencia mais proxima de nativo.";
        } else if (showIosInstructions) {
            message = "No iPhone ou iPad, toque em Compartilhar e depois em Adicionar a Tela de Inicio.";
        }

        return {
            isVisible,
            canPromptInstall,
            showIosInstructions,
            message
        };
    },

    promptInstall: async function () {
        const promptEvent = window.bookQuotesPwaInstall._deferredPrompt;

        if (!promptEvent) {
            return false;
        }

        promptEvent.prompt();
        const result = await promptEvent.userChoice;
        window.bookQuotesPwaInstall._deferredPrompt = null;

        if (result && result.outcome === "accepted") {
            window.localStorage.removeItem(window.bookQuotesPwaInstall._dismissKey);
        }

        window.bookQuotesPwaInstall._notifyStateChanged();
        return result && result.outcome === "accepted";
    },

    dismiss: function () {
        window.localStorage.setItem(window.bookQuotesPwaInstall._dismissKey, "true");
        window.bookQuotesPwaInstall._notifyStateChanged();
    },

    _handleBeforeInstallPrompt: function (event) {
        event.preventDefault();
        window.bookQuotesPwaInstall._deferredPrompt = event;
        window.bookQuotesPwaInstall._notifyStateChanged();
    },

    _handleAppInstalled: function () {
        window.bookQuotesPwaInstall._deferredPrompt = null;
        window.localStorage.removeItem(window.bookQuotesPwaInstall._dismissKey);
        window.bookQuotesPwaInstall._notifyStateChanged();
    },

    _notifyStateChanged: function () {
        if (window.bookQuotesPwaInstall._dotNetRef) {
            window.bookQuotesPwaInstall._dotNetRef.invokeMethodAsync("HandleStateChanged");
        }
    }
};
