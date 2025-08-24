using System;                                  
using System.Collections.Generic;               // Colecciones genéricas como List<T>
using System.Linq;                              
using System.Web;                               
using System.Web.Mvc;                           
using System.ComponentModel.DataAnnotations;    // Data Annotations (para atributos como [Display])

namespace ChatO.Models                          
{
    public class ChatVm                         // ViewModel para la vista de chat (transporta datos entre Controlador y Vista)
    {
        public ChatVm()                         // Constructor por defecto
        {
            Turns = new List<ChatTurn>();       // Inicializa la lista para evitar null para usarla en la vista/controlador
        }

        public List<ChatTurn> Turns { get; set; } // Historial de la conversación (cada elemento es un mensaje/turno)

        [Display(Name = "Tu mensaje")]          // Etiqueta amigable en el campo en la vista
        public string Input { get; set; }       // Texto que escribe el usuario

        public bool IsBusy { get; set; }        // Mientras se espera respuesta de la API
        public string Error { get; set; }       // Mensaje de error a mostrar si falla la API
    }
}