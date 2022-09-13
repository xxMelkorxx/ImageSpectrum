﻿using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;

namespace ImageSpectrum
{
	class Surface
	{
		public int width, height;
		public double[,] matrix;
		public double[,] spectrum;
		public Complex[,] matrixComplex;
		public Complex[,] spectrumComplex;

		public Surface(int width, int height)
		{
			this.width = width;
			this.height = height;
			matrix = new double[width, height];
		}

		private double GaussFunction(double x, double y, double a, double sigma_x, double sigma_y, double shift_x, double shift_y)
		{
			return a * Math.Exp(-(Math.Pow((x - shift_x) / sigma_x, 2) + Math.Pow((y - shift_y) / sigma_y, 2)));
		}

		public void CreateSurfaceGauss(double[] a, double[] sigma_x, double[] sigma_y, double[] shift_x, double[] shift_y)
		{
			for (int i = 0; i < width; i++)
				for (int j = 0; j < height; j++)
					for (int k = 0; k < a.Length; k++)
						matrix[i, j] += GaussFunction(i, j, a[k], sigma_x[k], sigma_y[k], shift_x[k], shift_y[k]);
		}

		public void AddNoise(double snr)
		{
			Random rnd = new Random(DateTime.Now.Millisecond);

			double[,] randomValues = new double[width, height];
			double energyRandValues = 0;
			for (int i = 0; i < width; i++)
				for (int j = 0; j < height; j++)
				{
					randomValues[i, j] = 0;
					for (int n = 0; n < 12; n++)
						randomValues[i, j] += rnd.Next(0, 100);
					randomValues[i, j] /= 12;
					energyRandValues += randomValues[i, j] * randomValues[i, j];
				}

			// Подсчёт энергии шума относительно энергии.
			double energySignal = 0;
			for (int i = 0; i < width; i++)
				for (int j = 0; j < height; j++)
					energySignal += matrix[i, j] * matrix[i, j];
			double energyNoise = energySignal * (snr / 100.0);

			// Нормировка случайной последовательности.
			for (int i = 0; i < width; i++)
				for (int j = 0; j < height; j++)
					randomValues[i, j] = randomValues[i, j] * Math.Sqrt(energyNoise / energyRandValues);

			// Накладывание шума на исходный сигнал.
			for (int i = 0; i < width; i++)
				for (int j = 0; j < height; j++)
					matrix[i, j] += randomValues[i, j];

		}

		public void FFT_2D(bool direct)
		{
			matrixComplex = ConvertToComplex(matrix);
			spectrumComplex = new Complex[width, height];
			
			Complex[] row = new Complex[width];
			Complex[] column = new Complex[height];

			for (int h = 0; h < height; h++)
			{
				for (int w = 0; w < width; w++)
					row[w] = spectrumComplex[h, w];
				row = FFT.DecimationInFrequency(row, direct);
				for (int w = 0; w < width; w++)
					spectrumComplex[w, h] = row[w];
			}

			for (int w = 0; w < width; w++)
			{
				for (int h = 0; h < width; h++)
					column[h] = spectrum[w, h];
				column = FFT.DecimationInFrequency(column, direct);
				for (int h = 0; h < width; h++)
					spectrumComplex[w, h] = column[h];
			}
		}

		public static double[,] TransformSpectrum(double[,] spectrum)
		{
			int width = spectrum.GetLength(0);
			int height = spectrum.GetLength(1);

			double[,] spectrumTransform = new double[width, height];

			return spectrumTransform;
		}

		public static Complex[,] ConvertToComplex(double [,] matrix)
		{
			int width = matrix.GetLength(0);
			int height = matrix.GetLength(1);

			Complex[,] matrixComplex = new Complex[width, height];
			for (int i = 0; i < width; i++)
				for (int j = 0; j < height; j++)
					matrixComplex[i, j] = matrix[i, j];

			return matrixComplex;
		}

		public static double[,] GetMagnitude(Complex[,] spectrum)
		{
			int width = spectrum.GetLength(0);
			int height = spectrum.GetLength(1);

			double[,] spectrumMatrix = new double[width, height];
			for (int i = 0; i < width; i++)
				for (int j = 0; j < height; j++)
					spectrumMatrix[i, j] = spectrum[i, j].Magnitude;

			return spectrumMatrix;
		}

		public static Bitmap GetBitmap(double[,] matrix)
		{
			int width = matrix.GetLength(0);
			int height = matrix.GetLength(1);

			Bitmap bmp = new Bitmap(width, height);
			int[,] matrixRGB = ConvertToMatrixRGB(matrix);
			for (int i = 0; i < width; i++)
				for (int j = 0; j < height; j++)
					bmp.SetPixel(i, j, Color.FromArgb(matrixRGB[i, j], matrixRGB[i, j], matrixRGB[i, j]));

			return bmp;
		}

		private static int[,] ConvertToMatrixRGB(double[,] matrix)
		{
			int width = matrix.GetLength(0);
			int height = matrix.GetLength(1);

			double max = 0;
			for (int i = 0; i < width; i++)
			{
				for (int j = 0; j < height; j++)
					if (matrix[i, j] > max) max = matrix[i, j];
			}

			int[,] matrixRGB = new int[width, height];
			for (int i = 0; i < width; i++)
				for (int j = 0; j < height; j++)
					matrixRGB[i, j] = (int)Math.Abs(matrix[i, j] / max * 255);

			return matrixRGB;
		}

		private static Complex[,] ConvertToMatrixComplex(double[,] matrix)
		{
			int width = matrix.GetLength(0);
			int height = matrix.GetLength(1);

			Complex[,] matrixComplex = new Complex[width, height];
			for (int i = 0; i < width; i++)
				for (int j = 0; j < height; j++)
					matrixComplex[i, j] = matrix[i, j];

			return matrixComplex;
		}
	}
}
