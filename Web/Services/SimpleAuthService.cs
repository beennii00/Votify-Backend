using System.Text.Json;
using Microsoft.JSInterop;

namespace Web.Services
{
    public class SimpleAuthService
    {
        private readonly IJSRuntime _jsRuntime;
        private const string AuthKey = "usuarioActivo";

        /// <summary>
        /// Se dispara cuando el estado de autenticación cambia (login, logout, actualización de perfil).
        /// El MainLayout se suscribe a este evento para refrescar el navbar automáticamente.
        /// </summary>
        public event Action? OnAuthStateChanged;

        public SimpleAuthService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task LogInAsync(string userJson)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", AuthKey, userJson);
            OnAuthStateChanged?.Invoke();
        }

        public async Task LogOutAsync()
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", AuthKey);
            OnAuthStateChanged?.Invoke();
        }

        public async Task<string?> GetUsuarioActivoAsync()
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", AuthKey);
        }
    }
}