namespace LangBiasTester.Operators
{
    internal class ErrorManager
    {
        public static void ThrowError(string message, string title = "Fatal Error")
        {
            MessageBox.Show(message, title);
            Application.Exit();
            throw new Exception(message);
        }
        public static void ShowError(string message, string title = "Error")
        {
            MessageBox.Show(message, title);
        }
    }
}
