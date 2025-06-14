﻿using ServidorPollitos.Models;
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
            if (client != null)
            {
                TcpClient cliente = (TcpClient)client;
                while (cliente.Connected)
                {
                    if (cliente.Available > 0)
                    {
                        byte[] buffer = new byte[cliente.Available];
                        cliente.Client.Receive(buffer);
                        string json = Encoding.UTF8.GetString(buffer);
                        PollitoDTO? pollo = JsonSerializer.Deserialize<PollitoDTO>(json);
                        if (pollo != null)
                        {
                            PollitoRecibido?.Invoke(pollo);
                        }
                    }
                }
            }
        }
        public void Retransmitir(PollitoDTO dto)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto));

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
        public void Retransmitir(PollitoDTO dto, string cliente)
            {
                byte[] buffer = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto)); ;
                var tcpPollito = clientsPollito.FirstOrDefault(x => x.Client.RemoteEndPoint?.ToString() == cliente);
                if (tcpPollito != null)
                {
                    tcpPollito.Client.Send(buffer);
                }
            }
    }
}
