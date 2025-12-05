window.CartStorage = {
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
    }
};
