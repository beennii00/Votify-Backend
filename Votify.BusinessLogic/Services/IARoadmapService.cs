using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Domain.Entitites;
using Microsoft.EntityFrameworkCore;
using Persistance.Repositories;
using Shared.DTO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace Votify.BusinessLogic.Services
{
    public class IARoadmapService : IIARoadmapService
    {
        private readonly IDAL _dal;
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly string _apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-3.1-flash-lite-preview:generateContent";


        public IARoadmapService(IDAL dal, HttpClient httpClient, IConfiguration configuration)
        {
            _dal = dal;
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<HojaRutaResponseDto> GenerarHojaDeRutaAsync(int concursanteId)
        {
            var concursanteExiste = await _dal.Query<Usuario>().AnyAsync(c => c.Id == concursanteId);

            if (!concursanteExiste)
            {
                throw new KeyNotFoundException("Concursante no encontrado.");
            }

            // Realizamos la consulta invertida desde Proyectos porque EF Core a veces descarta el Include en navegaciones polimórficas (TPH)
            var proyectosParticipados = await _dal.Query<Proyecto>()
                .Include(p => p.VotosProyecto)
                    .ThenInclude(v => v.Comentarios)
                .Where(p => p.Usuarios.Any(u => u.Id == concursanteId))
                .ToListAsync();

            var datosProyectos = new List<string>();

            foreach (var proyecto in proyectosParticipados)
            {
                var votos = proyecto.VotosProyecto.ToList();
                if (!votos.Any()) continue;

                var promedio = votos.Average(v => v.Valoracion ?? 0);
                var todosComentarios = votos
                    .SelectMany(v => v.Comentarios)
                    .Where(c => !string.IsNullOrWhiteSpace(c.Contenido))
                    .Select(c => c.Contenido)
                    .ToList();

                var notaStr = $"Proyecto: {proyecto.Nombre}. Nota promedio: {promedio.ToString("0.0")}";
                if (todosComentarios.Any())
                {
                    notaStr += $". Comentarios de jurados: {string.Join(" | ", todosComentarios)}";
                }
                datosProyectos.Add(notaStr);
            }

            if (!datosProyectos.Any())
            {
                throw new InvalidOperationException("No hay resultados previos evaluados en tu perfil para generar una hoja de ruta.");
            }

            var markdown = await LlamarIAAsync(datosProyectos);

            return new HojaRutaResponseDto
            {
                Markdown = markdown,
                FechaGeneracion = DateTime.UtcNow
            };
        }

        private async Task<string> LlamarIAAsync(List<string> datosProyectos)
        {
            var apiKey = _configuration["Gemini:ApiKey"];
            if (string.IsNullOrEmpty(apiKey)) return "Error interno: API Key no configurada.";

            var prompt = "Eres un asistente experto en analizar feedback de jurados en competiciones o hackathons de desarrollo de software.\n" +
                         "A continuación te proporciono las valoraciones y comentarios recibidos por un concursante en sus proyectos.\n" +
                         "Genera una Hoja de Ruta personalizada, orientada a su crecimiento. Organiza tus recomendaciones en puntos concretos, utilizando Markdown para dar formato (como títulos en negrita o viñetas).\n" +
                         "Datos recopilados:\n" +
                         string.Join("\n", datosProyectos);

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
                    .GetString() ?? "Hoja de ruta no disponible.";
            }
            catch
            {
                return "Hoja de ruta no disponible debido a un formato no esperado.";
            }
        }
    }
}
