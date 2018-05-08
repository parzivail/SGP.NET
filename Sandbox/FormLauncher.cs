using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
