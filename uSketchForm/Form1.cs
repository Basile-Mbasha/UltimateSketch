using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Printing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using DevExpress.Utils.Svg;
using Svg;


namespace Ultimate_Sketch
{
    public partial class Form1 : Form
    {
        public List<Point> points = new List<Point>();
        private Point? previousPoint; // to keep track of the previous point
        private const int GridSize = 20; // the size of the grid cells
        public Stack<List<Point>> undoStack = new Stack<List<Point>>();
        public Stack<List<Point>> redoStack = new Stack<List<Point>>();
        public Form1()
        {
            InitializeComponent();

            points = new List<Point>();

            // Add the File menu items
            ToolStripMenuItem fileMenuItem = new ToolStripMenuItem("File");
            fileMenuItem.ShortcutKeys = Keys.Control | Keys.F;
            menuStrip1.Items.Add(fileMenuItem);

            ToolStripMenuItem newMenuItem = new ToolStripMenuItem("New");
            newMenuItem.ShortcutKeys = Keys.Control | Keys.N;
            fileMenuItem.DropDownItems.Add(newMenuItem);

            ToolStripMenuItem saveMenuItem = new ToolStripMenuItem("Save as PNG");
            saveMenuItem.ShortcutKeys = Keys.Control | Keys.S;
            fileMenuItem.DropDownItems.Add(saveMenuItem);

            ToolStripMenuItem exportSvgMenuItem = new ToolStripMenuItem("Export as SVG");
            exportSvgMenuItem.ShortcutKeys = Keys.Control | Keys.E;
            fileMenuItem.DropDownItems.Add(exportSvgMenuItem);

            ToolStripMenuItem printMenuItem = new ToolStripMenuItem("Print");
            printMenuItem.ShortcutKeys = Keys.Control | Keys.P;
            fileMenuItem.DropDownItems.Add(printMenuItem);

            // Add the Actions menu items
            ToolStripMenuItem actionsMenuItem = new ToolStripMenuItem("Actions");
            actionsMenuItem.ShortcutKeys = Keys.Control | Keys.A;
            menuStrip1.Items.Add(actionsMenuItem);

            ToolStripMenuItem undoMenuItem = new ToolStripMenuItem("Undo");
            undoMenuItem.ShortcutKeys = Keys.Control | Keys.U;
            actionsMenuItem.DropDownItems.Add(undoMenuItem);

            ToolStripMenuItem redoMenuItem = new ToolStripMenuItem("Redo");
            redoMenuItem.ShortcutKeys = Keys.Control | Keys.R;
            actionsMenuItem.DropDownItems.Add(redoMenuItem);

            ToolStripMenuItem clearMenuItem = new ToolStripMenuItem("Clear");
            clearMenuItem.ShortcutKeys = Keys.Control | Keys.C;
            actionsMenuItem.DropDownItems.Add(clearMenuItem);

            // Add the Tools menu items
            ToolStripMenuItem toolsMenuItem = new ToolStripMenuItem("Tools");
            toolsMenuItem.ShortcutKeys = Keys.Control | Keys.T;
            menuStrip1.Items.Add(toolsMenuItem);

            ToolStripMenuItem selectMenuItem = new ToolStripMenuItem("Select");
            selectMenuItem.ShortcutKeys = Keys.Control | Keys.X;
            toolsMenuItem.DropDownItems.Add(selectMenuItem);

            ToolStripMenuItem showInBrowserMenuItem = new ToolStripMenuItem("Show in browser");
            showInBrowserMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.B;
            showInBrowserMenuItem.Click += ShowInBrowserMenuItem_Click;
            toolsMenuItem.DropDownItems.Add(showInBrowserMenuItem);

            // Add event handlers
            saveMenuItem.Click += SaveMenuItem_Click;
            printMenuItem.Click += PrintMenuItem_Click;
            exportSvgMenuItem.Click += ExportSvgMenuItem_Click;
            newMenuItem.Click += NewMenuItem_Click;

            undoMenuItem.Click += UndoMenuItem_Click;
            redoMenuItem.Click += RedoMenuItem_Click;
            clearMenuItem.Click += ClearMenuItem_Click;
            selectMenuItem.Click += SelectMenuItem_Click;

            pictureBox1.Paint += pictureBox1_Paint;
            pictureBox1.MouseDown += pictureBox1_MouseDown;
            pictureBox1.MouseMove += pictureBox1_MouseMove;
            pictureBox1.MouseUp += pictureBox1_MouseUp;

        }
        private void pictureBox1_Paint(object sender, PaintEventArgs e)
        {
            // Fill the control with white before drawing
            e.Graphics.Clear(Color.White);

            // Draw the grid lines
            for (int x = 0; x < pictureBox1.ClientSize.Width; x += GridSize)
            {
                e.Graphics.DrawLine(Pens.LightGray, x, 0, x, pictureBox1.ClientSize.Height);
            }
            for (int y = 0; y < pictureBox1.ClientSize.Height; y += GridSize)
            {
                e.Graphics.DrawLine(Pens.LightGray, 0, y, pictureBox1.ClientSize.Width, y);
            }

            // Draw the user's drawing onto the picture box
            if (points.Count > 1)
            {
                e.Graphics.DrawLines(Pens.Black, points.ToArray());
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            // Start drawing when the left mouse button is clicked
            if (e.Button == MouseButtons.Left)
            {
                previousPoint = e.Location;
            }
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            // Draw a line from the previous point to the current point
            if (e.Button == MouseButtons.Left && previousPoint.HasValue)
            {
                // Save the current state of the drawing
                var currentState = points.ToList();

                // Add the current state to the undo stack
                undoStack.Push(currentState);

                // Draw the line
                points.Add(e.Location);
                pictureBox1.Invalidate(); // Force a redraw of the picture box
                previousPoint = e.Location;
                redoStack.Clear(); // Clear the redo stack when the user makes a new change
            }
        }


        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            // Stop drawing when the left mouse button is released
            if (e.Button == MouseButtons.Left)
            {
                previousPoint = null;
            }
        }

