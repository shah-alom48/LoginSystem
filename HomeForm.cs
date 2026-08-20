using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace LoginSystem
{
    public partial class HomeForm : Form
    {
        public string WelcomeMessage { get; set; }
        public Form PreviousLoginForm { get; set; }
        public string CurrentUsername { get; set; }
        public HomeForm()
        {
            InitializeComponent();
        }

        private void HomeForm_Load(object sender, EventArgs e)
        {
            lblWelcome.Text = WelcomeMessage;
            LoadUsers();
        }

        private void LoadUsers()
        {
            string connStr = ConfigurationManager.ConnectionStrings["LoginDBConnection"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    // Only select safe columns - NEVER PasswordHash
                    string query = "SELECT UserID, Username, Email, CreatedAt FROM dbo.Users";

                    using (SqlDataAdapter adapter = new SqlDataAdapter(query, conn))
                    {
                        DataTable usersTable = new DataTable();
                        adapter.Fill(usersTable);

                        dgvUsers.DataSource = usersTable;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load users.\n\n" + ex.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            if (PreviousLoginForm != null)
            {
                PreviousLoginForm.Close();
            }

            Form1 loginForm = new Form1();
            loginForm.Show();

            this.Close();
        }

        private void btnChangePassword_Click(object sender, EventArgs e)
        {
            ChangePasswordForm changeForm = new ChangePasswordForm();
            changeForm.Username = CurrentUsername;
            changeForm.StartPosition = FormStartPosition.CenterScreen;
            changeForm.ShowDialog();
        }

        private void btnDeleteUser_Click(object sender, EventArgs e)
        {
            if (dgvUsers.CurrentRow == null)
            {
                MessageBox.Show("Please select a user first.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int userId = Convert.ToInt32(dgvUsers.CurrentRow.Cells["UserID"].Value);
            string username = dgvUsers.CurrentRow.Cells["Username"].Value.ToString();

            DialogResult confirm = MessageBox.Show(
                "Are you sure you want to delete user '" + username + "'?",
                "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes)
                return;

            string connStr = ConfigurationManager.ConnectionStrings["LoginDBConnection"].ConnectionString;

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "DELETE FROM dbo.Users WHERE UserID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        cmd.ExecuteNonQuery();
                    }
                }

                MessageBox.Show("User deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadUsers();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not delete user.\n\n" + ex.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnUpdateUser_Click(object sender, EventArgs e)
        {
            if (dgvUsers.CurrentRow == null)
            {
                MessageBox.Show("Please select a user first.", "No Selection",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int userId = Convert.ToInt32(dgvUsers.CurrentRow.Cells["UserID"].Value);
            string email = dgvUsers.CurrentRow.Cells["Email"].Value.ToString();

            string connStr = ConfigurationManager.ConnectionStrings["LoginDBConnection"].ConnectionString;
            string fullName = "";

            try
            {
                using (SqlConnection conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string query = "SELECT FullName FROM dbo.Users WHERE UserID = @UserID";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@UserID", userId);
                        object result = cmd.ExecuteScalar();
                        fullName = result != null ? result.ToString() : "";
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not load user details.\n\n" + ex.Message,
                    "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            UpdateUserForm updateForm = new UpdateUserForm();
            updateForm.UserID = userId;
            updateForm.SetInitialValues(email, fullName);
            updateForm.StartPosition = FormStartPosition.CenterScreen;
            updateForm.ShowDialog();

            if (updateForm.WasUpdated)
            {
                LoadUsers();
            }
        }
    }
}