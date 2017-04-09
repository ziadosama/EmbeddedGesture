/* 
* The purpose of this program is to provide a minimal example of using UDP to 
* send data.
* It transmits broadcast packets and displays the text in a console window.
* This was created to work with the program UDP_Minimum_Listener.
* Run both programs, send data with Talker, receive the data with Listener.
*/
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
class Program
{
    static void Main(string[] args)
    {

        Console.WriteLine("Listener");
        // This constructor arbitrarily assigns the local port number.
        try
        {
            IPAddress ipAd = IPAddress.Parse("192.168.1.166");

            /* Initializes the Listener */
            TcpListener myList = new TcpListener(ipAd, 11000);

            /* Start Listeneting at the specified port */
            myList.Start();

            Console.WriteLine("The server is running at port 11000...");
            Console.WriteLine("The local End point is  :" +
                              myList.LocalEndpoint);
            Console.WriteLine("Waiting for a connection.....");

            Socket s = myList.AcceptSocket();
            TcpClient tcpclnt = myList.AcceptTcpClient();
            Console.WriteLine("Connection accepted from " + s.RemoteEndPoint);

            Console.Write("Enter the string to be transmitted : ");

            String str = Console.ReadLine();
            Stream stm = tcpclnt.GetStream();

            ASCIIEncoding asen = new ASCIIEncoding();
            byte[] ba = asen.GetBytes(str);
            Console.WriteLine("Transmitting.....");

            stm.Write(ba, 0, ba.Length);

            byte[] bb = new byte[100];

            /* clean up */
            s.Close();
            myList.Stop();
            tcpclnt.Close();

        }
        catch (Exception e)
        {
            Console.WriteLine("Error..... " + e.StackTrace);
        }
    } // end of main()
} // end of class Program