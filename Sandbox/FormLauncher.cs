using System;
using System.Windows.Forms;

namespace Sandbox
{
    public partial class FormLauncher : Form
    {
        public FormLauncher()
        {
            InitializeComponent();
            Load += FormLauncher_Load;
        }

        private void FormLauncher_Load(object sender, EventArgs e)
        {
            new MainWindow().Run(20, 60);
            Close();
        }
    }
}
