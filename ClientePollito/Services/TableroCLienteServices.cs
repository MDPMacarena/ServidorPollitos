using ClientePollito.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ClientePollito.Services
{
    public  class TableroCLienteServices
    {
        TcpClient? clientepollito;
        public bool Conectar(string serveIp)
        {
            try
            {
                clientepollito = new();
                var endpoint = new IPEndPoint(IPAddress.Parse(serveIp), 1745);
                clientepollito.Connect(endpoint);
                Thread pluma = new(Escuchar);
                pluma.IsBackground = true;
                pluma.Start();
                return true;
            }
            catch (SocketException) { return false; }
        }
        public void Desconectar()
        {
            if (clientepollito != null)
            {
                clientepollito.Close();
                clientepollito = null;
            }
        }
        public void Enviar(PollitoClienteDTO pollitos)
        {
            try
            {
                if (clientepollito != null)
                {
                    IPEndPoint? localPollito = clientepollito.Client.LocalEndPoint as IPEndPoint;
                    if (localPollito != null) 
                    {
                        string direccion = $"{localPollito.Address.MapToIPv4()}:{localPollito.Port}";
                        pollitos.Cliente = direccion;
                        var json = JsonSerializer.Serialize(pollitos);
                        byte[] buffer = Encoding.UTF8.GetBytes(json);
                        clientepollito.Client.Send(buffer);
                    }
                }
            }
            catch(Exception ex) { }
        }
        public event Action<PollitoClienteDTO>? PollitoRecibido;
        public event Action<string>? PollitoDesconectado;
        public void Escuchar()
        {
            if (clientepollito != null)
            {
                while (clientepollito.Connected)
                {
                    try
                    {
                        if (clientepollito.Available > 0)
                        {
                            byte[] buffer = new byte[clientepollito.Available];
                            clientepollito.Client.Receive(buffer);
                            string json = Encoding.UTF8.GetString(buffer);
                            PollitoClienteDTO? pollito = JsonSerializer.Deserialize<PollitoClienteDTO>(json);
                            if (pollito != null)
                            {
                                if (pollito.Posicion == 255)
                                {
                                    PollitoDesconectado?.Invoke(pollito.Cliente ?? "");

                                }
                                else
                                {
                                    PollitoRecibido?.Invoke(pollito);
                                }
                            }
                        }
                    }
                    catch (SocketException) { }
                }
            }
        }
    }
}
