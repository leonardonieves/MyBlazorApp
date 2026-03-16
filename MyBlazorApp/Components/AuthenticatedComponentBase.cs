using Microsoft.AspNetCore.Components;
using MyBlazorApp.Services;

namespace MyBlazorApp.Components;

public class AuthenticatedComponentBase : ComponentBase
{
    [Inject]
    protected SessionService SessionService { get; set; } = default!;

    [Inject]
    protected NavigationManager Navigation { get; set; } = default!;

    protected override void OnInitialized()
    {
        // Establecer un ID único para este circuito de Blazor Server
        var circuitId = Guid.NewGuid().ToString();
        SessionService.SetCircuitId(circuitId);

        // Verificar si está autenticado
        if (!SessionService.IsAuthenticated())
        {
            Navigation.NavigateTo("/login", true);
        }

        base.OnInitialized();
    }
}
