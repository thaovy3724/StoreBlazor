window.CartStorage = {
    // cart item
    save: function (key, value) {
        sessionStorage.setItem(key, JSON.stringify(value));
    },
    load: function (key) {
        const raw = sessionStorage.getItem(key);
        if (!raw) return null;
        return JSON.parse(raw);
    },
    clear: function (key) {
        sessionStorage.removeItem(key);
    },
    // promotion
    savePromotion: function (key, value) {
        sessionStorage.setItem(key, JSON.stringify(value));
    },
    loadPromotion: function (key) {
        const raw = sessionStorage.getItem(key);
        if (!raw) return null;
        return JSON.parse(raw);
    },
    clearPromotion: function (key) {
        sessionStorage.removeItem(key);
    }
};
