using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;
using System.IO;
using System.Drawing.Printing;

namespace ex6_2
{
    public partial class frmPhoneDB : Form
    {
        SqlConnection phoneConnection;
        SqlCommand phoneCommand;
        SqlDataAdapter phoneAdapter;
        DataTable phoneTable;
        CurrencyManager phoneManager;
        public frmPhoneDB()
        {
            InitializeComponent();
        }
        string myState;
        int myBookmark;
        private void frmPhoneDB_Load(object sender, EventArgs e)
        {
            string path = Path.GetFullPath("SQLPhoneDB.mdf");
            // connect to Phone database
            phoneConnection = new
        SqlConnection("Data Source=.\\SQLEXPRESS; AttachDBFilename=" + path + "; Integrated Security=True; Connect Timeout=30;" +
        "User Instance=True");
            phoneConnection.Open();
            // establish command object
            phoneCommand = new SqlCommand("SELECT * FROM PhoneTable ORDER BY ContactName", phoneConnection);
            // establish data adapter / data table
             phoneAdapter = new SqlDataAdapter();
            phoneAdapter.SelectCommand = phoneCommand;
            phoneTable = new DataTable();
            phoneAdapter.Fill(phoneTable);
            // bind controls to data table
            txtID.DataBindings.Add("Text", phoneTable, "ContactID");
            txtName.DataBindings.Add("Text", phoneTable, "ContactName");
            txtNumber.DataBindings.Add("Text", phoneTable, "ContactNumber");
            // establish curency manager
           phoneManager = (CurrencyManager)
               this.BindingContext[phoneTable];
            SetState("View");
            foreach (DataRow phoneRow in phoneTable.Rows)
            {
                phoneRow["ContactNumber"] = " (206) " +
                    phoneRow["ContactNumber"].ToString();
            }
        }

        private void frmPhoneDB_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (myState.Equals("Edit") || myState.Equals("Add"))
            {
                MessageBox.Show("You must finish the current edit before " +
                    "stopping the application.", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                e.Cancel = true;
            }
            else
            {
                try
                {
                    // save the updated phone table
                    SqlCommandBuilder phoneAdapterCommands = new
                        SqlCommandBuilder(phoneAdapter);
                    phoneAdapter.Update(phoneTable);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving database to file:\r\n" +
                        ex.Message, "Save Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                // close connection
                phoneConnection.Close();
                // dispose of objects
                phoneConnection.Dispose();
                phoneCommand.Dispose();
                phoneAdapter.Dispose();
                phoneTable.Dispose();
            }
        }

        private void btnFirst_Click(object sender, EventArgs e)
        {
            phoneManager.Position = 0;
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            phoneManager.Position--;
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            phoneManager.Position++;
        }

        private void btnLast_Click(object sender, EventArgs e)
        {
            phoneManager.Position = phoneManager.Count - 1;
        }

        private void SetState(string appState)
        {
            myState = appState;
            switch (appState)
            {
                case "View":
                    btnFirst.Enabled = true;
                    btnPrevious.Enabled = true;
                    btnNext.Enabled = true;
                    btnLast.Enabled = true;
                    btnEdit.Enabled = true;
                    btnSave.Enabled = false;
                    btnCancel.Enabled = false;
                    btnAdd.Enabled = true;
                    btnDelete.Enabled = true;
                    btnDone.Enabled = true;
                    txtID.BackColor = Color.White;
                    txtID.ForeColor = Color.Black;
                    txtName.ReadOnly = true;
                    txtName.ReadOnly = true;
                    break;
                default: // "Edit" mode, "Add" mode
                    btnFirst.Enabled = false;
                    btnPrevious.Enabled = false;
                    btnNext.Enabled = false;
                    btnLast.Enabled = false;
                    btnEdit.Enabled = false;
                    btnSave.Enabled = true;
                    btnCancel.Enabled = true;
                    btnAdd.Enabled = false;
                    btnDelete.Enabled = false;
                    btnDone.Enabled = false;
                    txtID.BackColor = Color.Red;
                    txtID.ForeColor = Color.White;
                    txtName.ReadOnly = false;
                    txtName.ReadOnly = false;
                    break;
            }
            txtName.Focus();
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            SetState("Edit");
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            String savedName = txtName.Text;
            int savedRow;
            phoneManager.EndCurrentEdit();
            phoneTable.DefaultView.Sort = "ContactName";
            savedRow = phoneTable.DefaultView.Find(savedName);
            phoneManager.Position = savedRow;
            SetState("View");
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            phoneManager.CancelCurrentEdit();
            if (myState.Equals("Add"))
            {
                phoneManager.Position = myBookmark;
            }
            SetState("View");
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            myBookmark = phoneManager.Position;
            SetState("Add");
            phoneManager.AddNew();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Are you sure you want to delete this record?", "Delete Record", MessageBoxButtons.YesNo, 
                MessageBoxIcon.Question, MessageBoxDefaultButton.Button2) == DialogResult.Yes)
            {
                phoneManager.RemoveAt(phoneManager.Position);
            }
            SetState("View");
        }

