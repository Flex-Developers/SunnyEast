using Microsoft.AspNetCore.Components;

namespace Client.Layout;

public partial class CustomerLayout
{
    [Parameter] public RenderFragment ChildContent { get; set; } = null!;
}