using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TextEditor
{
    public partial class frmTextEditor : Form
    {
        /// <summary>
        /// Pazīme, ka teksts ir/nav saglabāts
        /// </summary>
        private bool textSaved = true;

        /// <summary>
        /// Īpašība, kas kontrolē teksta izmainīšanas pazīmi un atbilstoši
        /// aktualizē formas statusa joslu.
        /// </summary>
        public bool Dirty
        {
            get { return !this.textSaved; }
            set
            {
                this.textSaved = !value;
                stlStatus.Text = !this.textSaved ? "Izmainīts." : "";
            }
        }

        /// <summary>
        /// Teksta redaktorā pašreiz ielādētā faila vārds
        /// </summary>
        private string sFileName = "";

        private const string DefaultFileName = "jauns";

        /// <summary>
        /// Teksta redaktorā pašreiz ielādētā faila vārda uzstādīšana un
        /// atgriešana.
        /// </summary>
        public string OpenedFileName
        {
            get { return sFileName.Length == 0 ? DefaultFileName : sFileName; }
            set {
                sFileName = ((string)value).Length > 0
                  ? sFileName = value : DefaultFileName;
            }
        }

        /// <summary>
        /// Teksta redaktora formas loga virsraksta uzstādīšana, daļu
        /// informācijas iegūstot no lietojumprogrammas asamblejas atribūtiem.
        /// </summary>
        private void SetTitle(string sDocName)
        {
            object[] attributes
                = Assembly.GetExecutingAssembly().GetCustomAttributes(
                    typeof(AssemblyProductAttribute), false);

            this.Text = String.Format("{0} - {1}",
                ((AssemblyProductAttribute)attributes[0]).Product, sDocName);
        }

        /// <summary>
        /// Pazīme, ka teksts vēl ne reizi nav saglabāts.
        /// </summary>
        private bool bNewFile = true;

        /// <summary>
        /// Inicializē teksta redaktora formas loga un elementu īpašības jauna
        /// teksta sākšanas gadījumā
        /// </summary>
        private void NewFile()
        {
            if (Dirty)
            {
                if (SaveFile(true))
                {
                    txtContent.Clear();
                    bNewFile = true;
                    Dirty = false;
                    sFileName = "";
                    SetTitle(OpenedFileName);
                }
            }
            else
            {
                txtContent.Clear();
                bNewFile = true;
                Dirty = false;
                sFileName = "";
                SetTitle(OpenedFileName);
            }
            
        }

        /// <summary>
        /// Failu apstrādes dialogu filtru konstante.
        /// </summary>
        private const string csFileDialogFilter = "RTF Standarts|*.rtf|Teksta faili|*.txt|Visi faili|*.*";

        /// <summary>
        /// Failu apstrādes dialogu filtru konstante.
        /// </summary>
        public bool SaveFileAs()
        {
            dlgSaveFile.Title = "Saglabāt kā";
            dlgSaveFile.Filter = csFileDialogFilter;
            dlgSaveFile.InitialDirectory = Path.GetDirectoryName(OpenedFileName);
            dlgSaveFile.FileName = Path.GetFileName(OpenedFileName);

            // Faila saglabāšanas dialoga parādīšana un rezultāta pārbaude
            if (dlgSaveFile.ShowDialog() != DialogResult.OK)
                return false;

            OpenedFileName = dlgSaveFile.FileName;

            // Ievadītā teksta saglabāšana norādītajā failā
            File.WriteAllText(OpenedFileName, txtContent.Rtf);

            // Formas īpašību aktualizēšana
            bNewFile = Dirty = false;
            SetTitle(OpenedFileName);

            return true;
        }

        /// <summary>
        /// Esoša faila saglabāšanas metode.
        /// Ja parametrā norādīts true, tiek arī izvadīts apstiprinājuma dialogs.
        /// Ja saglabāšana ir veiksmīga, atgriež true.
        /// </summary>
        public bool SaveFile(bool bConfirm)
        {
            // Ja teksts nav mainīts, nekas nav jāsaglabā
            if (!Dirty)
                return true;

            if (bConfirm)
            {
                DialogResult result = MessageBox.Show("Vai saglabāt izmaiņas?", "", MessageBoxButtons.YesNoCancel);

                if (result == DialogResult.No)
                {
                    return true;
                }
                else if (result == DialogResult.Cancel)
                {
                    return false;
                }
            }

            // Ja teksts tiek saglabāts pirmo reizi, tad izvada
            // faila saglabāšanas dialogu
            if (bNewFile)
            {
                dlgSaveFile.Title = "Saglabāt";
                dlgSaveFile.Filter = csFileDialogFilter;
                dlgSaveFile.FileName = Path.GetFileName(OpenedFileName);

                if (dlgSaveFile.ShowDialog() != DialogResult.OK)
                    return false;
            }

            // Teksta saglabāšana failā
            File.WriteAllText(OpenedFileName, txtContent.Rtf);

            // Formas īpašību aktualizēšana
            Dirty = false;
            SetTitle(OpenedFileName);

            return true;
        }

        /// <summary>
        /// Esošā faila satura ielādēšannas metode.
        /// </summary>
        private void LoadFile()
        {
            // Ja nepieciešams, vispirms saglabā jau atvērto failu
            if (!SaveFile(true))
                return;

            dlgOpenFile.FileName = "";
            dlgOpenFile.Title = "Atvērt";
            dlgOpenFile.Filter = csFileDialogFilter;

            if (dlgOpenFile.ShowDialog() != DialogResult.OK)
                return;

            if (!File.Exists(dlgOpenFile.FileName))
                return;

            // Teksta ielādēšana no faila
            txtContent.Rtf = File.ReadAllText(dlgOpenFile.FileName);

            // Formas īpašību aktualizēšana
            OpenedFileName = dlgOpenFile.FileName;
            bNewFile = Dirty = false;
            SetTitle(OpenedFileName);
        }
        public frmTextEditor()
        {
            InitializeComponent();
            // Būtiski, lai šī metode tiktu izsaukta pēc InitializeComponent()!
            NewFile();
        }

        private void frmTextEditor_Load(object sender, EventArgs e)
        {

        }

        private void txtContent_TextChanged(object sender, EventArgs e)
        {
            // Tiklīdz teksts tiek izmainīts, aktualizē īpašību:
            Dirty = true;
        }

        private void mnuNew_Click(object sender, EventArgs e)
        {
            try
            {
                NewFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void mnuOpen_Click(object sender, EventArgs e)
        {
            try
            {
                LoadFile();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void mnuSave_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFile(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void mnuSaveAs_Click(object sender, EventArgs e)
        {
            try
            {
                SaveFileAs();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void mnuExit_Click(object sender, EventArgs e)
        {
            try
            {
                Application.Exit();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void mnuCopy_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtContent.SelectionLength > 0)
                    txtContent.Copy();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void mnuCut_Click(object sender, EventArgs e)
        {
            try
            {
                if (txtContent.SelectionLength > 0)
                    txtContent.Cut();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void mnuPaste_Click(object sender, EventArgs e)
        {
            try
            {
                if (Clipboard.GetDataObject().GetDataPresent(DataFormats.Text) == true)
                    txtContent.Paste();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void mnuFont_Click(object sender, EventArgs e)
        {
            dlgFont.Font = txtContent.Font;
            if (dlgFont.ShowDialog() == DialogResult.OK)
                txtContent.Font = dlgFont.Font;
        }

        private void mnuAbout_Click(object sender, EventArgs e)
        {
            (new frmAboutTextEditor()).ShowDialog();
        }

        private void mnuPrint_Click(object sender, EventArgs e)
        {
            sTextToPrint = txtContent.Text;

            // Printera izvēles dialoga atvēršana
            dlgPrintDocument.DocumentName = this.Text;
            dlgPrint.Document = dlgPrintDocument;
            DialogResult result = dlgPrint.ShowDialog();
            if (result != DialogResult.OK)
                return;

            // Dokumenta priekšapskates dialoga atvēršana
            dlgPrintPreview.Document = dlgPrintDocument;
            dlgPrintPreview.ShowDialog();
        }

        // īslaicīga teksta saglabāšana drukāšanas proces atbalstam
        private string sTextToPrint;

        private void dlgPrintDocument_PrintPage(object sender, System.Drawing.Printing.PrintPageEventArgs e)
        {
            // Nosaka, cik rakstzīmes un teksta rindas ietilpst tekošajā
            // lappusē:
            int nSymbols = 0;
            int nLines = 0;
            e.Graphics.MeasureString(this.sTextToPrint, txtContent.Font, e.MarginBounds.Size,
                StringFormat.GenericTypographic, out nSymbols, out nLines);

            // Izvada tekstu uz drukājamās lappuses:
            e.Graphics.DrawString(this.sTextToPrint, txtContent.Font, Brushes.Black, e.MarginBounds,
                StringFormat.GenericTypographic);

            // Aktualizē vēl drukājamo tekstu:
            this.sTextToPrint = this.sTextToPrint.Substring(nSymbols);

            e.HasMorePages = (this.sTextToPrint.Length > 0);

            if (!e.HasMorePages)
                this.sTextToPrint = txtContent.Text;
        }

        private void frmTextEditor_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (Dirty)
            {
                if (SaveFile(true))
                {
                    txtContent.Clear();
                    bNewFile = true;
                    Dirty = false;
                    sFileName = "";
                    SetTitle(OpenedFileName);
                }
                else
                    e.Cancel = true;
            }
        }

        private void mnuPrintPreview_Click(object sender, EventArgs e)
        {
            sTextToPrint = txtContent.Text;

            // Dokumenta priekšapskates dialoga atvēršana
            dlgPrintPreview.Document = dlgPrintDocument;
            dlgPrintPreview.ShowDialog();
        }

        private void mnuUndo_Click(object sender, EventArgs e)
        {
            txtContent.Undo();
        }

        private void txtContent_TextChanged_1(object sender, EventArgs e)
        {
            // Tiklīdz teksts tiek izmainīts, aktualizē īpašību: 
            Dirty = true;
        }

        private void mnuBold_Click(object sender, EventArgs e)
        {
            Font oldFont;
            Font newFont;

            // Izmantotā fonta iegūšana
            oldFont = this.txtContent.SelectionFont;

            // Ja tiek izmantots Bold fonts, tas tiek noņemts
            if (oldFont.Bold)
            {
                newFont = new Font(oldFont, oldFont.Style & ~FontStyle.Bold);
                tlsBold.Checked = false;
            }
            else
            {
                newFont = new Font(oldFont, oldFont.Style | FontStyle.Bold);
                tlsBold.Checked = true;
            }

            // Jauna fonta ievietošana
            this.txtContent.SelectionFont = newFont;
            this.txtContent.Focus();
        }

        private void mnuItalic_Click(object sender, EventArgs e)
        {
            Font oldFont;
            Font newFont;

            // Izmantotā fonta iegūšana
            oldFont = this.txtContent.SelectionFont;

            // Ja tiek izmantots Italic fonts, tas tiek noņemts
            if (oldFont.Italic)
            {
                newFont = new Font(oldFont, oldFont.Style & ~FontStyle.Italic);
                tlsItalic.Checked = false;
            }
            else
            {
                newFont = new Font(oldFont, oldFont.Style | FontStyle.Italic);
                tlsItalic.Checked = true;
            }

            // Jauna fonta ievietošana
            this.txtContent.SelectionFont = newFont;
            this.txtContent.Focus();
        }

        private void mnuUnderline_Click(object sender, EventArgs e)
        {
            Font oldFont;
            Font newFont;

            // Izmantotā fonta iegūšana
            oldFont = this.txtContent.SelectionFont;

            // Ja tiek izmantots Underline fonts, tastiek noņemts
            if (oldFont.Underline)
            {
                newFont = new Font(oldFont, oldFont.Style & ~FontStyle.Underline);
                tlsUnderline.Checked = false;
            }
            else
            {
                newFont = new Font(oldFont, oldFont.Style | FontStyle.Underline);
                tlsUnderline.Checked = true;
            }

            // Jauna fonta ievietošana
            this.txtContent.SelectionFont = newFont;
            this.txtContent.Focus();
        }
    }
}
