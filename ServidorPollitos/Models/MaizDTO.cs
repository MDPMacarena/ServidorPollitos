using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServidorPollitos.Models
{
    public partial class MaizDTO : ObservableObject
    {
        [ObservableProperty]
        byte posicion;
        [ObservableProperty]
        DateTime tiempo;
        [ObservableProperty]
        string? semillitas;
        public string? Smillita { get; set; }
    }
}
