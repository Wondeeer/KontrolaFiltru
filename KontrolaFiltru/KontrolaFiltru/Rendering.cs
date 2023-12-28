using System.Collections.Generic;
using System.Drawing;


namespace KontrolaFiltru {
    internal class Rendering {

        Point[] greenPoint = new Point[5];
        Point[] redPoint = new Point[5];

        Pen greenPen = new Pen(Color.Green, 15);
        Pen redPen = new Pen(Color.Red, 15);


        public void setGreenPoint(int indexArray) {
            this.greenPoint[indexArray] = new Point(1, 0);
        }
        public Point getGreenPoint(int indexArray) {
            return greenPoint[indexArray];
        }

        // Zakreslení filtru bez chyb
        public void greenRendering(int i, ref int firstLightPoint, Graphics graphics, Bitmap bmp, int pocetBoduY, int axisY, ref List<int> arrayMedian) {
            if (i == bmp.Height - 1) {
                greenPoint[2] = new Point(MathFunction.getMedian(arrayMedian) - 280, i);
                greenPoint[3] = new Point(MathFunction.getMedian(arrayMedian) + 10, i);
                renderingUpperPart(axisY, arrayMedian);
            }
            else {
                greenPoint[2] = new Point(MathFunction.getMedian(arrayMedian) - 280, i - 15);
                greenPoint[3] = new Point(MathFunction.getMedian(arrayMedian) + 10, i - 15);
                renderingUpperPart(axisY, arrayMedian);
            }

            if ((arrayMedian.Count > 50) || (arrayMedian.Count > 10 && pocetBoduY > 80))
                graphics.DrawLines(greenPen, greenPoint);

            firstLightPoint = 0;
            greenPoint[0] = Point.Empty;
            arrayMedian.Clear();
        }

        // Zakreslení chyby ve filtru
        public void errorRendering(Graphics graphics, int i, List<int> arrayMedian, ref int errorDetection, ref bool isErrorDetection) {
            redPoint[0] = new Point(MathFunction.getMedian(arrayMedian), i - errorDetection);
            redPoint[4] = redPoint[0];
            redPoint[1] = new Point(MathFunction.getMedian(arrayMedian) - 280, i - errorDetection);
            redPoint[3] = new Point(MathFunction.getMedian(arrayMedian), i);
            redPoint[2] = new Point(MathFunction.getMedian(arrayMedian) - 280, i);
            graphics.DrawLines(redPen, redPoint);
            redPoint[0] = Point.Empty;
            redPoint[1] = Point.Empty;
            redPoint[2] = Point.Empty;
            redPoint[3] = Point.Empty;
            redPoint[4] = Point.Empty;
            isErrorDetection = true;
            errorDetection = 0;
        }

        public void renderingUpperPart(int axisY, List<int> arrayMedian) {
            greenPoint[0] = new Point(MathFunction.getMedian(arrayMedian) + 10, axisY);
            greenPoint[4] = greenPoint[0];
            greenPoint[1] = new Point(MathFunction.getMedian(arrayMedian) - 280, axisY);
        }

    }
}

