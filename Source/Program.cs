using System;
using Vortice.XInput;
using System.Runtime.InteropServices;

namespace PadMouse
{
    public class Win32
    {
        [DllImport("User32.Dll")]
        public static extern bool GetCursorPos(out POINT point);

        [DllImport("User32.Dll")]
        public static extern long SetCursorPos(int x, int y);

        [DllImport("User32.Dll")]
        public static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);

        [DllImport("user32.dll", CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint cButtons, uint dwExtraInfo);
        //Mouse actions
        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;
        public const int MOUSEEVENTF_RIGHTDOWN = 0x08;
        public const int MOUSEEVENTF_RIGHTUP = 0x10;

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;

            public POINT(int X, int Y)
            {
                x = X;
                y = Y;
            }
        }
    }

    class Program
    {
        private const int Deadzone = 10000;

        private static float GetVector(short raw,short deadzone,float max)
        {
            float result = 0.0f;

            if(raw > deadzone)
            {
                result = (max * ((float)raw - (float)deadzone)) / (32767.0f - (float)deadzone);
            }
            else if(raw < -deadzone)
            {
                result = (max * ((float)raw + (float)deadzone)) / (32768.0f - (float)deadzone);
            }
            return result;
        }

        private static (float,float) NoDeadZone(float rawX, float rawY, float deadzone)
        {
            double rawlen = Math.Sqrt(rawX * rawX + rawY * rawY);
            if(rawlen > deadzone)
            {
                return (rawX / 32768.0f, rawY / 32768.0f);
            }
            return (0, 0);
        }

        static void Main(string[] args)
        {
            XInput.GetCapabilities(0, DeviceQueryType.Gamepad, out var capabilities);
            //Console.WriteLine($"{capabilities}");
            //Console.ReadLine();

            // pos = Cursor.
            //System.Windows.Input.

            bool prevLeft = false;
            bool prevRight = false;

            float posx = 0.0f;
            float posy = 0.0f;

            while(true)
            {
                //Cursor.
                XInput.GetState(0, out var state);
                //Console.WriteLine($"{state.PacketNumber} : {state.Gamepad}");

                (float cookedX, float cookedY) = NoDeadZone(state.Gamepad.LeftThumbX, state.Gamepad.LeftThumbY, Deadzone);
                Console.WriteLine($"{cookedX} {cookedY}");

                float vectorx = (0.5f + (state.Gamepad.RightTrigger / 255.0f)) * 10.0f * cookedX;
                float vectory = (0.5f + (state.Gamepad.RightTrigger / 255.0f)) * 10.0f * cookedY;

                bool left = (state.Gamepad.Buttons & GamepadButtons.A) == GamepadButtons.A;
                bool right = (state.Gamepad.Buttons & GamepadButtons.B) == GamepadButtons.B;

                if(left != prevLeft)
                {
                    if(left)
                    {
                        Win32.mouse_event(Win32.MOUSEEVENTF_LEFTDOWN, 0, 0, 0, 0);
                    }
                    else
                    {
                        Win32.mouse_event(Win32.MOUSEEVENTF_LEFTUP, 0, 0, 0, 0);
                    }
                }

                if (right != prevRight)
                {
                    if (left)
                    {
                        Win32.mouse_event(Win32.MOUSEEVENTF_RIGHTDOWN, 0, 0, 0, 0);
                    }
                    else
                    {
                        Win32.mouse_event(Win32.MOUSEEVENTF_RIGHTUP, 0, 0, 0, 0);
                    }
                }

                prevLeft = left;
                prevRight = right;

                //Console.WriteLine($"{state.Gamepad.Buttons} {vectorx} {vectory} {left} {right}");

                Win32.GetCursorPos(out var pos);

                if( pos.x != Math.Round(posx) ||
                    pos.y != Math.Round(posy) )
                {
                    posx = pos.x;
                    posy = pos.y;
                }

                posx = posx + vectorx;
                posy = posy - vectory;

                Win32.SetCursorPos((int)Math.Round(posx), (int)Math.Round(posy));

                System.Threading.Thread.Sleep(5);
            }

            Console.ReadLine();
            

        }
    }
}
