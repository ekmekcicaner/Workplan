window.workplanTheme = {
    get: function () {
        return localStorage.getItem('theme');
    },
    set: function (theme) {
        localStorage.setItem('theme', theme);
    },
    apply: function (isDark) {
        document.documentElement.classList.toggle('dark', isDark);
    },
    prefersDark: function () {
        return window.matchMedia('(prefers-color-scheme: dark)').matches;
    }
};
