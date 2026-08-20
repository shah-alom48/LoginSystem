using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace LoginSystem
{
    public partial class UpdateUserForm : Form
    {
        public int UserID { get; set; }
        public bool WasUpdated { get; private set; } = false;

        public UpdateUserForm()
        {
            InitializeComponent();
        }

        public void SetInitialValues(string email, string fullName)
        {
            txtUpdateEmail.Text = email;
            txtUpdateFullName.Text = fullName;
        }

        private void btnSaveUpdate_Click(object sender, EventArgs e)
        {
            string email = txtUpdateEmail.Text.Trim();
            string fullName = txtUpdateFullName.Text.Trim();

            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(fullName))
            {
                MessageBox.Show("Please fill in all fields.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!email.Contains("@"))
            {
                MessageBox.Show("Please enter a valid email address.", "Validation Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string connStr = ConfigurationManager.ConnectionStrings["LoginDBConnection"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "UPDATE dbo.Users SET Email = @Email, FullName = @FullName WHERE UserID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@Email", email);
                        cmd.Parameters.AddWithValue("@FullName", fullName);
                        cmd.Parameters.AddWithValue("@UserID", UserID);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("User updated successfully!", "Success",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                WasUpdated = true;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not update user.\n\n" + ex.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelUpdate_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}