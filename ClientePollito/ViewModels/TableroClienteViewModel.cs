using ClientePollito.Models;
using ClientePollito.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ClientePollito.ViewModels
{
    public enum Vistas { Conexion,Tablero,Modificar}
    public partial class TableroClienteViewModel : ObservableObject
    {
        [ObservableProperty]
        PollitoClienteDTO pollo = new();

        [ObservableProperty]
        TAbleroCliente tablerobb = new();

        [ObservableProperty]
        Vistas vistaactiva = Vistas.Conexion;

        [ObservableProperty]
        ConexionPollito conexion = new();

        [ObservableProperty]
        string titulo = "Pollitos en fuga";

        [ObservableProperty]
        string error = "";

        TableroCLienteServices pollitocliente = new();
        public string[] Pollitos { get; set; }= new string[]
        { "🐣","🐥","🐥"};
        public ICommand ConectarCommand { get; set;}
        public ICommand CambiarPosicionCommand { get; set; }
        public ICommand CancelarCommand { get; set; }
        public ICommand ModificarCommand { get; set; }
        public TableroClienteViewModel()
        {
            ConectarCommand = new RelayCommand(Conectar);
            CambiarPosicionCommand = new RelayCommand<byte>(CambiarPosicion);
            CancelarCommand = new RelayCommand(Cancelar);
            ModificarCommand = new RelayCommand(Modificar);
            pollitocliente.PollitoRecibido += pollitocliente_PollitoRecibido;
            pollitocliente.PollitoDesconectado += pollitocliente_PollitoDesconectado;
        }

        private void pollitocliente_PollitoDesconectado(string obj)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var polloTablero = Tablerobb.CeldasPollito.FirstOrDefault(p=> p != null && p.Cliente == obj);
                if (polloTablero != null)
                {
                    Tablerobb.CeldasPollito[polloTablero.Posicion] = null;
                }
            });
        }
        private void pollitocliente_PollitoRecibido(PollitoClienteDTO dTO)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
               var polloTablero = Tablerobb.CeldasPollito.FirstOrDefault(p => p != null && p.Cliente == dTO.Cliente);
                if (polloTablero == null)
                {
                    Tablerobb.CeldasPollito[dTO.Posicion] = dTO;
                }
                else
                {
                    if (dTO.Posicion != polloTablero.Posicion)
                    {
                        Tablerobb.CeldasPollito[polloTablero.Posicion] = new()
                        {
                            Posicion = polloTablero.Posicion
                        };
                        Tablerobb.CeldasPollito[dTO.Posicion] = polloTablero;
                        polloTablero.Posicion = dTO.Posicion;
                    }
                    polloTablero.Pollito = dTO.Pollito;
                    polloTablero.Posicion = dTO.Posicion;
                    //polloTablero.Posicion = dTO.Posicion;
                }
            });
        }
        private void Modificar()
        {
            Error = "";
            pollitocliente.Enviar(Pollo);
            Vistaactiva = Vistas.Tablero;
        }
        private void Cancelar()
        {
            Vistaactiva = Vistas.Tablero;
        }
        private void CambiarPosicion(byte posicion)
        {
            Pollo.Posicion = posicion;
            Vistaactiva = Vistas.Modificar;
        }
        private void Conectar()
        {
            Error = "";
            bool valido = IPAddress.TryParse(Conexion.Ip, out IPAddress? ip);
            if (!valido) 
            {
                Error = "IP no valida";
            }
            if (string.IsNullOrWhiteSpace(Conexion.Nombre))
            {
                Error = "Nombre no valido";
            }
            if (Error == "")
            {
                Pollo.Nombre = Conexion.Nombre;
                Pollo.Posicion = 0;
                Pollo.Pollito = Conexion.Pollito;
                if (pollitocliente.Conectar(Conexion.Ip ?? ""))
                {
                    Titulo = $"Pollo cliente - {Conexion.Nombre}";
                    pollitocliente.Enviar(Pollo);
                    Vistaactiva = Vistas.Tablero;
                }
                else
                {
                    Error = "No se pudo conectar al servidor";
                }
            }
        }
    }
}
