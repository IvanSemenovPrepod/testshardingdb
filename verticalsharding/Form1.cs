using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Configuration;
using MySql.Data.MySqlClient;
using System.IO;
using FluentFTP;

namespace verticalsharding
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        void refresh()
        {
            try
            {
                connection1.Open();
                string sql = "select * from users";
                MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sql, connection1);
                DataTable table = new DataTable();
                dataAdapter.Fill(table);
                dataGridView1.DataSource = table;
                connection1.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        MySqlConnection connection1, connection2;

        private void button3_Click(object sender, EventArgs e)
        {
            //добавление пользователя
            string name = textBox1.Text;
            string age = textBox3.Text;
            string email = textBox2.Text;
            string gender = comboBox1.SelectedItem.ToString();
            if ((string.IsNullOrEmpty(name)) ||
                (string.IsNullOrEmpty(age)) ||
                (string.IsNullOrEmpty(gender)) ||
                (string.IsNullOrEmpty(email)))
            {
                MessageBox.Show("Данные не введены");
                return;
            }
            MySqlCommand commandInsert = new MySqlCommand();
            commandInsert.Parameters.AddWithValue("@name", name);
            commandInsert.Parameters.AddWithValue("@age", age);
            commandInsert.Parameters.AddWithValue("@gender", gender);
            commandInsert.Parameters.AddWithValue("@email", email);
            commandInsert.CommandText = "insert into users(name,age,gender,email) values(@name,@age,@gender,@email)";
            commandInsert.Connection = connection1;
            connection1.Open();
            commandInsert.ExecuteNonQuery();
            connection1.Close();
            refresh();
        }
        int id;
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            //id = e.RowIndex+1;
            int id = Convert.ToInt32(dataGridView1.Rows[e.RowIndex].Cells[0].Value);
            refreshImages(id);
        }

        void refreshImages(int i)
        {
            MySqlCommand commandSelectImage = new MySqlCommand();
            commandSelectImage.Parameters.AddWithValue("@id_user", i);
            commandSelectImage.CommandText = "select * from photos where user_id=@id_user";
            commandSelectImage.Connection = connection2;
            DataTable table = new DataTable();
            connection2.Open();
            table.Load(commandSelectImage.ExecuteReader());
            connection2.Close();
            dataGridView2.DataSource = table;
        }


        private void button2_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            if (ofd.ShowDialog() != DialogResult.OK)
                return;
            Image img = Image.FromFile(ofd.FileName);
            FileInfo fileInf = new FileInfo(ofd.FileName);

            int i = Convert.ToInt32(dataGridView1.CurrentRow.Cells[0].Value);

            MySqlCommand commandInserImage = new MySqlCommand();
            commandInserImage.Parameters.AddWithValue("@date_load", File.GetCreationTime(ofd.FileName));
            commandInserImage.Parameters.AddWithValue("@size_image", (int)fileInf.Length / 1024 / 1024);
            commandInserImage.Parameters.AddWithValue("@hd_image", img.Width + "x" + img.Height);
            commandInserImage.Parameters.AddWithValue("@path_image", fileInf.Name);
            commandInserImage.CommandText = "insert into photos(date_load,size_image,hd_image,path_image,user_id) values (@date_load,@size_image,@hd_image,@path_image," + i + ")";
            commandInserImage.Connection = connection2;
            connection2.Open();
            commandInserImage.ExecuteNonQuery();
            connection2.Close();

            //загрузка файла на ftp
            FtpClient client = new FtpClient();
            client.Host = "192.168.8.101";
            client.Connect();
            client.UploadFile(ofd.FileName, @"/images/" + fileInf.Name, FtpRemoteExists.Overwrite, true, FtpVerify.Retry);
            client.Disconnect();
            refreshImages(i);
        }

        private void dataGridView2_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dataGridView2_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string path = dataGridView2.Rows[e.RowIndex].Cells["path_image"].Value.ToString();
            //загрузка файла с ftp сервера

            if (!File.Exists(@"D:\" + path))
            {
                FtpClient client = new FtpClient();
                client.Host = "192.168.8.101";
                client.Connect();
                client.DownloadFile(@"D:\" + path, @"/images/" + path, FtpLocalExists.Overwrite, FtpVerify.Retry);
                client.Disconnect();
            }
            pictureBox1.Load(@"D:\" + path);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string connectionString2 = ConfigurationManager.ConnectionStrings["connectionString2"].ConnectionString;
            string connectionString1 = ConfigurationManager.ConnectionStrings["connectionString1"].ConnectionString;
            connection1 = new MySqlConnection(connectionString1);
            connection2 = new MySqlConnection(connectionString2);
            refresh();
        }
    }
}
