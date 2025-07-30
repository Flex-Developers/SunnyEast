// In development, always fetch from the network and do not enable offline support.
// This is because caching would make development more difficult (changes would not
// be reflected on the first load after each change).
self.addEventListener('fetch', () => {
});

self.addEventListener('push', event => {
    const data = event.data ? event.data.json() : {};
    const title = data.title || 'Notification';
    const options = {body: data.body || '', icon: 'icon-192.png', badge: 'icon-192.png'};
    event.waitUntil(self.registration.showNotification(title, options));
});
self.addEventListener('notificationclick', event => {
    event.notification.close();
    event.waitUntil(clients.matchAll({type: 'window'}).then(windowClients => {
        for (let client of windowClients) {
            if (client.url === '/' && 'focus' in client) return client.focus();
        }
        if (clients.openWindow) return clients.openWindow('/');
    }));
});
