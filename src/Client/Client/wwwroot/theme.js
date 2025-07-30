// Удаляем ключевые слова export
window.isDarkMode = function () {
    return document.body.classList.contains('mud-theme-dark');
};

window.setupThemeObserver = function (dotNetRef) {
    const observer = new MutationObserver(mutations => {
        for (const m of mutations) {
            if (m.attributeName === 'class') {
                const dark = document.body.classList.contains('mud-theme-dark');
                dotNetRef.invokeMethodAsync('OnThemeChanged', dark);
            }
        }
    });
    observer.observe(document.body, { attributes: true, attributeFilter: ['class'] });
    return observer;
};
