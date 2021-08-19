using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace _3D_Renderer
{
    public partial class GUI : Form
    {
        public GUI()
        {
            InitializeComponent();
            //KeyDown += GUI_KeyDown;
            KeyUp += GUI_KeyUp;
            KeyPreview = true;
        }

        public void DisplayFrame(Bitmap Frame)
        {
            pictureBox1.Invoke((MethodInvoker)(() => { pictureBox1.Image = Frame; }));
        }

        public void UpdateCounter(string NewVal)
        {
            label1.Invoke((MethodInvoker)(() => { label1.Text = NewVal; }));
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            InputHandler.KeyDown(keyData.ToString());
            return true;
        }

        //Does not work for arrow keys or tab so instead using ProcessCmdKey
        //void GUI_KeyDown(object sender, KeyEventArgs e)
        //{
        //    InputHandler.KeyDown(e.KeyValue);
        //}

        void GUI_KeyUp(object sender, KeyEventArgs e)
        {
            InputHandler.KeyUp(e.KeyValue);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Help HelpWindow = new Help();
            Task HelpThread = new Task(() => { Application.Run(HelpWindow); });
            HelpThread.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            button1_Click(sender, e);
        }
    }
}
