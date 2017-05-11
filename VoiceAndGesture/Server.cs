using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Microsoft.Samples.Kinect.SpeechBasics
{

    public class Server
    {

        private static Socket s1;
        private static TcpListener myList1;
        private static TcpClient tcpclnt1;

        private static Socket s2;
        private static TcpListener myList2;
        private static TcpClient tcpclnt2;



        private static void client1()
        {
            IPAddress ipAd = IPAddress.Parse("192.168.0.172");

            /* Initializes the Listener */
            myList1 = new TcpListener(ipAd, 9513);

            /* Start Listeneting at the specified port */
            myList1.Start();
            
            Console.WriteLine("The local End point is  :" + myList1.LocalEndpoint);
            Console.WriteLine("Waiting for a connection.....");

            s1 = myList1.AcceptSocket();
            tcpclnt1 = myList1.AcceptTcpClient();
            Console.WriteLine("Connection accepted from " + s1.RemoteEndPoint);
        }

        private static void client2()
        {
            IPAddress ipAd = IPAddress.Parse("192.168.0.172");

            /* Initializes the Listener */
            myList2 = new TcpListener(ipAd, 1248); /*port for client 2*/

            /* Start Listeneting at the specified port */
            myList2.Start();

            Console.WriteLine("The local End point is  :" + myList2.LocalEndpoint);
            Console.WriteLine("Waiting for a connection.....");

            s2 = myList2.AcceptSocket();
            tcpclnt2 = myList2.AcceptTcpClient();
            Console.WriteLine("Connection accepted from " + s2.RemoteEndPoint);
        }

        public static void destructClient1()
        {
            s1.Close();
            myList1.Stop();
            tcpclnt1.Close();
        }
        public static void destructClient2()
        {
            s2.Close();
            myList2.Stop();
            tcpclnt2.Close();
        }


        public static void sendClient1(String str)
        {/*
            ASCIIEncoding asen = new ASCIIEncoding();
            Stream stm = tcpclnt1.GetStream();

            byte[] ba = asen.GetBytes(str);
            Console.WriteLine("Transmitting.....");

            stm.Write(ba, 0, ba.Length);*/
        }

        public static void sendClient2(String str)
        {
            /*Stream stm = tcpclnt2.GetStream();
            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes(str);
            Console.WriteLine("Transmitting.....");

            stm.Write(ba, 0, ba.Length);*/
        }


        public static void StartListening()
        {
            Console.WriteLine("Listener");
            // This constructor arbitrarily assigns the local port number.
            try
            {
                 //client1();
                 //client2();

            }
            catch (Exception e)
            {
                Console.WriteLine("Error..... " + e.StackTrace);
            }
        }
    }

}
