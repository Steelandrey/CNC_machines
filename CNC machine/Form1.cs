using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Security;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace CNC_machine
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            button10.Enabled = false;
        }

        public int[] arrINS = new int [30];//массив выборочных инструментов
        int i = 0;//счётчик
        
        public int t1=0;//первый инструмент
        public int t2=0;//последний инструмент

        public string CODE;//текст программы

        private void button1_Click(object sender, EventArgs e)//Инструменты по порядку
        {
            if (numericUpDown2.Value < numericUpDown1.Value)
            {
                richTextBox1.Text = "Макс значение не м.б. меньше мин значения!";
                return;
            }
            button1.Enabled = false;
            button2.Enabled = false;
            richTextBox1.Text = "Алгоритм инструментов:\n";
            richTextBox1.Text += Convert.ToString(numericUpDown1.Value) + " - " + Convert.ToString(numericUpDown2.Value);
            t1 = Convert.ToInt32(numericUpDown1.Value);
            t2 = Convert.ToInt32(numericUpDown2.Value);
        }

        private void button4_Click(object sender, EventArgs e)//Очистка
        {
            richTextBox1.Clear();
            richTextBox2.Clear();

            for (int y = 0; y < 30; y++)
            {
                arrINS[y] = 0;
            }
            
            t1 = 0;//первый инструмент
            t2 = 0;//последний инструмент
            i = 0;

            CODE = "";
            button1.Enabled = true;
            button2.Enabled = true;
            button5.Enabled = true;
        }

        private void button2_Click(object sender, EventArgs e)//Добавление инструментов по выбору
        {
            if (i > 29)
            {
                richTextBox1.Text += "Инструментов не может быть больше 30!";
                return;
            }
            button1.Enabled = false;
            richTextBox1.Text += Convert.ToString(numericUpDown3.Value) + " ";
            arrINS[i] = Convert.ToInt32(numericUpDown3.Value);
            i++;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (numericUpDown5.Value < numericUpDown4.Value)
            {
                richTextBox1.Text = "Макс значение не м.б. меньше мин значения!";
                return;
            }
            button5.Enabled = false;
            button2.Enabled = false;
            CODE = "";
            int ck1 = Convert.ToInt32(numericUpDown4.Value);//начальная СК
            int ck2 = Convert.ToInt32(numericUpDown5.Value);//финальная СК

            richTextBox1.Text += "\nАлгоритм систем координат:\n";
            richTextBox1.Text += Convert.ToString(numericUpDown4.Value) + " - " + Convert.ToString(numericUpDown5.Value) + "\n\n";

            string strINS;//строка инструмента
            string strCS;//строка системы координат

            int pos = 1;//порядковый номер инструмента

            if ((t2 - t1) != 0)//если инструменты по порядку
            {
                int q = t1;//счётчик
                for (int n = t1; n <= t2; n++)
                {
                    strINS = String.Format("T{0} M06\nM01\n\n", n);
                    CODE += strINS;

                    if (n == q)
                    {
                        for (int x = ck1; x <= ck2; x++)
                        {
                            strCS = String.Format("G154 P{0}\nM01\nM97 P{1}\n\n", x, pos * 10);
                            CODE += strCS;
                        }
                        q += 2;
                    }

                    if (n == (q - 1))
                    {
                        for (int x = ck2; x >= ck1; x--)
                        {
                            strCS = String.Format("G154 P{0}\nM01\nM97 P{1}\n\n", x, pos * 10);
                            CODE += strCS;
                        }
                    }
                    pos++;
                    CODE += "M9\nM5\n\n";
                }
                CODE += "G0 G53 Z0\nG0 G53 Y0\nM30\n";
                richTextBox2.Text += CODE;
            }
            else//если инструменты выборочно
            {
                int q = 0;//счётчик
                for (int n = 0; n <= (i-1); n++)
                {
                    strINS = String.Format("T{0} M06\nM01\n\n", arrINS[n]);
                    CODE += strINS;

                    if (n == q)
                    {
                        for (int x = ck1; x <= ck2; x++)
                        {
                            strCS = String.Format("G154 P{0}\nM01\nM97 P{1}\n\n", x, pos * 10);
                            CODE += strCS;
                        }
                        q += 2;
                    }

                    if (n == (q - 1))
                    {
                        for (int x = ck2; x >= ck1; x--)
                        {
                            strCS = String.Format("G154 P{0}\nM01\nM97 P{1}\n\n", x, pos * 10);
                            CODE += strCS;
                        }
                    }
                    CODE += "M9\nM5\n\n";
                    pos++;
                }
                CODE += "G0 G53 Z0\nG0 G53 Y0\nM30\n";
                richTextBox2.Text += CODE;
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // Create a SaveFileDialog to request a path and file name to save to.
            SaveFileDialog saveFile1 = new SaveFileDialog();

            // Initialize the SaveFileDialog to specify the RTF extension for the file.
            saveFile1.DefaultExt = "*.txt";
            saveFile1.Filter = "txt Files|*.txt";

            // Determine if the user selected a file name from the saveFileDialog.
            if (saveFile1.ShowDialog() == System.Windows.Forms.DialogResult.OK &&
               saveFile1.FileName.Length > 0)
            {
                // Save the contents of the RichTextBox into the file.
                richTextBox2.SaveFile(saveFile1.FileName, RichTextBoxStreamType.PlainText);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(richTextBox2.Text);

        }

        /*Реализация нарезки архива на файлы*/
        String ARH;//архив с программами
        int LenARH;//Длина архива (количество символов)
        int strs;//Количество строк архива
        private void button7_Click(object sender, EventArgs e)//Открыть файл архив
        {
            button10.Enabled = true;
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var sr = new StreamReader(openFileDialog1.FileName);
                    ARH = sr.ReadToEnd();
                    richTextBox3.Text = ARH;
                    LenARH = ARH.Length;
                    for (int x = 0; x < LenARH; x++)
                    {
                        if (ARH[x] == '\n')
                        {
                            strs++;
                        }

                    }
                    richTextBox4.Text = "Общее количество символов архива: ";
                    richTextBox4.Text += Convert.ToString(LenARH)+"\n";
                    richTextBox4.Text += "Общее количество строк архива: ";
                    richTextBox4.Text += Convert.ToString(strs) + "\n";
                }
                catch (SecurityException ex)
                {
                    MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                    $"Details:\n\n{ex.StackTrace}");
                }
            }
        }

        private void button9_Click(object sender, EventArgs e)//Очистка
        {
            richTextBox3.Clear();
            richTextBox4.Clear();
            ARH = "";
            strs = 0;
            LenARH = 0;
            progressBar1.Value = 100 * 0;
            button7.Enabled = true;
        }

        private void button10_Click(object sender, EventArgs e)//Нарезать архив с программами на отдельные файлы
        {
            button7.Enabled = false;
            if (comboBox1.Text == "Выберите каталог для сохранения файлов")
            {
                richTextBox4.Text = "Ошибка! Вы не выбрали каталог для сохранения файлов!";
                return;
            }
            if (checkBox1.Checked && checkBox2.Checked)
            {
                richTextBox4.Text = "Ошибка! Выберите либо Fanuc, либо Xaas!";
                return;
            }
            if (!(checkBox1.Checked) && !(checkBox2.Checked))
            {
                richTextBox4.Text = "Ошибка! Вы не выбрали операционную систему станка (Fanuc, Xaas).";
                return;
            }
            int s = 0;//Начало программы
            int f = 0;//Конец программы
            int ps = 0;//Количество программ в архиве
            String fullPath;//Пуст к exe файлу программы
            String path_name="";//Путь к файлу с программой с именем файла

            richTextBox4.Text = "Ждите.\n";

            fullPath = Application.StartupPath.ToString();
            string nm = comboBox1.Text;//выбранное название каталога
            DateTime thisDay = DateTime.Today;
            string d = thisDay.ToString();
            d = d.Substring(0, 10);
            fullPath = fullPath+nm+"-"+d;//Директория станка
            DirectoryInfo di = Directory.CreateDirectory(fullPath);//Создание директории

            for (s = 0; s < LenARH; s++)
            {
                progressBar1.Value = 100 * (s+1) / LenARH;
                if (ARH[s] == 'O')
                {
                    if (ARH[s-1] == '\n')
                    {
                        for (f = (s+4); f < LenARH; f++)
                        {
                            if (ARH[f] == 'O' || ARH[f]=='%')
                            {
                                if (ARH[f - 1] == '\n')
                                {
                                    char[] p = new char[f - s];
                                    p = ARH.ToCharArray(s, (f - s));
                                    String progr = new string(p);//Строка с программой
                                    progr = "%\n" + progr + "\n%\n";
                                    if (checkBox1.Checked)//Fanuc
                                    {
                                        char[] name = new char[4];
                                        name = ARH.ToCharArray(s+1, 4);
                                        String name_progr = new string(name);//Строка с названием программы
                                        richTextBox4.Text += name_progr+"\n";
                                        path_name = fullPath+"\\"+name_progr;
                                        ps++;
                                        try
                                        {
                                            using (FileStream fs = File.Create(path_name))
                                            {
                                                byte[] info = new UTF8Encoding(true).GetBytes(progr);
                                                fs.Write(info, 0, info.Length);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                                                $"Details:\n\n{ex.StackTrace}");
                                        }
                                    }
                                    
                                    if (checkBox2.Checked)//Xaas
                                    {
                                        char[] name = new char[5];
                                        name = ARH.ToCharArray(s + 1, 5);
                                        String name_progr = new string(name);//Строка с названием программы
                                        richTextBox4.Text += name_progr + "\n";
                                        path_name = fullPath + "\\" + name_progr;
                                        ps++;
                                        try
                                        {
                                            using (FileStream fs = File.Create(path_name))
                                            {
                                                byte[] info = new UTF8Encoding(true).GetBytes(progr);
                                                fs.Write(info, 0, info.Length);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            MessageBox.Show($"Security error.\n\nError message: {ex.Message}\n\n" +
                                                $"Details:\n\n{ex.StackTrace}");
                                        }
                                    }
                                    
                                    break;
                                }
                            }
                        }
                    }
                }
            }
            string end = String.Format("Всего найдено программ: {0}", ps);
            richTextBox4.Text += end+"\n";
            richTextBox4.Text += fullPath;
        }
    }
}