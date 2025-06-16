using ServidorPollitos.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace ServidorPollitos.Services
{
    public class TableroPollitoServices
    {
        TcpListener serverbb;
        List<TcpClient> clientsPollito = new();
        private Timer timerEliminarMaiz;
        private Timer timerMaiz;
        private Random random = new();
        private List<MaizDTO> semillasActivas = new();
        public TableroPollitoServices()
        {
            serverbb = new(System.Net.IPAddress.Any, 3000);
            serverbb.Start();
            Thread peticiones = new(AceptarPeticionesPollitos);
            peticiones.IsBackground = true;
            peticiones.Start();
            timerMaiz = new Timer(_ => GenerarYEnviarMaiz(), null, 0, 5000);
            timerEliminarMaiz = new Timer(_ => EliminarSemillasViejas(), null, 0, 1000);
        }
        void AceptarPeticionesPollitos()
        {
            try
            {
                while (true)
                {
                    TcpClient tcpClient = serverbb.AcceptTcpClient();
                    clientsPollito.Add(tcpClient);

                    Thread pluma = new(EscucharPollito);
                    pluma.IsBackground = true;
                    pluma.Start(tcpClient);

                    Thread.Sleep(100);
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex.Message);
            }

        }
        public event Action<PollitoDTO>? PollitoRecibido;
        public event Action<string>? PollitoDesconectado;
        public event Action<MaizDTO>? MaizGenerado;
        public event Action<byte>? SemillaEliminada;
        void EscucharPollito(object? cliente)
        {
            if (cliente != null)
            {
                TcpClient clienpollito = (TcpClient)cliente;
                while (clienpollito.Connected)
                {
                    try
                    {
                        if (clienpollito.Available > 0)
                        {
                            byte[] buffer = new byte[clienpollito.Available];
                            clienpollito.Client.Receive(buffer);
                            string json = Encoding.UTF8.GetString(buffer);
                            PollitoDTO? pollito = JsonSerializer.Deserialize<PollitoDTO>(json);

                            if (pollito != null)
                            {
                                PollitoRecibido?.Invoke(pollito);
                            }
                        }
                    }
                    catch
                    {

                    }
                }
            }

        }
        public void Retransmitir(PollitoDTO dto)
        {
            var mensaje = new
            {
                Tipo = "Pollito",
                Datos = dto
            };

            byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(mensaje));

            foreach (var c in clientsPollito.ToList())
            {
                try
                {
                    if (c.Connected)
                    {
                        c.Client.Send(buffer);
                    }
                }
                catch (Exception)
                {
                    if (c.Client.RemoteEndPoint != null)
                    {
                        PollitoDesconectado?.Invoke(c.Client.RemoteEndPoint?.ToString() ?? "");
                        c.Client.Close();
                        clientsPollito.Remove(c);
                    }
                }
            }
        }

        public void Retransmitir(PollitoDTO dto, string cliente)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto));

            var tcpClient = clientsPollito.FirstOrDefault(x => x.Client.RemoteEndPoint?.ToString() == cliente);

            if (tcpClient != null)
            {
                tcpClient.Client.Send(buffer);

            }
        }
        private void GenerarYEnviarMaiz()
        {
            int columna = random.Next(0, 5);
            int fila = random.Next(0, 5);
            byte posicion = (byte)(fila * 5 + columna);
            
            MaizDTO maiz = new()
            {
                Posicion = posicion,
                Tiempo = DateTime.Now,
                Smillita = "🌽"
            };
           
            lock (semillasActivas)
            {
                semillasActivas.Add(maiz);
            }

            MaizGenerado?.Invoke(maiz);
            var mensaje = new
            {
                Tipo = "Maiz",
                Datos = maiz

            };

            byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(mensaje));

            foreach (var c in clientsPollito.ToList())
            {
                try
                {
                    if (c.Connected)
                    {
                        c.Client.Send(buffer);
                    }
                }
                catch
                {
                    if (c.Client.RemoteEndPoint != null)
                    {
                        PollitoDesconectado?.Invoke(c.Client.RemoteEndPoint?.ToString() ?? "");
                        c.Client.Close();
                        clientsPollito.Remove(c);
                    }
                }
            }
        }
        private void EliminarSemillasViejas()
        {
            List<MaizDTO> semillasAEliminar;

            lock (semillasActivas)
            {
                semillasAEliminar = semillasActivas
                .Where(s => (DateTime.Now - s.Tiempo).TotalSeconds >= 10)
                .ToList();

                foreach (var semilla in semillasAEliminar)
                {
                    semillasActivas.Remove(semilla);
                }
            }

            foreach (var semilla in semillasAEliminar)
            {
                var mensajeEliminacion = new
                {
                    Tipo = "EliminarMaiz",
                    Posicion = semilla.Posicion
                };

                byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(mensajeEliminacion));

                foreach (var c in clientsPollito.ToList())
                {
                    try
                    {
                        if (c.Connected)
                        {
                            c.Client.Send(buffer);
                        }
                    }
                    catch
                    {
                        if (c.Client.RemoteEndPoint != null)
                        {
                            PollitoDesconectado?.Invoke(c.Client.RemoteEndPoint?.ToString() ?? "");
                            c.Client.Close();
                            clientsPollito.Remove(c);
                        }
                    }
                }

                SemillaEliminada?.Invoke(semilla.Posicion);
            }
        }
    }
}

