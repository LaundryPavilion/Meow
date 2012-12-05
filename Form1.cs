using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace object_to_nullable_nit_test
{
    public partial class Form1 : Form, IMessageFilter 
    {

        public Form1()
        {
            InitializeComponent();
            Application.AddMessageFilter(this);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            for (int i = 0; i < 10; i++)
                panel1.Controls.Add(new TextBox());

            string nullStr = null;

            string test = string.Format("this ia a test {0}", nullStr);

            object one = "asdasd";
            object two = "1212";
            object three = 123;
            object four = 123L;
            object five = 123.4;

            var oneResult = one as int?;
            var twoResult = two as int?;
            var threeResult = three as int?;
            var fourResult = four as int?;
            var fiveResult = five as int?;

            var oneResultB = one as long?;
            var twoResultB = two as long?;
            var threeResultB = three as long?;
            var fourResultB = four as long?;
            var fiveResultB = five as long?;


        }

        public void DisplayInvalidChars()
        {
            // Get a list of invalid path characters. 
            string[] chars = (from char c in Path.GetInvalidPathChars() where c != '\0' select c.ToString()).ToArray();
            this.textBox1.Text = string.Join(Environment.NewLine, chars);

            chars = (from char c in Path.GetInvalidFileNameChars() where c != '\0' select c.ToString()).ToArray();
            this.richTextBox1.Text = string.Join(Environment.NewLine, chars);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            CancellationTokenSource cts = new CancellationTokenSource();

            cts.Token.ThrowIfCancellationRequested();
            var task = Task.Factory.StartNew(() =>  
                {
                    for (int i = 0; i < 10; i++)
                    {
                        cts.Token.ThrowIfCancellationRequested();
                        Thread.Sleep(500);
                    }

                    // Won't get here if cancelled.
                    MessageBox.Show("was not cancelled");
                }, cts.Token);

               cts.Token.Register(() => {MessageBox.Show("Cancelled!");});

            // Wait for Task to startup
            while (task.Status != TaskStatus.Running) { };

            try
            {
                cts.Cancel();
                task.Wait();
            }
            catch (AggregateException aex)
            {
                aex.Handle(x => { return true; });
            }
        }

        public bool PreFilterMessage(ref Message m) {
            if (m.HWnd == this.panel1.Handle) {
                if (m.Msg == WM_MBUTTONDOWN)
                {
                    EnterReaderMode();
                }
            }
            return false;
        }

        public const int WM_MBUTTONDOWN = 0x0207;

        [DllImport("comctl32.dll", SetLastError=true,  EntryPoint="#383")]
        static extern void DoReaderMode(ref READERMODEINFO prmi);

        public delegate bool TranslateDispatchCallbackDelegate(ref MSG lpmsg);
        public delegate bool ReaderScrollCallbackDelegate(ref READERMODEINFO prmi, int dx, int dy);

        [StructLayout(LayoutKind.Sequential)]
        public struct READERMODEINFO
        {
            public int cbSize;
            public IntPtr hwnd;
            public int fFlags;
            public IntPtr prc;
            public ReaderScrollCallbackDelegate pfnScroll;
            public TranslateDispatchCallbackDelegate fFlags2;
            public IntPtr lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr hwnd;
            public UInt32 message;
            public IntPtr wParam;
            public IntPtr lParam;
            public UInt32 time;
            public POINT pt;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int x;
            public int y;
        }

        [StructLayout(LayoutKind.Sequential)]
        struct RECT
        {
            public int left, top, right, bottom;
        }

        private bool TranslateDispatchCallback(ref MSG lpMsg)
        {
            return false;
        }
        private bool ReaderScrollCallback(ref READERMODEINFO prmi, int dx, int dy)
        {
            // TODO: Scroll around within your control here

            return false;
        }

        private void EnterReaderMode()
        {
            READERMODEINFO readerInfo = new READERMODEINFO
            {
                hwnd = this.textBox1.Handle,
                fFlags = 0,
                prc = IntPtr.Zero,
                lParam = IntPtr.Zero,
                fFlags2 = new TranslateDispatchCallbackDelegate(this.TranslateDispatchCallback),
                pfnScroll = new ReaderScrollCallbackDelegate(this.ReaderScrollCallback)
            };
            readerInfo.cbSize = Marshal.SizeOf(readerInfo);

            DoReaderMode(ref readerInfo);
        }

        private void textBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Middle)
            {
                EnterReaderMode();
            }
        }
    }
}
