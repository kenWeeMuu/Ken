using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Layout;
using DevExpress.XtraBars;
using DevExpress.XtraEditors;

namespace Main
{
    [ToolboxItem(false)]
    public class ModuleBase : XtraUserControl
    {
        private Container components;

        private bool setManager;

        public ModuleBase()
        {
            this.SetStyle(ControlStyles.SupportsTransparentBackColor | ControlStyles.AllPaintingInWmPaint, true);

            this.InitializeComponent();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && this.components != null)
                this.components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.Font = new Font("Tahoma", 8.25f);
            this.Name = "ModuleBase";
            this.Size = new Size(784, 432);
        }

        public virtual void StartWhatsThis()
        {
        }

        public virtual void EndWhatsThis()
        {
        }

        public void AddMenuManager(BarManager manager)
        {
            if (this.setManager)
                return;
            this.AddManager(this.Controls, manager);
            this.setManager = true;
        }

        private void AddManager(Control.ControlCollection collection, BarManager manager)
        {
            foreach (Control ctrl in (ArrangedElementCollection)collection)
            {
                this.SetControlManager(ctrl, manager);
                this.AddManager(ctrl.Controls, manager);
            }
        }

        protected virtual void SetControlManager(Control ctrl, BarManager manager)
        {
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            this.VisibleChanged += new EventHandler(this.OnVisibleChanged);
        }

        protected virtual void OnVisibleChanged(object sender, EventArgs e)
        {
            this.DoVisibleChanged(this.Visible);
        }

        protected virtual void DoVisibleChanged(bool visible)
        {
        }
    }
}
