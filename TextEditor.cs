using EditorDeTexto;
using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Windows.Forms;

namespace TextEditor2
{
    public partial class TextEditor : MaterialForm
    {
        private static TextEditor instance;
        private string currentFilePath;
        private readonly List<string> textMementos;
        private int currentMementoIndex;
        private TextEditor()
        {
            InitializeComponent();

            textMementos = new List<string>();
            currentMementoIndex = -1;

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme(Primary.Indigo500,Primary.Indigo800,Primary.Indigo900,Accent.LightBlue200,TextShade.WHITE);
        }

        // Creacion del patron de diseño Singleton
        public static TextEditor Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = new TextEditor();
                }
                return instance;
            }
        }

        private void NuevoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentFilePath = null;
            richTextBox.Text = string.Empty;
            AddTextMemento();
        }

        private void AbrirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    currentFilePath = openFileDialog.FileName;
                    richTextBox.Text = File.ReadAllText(currentFilePath);
                    AddTextMemento();
                }
            }
        }

        private void GuardarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentFilePath))
            {
                GuardarComoToolStripMenuItem_Click(sender, e);
            }
            else
            {
                File.WriteAllText(currentFilePath, richTextBox.Text);
                AddTextMemento();
            }
        }

        private void GuardarComoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (SaveFileDialog saveFileDialog = new SaveFileDialog())
            {
                saveFileDialog.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    currentFilePath = saveFileDialog.FileName;
                    File.WriteAllText(currentFilePath, richTextBox.Text);
                    AddTextMemento();
                }
            }
        }

        private void SalirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void CortarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox.Cut();
        }

        private void CopiarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox.Copy();
        }

        private void PegarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox.Paste();
        }

        private void DeshacerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CanUndo())
            {
                currentMementoIndex--;
                RestoreTextMemento();
            }
        }

        private void RehacerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (CanRedo())
            {
                currentMementoIndex++;
                RestoreTextMemento();
            }
        }

        private void BuscarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using (FindDialog findDialog = new FindDialog())
            {
                if (findDialog.ShowDialog() == DialogResult.OK)
                {
                    string searchText = findDialog.SearchText.Trim(); // Eliminar espacios en blanco al inicio y al final

                    if (!string.IsNullOrEmpty(searchText)) // Verificar que el texto de búsqueda no esté vacío
                    {
                        richTextBox.Focus(); // Dar foco al richTextBox para asegurarse de que esté seleccionable

                        // Desubrayar el texto anteriormente resaltado
                        richTextBox.SelectAll();
                        richTextBox.SelectionBackColor = richTextBox.BackColor; // Restaurar el color de fondo predeterminado
                        richTextBox.SelectionColor = richTextBox.ForeColor; // Restaurar el color del texto predeterminado

                        int index = richTextBox.Text.IndexOf(searchText, StringComparison.CurrentCultureIgnoreCase);
                        if (index >= 0)
                        {
                            while (index >= 0)
                            {
                                richTextBox.Select(index, searchText.Length);
                                richTextBox.SelectionBackColor = System.Drawing.Color.Yellow; // Cambiar el fondo del texto seleccionado
                                richTextBox.SelectionColor = System.Drawing.Color.Black; // Cambiar el color del texto seleccionado
                                index = richTextBox.Text.IndexOf(searchText, index + 1, StringComparison.CurrentCultureIgnoreCase);
                            }
                            richTextBox.ScrollToCaret();
                        }
                        else
                        {
                            MessageBox.Show("No se encontró la palabra.", "Buscador", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        MessageBox.Show("Ingrese una palabra para buscar.", "Buscador", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }

        private void AddTextMemento()
        {
            // Store a snapshot of the current text
            textMementos.RemoveRange(currentMementoIndex + 1, textMementos.Count - currentMementoIndex - 1);
            textMementos.Add(richTextBox.Text);
            currentMementoIndex++;
        }

        private void RestoreTextMemento()
        {
            if (currentMementoIndex >= 0 && currentMementoIndex < textMementos.Count)
            {
                richTextBox.Text = textMementos[currentMementoIndex];
            }
        }

        private bool CanUndo()
        {
            return currentMementoIndex > 0;
        }

        private bool CanRedo()
        {
            return currentMementoIndex < textMementos.Count - 1;
        }

        private void ImprimirToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PrintDocument printDocument = new PrintDocument();
            printDocument.PrintPage += PrintDocument_PrintPage;

            PrintDialog printDialog = new PrintDialog
            {
                Document = printDocument
            };

            if (printDialog.ShowDialog() == DialogResult.OK)
            {
                printDocument.Print();
            }
        }

        private void PrintDocument_PrintPage(object sender, PrintPageEventArgs e)
        {
            Font font = richTextBox.Font;
            Brush brush = new SolidBrush(richTextBox.ForeColor);
            float lineHeight = font.GetHeight(e.Graphics);

            float xPos = e.MarginBounds.Left;
            float yPos = e.MarginBounds.Top;

            int linesPerPage = (int)(e.MarginBounds.Height / lineHeight);

            using (StringReader reader = new StringReader(richTextBox.Text))
            {
                string line;
                int lineCount = 0;

                while (lineCount < linesPerPage && (line = reader.ReadLine()) != null)
                {
                    yPos += lineHeight;
                    e.Graphics.DrawString(line, font, brush, xPos, yPos);

                    lineCount++;
                }

                if (reader.Peek() != -1)
                {
                    e.HasMorePages = true;
                }
                else
                {
                    e.HasMorePages = false;
                }
            }
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case Keys.Control | Keys.N: // Atajo de teclado para Nuevo
                    NuevoToolStripMenuItem_Click(null, null);
                    return true;
                case Keys.Control | Keys.O: // Atajo de teclado para Abrir
                    AbrirToolStripMenuItem_Click(null, null);
                    return true;
                case Keys.Control | Keys.S: // Atajo de teclado para Guardar
                    GuardarToolStripMenuItem_Click(null, null);
                    return true;
                case Keys.Control | Keys.F: // Atajo de teclado para Buscar
                    BuscarToolStripMenuItem_Click(null, null);
                    return true;
                case Keys.Control | Keys.Z: // Atajo de teclado para Deshacer
                    DeshacerToolStripMenuItem_Click(null, null);
                    return true;
                case Keys.Control | Keys.Y: // Atajo de teclado para Rehacer
                    RehacerToolStripMenuItem_Click(null, null);
                    return true;
                case Keys.Control | Keys.X: // Atajo de teclado para Cortar
                    CortarToolStripMenuItem_Click(null, null);
                    return true;
                case Keys.Control | Keys.C: // Atajo de teclado para Copiar
                    CopiarToolStripMenuItem_Click(null, null);
                    return true;
                case Keys.Control | Keys.V: // Atajo de teclado para Pegar
                    PegarToolStripMenuItem_Click(null, null);
                    return true;
                case Keys.Control | Keys.P: // Atajo de teclado para Imprimir
                    ImprimirToolStripMenuItem_Click((Keys)keyData, null);
                    return true;
                case Keys.Control | Keys.A: // Atajo de teclado para Seleccionar todo
                    SeleccionarTodoToolStripMenuItem_Click(null, null);
                    return true;
                case Keys.Alt | Keys.F4: // Atajo para cerrar aplicación
                    SalirToolStripMenuItem_Click(null, null);
                    return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void SeleccionarTodoToolStripMenuItem_Click(object sender, EventArgs e)
        {
            richTextBox.SelectAll();
        }
    }
}
