using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientePollito.Models
{
    public class TAbleroCliente
    {
        public ObservableCollection<PollitoClienteDTO?> CeldasPollito
        { get; set; } = new();
        public TAbleroCliente()
        {
            for (byte i = 0; i < 25; i++)
            {
                CeldasPollito.Add(new PollitoClienteDTO()
                {
                    Posicion = i
                });

            }
        }
    }
}
