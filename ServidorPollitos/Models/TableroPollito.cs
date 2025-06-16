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
        public ObservableCollection<MaizDTO> SemillasCorral { get; set; } = new();
        public TableroPollito()
        {

            CeldasPollito = new ObservableCollection<PollitoDTO?>();
            for (byte i = 0; i < 25; i++)
            {
                CeldasPollito.Add(null);
            }
            SemillasCorral = new ObservableCollection<MaizDTO>();
        }
    }
}
