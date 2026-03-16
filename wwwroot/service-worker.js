const cacheName = "bookquotes-dev-shell-v1";
const offlineFallbackUrl = "./index.html";
const assetsToPrecache = [
    "./",
    "./index.html",
    "./manifest.webmanifest",
    "./favicon.png",
    "./icon-192.png",
    "./icon-512.png",
    "./css/app.css",
    "./js/localStorage.js",
    "./js/camera.js",
    "./js/ocr.js",
    "./js/pwaInstall.js"
];

self.addEventListener("install", event => event.waitUntil(onInstall()));
self.addEventListener("activate", event => event.waitUntil(onActivate()));
self.addEventListener("fetch", event => event.respondWith(onFetch(event)));

async function onInstall() {
    self.skipWaiting();

    const cache = await caches.open(cacheName);
    await cache.addAll(assetsToPrecache);
}

async function onActivate() {
    const keys = await caches.keys();

    await Promise.all(
        keys
            .filter(key => key !== cacheName)
            .map(key => caches.delete(key))
    );

    await self.clients.claim();
}

async function onFetch(event) {
    const { request } = event;

    if (request.method !== "GET") {
        return fetch(request);
    }

    const url = new URL(request.url);

    if (url.origin !== self.location.origin) {
        return fetch(request);
    }

    if (request.mode === "navigate") {
        return networkFirst(request, offlineFallbackUrl);
    }

    return staleWhileRevalidate(request);
}

async function networkFirst(request, fallbackUrl) {
    const cache = await caches.open(cacheName);

    try {
        const response = await fetch(request);
        cache.put(request, response.clone());
        return response;
    } catch {
        const cachedResponse = await cache.match(request);

        if (cachedResponse) {
            return cachedResponse;
        }

        const fallbackResponse = await cache.match(fallbackUrl);
        return fallbackResponse || Response.error();
    }
}

async function staleWhileRevalidate(request) {
    const cache = await caches.open(cacheName);
    const cachedResponse = await cache.match(request);
    const fetchPromise = fetch(request)
        .then(response => {
            if (response && response.ok) {
                cache.put(request, response.clone());
            }

            return response;
        })
        .catch(() => null);

    if (cachedResponse) {
        return cachedResponse;
    }

    const networkResponse = await fetchPromise;
    return networkResponse || Response.error();
}
