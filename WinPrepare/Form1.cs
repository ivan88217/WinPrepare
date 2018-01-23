using System;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace WinPrepare
{
    public partial class Form1 : Form
    {
        private const int WH_KEYBOARD = 2;
        private const int WH_KEYBOARD_LL = 13;

        public delegate int HookProc(int nCode, IntPtr wParam, IntPtr lParam);

        private static int m_HookHandle = 0;    // Hook handle
        private HookProc m_KbdHookProc;            // 鍵盤掛鉤函式指標

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // 設置掛鉤.
        [DllImport("user32.dll", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int SetWindowsHookEx(int idHook, HookProc lpfn,
             IntPtr hInstance, int threadId);

        // 將之前設置的掛鉤移除。記得在應用程式結束前呼叫此函式.
        [DllImport("user32.dll", CharSet = CharSet.Auto,
             CallingConvention = CallingConvention.StdCall)]
        public static extern bool UnhookWindowsHookEx(int idHook);

        // 呼叫下一個掛鉤處理常式（若不這麼做，會令其他掛鉤處理常式失效）.
        [DllImport("user32.dll", CharSet = CharSet.Auto,
            CallingConvention = CallingConvention.StdCall)]
        public static extern int CallNextHookEx(int idHook, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll")]
        private static extern int GetCurrentThreadId();

        public Form1()
        {
            InitializeComponent();
            this.FormBorderStyle = FormBorderStyle.None;
            this.WindowState = FormWindowState.Maximized;
            this.TopMost = true;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            timer1.Enabled = true;
            timer2.Enabled = true;
            HookStart();
        }

        private void Form1_KeyPress(object sender, KeyPressEventArgs e)
        {
        }

        private int key = 0;

        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            switch (key)
            {
                case 0:
                    if (e.KeyCode == Keys.E && e.Control == true)
                    {
                        key = 1;
                    }
                    break;

                case 1:
                    if (e.KeyCode == Keys.S && e.Control == true)
                    {
                        key = 2;
                    }
                    else
                    {
                        key = 0;
                    }
                    break;

                case 2:
                    if (e.KeyCode == Keys.C && e.Control == true)
                    {
                        Close();
                    }
                    else
                    {
                        key = 0;
                    }
                    break;
            }
        }

        private int tm = 0,
            txt_time = 0,
            g = 120,
            b = 215;

        private void timer2_Tick(object sender, EventArgs e)
        {
            txt_time++;
            switch (txt_time)
            {
                case 1:
                    label1.Text = "我們正在準備您的電腦";
                    break;

                case 2:
                    label1.Text = "這可能需要幾分鐘的時間";
                    break;
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            //0,60,108
            //0,120,215
            if (lok == 1)
            {
                lok = 0;
                HookStart();
            }
            tm++;
            if (tm > 60 && tm < 120)
            {
                g -= 1;
                b -= 2;
                BackColor = Color.FromArgb(0, g, b);
            }
            else if (tm > 120 && tm < 180)
            {
                g += 1;
                b += 2;
                BackColor = Color.FromArgb(0, g, b);
            }
            else if (tm == 240)
            {
                tm = 0;
            }
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            HookStop();
        }

        private int lok = 0;

        // 設置鍵盤掛鉤
        public void HookStart()
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                m_KbdHookProc = new HookProc(KeyboardHookProc);
                m_HookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, m_KbdHookProc,
                    Marshal.GetHINSTANCE(Assembly.GetExecutingAssembly().GetModules()[0]), 0);
            }
            if (m_HookHandle == 0)
            {
                HookStop();
                MessageBox.Show("呼叫 SetWindowsHookEx 失敗!");
                return;
            }
        }

        private void Form1_MouseEnter(object sender, EventArgs e)
        {
            Cursor.Hide();
        }

        private void label1_MouseHover(object sender, EventArgs e)
        {
            Cursor.Hide();
        }

        public int clk_time = 0,
            clkr_time = 0,
            clkl_time = 0;

        private void Form1_Click(object sender, EventArgs e)
        {
            MouseEventArgs Mouse_e = (MouseEventArgs)e;
            clk(Mouse_e);
        }

        private void label2_Click(object sender, EventArgs e)
        {
            MouseEventArgs Mouse_e = (MouseEventArgs)e;
            clk(Mouse_e);
        }

        private void label1_Click(object sender, EventArgs e)
        {
            MouseEventArgs Mouse_e = (MouseEventArgs)e;
            clk(Mouse_e);
        }
        private void clk(MouseEventArgs Mouse_e)
        {
            //EventArgs继承自MouseEventArgs,所以可以强转
            

            //点鼠标右键,return
            if (Mouse_e.Button == MouseButtons.Right)
            {
                clkr_time++;
            }
            //if (Mouse_e.Button == MouseButtons.Middle)
            //{
            //    clk_time++;
            //}
            if (Mouse_e.Button == MouseButtons.Left)
            {
                clkl_time++;
            }

            if (/*clk_time >= 10 &&*/ clkr_time >= 10 && clkl_time >= 10)
            {
                Close();
            }
        }
        //钩子子程就是钩子所要做的事情。
        private int KeyboardHookProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            return 1;
        }

        // 解除鍵盤掛鉤
        public void HookStop()
        {
            bool ret = UnhookWindowsHookEx(m_HookHandle);
            if (ret == false)
            {
                MessageBox.Show("呼叫 UnhookWindowsHookEx 失敗!");
                return;
            }
            m_HookHandle = 0;
        }

        public struct KeyMSG
        {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }
    }
}

public class KeyboardInfo

{
    private KeyboardInfo()
    {
    }

    [DllImport("user32")]
    private static extern short GetKeyState(int vKey);

    public static KeyStateInfo GetKeyState(Keys key)

    {
        int vkey = (int)key;

        if (key == Keys.Alt)

        {
            vkey = 0x12;    // VK_ALT
        }

        short keyState = GetKeyState(vkey);

        int low = Low(keyState);

        int high = High(keyState);

        bool toggled = (low == 1);

        bool pressed = (high == 1);

        return new KeyStateInfo(key, pressed, toggled);
    }

    private static int High(int keyState)

    {
        if (keyState > 0)

        {
            return keyState >> 0x10;
        }
        else

        {
            return (keyState >> 0x10) & 0x1;
        }
    }

    private static int Low(int keyState)

    {
        return keyState & 0xffff;
    }
}

public struct KeyStateInfo

{
    private Keys m_Key;
    private bool m_IsPressed;
    private bool m_IsToggled;

    public KeyStateInfo(Keys key, bool ispressed, bool istoggled)
    {
        m_Key = key;

        m_IsPressed = ispressed;

        m_IsToggled = istoggled;
    }

    public static KeyStateInfo Default
    {
        get

        {
            return new KeyStateInfo(Keys.None, false, false);
        }
    }

    public Keys Key

    {
        get { return m_Key; }
    }

    public bool IsPressed

    {
        get { return m_IsPressed; }
    }

    public bool IsToggled

    {
        get { return m_IsToggled; }
    }
}