window.pushInterop = {
    requestNotificationPermission: async () => {
        return await Notification.requestPermission();
    },
    urlBase64ToUint8Array: (base64String) => {
        // Handle URL-safe base64
        let base64 = base64String.replace(/-/g, '+').replace(/_/g, '/');

        // Add padding if needed
        const padLength = (4 - (base64.length % 4)) % 4;
        base64 += '='.repeat(padLength);

        try {
            const rawData = window.atob(base64);
            const outputArray = new Uint8Array(rawData.length);

            for (let i = 0; i < rawData.length; ++i) {
                outputArray[i] = rawData.charCodeAt(i);
            }
            return outputArray;
        } catch (error) {
            console.error('Base64 decode error:', error, 'Input:', base64String);
            throw error;
        }
    },
    subscribeUser: async (vapidPublicKey) => {
        const registration = await navigator.serviceWorker.ready;
        const applicationServerKey = window.pushInterop.urlBase64ToUint8Array(vapidPublicKey);
        const sub = await registration.pushManager.subscribe({
            userVisibleOnly: true,
            applicationServerKey
        });
        const rawKey = sub.getKey('p256dh');
        const rawAuth = sub.getKey('auth');
        const key = rawKey ? btoa(String.fromCharCode(...new Uint8Array(rawKey))) : '';
        const auth = rawAuth ? btoa(String.fromCharCode(...new Uint8Array(rawAuth))) : '';
        return {Endpoint: sub.endpoint, Keys: {P256dh: key, Auth: auth}};
    },
    unsubscribeUser: async () => {
        const registration = await navigator.serviceWorker.ready;
        const sub = await registration.pushManager.getSubscription();
        if (sub) {
            const endpoint = sub.endpoint;
            await sub.unsubscribe();
            return endpoint;
        }
        return null;
    },
    getSubscription: async () => {
        const registration = await navigator.serviceWorker.ready;
        const sub = await registration.pushManager.getSubscription();
        if (!sub) return null;
        const rawKey = sub.getKey('p256dh');
        const rawAuth = sub.getKey('auth');
        const key = rawKey ? btoa(String.fromCharCode(...new Uint8Array(rawKey))) : '';
        const auth = rawAuth ? btoa(String.fromCharCode(...new Uint8Array(rawAuth))) : '';
        return {Endpoint: sub.endpoint, Keys: {P256dh: key, Auth: auth}};
    }
};