        private void btnDone_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnPrintRecord_Click(object sender, EventArgs e)
        {
            // declaring document
            PrintDocument recordDocument;
            // create document and name it
            recordDocument = new PrintDocument();
            recordDocument.DocumentName = "Titles Record";
            // handler
            recordDocument.PrintPage += new PrintPageEventHandler(this.PrintRecordPage);
            // Print document
            dlgPrint.Document = recordDocument;
            DialogResult result = dlgPrint.ShowDialog();
            if (result == DialogResult.OK)
            {
                recordDocument.Print();
            }
            // dispose of document 
            recordDocument.Dispose();
        }

        private void PrintRecordPage(object sender, PrintPageEventArgs e)
        {
            // print graphic and heading ( 1 inch in height)
            Pen myPen = new Pen(Color.Black, 3);
            e.Graphics.DrawRectangle(myPen, e.MarginBounds.Left, e.MarginBounds.Top, e.MarginBounds.Width, 100);
            e.Graphics.DrawImage(picPhone.Image, e.MarginBounds.Left + 10, e.MarginBounds.Top + 10, 80, 80);
            // print heading
            string s = "Phone DATABASE";
            Font myFont = new Font("Arial", 24, FontStyle.Bold);
            SizeF sSize = e.Graphics.MeasureString(s, myFont);
            e.Graphics.DrawString(s, myFont, Brushes.Black, e.MarginBounds.Left + 100 +
                Convert.ToInt32(0.5 * (e.MarginBounds.Width - 100 - sSize.Width)),
                e.MarginBounds.Top + Convert.ToInt32(0.5 * (100 - sSize.Height)));
            myFont = new Font("Arial", 12, FontStyle.Regular);
            int y = 300;
            int dy = Convert.ToInt32(e.Graphics.MeasureString("S",
                myFont).Height);
            // print ID
             e.Graphics.DrawString("ID: " + txtID.Text, myFont,
                Brushes.Black, e.MarginBounds.Left, y);
            y += 2 * dy;
            // print Name
            e.Graphics.DrawString("Name: " + txtName.Text, myFont,
               Brushes.Black, e.MarginBounds.Left, y);
            y += 2 * dy;
            // print Number
            e.Graphics.DrawString("Number: " + txtNumber.Text, myFont,
               Brushes.Black, e.MarginBounds.Left, y);
            y += 2 * dy;
            // print authors
            // y += 2 * dy;
            // e.Graphics.DrawString("Authors(s): ", myFont, Brushes.Black, e.MarginBounds.Left, y);
            // int x = e.MarginBounds.Left +
            //    Convert.ToInt32(e.Graphics.MeasureString("Author(s): ",
            //    myFont).Width);
            // if (ISBNAuthorsTable.Rows.Count != 0)
            //{
            //    for (int i = 0; i < ISBNAuthorsTable.Rows.Count; i++)
            //    {
            //        e.Graphics.DrawString(authorsCombo[i].Text, myFont, Brushes.Black, x, y);
            //        y += dy;
            //    }
            //}
            //else
            //{
            //    e.Graphics.DrawString(" None", myFont, Brushes.Black, x, y);
            //    y += dy;
            //}
            //x = e.MarginBounds.Left;
            //y += dy;
            ////Print other fields
            //e.Graphics.DrawString("ISBN: " + txtISBN.Text, myFont, Brushes.Black, x, y);
            //y += 2 * dy;
            //e.Graphics.DrawString("Year Published: " + txtYear.Text, myFont, Brushes.Black, x, y);
            //y += 2 * dy;
            //e.Graphics.DrawString("Publisher: " + cboPublisher.Text, myFont, Brushes.Black, x, y);
            //y += 2 * dy;
            //e.Graphics.DrawString("Description: " + txtDescription.Text, myFont, Brushes.Black, x, y);
            //y += 2 * dy;
            //e.Graphics.DrawString("Notes: " + txtNotes.Text, myFont, Brushes.Black, x, y);
            //y += 2 * dy;
            //e.Graphics.DrawString("Subject: " + txtSubject.Text, myFont, Brushes.Black, x, y);
            //y += 2 * dy;
            //e.Graphics.DrawString("Comments: " + txtComments.Text, myFont, Brushes.Black, x, y);
            //e.HasMorePages = false;
        }

