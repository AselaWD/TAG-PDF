using iTextSharp.text;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AcroPDFLib;
using IniParser;
using IniParser.Model;

namespace SATagPDF
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            chkWtrM.Checked = false;
            DataTable gvSource = DisplayFilesInGridView();
            DataRow gvRow;

            //Get All Folders Or Directories and add in table
            DirectoryInfo directory = new DirectoryInfo(@"input/");
            DirectoryInfo[] subDirectories = directory.GetDirectories();
            foreach (DirectoryInfo dirInfo in subDirectories)
            {
                gvRow = gvSource.NewRow();
                gvRow[0] = dirInfo.Name;
                gvSource.Rows.Add(gvRow);
            }
            //Get files in all directories 
            FileInfo[] files = directory.GetFiles("*.pdf", SearchOption.AllDirectories);
            foreach (FileInfo fileInfo in files)
            {
                PdfReader pdfReader = new PdfReader(fileInfo.FullName);
                int numberOfPages = pdfReader.NumberOfPages;

                gvRow = gvSource.NewRow();
                gvRow[0] = fileInfo.Name;
                gvRow[1] = numberOfPages.ToString();                
                gvSource.Rows.Add(gvRow);

            }
            dataGridView1.DataSource = gvSource;


            //Output

            DataTable outgvSource = DisplayFilesOutGridView();
            DataRow outgvRow;

            //Get All Folders Or Directories and add in table
            DirectoryInfo outdirectory = new DirectoryInfo(@"output/");
            DirectoryInfo[] outsubDirectories = outdirectory.GetDirectories();
            foreach (DirectoryInfo outdirInfo in outsubDirectories)
            {
                outgvRow = outgvSource.NewRow();
                outgvRow[0] = outdirInfo.Name;
                outgvSource.Rows.Add(outgvRow);
            }
            //Get files in all directories 
            FileInfo[] outfiles = outdirectory.GetFiles("*.pdf", SearchOption.AllDirectories);
            foreach (FileInfo outfileInfo in outfiles)
            {
                PdfReader pdfReader = new PdfReader(outfileInfo.FullName);
                int numberOfPages = pdfReader.NumberOfPages;

                outgvRow = outgvSource.NewRow();
                outgvRow[0] = outfileInfo.Name;
                outgvRow[1] = numberOfPages.ToString();
                outgvSource.Rows.Add(outgvRow);

            }

            dataGridView2.DataSource = outgvSource;


            // Initialize the button column.
            DataGridViewButtonColumn buttonColumn = new DataGridViewButtonColumn();
            buttonColumn.Text = "Remove";
            buttonColumn.AutoSizeMode =
            DataGridViewAutoSizeColumnMode.AllCells;

            // Use the Text property for the button text for all cells rather
            // than using each cell's value as the text for its own button.
            buttonColumn.UseColumnTextForButtonValue = true;

            // Add the button column to the control.
            dataGridView3.Columns.Insert(0, buttonColumn);


            //Change remove column to last-child
            DataGridViewColumnCollection columnCollection = dataGridView3.Columns;

            DataGridViewColumn firstVisibleColumn = columnCollection.GetFirstColumn(DataGridViewElementStates.Visible);
            DataGridViewColumn lastVisibleColumn = columnCollection.GetLastColumn(DataGridViewElementStates.Visible, DataGridViewElementStates.None);

            int firstColumn_sIndex = firstVisibleColumn.DisplayIndex;
            firstVisibleColumn.DisplayIndex = lastVisibleColumn.DisplayIndex;
            //lastVisibleColumn.DisplayIndex = firstColumn_sIndex;

        }

        private DataTable DisplayFilesInGridView()
        {
            DataTable dtGvSource = new DataTable();
            dtGvSource.Columns.Add(new DataColumn("File Name", typeof(System.String)));
            dtGvSource.Columns.Add(new DataColumn("Page Count", typeof(System.String)));
            //dtGvSource.Columns.Add(new DataColumn("Type", typeof(System.String)));
            return dtGvSource;
        }

        private DataTable DisplayFilesOutGridView()
        {
            DataTable dtGvSource = new DataTable();
            dtGvSource.Columns.Add(new DataColumn("File Name", typeof(System.String)));
            dtGvSource.Columns.Add(new DataColumn("Page Count", typeof(System.String)));
            //dtGvSource.Columns.Add(new DataColumn("Type", typeof(System.String)));
            return dtGvSource;
        }

        private void button1_Click(object sender, EventArgs e)
        {

            int fpage = int.Parse(numFrontMatter.Value.ToString());
            int bpage = int.Parse(numBackMatter.Value.ToString());

            // Load INI file from path, Stream or TextReader.
            var parser = new FileIniDataParser();


            // is also the default comment string, you need to change it
            parser.Parser.Configuration.CommentString = "//";

            IniData data = parser.ReadFile(@"bin/SATagPDF.ini");


            // INI Sections Assign, Separater is mid dash "|"
            int iniFrontTag = int.Parse(data["Customize_Tag_Position"]["FrontMatter"]);
            int iniBackTag = int.Parse(data["Customize_Tag_Position"]["BackMatter"]);

            //using (Stream pdfStream = new FileStream(@"input/"+lblFilePath.Text, FileMode.Open, FileAccess.ReadWrite))

            if (String.IsNullOrEmpty(lblFilePath.Text))
            {
                MessageBox.Show(this, "Please select an input PDF first.", "Error!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            using (Stream newpdfStream = new FileStream(@"output/" + lblFilePath.Text, FileMode.Create, FileAccess.ReadWrite))
            {
                PdfReader pdfReader = new PdfReader(System.IO.File.ReadAllBytes(@"input/" + lblFilePath.Text));
                PdfStamper pdfStamper = new PdfStamper(pdfReader, newpdfStream);
                PdfContentByte pdfContentByte = pdfStamper.GetOverContent(fpage);
                PdfContentByte pdfContentByteEnd = pdfStamper.GetOverContent(bpage);
                PdfContentByte wtContentByteEnd = pdfStamper.GetOverContent(1);
                BaseFont baseFont = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1250, BaseFont.NOT_EMBEDDED);

                if (checkBox1.Checked)
                {
                    if (dataGridView3.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataGridView3.Rows.Count; i++)
                        {
                            int pg = int.Parse(dataGridView3.Rows[i].Cells[1].Value.ToString());

                            PdfContentByte blankContentByteEnd = pdfStamper.GetOverContent(pg);
                            blankContentByteEnd.SetColorFill(BaseColor.BLUE);
                            blankContentByteEnd.SetFontAndSize(baseFont, 16);
                            blankContentByteEnd.BeginText();
                            blankContentByteEnd.ShowTextAligned(PdfContentByte.ALIGN_CENTER, "<SA_BLANK_PAGE>", pdfReader.GetPageSize(pg).Width / 2, pdfReader.GetPageSize(pg).Height / 4, 0);
                            blankContentByteEnd.EndText();

                        }

                    }
                }
                else
                {
                    
                    pdfContentByte.SetColorFill(BaseColor.RED);
                    pdfContentByte.SetFontAndSize(baseFont, 14);
                    pdfContentByte.BeginText();
                    pdfContentByte.ShowTextAligned(PdfContentByte.ALIGN_CENTER, "<FrontMatter>", pdfReader.GetPageSize(fpage).Width / 2, pdfReader.GetPageSize(fpage).Height - iniFrontTag, 0);
                    pdfContentByte.EndText();

                    pdfContentByteEnd.SetColorFill(BaseColor.RED);
                    pdfContentByteEnd.SetFontAndSize(baseFont, 14);
                    pdfContentByteEnd.BeginText();
                    pdfContentByteEnd.ShowTextAligned(PdfContentByte.ALIGN_CENTER, "</FrontMatter>", pdfReader.GetPageSize(bpage).Width / 2, iniBackTag, 0);
                    pdfContentByteEnd.EndText();

                    if (chkWtrM.Checked)
                    {
                        wtContentByteEnd.SetColorFill(BaseColor.BLUE);
                        wtContentByteEnd.SetFontAndSize(baseFont, 16);
                        wtContentByteEnd.BeginText();
                        wtContentByteEnd.ShowTextAligned(PdfContentByte.ALIGN_CENTER, "<WATERMARKED>", pdfReader.GetPageSize(1).Width / 2, pdfReader.GetPageSize(1).Height / 2, 0);
                        wtContentByteEnd.EndText();
                    }

                    if (dataGridView3.Rows.Count > 0)
                    {
                        for (int i = 0; i < dataGridView3.Rows.Count; i++)
                        {
                            int pg = int.Parse(dataGridView3.Rows[i].Cells[1].Value.ToString());

                            PdfContentByte blankContentByteEnd = pdfStamper.GetOverContent(pg);
                            blankContentByteEnd.SetColorFill(BaseColor.BLUE);
                            blankContentByteEnd.SetFontAndSize(baseFont, 16);
                            blankContentByteEnd.BeginText();
                            blankContentByteEnd.ShowTextAligned(PdfContentByte.ALIGN_CENTER, "<SA_BLANK_PAGE>", pdfReader.GetPageSize(pg).Width / 2, pdfReader.GetPageSize(pg).Height / 4, 0);
                            blankContentByteEnd.EndText();

                        }

                    }
                }

                pdfStamper.Close();

            }



            DataTable gvSource = DisplayFilesOutGridView();
            DataRow gvRow;

            //Get All Folders Or Directories and add in table
            DirectoryInfo directory = new DirectoryInfo(@"output/");
            DirectoryInfo[] subDirectories = directory.GetDirectories();
            foreach (DirectoryInfo dirInfo in subDirectories)
            {
                gvRow = gvSource.NewRow();
                gvRow[0] = dirInfo.Name;
                gvSource.Rows.Add(gvRow);
            }
            //Get files in all directories 
            FileInfo[] files = directory.GetFiles("*.pdf", SearchOption.AllDirectories);
            foreach (FileInfo fileInfo in files)
            {
                PdfReader pdfReader = new PdfReader(fileInfo.FullName);
                int numberOfPages = pdfReader.NumberOfPages;

                gvRow = gvSource.NewRow();
                gvRow[0] = fileInfo.Name;
                gvRow[1] = numberOfPages.ToString();
                gvSource.Rows.Add(gvRow);

            }

            chkWtrM.Checked=false;
            dataGridView2.DataSource = gvSource;
            dataGridView3.Rows.Clear();
            numFrontMatter.Value = 1;
            numBackMatter.Value = 1;

            axAcroPDF1.LoadFile("none");
        }



        private void dataGridView1_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            //MessageBox.Show(@"input/" + dataGridView1.SelectedRows[e.ColumnIndex].Cells[0].Value.ToString());

            try
            {
                axAcroPDF1.LoadFile(@"input/" + dataGridView1.SelectedRows[0].Cells[0].Value.ToString());
                axAcroPDF1.setShowToolbar(true); //disable pdf toolbar.
                axAcroPDF1.Enabled = true;
                axAcroPDF1.setPageMode("none");
                axAcroPDF1.setView("Fit");
                axAcroPDF1.setLayoutMode("SinglePage");



                //axAcroPDF1.LoadFile(@"input/" + dataGridView1.SelectedRows[e.ColumnIndex].Cells[0].Value.ToString());

                lblFilePath.Text = dataGridView1.SelectedRows[e.ColumnIndex].Cells[0].Value.ToString();
                PdfReader pdfReader = new PdfReader(System.IO.File.ReadAllBytes(@"input/" + dataGridView1.SelectedRows[e.ColumnIndex].Cells[0].Value.ToString()));
                lblTotalPageCount.Text = pdfReader.NumberOfPages.ToString();
            }
            catch (Exception ex)
            {
                if(ex.HResult == -2146233086)
                {
                    //
                }
            }
            

        }

        private void btnFrontMatter_Click(object sender, EventArgs e)
        {
            //numFrontMatter.Value= axAcroPDF1.setCurrentPage+1;
            //MessageBox.Show(pdfViewer1.CurrentIndex.ToString());


        }

        private void btnBackMatter_Click(object sender, EventArgs e)
        {
            //numBackMatter.Value = axAcroPDF1.CurrentIndex + 1;
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyValue == (char)13)
            {
                
                    axAcroPDF1.setCurrentPage(int.Parse(textBox1.Value.ToString()));
                
                    //MessageBox.Show("Please Enter Valid Page Number.", "Invalid", MessageBoxButtons.OK,MessageBoxIcon.Error);
                
                
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            numFrontMatter.Value = 1;
            numBackMatter.Value = 1;
        }

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            foreach (DataGridViewRow dr in dataGridView1.Rows)
            {
                foreach (DataGridViewRow drout in dataGridView2.Rows)
                {
                    if (dr.Cells[0].Value.ToString() == drout.Cells[0].Value.ToString())
                    {
                        CurrencyManager currencyManager1 = (CurrencyManager)BindingContext[dataGridView1.DataSource];
                        currencyManager1.SuspendBinding();
                        dataGridView1.Rows[dr.Index].Visible = false;
                        currencyManager1.ResumeBinding();
                    }
                }
                
            }
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView3.Rows.Count; i++)
            {
                if (numAddBlankPage.Value.ToString() == dataGridView3.Rows[i].Cells[1].Value.ToString())
                {
                    MessageBox.Show(this, "Page is already added.", "Duplicate!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }

            if (numAddBlankPage.Value!=0)
            {
                object[] rowData = new object[dataGridView3.Columns.Count];
                rowData[1] = numAddBlankPage.Value;
                dataGridView3.Rows.Add(rowData);
            }
            else
            {
                MessageBox.Show(this, "Please enter valid page number", "Invalid!", MessageBoxButtons.OK,MessageBoxIcon.Error);
                numAddBlankPage.Focus();
            }
            

            numAddBlankPage.Value = 0;

        }

        private void dataGridView3_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            
        }

        private void dataGridView3_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.ColumnIndex == 1)
                {
                    dataGridView3.Rows[e.RowIndex].Cells[1].ReadOnly = true;

                }

                if (e.ColumnIndex == 0)
                {
                    String currentValue = dataGridView3.CurrentRow.Cells[1].Value.ToString();

                    if (dataGridView3.Columns[e.ColumnIndex].Index == 0)
                    {
                        dataGridView3.Rows.RemoveAt(dataGridView3.CurrentRow.Index);
                    }

                }

            }
            catch (Exception ex)
            {
                if (ex.HResult == -2147467261)
                {
                    return;
                }
                else
                {
                    return;
                    //MessageBox.Show("Message: " + ex.HResult, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                chkWtrM.Enabled = false;
                numBackMatter.Enabled = false;
                numFrontMatter.Enabled = false;
                btnClear.Enabled = false;
            }
            else{
                chkWtrM.Enabled = true;
                numBackMatter.Enabled = true;
                numFrontMatter.Enabled = true;
                btnClear.Enabled = true;
            }
            
        }
    }
}
