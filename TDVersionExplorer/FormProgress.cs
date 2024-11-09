using System;
using System.Windows.Forms;

namespace TDVersionExplorer
{
    public partial class FormProgress : Form
    {
        public Action CancelAction { get; set; }

        public FormProgress()
        {
            InitializeComponent();
        }

        public void UpdateProgress(int progress, string msg)
        {
            progressBar.Value = progress;
            textBoxMsg.Text = msg;
        }

        public void SetMaximum(int max)
        {
            progressBar.Maximum = max;
        }

        private void ButtonCancel_Click(object sender, EventArgs e)
        {
            CancelAction?.Invoke();
        }
    }
}
