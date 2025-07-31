// wwwroot/homePage.js

// Функция для инициализации бесконечной карусели товаров
function initializeProductCarousel() {
    const carousel = document.getElementById('productsCarousel');
    if (!carousel) return;

    const track = carousel.querySelector('.homepage-carousel-track');
    if (!track) return;

    // Карусель автоматически двигается вправо без остановки
    // CSS анимация уже настроена в стилях
}

// Функция для инициализации анимаций при скролле
function initializeScrollAnimations() {
    // Создаем наблюдатель для элементов
    const observerOptions = {
        threshold: 0.1, // Срабатывает когда 10% элемента видно
        rootMargin: '0px 0px -50px 0px' // Отступ снизу для более раннего срабатывания
    };

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                // Добавляем класс для анимации
                entry.target.classList.add('homepage-in-view');
                // Перестаем наблюдать за элементом после анимации
                observer.unobserve(entry.target);
            }
        });
    }, observerOptions);

    // Находим все элементы для анимации
    const animatedElements = document.querySelectorAll('.homepage-scroll-animate');
    animatedElements.forEach(element => {
        observer.observe(element);
    });
}

// Экспортируем функции для Blazor
window.initializeProductCarousel = initializeProductCarousel;
window.initializeScrollAnimations = initializeScrollAnimations;