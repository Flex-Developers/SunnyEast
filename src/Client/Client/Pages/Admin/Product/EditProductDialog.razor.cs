using Application.Contract.Product.Commands;
using Microsoft.AspNetCore.Components;
using MudBlazor;

namespace Client.Pages.Admin.Product;

public partial class EditProductDialog
{
    [CascadingParameter]
    MudDialogInstance MudDialog { get; set; } = null!;

    [Parameter]
    public string CategoryName { get; set; } = string.Empty;

    [Parameter]
    public Action OnSuccessfullyCreate { get; set; } = null!;

    [Parameter]
    public List<string> Categories { get; set; } = null!;

    [Parameter]
    public UpdateProductCommand UpdateCommand { get; set; } = null!;

    private List<string> _images;
    private MudForm _form = null!;
    private bool _showTable;

    protected override void OnInitialized()
    {
        _images = UpdateCommand.Images!.ToList();

        // Добавление пустых строк, чтобы было предварительно 10 полей
        _images.AddRange(Enumerable.Repeat(string.Empty, 10 - _images.Count));

        UpdateCommand.Images = _images.ToArray(); // Чтобы при изменении, добавленные пустые строки не исчезли
    }

    private void ShowImageTable()
    {
        _showTable = true;
    }

    private void SaveImagesChanges()
    {
        if (_images.All(string.IsNullOrEmpty)) // if images not added
        {
            _showTable = false;
            return;
        }

        _showTable = false;
        _images = SortList(_images);
        UpdateCommand.Images = _images.ToArray();

        Snackbar.Add("Изображения сохранены", Severity.Success);

        List<string> SortList(List<string> images)
        {
            // Сортирует список на случай если пользователь не введет данные подряд и оставит строки пустыми 
            for (int i = 0; i < images.Count; i++)
            {
                if (string.IsNullOrEmpty(images[i]))
                {
                    for (int j = i; j < images.Count; j++)
                    {
                        if (!string.IsNullOrEmpty(images[j]))
                        {
                            (images[j], images[i]) = (images[i], images[j]); // swap
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
        _images = UpdateCommand.Images?.ToList() ?? new List<string>(Enumerable.Repeat("", 10));
        _showTable = false;
    }

    private void Cancel()
    {
        MudDialog.Cancel();
    }

    private async Task EditAsync()
    {
        await _form.Validate();

        if (_form.IsValid)
        {
            UpdateCommand.ProductCategorySlug = (await CategoryService.GetByName(CategoryName))!.Slug;
            UpdateCommand.Images =
                _images.Where(i => !string.IsNullOrWhiteSpace(i)).ToArray(); // Избавляемся от пустых ссылок-строк

            var success = await ProductService.Put(UpdateCommand);
            if (success)
            {
                Snackbar.Add("Продукт успешно изменен!", Severity.Success);
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