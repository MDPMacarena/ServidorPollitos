using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientePollito.Models
{
    public partial class PollitoClienteDTO : ObservableObject
    {
        [ObservableProperty]
        string? nombre;
        [ObservableProperty]
        string? pollito;
        [ObservableProperty]
        byte posicion;

        public string? Cliente { get; set; }
    }
}
