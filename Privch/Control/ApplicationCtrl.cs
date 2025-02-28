﻿using System.IO;
using System.IO.Pipes;
using System.Threading.Tasks;
using System.Windows;

namespace Privch.Control
{
    internal static class ApplicationCtrl
    {
        public const string MessageShowHome = "Show-Home";
        private const string pipe_name = "Privch-Pipe";

        private static NamedPipeServerStream pipeServer;
        private static StreamReader streamReader;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public static void Initialize()
        {
            pipeServer = new NamedPipeServerStream(pipe_name, PipeDirection.In);
            streamReader = new StreamReader(pipeServer);

            Task.Run(() =>
            {
                while (true)
                {
                    // accept connection
                    try
                    {
                        pipeServer.WaitForConnection();
                    }
                    catch
                    {
                        break;
                    }

                    // process message
                    while (true)
                    {
                        string message = streamReader.ReadLine();
                        if (string.IsNullOrWhiteSpace(message))
                        {
                            pipeServer.Disconnect();
                            break;
                        }
                        else if (message == MessageShowHome)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                App.ShowMainWindow();
                            });
                        }
                    }
                }
            });
        }

        public static void Dispose()
        {
            if (pipeServer.IsConnected)
            {
                pipeServer.Disconnect();
            }

            pipeServer.Close();
            pipeServer.Dispose();

            streamReader.Dispose();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "<Pending>")]
        public static void Send(string message)
        {
            NamedPipeClientStream pipeClient = new NamedPipeClientStream(".", pipe_name, PipeDirection.Out);
            StreamWriter streamWriter = new StreamWriter(pipeClient);

            try
            {
                pipeClient.Connect();
                streamWriter.WriteLine(message);
                streamWriter.Flush();
            }
            catch { }
            finally
            {
                streamWriter.Close();
                streamWriter.Dispose();
            }
        }
    }
}
