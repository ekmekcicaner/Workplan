const workplanThemeStorageKey = 'theme';

function resolveWorkplanTheme(theme) {
    return theme === 'dark' || (theme !== 'light' && window.matchMedia('(prefers-color-scheme: dark)').matches);
}

window.workplanTheme = {
    get: function () {
        return localStorage.getItem(workplanThemeStorageKey);
    },
    set: function (theme) {
        if (theme === 'light' || theme === 'dark') {
            localStorage.setItem(workplanThemeStorageKey, theme);
        } else {
            localStorage.removeItem(workplanThemeStorageKey);
        }

        return this.apply(theme);
    },
    apply: function (theme) {
        var isDark = resolveWorkplanTheme(theme);
        document.documentElement.classList.toggle('dark', isDark);
        document.documentElement.style.colorScheme = isDark ? 'dark' : 'light';
        return isDark;
    },
    prefersDark: function () {
        return window.matchMedia('(prefers-color-scheme: dark)').matches;
    }
};
