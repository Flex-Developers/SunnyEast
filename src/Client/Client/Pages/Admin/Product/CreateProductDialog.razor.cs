using Application.Contract.Product.Commands;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Pages.Admin.Product;

public partial class CreateProductDialog
{
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public string CategoryName { get; set; } = string.Empty;

    [Parameter]
    public Action OnSuccessfullyCreate { get; set; } = null!;

    [Parameter]
    public List<string> Categories { get; set; } = null!;

    private List<string> _images = Enumerable.Repeat(string.Empty, 10).ToList();
    private MudForm _form = null!;
    private bool _showTable;

    private readonly CreateProductCommand _command = new()
    {
        Name = "",
        ProductCategorySlug = "",
        Description = "",
    };

    protected override void OnInitialized()
    {
        if (CategoryName == "Все продукты")
            CategoryName = string.Empty;
    }

    private void ShowImageTable()
    {
        _showTable = true;
    }

    private void SaveImagesChanges()
    {
        if (_images.All(string.IsNullOrEmpty)) // if images aren't added
        {
            _showTable = false;
            return;
        }

        _showTable = false;
        _images = SortList(_images);
        _command.Images = _images.ToArray();

        Snackbar.Add("Изображения сохранены", Severity.Success);

        List<string> SortList(List<string> images)
        {
            // Сортирует список на случай, если пользователь не введет данные подряд и оставит строки пустыми 
            for (int i = 0; i < images.Count; i++)
            {
                if (string.IsNullOrEmpty(images[i]))
                {
                    for (int j = i; j < images.Count; j++)
                    {
                        if (!string.IsNullOrEmpty(images[j]))
                        {
                            (images[j], images[i]) = (images[i], images[j]);
                            j = images.Count;
                        }
                    }
                }
            }

            return images;
        }
    }

    private void CancelImagesChanges()
    {
        _images = ((_command.Images?.ToList() ?? Enumerable.Repeat(string.Empty, 10).ToList()));
        _showTable = false;
    }

    private void Cancel()
    {
        MudDialog.Close(DialogResult.Cancel());
    }

    private async Task CreateAsync()
    {
        await _form.Validate();

        if (_form.IsValid)
        {
            _command.ProductCategorySlug = (await CategoryService.GetByName(CategoryName))!.Slug;
            _command.Images =
                _images.Where(i => !string.IsNullOrWhiteSpace(i)).ToArray(); // Избавляемся от пустых ссылок-строк

            var success = await ProductService.Post(_command);
            if (success)
            {
                Snackbar.Add("Продукт успешно создан!", Severity.Success);
                MudDialog.Close();
                OnSuccessfullyCreate();
            }
        }
        else
        {
            Snackbar.Add("Пожалуйста, заполните все обязательные поля правильно!", Severity.Warning);
        }
    }
}