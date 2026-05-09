using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using MudBlazor;

namespace TKC_Cloud.Web.Services;

public class GlobalErrorService : IAsyncDisposable
{
    private readonly ISnackbar _snackbar;
    private readonly IJSRuntime _jsRuntime;
    private readonly NavigationManager _navigation;

    private DotNetObjectReference<GlobalErrorService>? _objRef;

    public GlobalErrorService(
        ISnackbar snackbar,
        IJSRuntime jsRuntime,
        NavigationManager navigation)
    {
        _snackbar = snackbar;
        _jsRuntime = jsRuntime;
        _navigation = navigation;
    }

    public async Task InitializeAsync()
    {
        _objRef = DotNetObjectReference.Create(this);

        await _jsRuntime.InvokeVoidAsync(
            "globalErrorHandler.register",
            _objRef);
    }

    [JSInvokable]
    public void HandleError(string message)
    {
        _snackbar.Add(
            $"Fehler: {message}",
            Severity.Error,
            config =>
            {
                config.RequireInteraction = true;
                config.ShowCloseIcon = true;
                config.VisibleStateDuration = 10000;

                config.Action = "Neu laden";

                config.OnClick = _ =>
                {
                    _navigation.NavigateTo(
                        _navigation.Uri,
                        forceLoad: true);

                    return Task.CompletedTask;
                };
            });
    }

    [JSInvokable]
    public void HandleDisconnect()
    {
        _snackbar.Add(
            "Die Verbindung zum Server wurde unterbrochen.",
            Severity.Warning,
            config =>
            {
                config.RequireInteraction = true;
                config.ShowCloseIcon = true;
            });
    }

    [JSInvokable]
    public void HandleReconnect()
    {
        _snackbar.Add(
            "Verbindung wiederhergestellt.",
            Severity.Success,
            config =>
            {
                config.ShowCloseIcon = true;
            });
    }

    public async ValueTask DisposeAsync()
    {
        _objRef?.Dispose();
    }
}