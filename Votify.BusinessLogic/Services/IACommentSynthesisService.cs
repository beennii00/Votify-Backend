using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Votify.BusinessLogic.Services
{
    public interface IIACommentSynthesisService
    {
        Task<string> ResumirComentariosAsync(int proyectoId, List<string> comentarios);
    }

    public class IACommentSynthesisService : IIACommentSynthesisService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.1-flash-lite-preview:generateContent";

        public IACommentSynthesisService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<string> ResumirComentariosAsync(int proyectoId, List<string> comentarios)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) return "Error interno: API Key no configurada.";

            if (comentarios == null || comentarios.Count == 0)
                return "No hay suficientes comentarios para generar un resumen.";

            var prompt = "Resume las siguientes opiniones de un proyecto en una única frase breve que sintetice la idea general. Requisitos estrictos: NO uses introducciones, agradecimientos ni expresiones iniciales. NO incluyas viñetas, guiones, asteriscos ni texto en negrita. Devuelve exclusivamente la frase del resumen, escrita de forma directa para que tenga sentido al leerse después del texto 'Los usuarios opinan que: '. \nComentarios:\n" + 
                         string.Join("\n- ", comentarios);

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[] { new { text = prompt } }
                    }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_apiUrl}?key={apiKey}", content);

            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                return $"Error con la IA: {response.StatusCode} - {responseString}";

            using var jsonDocument = JsonDocument.Parse(responseString);
            
            try
            {
                return jsonDocument.RootElement
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "Resumen no disponible.";
            }
            catch
            {
                return "Resumen no disponible.";
            }
        }
    }
}