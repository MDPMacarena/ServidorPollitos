using ServidorPollitos.Models;
using ServidorPollitos.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Diagnostics;


namespace ServidorPollitos.ViewModels
{
    public partial class TableroPollitoViewModel : ObservableObject
    {
        public TableroPollito Tablero { get; set; } = new();
        TableroPollitoServices server = new();
        public TableroPollitoViewModel()
        {
            server.PollitoDesconectado += Services_PollitoDesconectado;
            server.PollitoRecibido += Services_PollitoRecibido;
            server.MaizGenerado += Services_MaizGenerado;
            server.SemillaEliminada += Services_SemillaEliminada;

        }
        private void Services_MaizGenerado(MaizDTO dto)
        {

            Application.Current.Dispatcher.Invoke(() =>
            {
                var existente = Tablero.SemillasCorral.FirstOrDefault(m => m.Posicion == dto.Posicion);
                if (existente != null)
                    Tablero.SemillasCorral.Remove(existente);

                Tablero.SemillasCorral.Add(dto);
            });

        }
        private void Services_PollitoRecibido(PollitoDTO dto)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    var pollitoEnTablero = Tablero.CeldasPollito.FirstOrDefault(x => x != null && x.Cliente == dto.Cliente);

                    if (pollitoEnTablero == null)
                    {
                        if (Tablero.CeldasPollito[dto.Posicion] == null)
                        {
                            Tablero.CeldasPollito[dto.Posicion] = dto;
                        }
                        else
                        {
                            for (byte i = 0; i < Tablero.CeldasPollito.Count; i++)
                            {
                                if (Tablero.CeldasPollito[i] == null)
                                {
                                    Tablero.CeldasPollito[i] = dto;
                                    dto.Posicion = i;
                                    break;
                                }
                            }
                        }

                        server.Retransmitir(dto);

                        foreach (var item in Tablero.CeldasPollito)
                        {
                            if (item != null && item != dto)
                            {
                                server.Retransmitir(item, dto.Cliente ?? "");
                            }
                        }
                    }
                    else
                    {
                        if (dto.Posicion != pollitoEnTablero.Posicion)
                        {
                            if (Tablero.CeldasPollito[dto.Posicion] == null)
                            {
                                Tablero.CeldasPollito[dto.Posicion] = pollitoEnTablero;
                                Tablero.CeldasPollito[pollitoEnTablero.Posicion] = null;
                                pollitoEnTablero.Posicion = dto.Posicion;

                                var semilla = Tablero.SemillasCorral.FirstOrDefault(s => s.Posicion == dto.Posicion);
                                if (semilla != null)
                                {
                                    pollitoEnTablero.Totalsemillas++;
                                    Tablero.SemillasCorral.Remove(semilla);
                                    //server.SemillaEliminada?.Invoke(semilla.Posicion);
                                    server.Retransmitir(pollitoEnTablero);
                                }

                            }
                            else
                            {
                                return;
                            }
                        }

                        pollitoEnTablero.Pollito = dto.Pollito;

                        server.Retransmitir(dto);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Error en Services_PollitoRecibido: {ex.Message}");
                }
            });
        }
        private void Services_PollitoDesconectado(string idcliente)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var pollitoEnTablero = Tablero.CeldasPollito.FirstOrDefault(x => x != null && x.Cliente == idcliente);

                if (pollitoEnTablero != null)
                {
                    Tablero.CeldasPollito[pollitoEnTablero.Posicion] = null;
                    pollitoEnTablero.Posicion = 255;
                    server.Retransmitir(pollitoEnTablero);
                }
            });
        }
        private void Services_SemillaEliminada(byte posicion)
        {
            Debug.WriteLine($"Eliminando semilla en posición {posicion}");

            Application.Current.Dispatcher.Invoke(() =>
            {
                var semilla = Tablero.SemillasCorral.FirstOrDefault(m => m.Posicion == posicion);
                if (semilla != null)
                {
                    Tablero.SemillasCorral.Remove(semilla);
                }
            });
        }
    }
}