        private void SaveMenuItem_Click(object sender, EventArgs e)
        {
            if (points.Count == 0)
            {
                MessageBox.Show("There is no drawing to save.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                // Create a bitmap of the picture box
                Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                pictureBox1.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));

                // Draw the user's drawing onto the bitmap
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.DrawLines(new Pen(Color.Black), points.ToArray());
                }

                // Display the SaveFileDialog to get the file name and location
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Filter = "PNG Image|*.png";
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    // Save the bitmap as a PNG file
                    bmp.Save(saveFileDialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
                    MessageBox.Show("Drawing saved successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving drawing: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PrintMenuItem_Click(object sender, EventArgs e)
        {
            if (points.Count == 0)
            {
                MessageBox.Show("There is no drawing to print.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            PrintDocument pd = new PrintDocument();
            pd.PrintPage += pd_PrintPage;
            PrintDialog pdlg = new PrintDialog();
            pdlg.Document = pd;

            if (pdlg.ShowDialog() == DialogResult.OK)
            {
                pd.Print();
            }
        }

        private void pd_PrintPage(object sender, PrintPageEventArgs e)
        {
            // Create a bitmap of the picture box
            Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            pictureBox1.DrawToBitmap(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height));

            // Draw the bitmap onto the print document
            e.Graphics.DrawImage(bmp, e.PageBounds);
        }



        private void ExportSvgMenuItem_Click(object sender, EventArgs e)
        {
            if (points.Count == 0)
            {
                MessageBox.Show("There is no drawing to export.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Prompt the user to select a file path for the exported SVG file
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "SVG files (*.svg)|*.svg";
            saveFileDialog.Title = "Export as SVG";
            saveFileDialog.ShowDialog();

            if (saveFileDialog.FileName != "")
            {
                try
                {
                    // Create the SVG file and write the contents
                    using (System.IO.StreamWriter writer = new System.IO.StreamWriter(saveFileDialog.FileName))
                    {
                        writer.WriteLine("<?xml version=\"1.0\" standalone=\"no\"?>");
                        writer.WriteLine("<!DOCTYPE svg PUBLIC \"-//W3C//DTD SVG 1.1//EN\"");
                        writer.WriteLine("  \"http://www.w3.org/Graphics/SVG/1.1/DTD/svg11.dtd\">");
                        writer.WriteLine("<svg width=\"" + pictureBox1.Width + "\" height=\"" + pictureBox1.Height + "\" version=\"1.1\"");
                        writer.WriteLine("xmlns=\"http://www.w3.org/2000/svg\">");
                        writer.WriteLine("<desc>SVG export from Ultimate Sketch</desc>");
                        writer.WriteLine("<rect x=\"0\" y=\"0\" width=\"" + pictureBox1.Width + "\" height=\"" + pictureBox1.Height + "\" style=\"fill:white;stroke:none;\"/>");

                        // Draw the grid lines
                        for (int x = 0; x < pictureBox1.ClientSize.Width; x += GridSize)
                        {
                            writer.WriteLine("<line x1=\"" + x + "\" y1=\"0\" x2=\"" + x + "\" y2=\"" + pictureBox1.Height + "\" style=\"stroke:rgb(192,192,192);stroke-width:1\" />");
                        }
                        for (int y = 0; y < pictureBox1.ClientSize.Height; y += GridSize)
                        {
                            writer.WriteLine("<line x1=\"0\" y1=\"" + y + "\" x2=\"" + pictureBox1.Width + "\" y2=\"" + y + "\" style=\"stroke:rgb(192,192,192);stroke-width:1\" />");
                        }

                        // Draw the user's drawing onto the SVG file
                        if (points.Count > 1)
                        {
                            writer.Write("<polyline points=\"");
                            foreach (Point p in points)
                            {
                                writer.Write(p.X + "," + p.Y + " ");
                            }
                            writer.Write("\" style=\"fill:none;stroke:black;stroke-width:2\"/>");
                        }

                        writer.WriteLine("</svg>");
                    }

                    MessageBox.Show("The drawing has been exported as SVG successfully.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("An error occurred while exporting the drawing as SVG. Error message: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }





        private void NewMenuItem_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show("Do you want to save the current drawing?", "Save Drawing", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                SaveMenuItem_Click(sender, e);
            }
            else if (result == DialogResult.Cancel)
            {
                return;
            }

            // Clear the points list and invalidate the picture box to force a redraw
            points.Clear();
            pictureBox1.Invalidate();
        }
        private void ClearMenuItem_Click(object sender, EventArgs e)
        {
            if (points.Count > 0)
            {
                // Clear the drawing
                points.Clear();
                pictureBox1.Invalidate();

                // Clear the undo/redo stacks
                undoStack.Clear();
                redoStack.Clear();
            }

            else
            {
                // Show a message indicating that there is nothing to clear
                MessageBox.Show("There is nothing to clear.", "Clear");
            }
        }


        private void UndoMenuItem_Click(object sender, EventArgs e)
        {
            // Check if there's a state to undo to
            if (undoStack.Count > 0)
            {
                // Save the current state of the drawing
                var currentState = points.ToList();
                redoStack.Push(currentState);

                // Restore the previous state
                points = undoStack.Pop();
                pictureBox1.Invalidate(); // Force a redraw of the picture box
            }
            else
            {
                // Show a message indicating that there is nothing to undo
                MessageBox.Show("There is nothing to undo.", "Undo");
            }
        }


        private void RedoMenuItem_Click(object sender, EventArgs e)
        {
            // Check if there's a state to redo to
            if (redoStack.Count > 0)
            {
                // Save the current state of the drawing
                var currentState = points.ToList();
                undoStack.Push(currentState);

                // Restore the next state
                points = redoStack.Pop();
                pictureBox1.Invalidate(); // Force a redraw of the picture box
            }
            else
            {
                // Show a message indicating that there is nothing to redo
                MessageBox.Show("There is nothing to redo.", "Redo");
            }
        }



        private void SelectMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void SaveAsSvg(string filePath)
        {
            // Replace this with your code for drawing the elements
            List<string> elements = new List<string>
    {
        "<rect x=\"10\" y=\"10\" width=\"50\" height=\"50\" fill=\"red\" />",
        "<circle cx=\"50\" cy=\"50\" r=\"25\" fill=\"blue\" />",
        "<text x=\"10\" y=\"100\" font-size=\"20\">Hello, world!</text>"
    };

            // Create the SVG code with the drawn elements
            string svgCode = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?><svg xmlns=\"http://www.w3.org/2000/svg\">";
            svgCode += string.Join("", elements);
            svgCode += "</svg>";

            // Save the SVG code to the specified file path
            File.WriteAllText(filePath, svgCode);
        }

        private void ShowInBrowserMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                // Set the file name and path for the temporary SVG file
                string fileName = "temp.svg";
                string filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads", fileName);

                // Save the drawn elements as an SVG file to the temporary file path
                SaveAsSvg(filePath);

                // Open the SVG file in Chrome
                var process = new System.Diagnostics.Process();
                process.StartInfo.FileName = @"C:\Program Files\Google\Chrome\Application\chrome.exe";
                process.StartInfo.Arguments = filePath;
                process.Start();
            }
            catch (Exception ex)
            {
                // Show an error message if there was a problem
                MessageBox.Show($"Error opening file: {ex.Message}");
            }
        }





    }
}