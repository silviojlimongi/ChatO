using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using ChatO.Models;
using System.Text;

namespace ChatO.Services                            
{
    public class OpenAiCompatClient : IDisposable // Cliente HTTP compatible con el esquema OpenAI (también sirve para Groq)
    {
        private readonly HttpClient _http;        // Instancia de HttpClient reutilizable para las llamadas
        private readonly string _model;           // Nombre del modelo a usar (desde configuración)

        public OpenAiCompatClient()               // Constructor: inicializa HttpClient y configura base URL/headers
        {
            var baseUrl = ConfigurationManager.AppSettings["OpenAI_BaseUrl"]?.TrimEnd('/');
            //  Lee la base URL de appSettings (p.ej., https://api.groq.com/openai/v1) y quita el '/' final

            var apiKey = ConfigurationManager.AppSettings["OpenAI_ApiKey"];
            //  Lee la API key (si la guardaste en Web.config). Recomendado: usar variable de entorno y dejar aquí fallback.

            _model = ConfigurationManager.AppSettings["OpenAI_Model"] ?? "gpt-5";
            //  Modelo por defecto si no está configurado (se puede ajustar según proveedor, p.ej. Llama en Groq)

            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException("OpenAI/Groq base URL y/o API key no están configurados.");
            //  Valida que baseUrl exista. (El mensaje menciona también API key para guiar el diagnóstico)

            _http = new HttpClient { BaseAddress = new Uri(baseUrl + "/") };
            // Fija BaseAddress. OJO: aquí ya queda con '/': si baseUrl termina en '/v1', ahora tienes '/v1/'

            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
                //  Si hay API key en config, añade el header Authorization Bearer
            }
        }

        public async Task<string> CreateChatCompletionAsync(IList<ChatTurn> turns, double temperature = 0.7)
        {
            var req = new
            {
                model = _model,                 // Modelo a usar
                messages = Map(turns),          // Historial transformado al formato {role, content}
                temperature = temperature       // Parámetro de creatividad/control de aleatoriedad
            };

            var json = JsonConvert.SerializeObject(req); // Serializa el payload a JSON

            var content = new StringContent(json, Encoding.UTF8, "application/json"); //  Contenido HTTP con tipo application/json

            using (var resp = await _http.PostAsync("v1/chat/completions", content).ConfigureAwait(false))
            // ^ Realiza POST al endpoint. ATENCIÓN:
            //   Si BaseAddress ya es .../v1/ (por ejemplo en Groq), aquí se duplicará 'v1' → .../v1/v1/chat/completions.
            //   En ese caso, la ruta correcta debería ser solo "chat/completions".
            {
                var body = await resp.Content.ReadAsStringAsync().ConfigureAwait(false);// Lee el cuerpo de la respuesta como string

                if (!resp.IsSuccessStatusCode)
                {
                    throw new InvalidOperationException($"Error {resp.StatusCode}: {body}"); // Propaga un error con el detalle devuelto por la API
                }

                dynamic data = JsonConvert.DeserializeObject(body); // Deserializa dinámicamente para acceder a campos de la respuesta

                // OpenAI-compatible: choices[0].message.content
                try
                {
                    return (string)data.choices[0].message.content; //  Ruta estándar del contenido en Chat Completions
                }
                catch
                {
                    // Algunos proveedores/dev modes usan 'delta' (streaming) u otra estructura
                    try { return (string)data.choices[0].delta.content; }
                    catch { return body; } // Último recurso: devuelve el JSON completo crudo
                }
            }
        }

        private static List<object> Map(IList<ChatTurn> turns)
        {
            var list = new List<object>(); // Lista destino para los mensajes en el formato requerido por la API

            if (turns == null) return list; // Si no hay historial, devuelve lista vacía

            foreach (var t in turns)
            {
                if (string.IsNullOrWhiteSpace(t?.Role) || string.IsNullOrWhiteSpace(t?.Content)) continue; // Ignora elementos nulos o sin datos

                list.Add(new { role = t.Role, content = t.Content }); // Añade mensaje en el formato { role, content }
            }
            return list;
        }

        public void Dispose() => _http?.Dispose(); // Libera los recursos de HttpClient cuando se desecha el cliente
    }
}
