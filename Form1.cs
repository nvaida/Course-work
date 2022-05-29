using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace Course_work
{
    public partial class Form1 : Form
    {
        private readonly GaloisField DATA_FIELD = new GaloisField(0x011D, 256, 0); // x^8 + x^4 + x^3 + x^2 + 1

        public Form1()
        {
            InitializeComponent();
            this.WindowState = FormWindowState.Maximized;

        }


        //сохранить закодированный файл
        private void button5_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.DefaultExt = ".txt";
            save.Filter = "Coder File|*.txt";
            if (save.ShowDialog() == DialogResult.OK && save.FileName.Length > 0)
            {
                using (StreamWriter sw = new StreamWriter(save.FileName, true))
                {
                    byte[] encodedData = this.hexStringToByteArray(textBox3.Text);
                    sw.BaseStream.Write(encodedData, 0, encodedData.Length);
                    sw.Flush();
                    sw.Close();
                }
            }
        }
        //открыть файл для кодирования
        private void button4_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = File.ReadAllText(open.FileName);
            }
        }

        //сохранить декодированный файл
        private void button3_Click(object sender, EventArgs e)
        {
            SaveFileDialog save = new SaveFileDialog();
            save.DefaultExt = ".txt";
            save.Filter = "File|*.txt";
            if (save.ShowDialog() == DialogResult.OK && save.FileName.Length > 0)
            {
                using (StreamWriter sw = new StreamWriter(save.FileName, true))
                {
                    sw.WriteLine(textBox1.Text);
                    sw.Close();
                }
            }
        }

        //открыть закодированный файл
        private void button6_Click(object sender, EventArgs e)
        {
            OpenFileDialog open = new OpenFileDialog();
            if (open.ShowDialog() == DialogResult.OK)
            {
                byte[] encodedData = File.ReadAllBytes(open.FileName);
                textBox3.Text = BitConverter.ToString(encodedData).Replace("-", "");
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Clear();
            textBox3.Clear();
            chart1.Visible = false;
            chart1.Series[0].Points.Clear();

        }

        //кодирование сообщения
        private void button1_Click(object sender, EventArgs e)
        {
            ReedSolomonCoder coder = new ReedSolomonCoder(DATA_FIELD, (int)numericUpDown1.Value, (int)numericUpDown2.Value);
            byte[] encodedText = coder.encode(Encoding.UTF8.GetBytes(textBox1.Text));
            textBox3.Text = BitConverter.ToString(encodedText).Replace("-", "");
            /*textBox3.MaxLength = textBox3.Text.Length;*/

        }

        private byte[] hexStringToByteArray(String hexString)
        {

            int charNumber = hexString.Length;
            if (charNumber % 2 == 1)
            {
                hexString += "0";
                charNumber += 1;
            }

            byte[] result = new byte[charNumber / 2];
            for (int i = 0; i < charNumber; i += 2)
            {
                result[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }
            return result;
        }

        //декодирование сообщения
        private void button7_Click(object sender, EventArgs e)
        {
            ReedSolomonCoder coder = new ReedSolomonCoder(DATA_FIELD, (int)numericUpDown1.Value, (int)numericUpDown2.Value);
            byte[] encodedData = this.hexStringToByteArray(textBox3.Text);
            byte[] decodedText = coder.decode(encodedData);
            textBox1.Text = Encoding.UTF8.GetString(decodedText);



        }
        public static bool ishexdigit(char c)
        {
            if (('0' <= c && '9' >= c)
             || ('A' <= c && 'F' >= c))
            {
                return true;
            }
            return false;
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (!(ishexdigit(e.KeyChar)))
            {
                if (e.KeyChar != (char)Keys.Back)
                {
                    e.Handled = true;
                }
            }
        }
        //график
        private void button8_Click(object sender, EventArgs e)
        {

            ReedSolomonCoder coder = new ReedSolomonCoder(DATA_FIELD, (int)numericUpDown1.Value, (int)numericUpDown2.Value);
            byte[] encodedData = this.hexStringToByteArray(textBox3.Text);
            try
            {
                double[] probability = this.probability(encodedData, (int)numericUpDown1.Value, (int)numericUpDown2.Value);
                chart1.Series[0].Points.DataBindY(probability);
                chart1.Visible = true;
            }
            catch
            {
                MessageBox.Show("Невозможно декодировать, исходные данные были изменены", "Ошибка!");
            }
        }

        static int MAX_ERRORS_PER_BLOCK = 12;
        static int EXPERIMENTS_COUNT = 10;

        private double[] probability(
            byte[] encodedData, // закодированное сообщение
            int a, // длина информационного блока
            int b // количество избыточных символов
        )
        {
            int n = encodedData.Length / (a + b); // количество информационных блоков
            if (encodedData.Length % (a + b) != 0)
            {
                n += 1;
            }

            ReedSolomonCoder coder = new ReedSolomonCoder(DATA_FIELD, a, b);

            double[] result = new double[MAX_ERRORS_PER_BLOCK];

            for (int k = 1; k <= MAX_ERRORS_PER_BLOCK; k++)
            {
                double statistic = 0;
                for (int i = 0; i < EXPERIMENTS_COUNT; i++)
                {
                    byte[] dataWithErrors = makeChannelErrors(encodedData, a + b, n, k);
                    byte[] decodedData = coder.decode(dataWithErrors);
                    int errorBlocksCount = this.getErrorBlocksCount(decodedData, a);
                    statistic += ((double)errorBlocksCount / (double)n);
                }
                result[k - 1] = (1 - (statistic / EXPERIMENTS_COUNT));
            }
            return result;
        }

        private byte[] makeChannelErrors(byte[] encodedData, int blockLength, int blocksCount, int errorsCount)
        {
            byte[] result = new byte[encodedData.Length];
            Array.Copy(encodedData, 0, result, 0, encodedData.Length);
            for (int i = 0; i < blocksCount; i++)
            {
                makeBlockErrors(result, i * blockLength, blockLength, errorsCount);
                byte[] source = new byte[blockLength], r = new byte[blockLength];
                Array.Copy(encodedData, i * blockLength, source, 0, Math.Min(blockLength, encodedData.Length - i));
                Array.Copy(result, i * blockLength, r, 0, blockLength);
            }
            return result;
        }

        private void makeBlockErrors(byte[] data, int offset, int blockLength, int errorsCount)
        {
            int length = Math.Min(blockLength, data.Length - offset);
            byte[] source = new byte[length];
            Random r = new Random();
            Array.Copy(data, offset, source, 0, length);
            for (int i = 0; i < errorsCount; i++)
            {
                int byteNumber = r.Next(0, length - 1);
                byte error = (byte)(1 << r.Next(0, 7));
                data[byteNumber + offset] = (byte)(source[byteNumber] ^ error);
            }
        }

        private int getErrorBlocksCount(byte[] dataWithErrors, int blockLength)
        {
            int errorBlocksCount = 0;
            for (int i = 0; i < dataWithErrors.Length; i += blockLength)
            {
                byte[] block = new byte[ReedSolomonCoder.ERR_BLOCK.Length];
                Array.Copy(dataWithErrors, i, block, 0, ReedSolomonCoder.ERR_BLOCK.Length);
                if (ReedSolomonCoder.ERR_BLOCK.SequenceEqual(block))
                {
                    errorBlocksCount += 1;
                }
            }
            return errorBlocksCount;
        }


    }
}
