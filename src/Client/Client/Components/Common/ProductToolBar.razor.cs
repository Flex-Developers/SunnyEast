// Components/ProductToolBar.razor.cs

using Application.Contract.ProductCategory.Responses;
using Microsoft.AspNetCore.Components;

namespace Client.Components.Common; // замените на фактический namespace проекта

public partial class ProductToolBar
{
    /// <summary>
    /// Список категорий, приходящий от родительского компонента/страницы.
    /// </summary>
    [Parameter, EditorRequired]
    public IEnumerable<ProductCategoryResponse> Categories
    {
        get => _categories;
        set => _categories = value;
    }

    /// <summary>
    /// Текущая выбранная категория (двустороннее связывание).
    /// </summary>
    [Parameter]
    public string SelectedCategory { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> SelectedCategoryChanged { get; set; }   // нужен для @bind

    // локальное поле для строки поиска
    private string _searchTerm = string.Empty;

    /// <summary>
    /// Строка поиска (двустороннее связывание).
    /// Каждый ввод символа мгновенно передаётся родителю через
    /// EventCallback SearchTermChanged.
    /// </summary>
    [Parameter]
    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            if (_searchTerm == value) return;                 // ничего не поменялось
            _searchTerm = value ?? string.Empty;

            // уведомляем родительский компонент
            if (SearchTermChanged.HasDelegate)
                _ = SearchTermChanged.InvokeAsync(_searchTerm);
        }
    }


    [Parameter]
    public EventCallback<string> SearchTermChanged { get; set; }         // нужен для @bind

    /* --------------------------------------------------------- */

    private IEnumerable<ProductCategoryResponse> _categories = new List<ProductCategoryResponse>();
    private bool _isSearchOpen;

    /// <summary>
    /// Выбор категории из списка.
    /// </summary>
    private async Task OnCategoryChanged(string newCategory)
    {
        SelectedCategory = newCategory;

        if (SelectedCategoryChanged.HasDelegate)
            await SelectedCategoryChanged.InvokeAsync(newCategory);
    }

    /// <summary>
    /// Показ / скрытие поля поиска на мобильном.
    /// </summary>
    private void ToggleSearch() => _isSearchOpen = !_isSearchOpen;
}