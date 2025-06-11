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

namespace ServidorPollitos.Services
{
    public class TableroPollitoServices
    {
            TcpListener serverbb;
            List<TcpClient> clientsPollito = new();

            public TableroPollitoServices()
            {
                serverbb = new TcpListener(IPAddress.Any, 1745);
                serverbb.Start();
                Thread pluma = new Thread(AceptarPeticionesPollitos);
                pluma.IsBackground = true;
                pluma.Start();
            }

            void AceptarPeticionesPollitos()
            {
                try
                {
                    while (true)
                    {
                        TcpClient clientebb = serverbb.AcceptTcpClient();
                        clientsPollito.Add(clientebb);
                        Thread hilitos = new Thread(EscucharPollito);
                        hilitos.IsBackground = true;
                        hilitos.Start(clientebb);
                        Thread.Sleep(1000);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            public event Action<PollitoDTO>? PollitoRecibido;
            public event Action<string>? PollitoDesconectado;

            void EscucharPollito(object? client)
            {
                if (client is TcpClient cliente)
                {
                    StringBuilder acumulador = new();
                    byte[] buffer = new byte[1024];

                    while (cliente.Connected)
                    {
                        try
                        {
                            int bytesRead = cliente.Client.Receive(buffer);
                            if (bytesRead > 0)
                            {
                                string recibido = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                                acumulador.Append(recibido);

                                string[] mensajes = acumulador.ToString().Split('\n');

                                for (int i = 0; i < mensajes.Length - 1; i++)
                                {
                                    try
                                    {
                                        var pollo = JsonSerializer.Deserialize<PollitoDTO>(mensajes[i]);
                                        if (pollo != null)
                                        {
                                            PollitoRecibido?.Invoke(pollo);
                                        }
                                    }
                                    catch (JsonException ex)
                                    {
                                        Debug.WriteLine($"Error de deserialización: {ex.Message}");
                                    }
                                }

                                acumulador.Clear();
                                acumulador.Append(mensajes.Last());
                            }
                        }
                        catch (SocketException) { }
                    }
                }
            }

            public void Retransmitir(PollitoDTO dto)
            {
                string json = JsonSerializer.Serialize(dto) + "\n";
                byte[] buffer = Encoding.UTF8.GetBytes(json);

                foreach (var b in clientsPollito.ToList())
                {
                    try
                    {
                        if (b.Connected)
                        {
                            b.Client.Send(buffer);
                        }
                    }
                    catch (Exception)
                    {
                        if (b.Client.RemoteEndPoint != null)
                        {
                            PollitoDesconectado?.Invoke(b.Client.RemoteEndPoint?.ToString() ?? "");
                            b.Client.Close();
                            clientsPollito.Remove(b);
                        }
                    }
                }
            }

            public void Retransmitir(PollitoDTO dto, string clin)
            {
                string json = JsonSerializer.Serialize(dto) + "\n";
                byte[] buffer = Encoding.UTF8.GetBytes(json);
                var tcpPollito = clientsPollito.FirstOrDefault(x => x.Client.RemoteEndPoint?.ToString() == clin);
                if (tcpPollito != null)
                {
                    tcpPollito.Client.Send(buffer);
                }
            }
    }
}
