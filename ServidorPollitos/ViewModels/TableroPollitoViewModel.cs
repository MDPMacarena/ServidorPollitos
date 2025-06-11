using ServidorPollitos.Models;
using ServidorPollitos.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using System.Windows;


namespace ServidorPollitos.ViewModels
{
    public partial class TableroPollitoViewModel
    {
        public TableroPollito Tablero { get; set; } = new();
        TableroPollitoServices server = new();
        public TableroPollitoViewModel()
        {
            server.PollitoDesconectado += Services_PollitoDesconectado;
            server.PollitoRecibido += Services_PollitoRecibido;
        }
        private void Services_PollitoRecibido(PollitoDTO dTO)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var pollitoTablero = Tablero.CeldasPollito.FirstOrDefault(x => x != null && x.Cliente == dTO.Cliente);
                if (pollitoTablero == null)
                {
                    if (Tablero.CeldasPollito[dTO.Posicion] == null)
                    {
                        Tablero.CeldasPollito[dTO.Posicion] = dTO;
                    }
                    else
                    {
                        for (byte i = 0; i < Tablero.CeldasPollito.Count; i++)
                        {
                            if (Tablero.CeldasPollito[i] == null)
                            {
                                Tablero.CeldasPollito[i] = dTO;
                                dTO.Posicion = i;
                                break;
                            }
                        }
                    }
                    server.Retransmitir(dTO);
                    foreach (var item in Tablero.CeldasPollito)
                    {
                        if (item != null && item != dTO)
                        {
                            server.Retransmitir(item, dTO.Cliente ?? "");
                        }
                    }
                }
                else
                {
                    if (dTO.Posicion != pollitoTablero.Posicion)
                    {
                        if (Tablero.CeldasPollito[dTO.Posicion] == null)
                        {
                            Tablero.CeldasPollito[dTO.Posicion] = pollitoTablero;
                            Tablero.CeldasPollito[pollitoTablero.Posicion] = null;
                            pollitoTablero.Posicion = dTO.Posicion;
                        }
                        else
                        {
                            return;
                        }
                    }
                    pollitoTablero.Pollito = dTO.Pollito;
                    server.Retransmitir(dTO);
                }
            });
        }
        private void Services_PollitoDesconectado(string idcliente)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var pollitoTablero = Tablero.CeldasPollito.FirstOrDefault(x => x != null && x.Cliente == idcliente);
                if (pollitoTablero != null)
                {
                    Tablero.CeldasPollito[pollitoTablero.Posicion] = null;
                    pollitoTablero.Posicion = 255;
                    server.Retransmitir(pollitoTablero);
                }
            });
        }
    }
}
