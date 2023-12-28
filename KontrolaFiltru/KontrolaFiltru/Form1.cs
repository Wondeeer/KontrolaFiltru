using System;
using System.Windows.Forms;

namespace KontrolaFiltru {
    public partial class Form1 : Form {      
        public Form1() {
            InitializeComponent();
        }
        private void ProcessBtn_Click(object sender, EventArgs e) {
            new PictureManager();    
       }          
    }
}
