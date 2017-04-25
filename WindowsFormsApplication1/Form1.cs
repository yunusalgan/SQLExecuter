using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private const string folder = @"";
        private const string connectionstring = "Data Source= ; Initial Catalog=; Persist Security Info=True; User ID=; Password=";

        private void button1_Click(object sender, EventArgs e)
        {
            string[] fileEntries = Directory.GetFiles(folder);
            int lineNum = 0;

            foreach (string fileName in fileEntries)
            {
                if (fileName.IndexOf("Errors") >= 0)
                {
                    File.Delete(fileName);
                }
            }

            fileEntries = Directory.GetFiles(folder);
            SqlConnection sqlConnection1 = new SqlConnection(connectionstring);
            sqlConnection1.Open();

            foreach (string fileName in fileEntries)
            {
                if (fileName.IndexOf("ReadMe.txt") >= 0)
                    continue;

                lineNum = 0;
                try
                {

                    DateTime startTime = DateTime.Now;
                    StringBuilder sbr = new StringBuilder();
                    using (StreamReader sr = File.OpenText(fileName))
                    {
                        sr.ReadLine();
                        sr.ReadLine();
                        while (sr.Peek() != -1)
                        {
                            string line = sr.ReadLine();
                            lineNum++;
                            sbr.AppendLine(line);
                            if (line.IndexOf("GO", StringComparison.InvariantCultureIgnoreCase) >= 0)
                            {
                                WriteToFile(sqlConnection1, sbr, lineNum, fileName);
                            }
                        }

                        if (sbr.ToString() != "")
                        {
                            WriteToFile(sqlConnection1, sbr, lineNum, fileName);
                        }
                    }
                    listBox1.Items.Add(fileName + " dosyası aktarıldı. Satır Sayısı =  " + lineNum + ". Başlangıç : " + startTime.ToString("dd/MM/yyyy hh:mm:ss") + ". Bitiş : " + DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss"));
                }
                catch (Exception ex)
                {
                    listBox1.Items.Add(fileName + " dosyası aktarılırken hata oluştu. Satır No = " + lineNum);
                    listBox1.Items.Add(ex.Message);
                }
                finally
                {
                    sqlConnection1.Close();
                }
            }
            MessageBox.Show("İşlem Tamamlandı");
        }

        private void WriteToFile(SqlConnection conn, StringBuilder sbr, int lineNum, string fileName)
        {
            try
            {
                string sql = sbr.ToString();
                using (SqlCommand cmd = new SqlCommand())
                {
                    if (sql.IndexOf("GO") >= 0)
                    {
                        sql = sql.Substring(0, sbr.ToString().Length - 4);
                    }

                    cmd.CommandText = sql;
                    cmd.CommandType = CommandType.Text;
                    cmd.Connection = conn;

                    cmd.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                listBox1.Items.Add(fileName + " dosyası aktarılırken hata oluştu. Satır No = " +
                                   lineNum);

                string fileNameErrors = fileName.Replace("." + Path.GetExtension(fileName), "") +
                                        "_Errors." + Path.GetExtension(fileName);
                File.AppendAllText(fileNameErrors, sbr.ToString());
            }
            finally
            {
                sbr.Clear();
            }
        }
    }
}
