using System;
using System.IO;
using System.Collections.Generic;
using System.Windows.Forms;
using InTheHand.Net;
using InTheHand.Net.Bluetooth;
using InTheHand.Net.Sockets;

public class Device
{
    public string DeviceName { get; set; }
    public BluetoothAddress Address { get; set; }
    public bool Connected { get; set; }

    public Device(BluetoothDeviceInfo device_info)
    {
         this.Address = device_info.DeviceAddress;
         this.Connected = device_info.Connected;
         this.DeviceName = device_info.DeviceName;
    }

    public override string ToString()
    {
        return this.DeviceName;
    }
}

class Program
{
    public static BluetoothClient bc = new BluetoothClient();
    public static BluetoothDeviceInfo[] devicelist = bc.DiscoverDevices();
    public static List<Device> devices = new List<Device>();
    public static Guid myGuid = BluetoothService.SerialPort;
    public static BluetoothAddress targetaddress = BluetoothAddress.Parse("000BCE00D473");
    public static BluetoothEndPoint ep = new BluetoothEndPoint(targetaddress, myGuid);

    static void Main()
    {
        int size = devicelist.Length;
        for (int i = 0; i < size; i++)
        {
            Device device = new Device(devicelist[i]);
            if (device.Address == targetaddress)
            {
                devices.Add(device);
            }
        }
        bc.Connect(ep);
        Stream stream = bc.GetStream();
        foreach (Device d in devices)
        {
            byte[] buffer = new byte[1000];
            int readLen = stream.Read(buffer, 0, buffer.Length);
            Console.Write("Device name: ");
            Console.WriteLine(d.DeviceName);
            Console.Write("Device connected: ");
            Console.WriteLine(d.Connected);
            if (readLen == 0)
            {
                Console.WriteLine("Connection is closed");
            }
            else
            {
                Console.WriteLine("Recevied {0} bytes", readLen);
            }
        }
    }
}
