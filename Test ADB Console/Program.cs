using SharpAdbClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Test_ADB_Console
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            AdbServer server = new AdbServer();
            server.StartServer(@"E:\LDPlayer\LDPlayer9\adb.exe", restartServerIfNewer: false);

            AdbClient client = new AdbClient();
            List<DeviceData> devices = client.GetDevices();

            if (devices.Count == 0)
            {
                Console.WriteLine("Không có thiết bị nào được kết nối.");
            }
            else
            {
                Console.WriteLine("Các thiết bị được kết nối:");
                foreach (var device in devices)
                {
                    Console.WriteLine($"- {device.Name}: {device.Model}");
                }

                //// Gọi phương thức để lấy danh sách các ứng dụng
                //List<string> packages = GetInstalledPackages(client, devices[0]);
                //Console.WriteLine("Danh sách các ứng dụng:");
                //foreach (var package in packages)
                //{
                //    Console.WriteLine($"- {package}");
                //}

                // Gửi văn bản
                string textToSend = "Nội dung văn bản bạn muốn gửi";
                SendTextToApp(client, devices[0], textToSend);

                // Lấy độ phân giải của màn hình
                string resolution = GetScreenResolution(client, devices[0]);
                Console.WriteLine($"Độ phân giải của màn hình: {resolution}");

                //TestClickXY(38, 64, client, devices[0]);
            }
            
        }

        static void TestClickXY(int x, int y, AdbClient client, DeviceData devices)
        {
            DeviceData controlDevice = devices;
            string command = $"input tap {x} {y}";

            client.ExecuteRemoteCommand(command, controlDevice, new ConsoleOutputReceiver());

            Console.WriteLine($"Đã click vào tọa độ {x}, {y}.");
        }

        static List<string> GetInstalledPackages(AdbClient client, DeviceData device)
        {
            List<string> packages = new List<string>();

            // Gửi lệnh pm list packages để lấy danh sách các ứng dụng
            string command = "pm list packages";
            var receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand(command, device, receiver);

            // Lọc và thêm tên của các ứng dụng vào danh sách
            string[] lines = receiver.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                string packageName = line.Replace("package:", "").Trim();
                packages.Add(packageName);
            }

            return packages;
        }

        static void SendTextToApp(AdbClient client, DeviceData device, string text)
        {
            // Gửi lệnh input text để nhập văn bản vào ứng dụng cụ thể
            string command = $"input text \"{text}\"";
            client.ExecuteRemoteCommand(command, device, new ConsoleOutputReceiver());

            Console.WriteLine($"Đã gửi văn bản \"{text}\" ");
        }

        static string GetScreenResolution(AdbClient client, DeviceData device)
        {
            // Gửi lệnh wm size để lấy độ phân giải của màn hình
            string command = "wm size";
            var receiver = new ConsoleOutputReceiver();
            client.ExecuteRemoteCommand(command, device, receiver);

            // Lấy độ phân giải từ kết quả trả về
            string[] lines = receiver.ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in lines)
            {
                if (line.StartsWith("Physical size:"))
                {
                    string[] parts = line.Split(':');
                    if (parts.Length == 2)
                    {
                        return parts[1].Trim();
                    }
                }
            }

            return "Không thể lấy độ phân giải.";
        }
    }
}
