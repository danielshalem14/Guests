﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows.Forms;

namespace Guests
{
    public partial class Main : Form
    {
        // מחרוזת חיבור למסד הנתונים
        public string ConnectionString = "server=DANIEL\\SQLEXPRESS;initial catalog=shabat; user id=sa;password=1234; TrustServerCertificate=Yes";

        // חיבור למסד הנתונים
        public SqlConnection Connection;

        // פעולה המאתחלת את הפורם ומציגה את כל האורחים
        public Main()
        {
            InitializeComponent();
            ShowAllGuests("");
        }

        // פעולה המחברת למסד הנתונים
        public bool Connect()
        {
            try
            {
                Connection = new SqlConnection(ConnectionString);
                Connection.Open();
                return true;
            }
            catch (SqlException ex)
            {
                MessageBox.Show(ex.Message);
                return false;
            }
        }

        // פעולה המציגה את כל האורחים לפי שם
        private void ShowAllGuests(string name)
        {
            string select = "SELECT name FROM guests WHERE name LIKE '%' + @name + '%'";
            if (Connect())
            {
                SqlCommand command = new SqlCommand(select, Connection);
                command.Parameters.AddWithValue("@name", name);
                SqlDataReader reader = command.ExecuteReader();
                lstGuestsName.Items.Clear();

                // בדיקה האם קיימות שורות בטבלה
                if (reader.HasRows)
                {
                    // ריצה על כל השורות במבנה טבלאי
                    while (reader.Read())
                    {
                        lstGuestsName.Items.Add(reader[0].ToString());
                    }
                }
                Connection.Close();
            }
        }

        // פעולה שמתרחשת בלחיצה על כפתור הכנסה של שם אורח
        private void btnEnterGuestName_Click(object sender, EventArgs e)
        {
            if (txtGuestName.Text == "")
            {
                MessageBox.Show("נא למלא את כל השם");
                return;
            }
            string insert = @"
                IF (@name_guest != '')
                BEGIN
                    IF NOT EXISTS (SELECT name FROM Guests WHERE name = @name_guest)
                    BEGIN
                        INSERT INTO Guests (name) VALUES (@name_guest)
                    END
                END";

            if (Connect())
            {
                SqlCommand command = new SqlCommand(insert, Connection);
                command.Parameters.AddWithValue("@name_guest", txtGuestName.Text);
                int updateRows = command.ExecuteNonQuery();
                Connection.Close();

                // אם נוספו שורות - הצגת כל האורחים והקטגוריות
                if (updateRows > 0) ShowAllGuests("");
                ShowCategories(txtGuestName.Text);
                txtGuestName.Text = string.Empty;
                Connection.Close();
            }
        }

        // פעולה המציגה את כל הקטגוריות
        private void ShowCategories(string name)
        {
            if (name == "") return;

            string select = "SELECT name FROM Categories";
            if (Connect())
            {
                SqlCommand command = new SqlCommand(select, Connection);
                SqlDataReader reader = command.ExecuteReader();
                List<Categories> categories = new List<Categories>();
                int index = 0;

                if (reader.HasRows)
                {
                    while (reader.Read())
                    {
                        categories.Add(new Categories(this, reader[0].ToString(), name, index++));
                    }
                    categories.First().SetCategories = categories;
                    categories.First().Show();
                }
                Connection.Close();
            }
        }

        // פעולה שמתרחשת כשנבחר אורח מרשימת האורחים
        private void lstGuestsName_SelectedIndexChanged(object sender, EventArgs e)
        {
            txtGuestName.Text = lstGuestsName.Text;
        }

        // פעולה שמתרחשת כשהטקסט בתיבת הטקסט של שם האורח משתנה
        private void txtGuestName_TextChanged(object sender, EventArgs e)
        {
            ShowAllGuests(txtGuestName.Text);
        }
    }
}
