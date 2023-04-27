using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using Ultimate_Sketch;

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


    }
}
