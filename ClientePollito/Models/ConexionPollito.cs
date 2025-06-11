using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClientePollito.Models
{
    public partial class ConexionPollito : ObservableObject
    {
        [ObservableProperty]
        private string? ip = "127.0.0.1";
        [ObservableProperty]
        private string? nombre;
        [ObservableProperty]
        private string? pollito;
    }
}
