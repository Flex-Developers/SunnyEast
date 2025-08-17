// Components/ProductToolBar.razor.cs
using Application.Contract.ProductCategory.Responses;
using Microsoft.AspNetCore.Components;

namespace Client.Components.Common;

public partial class ProductToolBar
{
    /// <summary>
    /// Список категорий, приходит от родителя.
    /// </summary>
    [Parameter, EditorRequired]
    public IEnumerable<ProductCategoryResponse> Categories
    {
        get => _categories;
        set => _categories = value ?? [];
    }

    /// <summary>
    /// Текущая выбранная категория (двустороннее связывание).
    /// </summary>
    [Parameter]
    public string SelectedCategory { get; set; } = string.Empty;

    [Parameter]
    public EventCallback<string> SelectedCategoryChanged { get; set; }

    /// <summary>
    /// Строка поиска (двустороннее связывание).
    /// При каждом изменении коротко показываем прогресс-бар (_isSearchBusy).
    /// </summary>
    [Parameter]
    public string SearchTerm
    {
        get => _searchTerm;
        set
        {
            var newValue = value ?? string.Empty;
            if (_searchTerm == newValue) return;
            _searchTerm = newValue;

            if (SearchTermChanged.HasDelegate)
                _ = SearchTermChanged.InvokeAsync(_searchTerm);

            // визуальный «эффект поиска» после каждого изменения
            _ = TriggerSearchBusyAsync();
        }
    }

    [Parameter]
    public EventCallback<string> SearchTermChanged { get; set; }

    /// <summary>
    /// Показывать ли индикатор загрузки в тулбаре (внешний, например при получении данных).
    /// </summary>
    [Parameter] public bool IsBusy { get; set; }

    /// <summary>
    /// Длительность «эффекта поиска» в миллисекундах.
    /// </summary>
    [Parameter]
    public int SearchBusyMs { get; set; } = 400;

    /* ----------------- internal state ----------------- */
    private IEnumerable<ProductCategoryResponse> _categories = new List<ProductCategoryResponse>();
    private bool _isSearchOpen;
    private string _searchTerm = string.Empty;

    private bool _isSearchBusy;
    private CancellationTokenSource? _searchCts;

    private async Task OnCategoryChanged(string newCategory)
    {
        SelectedCategory = newCategory;
        if (SelectedCategoryChanged.HasDelegate)
            await SelectedCategoryChanged.InvokeAsync(newCategory);
    }

    private void ToggleSearch() => _isSearchOpen = !_isSearchOpen;

    private async Task TriggerSearchBusyAsync()
    {
        // отменяем предыдущее «мигание», чтобы не наслаивалось
        await _searchCts?.CancelAsync()!;
        _searchCts?.Dispose();
        var cts = new CancellationTokenSource();
        _searchCts = cts;

        _isSearchBusy = true;
        StateHasChanged();

        try
        {
            await Task.Delay(SearchBusyMs, cts.Token);
        }
        catch (TaskCanceledException)
        {
            // игнорируем
        }

        if (!cts.IsCancellationRequested)
        {
            _isSearchBusy = false;
            StateHasChanged();
        }
    }
}
