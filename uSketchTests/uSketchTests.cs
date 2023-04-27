using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace Ultimate_Sketch.uSketchTests
{
    public class uSketchTests
    {
        [Test]
        public void SaveAsSvg_ShouldCreateValidSvgFile()
        {
            // Arrange
            string filePath = "test.svg";
            List<string> elements = new List<string>
        {
            "<rect x=\"10\" y=\"10\" width=\"50\" height=\"50\" fill=\"red\" />",
            "<circle cx=\"50\" cy=\"50\" r=\"25\" fill=\"blue\" />",
            "<text x=\"10\" y=\"100\" font-size=\"20\">Hello, world!</text>"
        };
            string expectedSvgCode = "<?xml version=\"1.0\" encoding=\"UTF-8\" standalone=\"no\"?><svg xmlns=\"http://www.w3.org/2000/svg\">" + string.Join("", elements) + "</svg>";

            // Act
            SaveAsSvg(filePath);

            // Assert
            Assert.IsTrue(File.Exists(filePath), "File was not created.");
            string actualSvgCode = File.ReadAllText(filePath);
            Assert.AreEqual(expectedSvgCode, actualSvgCode, "SVG code does not match.");
        }

        private void SaveAsSvg(string filePath)
        {
            throw new NotImplementedException();
        }

        [Test]
        public void ExportSvgMenuItem_Click_WhenPointsEmpty_ShouldShowErrorMessage()
        {
            // Arrange
            var form = new Form1();
            form.points.Clear();
            var expectedMessage = "There is no drawing to export.";

            var saveFileDialogMock = new Mock<SaveFileDialog>();
            saveFileDialogMock.Setup(x => x.ShowDialog()).Returns(DialogResult.OK);
            saveFileDialogMock.Setup(x => x.FileName).Returns("test.svg");

            // Act
            form.ExportSvgMenuItem_Click(null, null);

            // Assert
            Assert.IsTrue(form.DisplayedErrorMessage);
        }

        [Test]
        public void ClearMenuItem_Click_WhenPointsNotEmpty_ShouldClearDrawingAndStacks()
        {
            // Arrange
            var form = new Form1();
            form.points.Add(new Point(10, 10));
            form.undoStack.Push(new List<Point>(form.points));
            form.redoStack.Push(new List<Point>(form.points));

            // Act
            form.ClearMenuItem_Click(null, null);

            // Assert
            Assert.IsEmpty(form.points);
            Assert.IsEmpty(form.undoStack);
            Assert.IsEmpty(form.redoStack);
        }

        [Test]
        public void ClearMenuItem_Click_WhenPointsEmpty_ShouldShowMessage()
        {
            // Arrange
            var form = new Form1();
            form.points.Clear();
            var expectedMessage = "There is nothing to clear.";

            // Act
            form.ClearMenuItem_Click(null, null);

            // Assert
            Assert.AreEqual(expectedMessage, form.LastShownMessage);
        }



    }
}
