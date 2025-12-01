using Microsoft.AspNetCore.Components;

namespace StoreBlazor.Components.Pages.Admin
{
    public abstract class BaseParent<T> : ComponentBase
        where T : class, new()
    {
        protected T SelectedItem { get; set; } = new();

        protected bool IsEditMode { get; set; }
        protected void CloseForm()
        {
            SelectedItem = new();
            IsEditMode = false;
        }

    }
}
