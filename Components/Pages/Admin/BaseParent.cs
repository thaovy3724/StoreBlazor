using Microsoft.AspNetCore.Components;
<<<<<<< HEAD
using Microsoft.JSInterop;
using StoreBlazor.DTO.Admin;
=======
>>>>>>> 4589f5fe6f438f0040c39c4ed8666a379b6a96e5

namespace StoreBlazor.Components.Pages.Admin
{
    public abstract class BaseParent<T> : ComponentBase
        where T : class, new()
    {
        protected T SelectedItem { get; set; } = new();

        protected bool IsEditMode { get; set; }
<<<<<<< HEAD

        [Inject]
        protected IJSRuntime JS { get; set; } = default!;
=======
>>>>>>> 4589f5fe6f438f0040c39c4ed8666a379b6a96e5
        protected void CloseForm()
        {
            SelectedItem = new();
            IsEditMode = false;
        }

<<<<<<< HEAD
        protected async Task ShowAlertAsync(ServiceResult serviceResult)
        {
            await JS.InvokeVoidAsync("showAlert", serviceResult.Message, serviceResult.Type);
        }

        protected async Task<bool> ConfirmDeleteAsync()
        {
            return await JS.InvokeAsync<bool>("showDeleteAlert");
        }

=======
>>>>>>> 4589f5fe6f438f0040c39c4ed8666a379b6a96e5
    }
}
