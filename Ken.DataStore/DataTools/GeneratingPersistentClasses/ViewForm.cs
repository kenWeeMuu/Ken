using DevExpress.Data.Filtering;
using DevExpress.LookAndFeel;
using DevExpress.Skins;
using DevExpress.Xpo.DB;
using DevExpress.Xpo.DB.Exceptions;
using DevExpress.XtraEditors;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Text;
using DevExpress.Xpo;
using DevExpress.Xpo.Metadata;

namespace DataTools
{
    /// <summary>
    ///     Summary description for ViewForm.
    /// </summary>
    public partial class XtraForm1 : XtraForm
    {
        public XtraForm1()
        {
            InitializeComponent();
        }

        public string Show(string source, string connectionString, bool useWindowsXPTheme, LookAndFeelStyle style,
            string skinName, bool allowFormSkins)
        {
            UserLookAndFeel.Default.UseWindowsXPTheme = useWindowsXPTheme;
            UserLookAndFeel.Default.Style = style;
            UserLookAndFeel.Default.SkinName = skinName;
            if (allowFormSkins)
                SkinManager.EnableFormSkins();
            Session.DefaultSession.AutoCreateOption = AutoCreateOption.SchemaAlreadyExists;
            Session.DefaultSession.ConnectionString = connectionString;
            var prov = new CSharpCodeProvider();
            var param = new CompilerParameters();
            param.GenerateInMemory = true;
            param.ReferencedAssemblies.Add("System.dll");
            param.ReferencedAssemblies.Add("System.Xml.dll");
            param.ReferencedAssemblies.Add(typeof(XPObject).Assembly.CodeBase.Substring(8));
            param.ReferencedAssemblies.Add(typeof(CriteriaOperator).Assembly.CodeBase.Substring(8));
            var res = prov.CompileAssemblyFromSource(param, source);
            if (res.Errors.HasErrors)
            {
                var errors = new StringBuilder();
                foreach (CompilerError e in res.Errors)
                {
                    errors.Append(e);
                    errors.Append("\r\n");
                }
                return errors.ToString();
                ;
            }
            var classes = Session.DefaultSession.Dictionary.CollectClassInfos(res.CompiledAssembly);
            foreach (var ci in classes)
            {
                comboBoxEdit1.Properties.Items.Add(ci);
            }
            try
            {
                ShowDialog();
            }
            catch (Exception e)
            {
                return e.Message;
            }
            return null;
        }

        private void comboBoxEdit1_SelectedIndexChanged(object sender, EventArgs e)
        {
            var collection = new XPCollection(Session.DefaultSession, (XPClassInfo)comboBoxEdit1.SelectedItem);
            collection.TopReturnedObjects = Convert.ToInt32(seTopReturnedObjects.Value);
            gridControl1.DataSource = null;
            gridControl1.MainView.PopulateColumns();
            try
            {
                gridControl1.DataSource = collection;
            }
            catch (SchemaCorrectionNeededException ex)
            {
                XtraMessageBox.Show(ex.Message);
            }
        }
    }
}