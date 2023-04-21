using System;
using TelloSharp;
using SharpDX;
using SharpDX.DirectInput;

namespace TelloJoystick;

class Program
{
    static void Main()
    {
        Console.Clear();
        Console.SetCursorPosition(0,0);
        Console.WriteLine("Please connect to tello drone. Press any key to continue...");
        Console.Read();
        Console.WriteLine("");
        Console.WriteLine("Please make sure joystick is plugged in. Press any key to continue...");
        Console.Read();

        var directInput = new DirectInput();

        var joystickGuid = Guid.Empty;
        
        if (joystickGuid == Guid.Empty)
            foreach (var deviceInstance in directInput.GetDevices(DeviceType.Joystick,
                         DeviceEnumerationFlags.AllDevices))
                joystickGuid = deviceInstance.InstanceGuid;
           
        // If Joystick not found, throws an error
        if (joystickGuid == Guid.Empty)
        {
            Console.WriteLine("No joystick/Gamepad found.");
            Console.ReadKey();
            Environment.Exit(1);
        }
        
        var joystick = new Joystick(directInput, joystickGuid);
        
        var allEffects = joystick.GetEffects();
        foreach (var effectInfo in allEffects)
            Console.WriteLine("Effect available {0}", effectInfo.Name);

        // Set BufferSize in order to use buffered data.
        joystick.Properties.BufferSize = 128;

        // Acquire the joystick
        joystick.Acquire();
        
        Tello drone = new Tello();
        drone.Connect();

        int x = 0;
        int y = 0;
        int z = 0;
        int w = 0;
        
        while (true)
        {
            
            joystick.Poll();
            var datas = joystick.GetBufferedData();
            
            foreach (var state in datas)
            {

                if (Convert.ToString(state.Offset) == "X")
                { 
                    x = Convert.ToInt32((state.Value - 32511) / 325.11); 
                    if (x > 100) x = 100;
                }
                else if (Convert.ToString(state.Offset) == "Y")
                {
                    y = Convert.ToInt32((state.Value - 32511) / 325.11);
                    if (y > 100) y = 100;
                }
                else if (Convert.ToString(state.Offset) == "RotationZ")
                {
                    z = Convert.ToInt32((state.Value - 32511) / 325.11);
                    if (z > 100) z = 100;
                }
                else if (Convert.ToString(state.Offset) == "Buttons4")
                {
                    w = Convert.ToInt32(state.Value);
                    if (w == 128)
                    {
                        w = 40;
                    }
                }
                else if (Convert.ToString(state.Offset) == "Buttons2")
                {
                    w = Convert.ToInt32(state.Value);
                    if (w == 128)
                    {
                        w = -40;
                    }
                }
                else if (Convert.ToString(state.Offset) == "Buttons5")
                {
                        //ws.Send("toggleTrack");
                }
                else if (Convert.ToString(state.Offset) == "Buttons3")
                {
                    drone.Land();
                }
                else if (Convert.ToString(state.Offset) == "PointOfViewControllers0")
                {
                    var val = Convert.ToInt32(state.Value);
                    Tello.FlipDirection? dir = null;
                    if (val == 0)
                    {
                        dir = Tello.FlipDirection.Forward;
                    }
                    else if (val == 27000)
                    {
                        dir = Tello.FlipDirection.Left;
                    }
                    else if (val == 18000)
                    {
                        dir = Tello.FlipDirection.Back;
                    }
                    else if (val == 9000)
                    {
                        dir = Tello.FlipDirection.Right;
                    }
                    if (dir != null)
                    {
                        drone.Flip(dir.Value);
                    }
                }
                Console.WriteLine(state);
                string package = "rc "+Convert.ToString(x)+" "+Convert.ToString(-y)+" "+ Convert.ToString(w)+ " "+ Convert.ToString(z);
                //Console.WriteLine(package);
                drone.SendCommand(package, false);
                    
            }
        }
        
        
    }
}