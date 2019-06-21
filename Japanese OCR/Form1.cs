using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Tesseract;
using AForge.Video;
using AForge.Video.DirectShow;

namespace Japanese_OCR
{
    public partial class Form1 : Form
    {
        VideoCaptureDevice frame;
        FilterInfoCollection Devices;
        OpenFileDialog openfile = new OpenFileDialog();
        String inputText;
        enum Mode
        {
            Hiragana,
            Katakana,
            Romaji,
        }

        private List<string> Database = new List<string>();

        public Form1()
        {
            InitializeComponent();
            this.MaximizeBox = false;
            GetDatabase();
            tabPage1.Text = "Photos";
            tabPage2.Text = "Camera";
        }

        private void GetDatabase()
        {
            using (System.IO.StreamReader sr = new System.IO.StreamReader("Database.txt"))
            {
                while (!sr.EndOfStream)
                {
                    string splitMe = sr.ReadLine();
                    Database.Add(splitMe);
                }
            }
        }

        private string Convert(string text, Mode convertMode)
        {
            text = text.ToLower();

            string roma = string.Empty;
            string hira = string.Empty;
            string kata = string.Empty;

            foreach (string row in Database)
            {
                var split = row.Split('@');
                roma = split[0];
                hira = split[1];
                kata = split[2];

                switch (convertMode)
                {
                    case Mode.Romaji:
                        text = text.Replace(hira, roma);
                        text = text.Replace(kata, roma.ToUpper());
                        break;
                    case Mode.Hiragana:
                        text = text.Replace(roma, hira);
                        text = text.Replace(kata, hira);
                        break;
                    case Mode.Katakana:
                        text = text.Replace(roma, kata);
                        text = text.Replace(hira, kata);
                        break;
                }
            }
            return text;
        }
        //Process from photo setting
        private String process()
        {
            Bitmap img = new Bitmap(richTextBox2.Text);
            TesseractEngine ocr = new TesseractEngine("./tessdata", "jpn", EngineMode.Default);
            Page page = ocr.Process(img, PageSegMode.Auto);
            inputText = page.GetText();
            return inputText;
        }
        //Camera Setting
        private void Start_Cam()
        {
            try
            {
                Devices = new FilterInfoCollection(FilterCategory.VideoInputDevice);
                frame = new VideoCaptureDevice(Devices[0].MonikerString);
                frame.NewFrame += new AForge.Video.NewFrameEventHandler(NewFrame_event);
                frame.Start();
                button6.Enabled = true;
                button8.Enabled = true;
            }
            catch(Exception)
            {
                richTextBox3.Text = "No camera device applied!";
            }
        }

        private void NewFrame_event(object send, NewFrameEventArgs e)
        {
            try
            {
                pictureBox1.Image = (Image)e.Frame.Clone();
            }
            catch (Exception)
            {
                richTextBox3.Text = "No camera device applied!";
            }
        }
        //Button setting
        private void button1_Click_1(object sender, EventArgs e)
        {
            String inputText = process();
            richTextBox1.Text = Convert(inputText, Mode.Hiragana);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            String inputText = process();
            richTextBox1.Text = Convert(inputText, Mode.Katakana);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            String inputText = process();
            richTextBox1.Text = Convert(inputText, Mode.Romaji);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (openfile.ShowDialog() == DialogResult.OK)
            {
                richTextBox2.Text = openfile.FileName;
            }
            else
            {
                inputText = "";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Start_Cam();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            pictureBox1.Image = null;
            if(frame!=null)
                frame.Stop();
            button8.Enabled = false;
            button6.Enabled = false;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(frame!=null)
                frame.Stop();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            if (pictureBox1.Image == null)
                richTextBox3.Text = "No camera device applied!";
            else
            {
                Bitmap img = new Bitmap(pictureBox1.Image);
                TesseractEngine ocr = new TesseractEngine("./tessdata", "jpn", EngineMode.Default);
                Page page = ocr.Process(img, PageSegMode.Auto);
                String inputText = page.GetText();
                if (radioButton1.Checked)
                {
                    richTextBox3.Text = Convert(inputText, Mode.Hiragana);
                }
                else if (radioButton2.Checked)
                {
                    richTextBox3.Text = Convert(inputText, Mode.Katakana);
                }
                else if (radioButton3.Checked)
                {
                    richTextBox3.Text = Convert(inputText, Mode.Romaji);
                }
            }
        }
    }
}
