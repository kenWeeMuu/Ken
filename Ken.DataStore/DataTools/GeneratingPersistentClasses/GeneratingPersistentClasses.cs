using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.Xpo.DB;
using DevExpress.XtraEditors;

namespace DataTools
{
    public partial class GeneratingPersistentClasses : DevExpress.XtraEditors.XtraUserControl
    {
        public GeneratingPersistentClasses()
        {
            InitializeComponent();
        }

        private void radioGroup1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var disable = radioGroup1.SelectedIndex == 0;
            DisableSQLServerAuthentication(disable);
        }

        private void DisableSQLServerAuthentication(bool disable)
        {
            teLogin.Enabled = !disable;
            tePassword.Enabled = !disable;
        }

        private string GetServerConnectionString()
        {
            var connectionString = String.Format("data source={0};integrated security=SSPI", teServer.Text);
            if (radioGroup1.SelectedIndex == 1)
                connectionString = String.Format("data source={0};user id={1};password={2}", teServer.Text, teLogin.Text,
                    tePassword.Text);
            return connectionString;
        }

        private string GetDataBaseConnectionString()
        {
            var connectionString = GetServerConnectionString();
            return connectionString + ";initial catalog=" + cbDatabase.Text;
        }

        private void comboBoxEdit1_QueryPopUp(object sender, CancelEventArgs e)
        {
            using (var connection = new SqlConnection(GetServerConnectionString()))
            {
                try
                {
                    connection.Open();
                }
                catch
                {
                    XtraMessageBox.Show("You have failed to connect to a server.", "Connection Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                using (var command = new SqlCommand("select name from master..sysdatabases", connection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        cbDatabase.Properties.Items.Clear();
                        while (reader.Read())
                        {
                            var name = reader.GetString(0);
                            if (name != "master" && name != "model" && name != "tempdb" && name != "msdb" &&
                                name != "pubs")
                                cbDatabase.Properties.Items.Add(name);
                        }
                    }
                }
            }
        }

        private void simpleButton1_Click(object sender, EventArgs e)
        {
            GeneratePersistentClasses();
        }

        private void GeneratePersistentClasses()
        {
            var code = new StringWriter();
            var log = new StringWriter();
            var connection = new SqlConnection(GetDataBaseConnectionString());
            try
            {
                connection.Open();
            }
            catch
            {
                XtraMessageBox.Show("You have failed to connect to a server.", "Connection Error", MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }
            PersistentClassGenerator.Generate(code, log,
                new MSSqlConnectionProvider(connection, AutoCreateOption.SchemaAlreadyExists));
            meCode.Text = code.ToString();
            meLog.Text = log.ToString();
        }
        private void cbDatabase_EditValueChanged(object sender, EventArgs e)
        {
            if (cbDatabase.Text == String.Empty)
                simpleButton1.Enabled = false;
            else
                simpleButton1.Enabled = true;
        }

        private void simpleButton2_Click(object sender, EventArgs e)
        {
            var setup = new AppDomainSetup();
            setup.ApplicationBase = AppDomain.CurrentDomain.BaseDirectory;
            var dom = AppDomain.CreateDomain("test", null, setup);
            var source = meCode.Text;

            var form =
                (XtraForm1)
                    dom.CreateInstanceFromAndUnwrap(typeof(XtraForm1).Assembly.CodeBase, typeof(XtraForm1).FullName);
            var errors = form.Show(source, GetDataBaseConnectionString(), UserLookAndFeel.Default.UseWindowsXPTheme,
                UserLookAndFeel.Default.Style, UserLookAndFeel.Default.SkinName, SkinManager.AllowFormSkins);
            AppDomain.Unload(dom);
            if (errors != null)
            {
                meLog.Text += errors;
                xtraTabControl1.SelectedTabPageIndex = 1;
            }
        }

        private void meCode_EditValueChanged(object sender, EventArgs e)
        {
            simpleButton2.Enabled = meCode.Text != String.Empty;
        }
    }
}
