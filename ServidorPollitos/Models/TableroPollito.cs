using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServidorPollitos.Models
{
    public  class TableroPollito
    {
        public ObservableCollection<PollitoDTO?> CeldasPollito
        { get; set; } = new();
        public TableroPollito()
        {
            for (int i = 0; i < 25; i++)
            {
                CeldasPollito.Add(null);
            }
        }
    }
}