        private void btnPrintLastName_Click(object sender, EventArgs e)
        {
            Button whichButton = (Button)sender;
            //string sql = "SELECT * FROM PhoneTable WHERE ContactName LIKE 'S%'";
            ////int i = Convert.ToInt32(whichButton.Name);
            ////if (i >= 0 && i <= 24) // A to Y
            ////{
            ////    sql += "WHERE Name >= '" + (char)(i + 65) + "' AND Name < '" + (char)(i + 65 + 1) + "'";
            ////}
            ////else if (i == 25) // Z
            ////{
            ////    sql += "WHERE Name >= 'Z'";
            ////}
            ////else // Other
            ////{
            ////    sql += "WHERE Name < 'A'";
            ////}
            //sql += " ORDER BY ContactName";
            //phoneCommand = new SqlCommand(sql, phoneConnection);
            //// establish data adapter/data table
            //phoneAdapter = new SqlDataAdapter();
            //phoneAdapter.SelectCommand = phoneCommand;
            //DataTable phoneTable2 = new DataTable();
            //phoneAdapter.Fill(phoneTable2);
            // set up printdocument
            PrintDocument phoneDocument;
            // create the document and name it
            phoneDocument = new PrintDocument();
            phoneDocument.DocumentName = "Phone Numbers";
            // add code handler
            phoneDocument.PrintPage += new PrintPageEventHandler(this.PrintSLastNames);
            // print document
            // pageNumber = 1;
            // dlgPreview.Document = publishersDocument;
            // dlgPreview.ShowDialog();
            dlgPrint.Document = phoneDocument;
            DialogResult result = dlgPrint.ShowDialog();
            if (result == DialogResult.OK)
            {
                phoneDocument.Print();
            }
            // dispose of object when done printing
            phoneDocument.Dispose();
        }

        private void PrintSLastNames(object sender, PrintPageEventArgs e)
        {
            string sql = "SELECT * FROM PhoneTable WHERE ContactName LIKE 'S%'";
            //int i = Convert.ToInt32(whichButton.Name);
            //if (i >= 0 && i <= 24) // A to Y
            //{
            //    sql += "WHERE Name >= '" + (char)(i + 65) + "' AND Name < '" + (char)(i + 65 + 1) + "'";
            //}
            //else if (i == 25) // Z
            //{
            //    sql += "WHERE Name >= 'Z'";
            //}
            //else // Other
            //{
            //    sql += "WHERE Name < 'A'";
            //}
            sql += " ORDER BY ContactName";
            phoneCommand = new SqlCommand(sql, phoneConnection);
            // establish data adapter/data table
            phoneAdapter = new SqlDataAdapter();
            phoneAdapter.SelectCommand = phoneCommand;
            DataTable phoneTable2 = new DataTable();
            phoneAdapter.Fill(phoneTable2);
            // print headings
            Font myFont = new Font("Arial", 18, FontStyle.Bold);
            int y = Convert.ToInt32(e.MarginBounds.Top);
            e.Graphics.DrawString("Last Names Starting With S " +
                DateTime.Now.ToString(), myFont, Brushes.Black,
                e.MarginBounds.Left, y);
            y += Convert.ToInt32(myFont.GetHeight());
            //e.Graphics.DrawString("Page " + pageNumber.ToString(),
            //    myFont, Brushes.Black, e.MarginBounds.Left, y);
            y += Convert.ToInt32(myFont.GetHeight()) + 10;
            e.Graphics.DrawLine(Pens.Black, e.MarginBounds.Left, y,
                e.MarginBounds.Right, y);
            foreach (DataRow row in phoneTable.Rows)
            {
                e.Graphics.DrawString("Last Names: " +
                row["ContactName"].ToString(), myFont,
                Brushes.Black, e.MarginBounds.Left, y);
                y += Convert.ToInt32(myFont.GetHeight());
            }
            //y += Convert.ToInt32(myFont.GetHeight());
            //myFont = new Font("Courier new", 12, FontStyle.Regular);
            //int iEnd = recordsPerPage * pageNumber;
            //if (iEnd > publishersTable.Rows.Count)
            //{
            //    iEnd = publishersTable.Rows.Count;
            //    e.HasMorePages = false;
            //}
            //else
            //{
            //    e.HasMorePages = true;
            //}
            //for (int i = recordsPerPage * (pageNumber - 1); i < iEnd; i++)
            //{
            //    // display current record
            //    e.Graphics.DrawString("Publisher: " +
            //        publishersTable.Rows[i]["Name"].ToString(), myFont,
            //        Brushes.Black, e.MarginBounds.Left, y);
            //    y += Convert.ToInt32(myFont.GetHeight());
            //    e.Graphics.DrawString("Address:   " +
            //        publishersTable.Rows[i]["Address"].ToString(), myFont,
            //        Brushes.Black, e.MarginBounds.Left, y);
            //    y += Convert.ToInt32(myFont.GetHeight());
            //    e.Graphics.DrawString("City:   " +
            //        publishersTable.Rows[i]["City"].ToString(), myFont,
            //        Brushes.Black, e.MarginBounds.Left, y);
            //    y += Convert.ToInt32(myFont.GetHeight());
            //    e.Graphics.DrawString("State:  " +
            //        publishersTable.Rows[i]["State"].ToString(), myFont,
            //        Brushes.Black, e.MarginBounds.Left, y);
            //    y += Convert.ToInt32(myFont.GetHeight());
            //    e.Graphics.DrawString("Zip:   " +
            //        publishersTable.Rows[i]["Zip"].ToString(), myFont,
            //        Brushes.Black, e.MarginBounds.Left, y);
            //    y += Convert.ToInt32(myFont.GetHeight());
            //    y += 2 * Convert.ToInt32(myFont.GetHeight());
            //}
            //if (e.HasMorePages)
            //    pageNumber++;
            //else
            //    pageNumber = 1;
        }
    }
}
