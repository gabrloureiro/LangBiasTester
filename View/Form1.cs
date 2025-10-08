using LangBiasTester.Operators;

namespace LangBiasTester
{
    public partial class Form1 : Form
    {
        internal readonly KeyManager keyManager;

        public Form1()
        {
            InitializeComponent();
            this.KeyPreview = true;
            this.keyManager = new KeyManager(this);
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
