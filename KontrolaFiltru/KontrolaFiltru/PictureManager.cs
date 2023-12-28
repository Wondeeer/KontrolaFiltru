
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;


/*
 * Skript na zakreslení bezchybné či chybné části filtru. Pomocí metody splitImage rozdělíme obrázek na část kde je pouze filtr
 * díky čemuž rychlost celého procesu ztrojnásobíme, posléze všechny části spojíme a to metodou mergeImage a následně uložíme.
 * 
 * Cesta k uložení je určena na plochu počítače
 * 
 * atribut threshold je nastaven na 128 aby detekoval rozdíl mezi světlejší a tmavší části(pixelu) na obrázku
 */

namespace KontrolaFiltru {
    public class PictureManager {
        Rendering Rendering = new Rendering();
        Form1 form1 = new Form1();

        string[] filenames;
        string savePath;
        string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);



        int brightness;
        int threshold = 128, dark = 0, firstLightPoint = 0, axisY = 0, countOfAxisY = 0, countOfLightPointInLine = 0;
        int errorDetection = 0, standingOfSavedFile = 1;

        int[] averageArray = new int[2];

        Boolean lightPoint = false, onlyDark = false, isErrorDetection = false, isTestedRightSide;



        List<int> arrayMedian = new List<int>();
        List<int> arrayOfLightPoints = new List<int>();

        Bitmap[] imageParts;

        public PictureManager() {
            Render();
        }

        // Hlavní metoda pro zpracování obrázků |Otevření + uložení|
        public void Render() {
            using (OpenFileDialog dlg = new OpenFileDialog()) {
                dlg.Multiselect = true;
                if (dlg.ShowDialog() == DialogResult.OK) {
                    filenames = dlg.FileNames;
                    foreach (string filename in filenames) {
                        
                        isTestedRightSide = false;
                        Bitmap bmp = new Bitmap(filename);

                        for (int i = 0; i < 2; i++)
                            averageArray[i] = SplitTest(bmp);
                        Array.Sort(averageArray);

                        imageParts = splitImage(bmp, averageArray);

                        Graphics graphics = Graphics.FromImage(imageParts[1]);

                        imageParts[1] = modifyImage(graphics, imageParts[1]);

                        Bitmap mergedImage = mergeImages(imageParts);

                        savePath = @"" + desktopPath + "/finální_obrázek" + standingOfSavedFile + ".jpeg";
                        mergedImage.Save(savePath, ImageFormat.Jpeg);
                        standingOfSavedFile++;
                        Array.Clear(imageParts, 0, imageParts.Length);
                    }
                }
            }
        }

        // Výpočet pro efektivní rozdělení obrázku na části, 20 cyklů nám zajistí potřebné informace
        public int SplitTest(Bitmap bmp) {

            int leftRightWidth = isTestedRightSide ? 520 : 280;
            for (int height = 0; height < bmp.Height; height += bmp.Height / 20) {

                for (int width = isTestedRightSide ? 0 : bmp.Width - 1; width != leftRightWidth; width += isTestedRightSide ? +1 : -1) //Šířka -280 jelikož filtr je cca tak široký a nemusí počítat dále i za cenu, že bude filtr uplně na kraji
                {
                    Color color = bmp.GetPixel(width, height);
                    brightness = (int)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
                    if (brightness > threshold) {
                        arrayOfLightPoints.Add(width);
                        break;
                    }
                }
            }
            int average = MathFunction.getAverage(arrayOfLightPoints, isTestedRightSide);
            arrayOfLightPoints.Clear();
            isTestedRightSide = true;
            return average;
        }

        // Rozdělení obrázku vždy na 3 celky
        public Bitmap[] splitImage(Bitmap originalImage, int[] array) {
            int partHeight = originalImage.Height;
            int tmp = 0;

            Bitmap[] imageParts = new Bitmap[3];

            for (int i = 0; i <= array.Length; i++) {
                switch (i) {
                    case 0: tmp = array[i]; break;
                    case 1: tmp = array[i] - array[i - 1]; break;
                    case 2: tmp = originalImage.Width - array[i - 1]; break;
                }
                imageParts[i] = new Bitmap(tmp, partHeight);
                using (Graphics g = Graphics.FromImage(imageParts[i])) {
                    g.DrawImage(originalImage, new Rectangle(0, 0, tmp, partHeight),
                        new Rectangle(i == 0 ? 0 : array[i - 1], 0, tmp, partHeight), GraphicsUnit.Pixel);
                }
            }
            return imageParts;
        }

        // Spojení modifikovaného obrázku se zbytkem
        public Bitmap mergeImages(Bitmap[] imageParts) {
            int totalWidth = 0;
            int maxHeight = 0;

            foreach (var part in imageParts) {
                totalWidth += part.Width;
                maxHeight = Math.Max(maxHeight, part.Height);
            }

            Bitmap mergedImage = new Bitmap(totalWidth, maxHeight);

            using (Graphics g = Graphics.FromImage(mergedImage)) {
                int x = 0;
                foreach (var part in imageParts) {
                    g.DrawImage(part, new Rectangle(x, 0, part.Width, part.Height));
                    x += part.Width;
                }
            }

            return mergedImage;
        }

        // Operace s rozdělenou částí obrázku
        public Bitmap modifyImage(Graphics graphics, Bitmap splitBitmap) {
            for (int i = 0; i < splitBitmap.Height; i++)//Výška
            {
                if (firstLightPoint > 0 && Rendering.getGreenPoint(0) == Point.Empty) {
                    axisY = i;
                    Rendering.setGreenPoint(0);
                }
                if (isErrorDetection) {
                    errorDetection++;
                    isErrorDetection = false;
                }
                else {
                    if (errorDetection > 25) {
                        Rendering.errorRendering(graphics, i, arrayMedian, ref errorDetection, ref isErrorDetection);
                    }
                    errorDetection = 0;
                }


                if (lightPoint) {
                    dark = 0;
                }
                else if (onlyDark) {
                    dark++;
                }
                if ((dark == 15) || (i == splitBitmap.Height - 1 && arrayMedian.Count > 10)) {
                    Rendering.greenRendering(i, ref firstLightPoint, graphics, imageParts[1], countOfAxisY, axisY, ref arrayMedian);
                }
                lightPoint = false;
                onlyDark = false;
                countOfAxisY = 0;

                for (int j = splitBitmap.Width - 1; j >= 0; j--)//Šířka
                {
                    Color color = splitBitmap.GetPixel(j, i);
                    brightness = (int)(0.299 * color.R + 0.587 * color.G + 0.114 * color.B);
                    if (brightness > threshold) {
                        countOfLightPointInLine++;
                        countOfAxisY++;

                        if (firstLightPoint == 0) {
                            firstLightPoint = j;
                        }
                        if (!lightPoint) {
                            lightPoint = true;
                            arrayMedian.Add(j);
                        }
                        onlyDark = false;
                    }
                    else {
                        if (countOfLightPointInLine > 30) {
                            isErrorDetection = true;
                        }

                        countOfLightPointInLine = 0;
                        if (!lightPoint)
                            onlyDark = true;
                    }
                }

            }
            return splitBitmap;
        }
    }
}


