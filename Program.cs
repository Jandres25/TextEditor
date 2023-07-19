using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextEditor2
{
    internal static class Program
    {
        /// <summary>
        /// Punto de entrada principal para la aplicación.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            TextEditor textEditor = TextEditor.Instance; // Obtener la instancia existente de TextEditor
            Application.Run(textEditor); // Mostrar el formulario existente
        }
    }
}
