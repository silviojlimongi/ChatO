using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ChatO.Models             
{
    public class ChatTurn          // Representa un turno/mensaje individual dentro del chat
    {
        public string Role { get; set; }     // Rol del emisor: "system" | "user" | "assistant"
        public string Content { get; set; }  // Texto del mensaje enviado por ese rol
    }
}