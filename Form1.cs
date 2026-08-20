using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace LoginSystem
{
    public partial class Form1 : Form
    {
        private int failedAttempts = 0;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            TestConnection();
        }

        private void TestConnection()
        {
            string connStr = ConfigurationManager.ConnectionStrings["LoginDBConnection"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    MessageBox.Show("Connection successful!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not connect to the database.\n\n" + ex.Message,
                    "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnGoToRegister_Click(object sender, EventArgs e)
        {
            this.Hide();
            RegisterForm regForm = new RegisterForm();
            regForm.Show();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {

            string username = txtLoginUsername.Text.Trim();
            string password = txtLoginPassword.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Please enter both username and password.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["LoginDBConnection"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // Parameterized lookup using SqlDataReader
                    string query = "SELECT PasswordHash, FullName FROM dbo.Users WHERE Username = @Username";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Username", username);

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                string storedHash = reader["PasswordHash"].ToString();
                                string fullName = reader["FullName"].ToString();

                                string enteredHash = HashPassword(password);

                                if (enteredHash == storedHash)
                                {
                                    // SUCCESS
                                    reader.Close();
                                    failedAttempts = 0;
                                    HomeForm home = new HomeForm();
                                    home.WelcomeMessage = "Welcome, " + fullName;
                                    home.PreviousLoginForm = this;
                                    home.CurrentUsername = username;
                                    this.Hide();
                                    home.Show();
                                    return;
                                }
                            }
                        }
                    }
                }

                // If we reach here, login failed (wrong username or password)
                HandleFailedLogin();
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred while logging in.\n\n" + ex.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void HandleFailedLogin()
        {
            failedAttempts++;
            int remaining = 3 - failedAttempts;

            if (remaining > 0)
            {
                MessageBox.Show("Invalid username or password. Attempts remaining: " + remaining,
                    "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                MessageBox.Show("Too many failed attempts. Login has been disabled.",
                    "Account Locked", MessageBoxButtons.OK, MessageBoxIcon.Error);
                btnLogin.Enabled = false;
            }
        }

        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                foreach (byte b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}