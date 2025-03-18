using Microsoft.AspNetCore.Components;

namespace Client.Layout;

public partial class AdminLayout
{
    [Parameter]
    public RenderFragment ChildContent { get; set; } = null!;
}