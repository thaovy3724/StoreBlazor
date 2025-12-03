using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using StoreBlazor.DTO.Admin;

namespace StoreBlazor.Components.Pages.Admin
{
    public abstract class BaseParent<T> : ComponentBase
        where T : class, new()
    {
        protected T SelectedItem { get; set; } = new();

        protected bool IsEditMode { get; set; }

        protected bool IsDetailMode { get; set; }


        [Inject]
        protected IJSRuntime JS { get; set; } = default!;

        // ==== MODAL ====
        protected void CloseForm()
        {
            SelectedItem = new();
            IsEditMode = false;
            IsDetailMode = false;
        }

        protected async Task ShowAlertAsync(ServiceResult serviceResult)
        {
            await JS.InvokeVoidAsync("showAlert", serviceResult.Message, serviceResult.Type);
        }

        protected async Task<bool> ConfirmDeleteAsync()
        {
            return await JS.InvokeAsync<bool>("showDeleteAlert");
        }

        // ==== PAGINATION ====
        protected int CurrentPage;
        protected int TotalPages;

        protected abstract Task LoadPageAsync(int page);

        protected async Task<bool> ConfirmLockAsync(string title)
        {
            return await JS.InvokeAsync<bool>("showLockAlert",title);
        }
    }
}
