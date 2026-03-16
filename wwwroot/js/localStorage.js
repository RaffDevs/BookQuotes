window.localStorageAdapter = {
    setItem: function(key, value) {
        localStorage.setItem(key, JSON.stringify(value))
    },

    getItem: function(key) {
        const item = localStorage.getItem(key);

        if (!item) {
            return null;
        }

        return JSON.parse(item);
    },

    removeItem: function(key) {
        localStorage.removeItem(key);
    }
};