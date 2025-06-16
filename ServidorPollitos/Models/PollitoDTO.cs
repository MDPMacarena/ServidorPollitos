using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServidorPollitos.Models
{
    public partial class PollitoDTO : ObservableObject
    {
        [ObservableProperty]
        string? nombre;
        [ObservableProperty]
        string? pollito;
        [ObservableProperty]
        byte posicion;
        [ObservableProperty]
        int totalsemillas;
        public string? Cliente { get; set; }

    }
}
