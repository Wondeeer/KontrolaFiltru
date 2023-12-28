using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace KontrolaFiltru {
    internal static class MathFunction {
        //Vypočet průměrné + přidanou hodnotu pro rozdělení obrázku 
        public static int getAverage(List<int> array, bool testedSide) {
            try {
                // "RemoveAt" namísto "Remove.Min-Max" pro případ vícero čísel v MINMAXu.
                array.Sort();
                array.RemoveAt(0);
                array.RemoveAt(array.Count - 1);

                int average = (int)array.Average();
                average = testedSide ? average - (array.Max() - average) * 2 : average + (array.Max() - average) * 2;
                return average;
            }
            catch (Exception ex) {
                MessageBox.Show("Pole neobsauje požadovaný počet polí v metodě getAverage");
                Environment.Exit(1);
            }
            return -1;
        }

        // Výpočet mediánu pro správné zakreslení Ok/Error filtru
        public static int getMedian(List<int> array) {
            try {
                int median = 0;
                array.Sort();
                if (array.Count % 2 == 0) {
                    median = (array[array.Count / 2] + array[array.Count / 2 - 1]) / 2;
                }
                else {
                    median = array[array.Count / 2];
                }
                return median;
            }
            catch (Exception ex) {
                MessageBox.Show("Pole v metodě getMedian je prázdné");
                Environment.Exit(1);
            }
            return -1;
        }
    }
}
