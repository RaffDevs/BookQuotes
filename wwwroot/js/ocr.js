window.bookQuotesOcr = {
    _worker: null,
    _workerLanguage: null,
    _timeoutMs: 30000,

    extractTextFromElement: async function (elementId, language) {
        if (!window.Tesseract || typeof window.Tesseract.createWorker !== "function") {
            throw new Error("Tesseract.js nao carregado corretamente.");
        }

        const element = document.getElementById(elementId);

        if (!element) {
            throw new Error("Elemento de imagem nao encontrado para OCR.");
        }

        const resolvedLanguage = language || "por";

        try {
            await window.bookQuotesOcr.ensureElementIsReady(element);
            const canvas = window.bookQuotesOcr.elementToCanvas(element);
            const worker = await window.bookQuotesOcr.getWorker(resolvedLanguage);
            const result = await window.bookQuotesOcr.withTimeout(
                worker.recognize(canvas),
                window.bookQuotesOcr._timeoutMs,
                "Tempo limite excedido ao executar OCR."
            );

            return result?.data?.text ?? "";
        } catch (error) {
            const message = error?.message || String(error) || "Falha ao processar imagem para OCR.";
            throw new Error(`Falha ao processar imagem para OCR. ${message}`);
        }
    },

    getWorker: async function (language) {
        if (window.bookQuotesOcr._worker && window.bookQuotesOcr._workerLanguage === language) {
            return window.bookQuotesOcr._worker;
        }

        if (window.bookQuotesOcr._worker) {
            await window.bookQuotesOcr._worker.terminate();
            window.bookQuotesOcr._worker = null;
            window.bookQuotesOcr._workerLanguage = null;
        }

        const worker = await window.bookQuotesOcr.withTimeout(
            window.Tesseract.createWorker(language),
            window.bookQuotesOcr._timeoutMs,
            "Tempo limite excedido ao criar o worker OCR."
        );
        window.bookQuotesOcr._worker = worker;
        window.bookQuotesOcr._workerLanguage = language;

        return worker;
    },

    ensureElementIsReady: function (element) {
        if (element instanceof HTMLImageElement) {
            return new Promise((resolve, reject) => {
                if (element.complete && element.naturalWidth > 0 && element.naturalHeight > 0) {
                    resolve();
                    return;
                }

                if (typeof element.decode === "function") {
                    element.decode()
                        .then(() => {
                            resolve();
                        })
                        .catch(() => {
                            window.bookQuotesOcr.waitImageBySrc(element.currentSrc || element.src)
                                .then(() => resolve())
                                .catch(reject);
                        });
                    return;
                }

                const onLoad = () => {
                    cleanup();
                    resolve();
                };

                const onError = () => {
                    cleanup();
                    reject(new Error("O navegador nao conseguiu carregar a imagem renderizada."));
                };

                const cleanup = () => {
                    element.removeEventListener("load", onLoad);
                    element.removeEventListener("error", onError);
                };

                element.addEventListener("load", onLoad);
                element.addEventListener("error", onError);
            });
        }

        return Promise.resolve();
    },

    waitImageBySrc: function (src) {
        return new Promise((resolve, reject) => {
            if (!src) {
                reject(new Error("Imagem renderizada sem src."));
                return;
            }

            const img = new Image();

            img.onload = () => {
                resolve();
            };

            img.onerror = () => {
                reject(new Error("O navegador nao conseguiu carregar a imagem renderizada."));
            };

            img.src = src;
        });
    },

    elementToCanvas: function (element) {
        const width = element.naturalWidth || element.width;
        const height = element.naturalHeight || element.height;

        if (!width || !height) {
            throw new Error("Elemento de imagem sem dimensoes validas para OCR.");
        }

        const canvas = document.createElement("canvas");
        canvas.width = width;
        canvas.height = height;

        const context = canvas.getContext("2d");

        if (!context) {
            throw new Error("Nao foi possivel criar o canvas para OCR.");
        }

        context.drawImage(element, 0, 0, width, height);
        return canvas;
    },

    disposeWorker: async function () {
        if (window.bookQuotesOcr._worker) {
            await window.bookQuotesOcr._worker.terminate();
            window.bookQuotesOcr._worker = null;
            window.bookQuotesOcr._workerLanguage = null;
        }
    },

    withTimeout: function (promise, timeoutMs, message) {
        return new Promise((resolve, reject) => {
            const timer = setTimeout(() => {
                reject(new Error(message));
            }, timeoutMs);

            promise
                .then(result => {
                    clearTimeout(timer);
                    resolve(result);
                })
                .catch(error => {
                    clearTimeout(timer);
                    reject(error);
                });
        });
    }
};
