using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace testshardingdb
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        MySqlConnection connection1, connection2;

        private void button2_Click(object sender, EventArgs e)
        {
            string name = textBox1.Text;
            string age = textBox2.Text;
            string email = textBox3.Text;
            string gender = comboBox1.SelectedItem.ToString();
            if((string.IsNullOrEmpty(name))|| 
                (string.IsNullOrEmpty(age))|| 
                (string.IsNullOrEmpty(gender))|| 
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
            if(gender=="М")
            {
                commandInsert.Connection = connection1;
                connection1.Open();
                commandInsert.ExecuteNonQuery();
                connection1.Close();
            }
            else
            {
                commandInsert.Connection = connection2;
                connection2.Open();
                commandInsert.ExecuteNonQuery();
                connection2.Close();
            }
            refresh();
        }

        void refresh()
        {
            try
            {
                connection1.Open();
                connection2.Open();
               // button1.BackColor = Color.GreenYellow;

                string sql = "select * from users";

                MySqlDataAdapter dataAdapter = new MySqlDataAdapter(sql, connection1);
                DataTable table = new DataTable();
                dataAdapter.Fill(table);

                MySqlDataAdapter dataAdapter2 = new MySqlDataAdapter(sql, connection2);
                dataAdapter2.Fill(table);

                dataGridView1.DataSource = table;

                connection1.Close();
                connection2.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
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
