const CACHE_NAME = "idle-sdk-web-demo-v46";
const ASSETS = [
    "./",
    "./index.html",
    "./styles.css?v=46",
    "./app.js?v=46",
    "./manifest.webmanifest",
    "./icon.svg",
    "./assets/icons/afk.png",
    "./assets/icons/slots_dark.png",
    "./assets/icons/scratcher_dark.png",
    "./assets/icons/keno_dark.png",
    "./assets/icons/chip_green_dark.png",
    "./assets/icons/chip_blue_dark.png",
    "./assets/icons/cash_dark.png",
    "./assets/icons/credit_dark.png"
];

self.addEventListener("install", (event) => {
    self.skipWaiting();
    event.waitUntil(
        caches.open(CACHE_NAME).then((cache) => cache.addAll(ASSETS))
    );
});

self.addEventListener("activate", (event) => {
    event.waitUntil(
        caches.keys().then((keys) =>
            Promise.all(keys.filter((key) => key !== CACHE_NAME).map((key) => caches.delete(key)))
        ).then(() => self.clients.claim())
    );
});

self.addEventListener("fetch", (event) => {
    const request = event.request;
    const url = new URL(request.url);

    if (request.mode === "navigate") {
        event.respondWith(
            fetch(request).catch(() => caches.match("./index.html"))
        );
        return;
    }

    if (url.pathname.endsWith(".js") || url.pathname.endsWith(".css")) {
        event.respondWith(
            fetch(request)
                .then((response) => {
                    const copy = response.clone();
                    caches.open(CACHE_NAME).then((cache) => cache.put(request, copy));
                    return response;
                })
                .catch(() => caches.match(request))
        );
        return;
    }

    event.respondWith(
        caches.match(request).then((response) => response || fetch(request))
    );
});
