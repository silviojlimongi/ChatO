using System;                                   
using System.Collections.Generic;               // List<T> para el historial
using System.Linq;                              
using System.Threading.Tasks;                   // Soporte para async/await
using System.Web;                               
using System.Web.Mvc;                           // MVC: Controller, ActionResult, atributos
using ChatO.Models;                             // Modelos: ChatVm, ChatTurn
using ChatO.Services;                           // Servicio OpenAiCompatClient (cliente HTTP)

namespace ChatO.Controllers                      // Namespace del controlador
{
    public class ChatController : Controller     // Controlador MVC para la página de chat
    {
        private const string SessionKey = "ChatO_Turns"; // Clave para guardar/leer el historial en Session

        // GET: Chat
        [HttpGet]                                // Acción para renderizar la vista inicialmente (o tras GET)
        public ActionResult Index()
        {
            var turns = Session[SessionKey] as List<ChatTurn> ?? new List<ChatTurn>(); // crea una lista vacía

            var vm = new ChatVm { Turns = turns };// ^ Crea el ViewModel y le pasa el historial

            return View(vm);// ^ Devuelve la vista con el ViewModel
        }

        [HttpPost, ValidateAntiForgeryToken]      // Acción que recibe el mensaje del usuario (form POST)
        public async Task<ActionResult> Index(ChatVm vm)
        {
            var turns = Session[SessionKey] as List<ChatTurn> ?? new List<ChatTurn>(); // Lee el historial desde Session (por si ya había mensajes)

            if (!string.IsNullOrWhiteSpace(vm.Input))
            {
                turns.Add(new ChatTurn { Role = "user", Content = vm.Input }); //  Agrega el turno del usuario al historial

                vm.Input = string.Empty; // Limpia la caja de texto para la vista

                try
                {
                    using (var client = new OpenAiCompatClient())
                    {
                        var reply = await client.CreateChatCompletionAsync(turns); // Llama al servicio de IA lo pasa al historial (user/assistant) y espera la respuesta del asistente

                        turns.Add(new ChatTurn { Role = "assistant", Content = reply }); // Agrega la respuesta del asistente al historial
                    }
                }
                catch (Exception ex)
                {
                    vm.Error = $"{ex.Message} | {ex.InnerException?.Message}"; // Captura errores de red/servicio y guarda un mensaje para mostrar en la vista
                }

                Session[SessionKey] = turns; // Actualiza la Session con el historial modificado
            }

            vm.Turns = turns; //  Asegura que el ViewModel tenga el historial actualizado

            return View(vm);
            //  Renderiza la vista (muestra el nuevo mensaje y/o el error)
            //  Nota: Podrías usar PRG (RedirectToAction) para evitar reenvío del formulario en refresh.
        }

        [HttpPost, ValidateAntiForgeryToken]      // Acción para reiniciar la conversación (form POST)
        public ActionResult Reset()
        {
            Session.Remove(SessionKey);//  Elimina el historial almacenado en Session

            return RedirectToAction("Index"); // Redirige al GET Index para mostrar la vista vacía
        }
    }
}