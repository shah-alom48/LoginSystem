using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;

namespace LoginSystem
{
    public partial class ChangePasswordForm : Form
    {
        public string Username { get; set; }

        public ChangePasswordForm()
        {
            InitializeComponent();
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            string oldPassword = txtOldPassword.Text;
            string newPassword = txtNewPassword.Text;
            string confirmNewPassword = txtConfirmNewPassword.Text;

            if (string.IsNullOrEmpty(oldPassword) || string.IsNullOrEmpty(newPassword) ||
                string.IsNullOrEmpty(confirmNewPassword))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPassword.Length < 6)
            {
                MessageBox.Show("New password must be at least 6 characters long.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (newPassword != confirmNewPassword)
            {
                MessageBox.Show("New passwords do not match.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["LoginDBConnection"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();

                    // Step 1: Verify old password
                    string checkQuery = "SELECT PasswordHash FROM dbo.Users WHERE Username = @Username";
                    string storedHash = "";
                    using (SqlCommand checkCmd = new SqlCommand(checkQuery, conn))
                    {
                        checkCmd.Parameters.AddWithValue("@Username", Username);
                        object result = checkCmd.ExecuteScalar();
                        if (result != null)
                            storedHash = result.ToString();
                    }

                    string oldHash = HashPassword(oldPassword);
                    if (oldHash != storedHash)
                    {
                        MessageBox.Show("Old password is incorrect.", "Validation Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    // Step 2: Update to new password hash
                    string newHash = HashPassword(newPassword);
                    string updateQuery = "UPDATE dbo.Users SET PasswordHash = @PasswordHash WHERE Username = @Username";
                    using (SqlCommand updateCmd = new SqlCommand(updateQuery, conn))
                    {
                        updateCmd.Parameters.AddWithValue("@PasswordHash", newHash);
                        updateCmd.Parameters.AddWithValue("@Username", Username);
                        updateCmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("Password changed successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not change password.\n\n" + ex.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void btnCancelChange_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}