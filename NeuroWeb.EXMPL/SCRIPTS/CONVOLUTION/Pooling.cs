﻿using System;
using System.Collections.Generic;
using NeuroWeb.EXMPL.OBJECTS;

namespace NeuroWeb.EXMPL.SCRIPTS {
    public static class Pooling {
        public static Tensor MaxPool(Tensor picture, int poolSize) {
            var tensor = new Tensor(new List<Matrix>());
            
            foreach (var matrix in picture.Body) {
                tensor.Body.Add(MatrixMaxPool(matrix, poolSize));
            }
            
            return tensor;
        }

        public static Tensor AveragePool(Tensor picture) {
            var tensor = new Tensor(new List<Matrix>());
            
            foreach (var matrix in picture.Body) {
                tensor.Body.Add(MatrixAveragePool(matrix, 3, 1));
            }
            
            return tensor;
        }

        private static Matrix MatrixMaxPool(Matrix matrix, int poolSize) {
            var inputWidth = matrix.Body.GetLength(0);
            var inputHeight = matrix.Body.GetLength(1);
            
            var outputWidth = inputWidth / poolSize;
            var outputHeight = inputHeight / poolSize;

            var output = new double[outputWidth, outputHeight];

            for (var x = 0; x < outputWidth; x++) {
                for (var y = 0; y < outputHeight; y++) {
                    var maxVal = double.MinValue;
                    
                    for (var i = 0; i < poolSize; i++) {
                        for (var j = 0; j < poolSize; j++) {
                            var inputVal = matrix.Body[x * poolSize + i, y * poolSize + j];
                            maxVal = Math.Max(maxVal, inputVal);
                        }
                    }
                    output[x, y] = maxVal;
                }
            }

            return new Matrix(output);
        }
        
        private static Matrix MatrixAveragePool(Matrix matrix, int poolSize, int stride) {
            var inputHeight = matrix.Body.GetLength(0);
            var inputWidth = matrix.Body.GetLength(1);

            var outputHeight = (inputHeight - poolSize) / stride + 1;
            var outputWidth = (inputWidth - poolSize) / stride + 1;

            var output = new double[outputHeight, outputWidth];

            for (var i = 0; i < outputHeight; i++) {
                for (var j = 0; j < outputWidth; j++) {
                    var sum = 0.0;
                    for (var k = i * stride; k < i * stride + poolSize; k++) 
                        for (var l = j * stride; l < j * stride + poolSize; l++) 
                            sum += matrix.Body[k, l];
                    
                    output[i, j] = sum / (poolSize * poolSize);
                }
            }

            return new Matrix(output);
        }
    }
}