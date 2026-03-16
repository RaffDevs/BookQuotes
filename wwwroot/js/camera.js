window.bookQuotesCamera = {
    _streams: {},

    startCamera: async function (videoElementId, facingMode) {
        if (!navigator.mediaDevices || typeof navigator.mediaDevices.getUserMedia !== "function") {
            throw new Error("Camera nao suportada neste navegador.");
        }

        const video = document.getElementById(videoElementId);

        if (!video) {
            throw new Error("Elemento de video nao encontrado.");
        }

        await window.bookQuotesCamera.stopCamera(videoElementId);

        const constraints = {
            audio: false,
            video: {
                facingMode: {
                    ideal: facingMode || "environment"
                }
            }
        };

        const stream = await navigator.mediaDevices.getUserMedia(constraints);
        window.bookQuotesCamera._streams[videoElementId] = stream;

        video.srcObject = stream;
        video.setAttribute("playsinline", "true");
        await video.play();
    },

    captureFrame: function (videoElementId, maxWidth, maxHeight, quality) {
        const video = document.getElementById(videoElementId);

        if (!video || !video.videoWidth || !video.videoHeight) {
            throw new Error("A camera ainda nao esta pronta para capturar.");
        }

        const widthRatio = maxWidth ? maxWidth / video.videoWidth : 1;
        const heightRatio = maxHeight ? maxHeight / video.videoHeight : 1;
        const ratio = Math.min(widthRatio, heightRatio, 1);
        const targetWidth = Math.max(1, Math.round(video.videoWidth * ratio));
        const targetHeight = Math.max(1, Math.round(video.videoHeight * ratio));

        const canvas = document.createElement("canvas");
        canvas.width = targetWidth;
        canvas.height = targetHeight;

        const context = canvas.getContext("2d");

        if (!context) {
            throw new Error("Nao foi possivel preparar o canvas para capturar a foto.");
        }

        context.drawImage(video, 0, 0, targetWidth, targetHeight);
        return canvas.toDataURL("image/jpeg", quality || 0.92);
    },

    stopCamera: async function (videoElementId) {
        const stream = window.bookQuotesCamera._streams[videoElementId];
        const video = document.getElementById(videoElementId);

        if (stream) {
            stream.getTracks().forEach(track => track.stop());
            delete window.bookQuotesCamera._streams[videoElementId];
        }

        if (video) {
            video.pause();
            video.srcObject = null;
        }
    }
};
