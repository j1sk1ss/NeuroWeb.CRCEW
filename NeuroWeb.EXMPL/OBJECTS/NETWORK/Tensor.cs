﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using NeuroWeb.EXMPL.OBJECTS.MATH;

namespace NeuroWeb.EXMPL.OBJECTS.NETWORK {
    public class Tensor {
        public Tensor(Matrix matrix) => Channels = new List<Matrix> { matrix };
        
        public Tensor(List<Matrix> matrix) => Channels = matrix;

        public List<Matrix> Channels { get; protected set; }
        
        public List<double> Flatten() {
            var flatten = new List<double>();
            foreach (var matrix in Channels) flatten.AddRange(matrix.GetAsList());
            return flatten;
        }
        
        public Filter GetFlip() {
            var tensor = Channels;
            
            foreach (var matrix in tensor)
                matrix.GetFlip();
            
            return new Filter(tensor);
        }

        public Tensor GetSameChannels(Tensor reference) {
            if (Channels.Count != reference.Channels.Count) {
                return Channels.Count < reference.Channels.Count
                    ? IncreaseChannels(reference.Channels.Count - Channels.Count)
                    : CropChannels(reference.Channels.Count);
            }

            return this;
        }

        private Tensor IncreaseChannels(int channels) {
            var tensor = new Tensor(Channels);
            
            for (var i = 0; i < channels; i++) tensor.Channels.Add(tensor.Channels[^1]);
            
            return tensor;
        }
        
        private Tensor CropChannels(int channels) {
            var matrix = new List<Matrix>();

            for (var i = 0; i < channels * 2; i += 2) {
                matrix.Add(Channels[i]);
                for (var x = 0; x < Channels[i].Body.GetLength(0); x++) {
                    for (var y = 0; y < Channels[i].Body.GetLength(1); y++) {
                        matrix[^1].Body[x,y] = Math.Min(Channels[i].Body[x, y], Channels[i + 1].Body[x, y]);
                    }
                }
            }

            return new Tensor(matrix);
        }
        
        public double TensorSum() => Channels.Sum(matrix => matrix.GetSum());
        
        public static Tensor operator +(Tensor tensor1, Tensor tensor2) {
            var endTensor = new Tensor(tensor1.Channels);
            
            for (var i = 0; i < endTensor.Channels.Count; i++)
                endTensor.Channels[i] = tensor1.Channels[i] + tensor2.Channels[i];
            
            return endTensor;
        }
        
        public static Tensor operator -(Tensor tensor1, Tensor tensor2) {
            var endTensor = new Tensor(tensor1.Channels);
            
            for (var i = 0; i < endTensor.Channels.Count; i++)
                endTensor.Channels[i] = tensor1.Channels[i] - tensor2.Channels[i];
            
            return endTensor;
        }
        
        public static Tensor operator *(Tensor tensor1, Tensor tensor2) {
            var endTensor = new Tensor(tensor1.Channels);

            for (var i = 0; i < endTensor.Channels.Count; i++)
                endTensor.Channels[i] = tensor1.Channels[i] * tensor2.Channels[i];

            return endTensor;
        }
        
        public static Tensor operator -(Tensor tensor1, double value) {
            var endTensor = new Tensor(tensor1.Channels);

            for (var i = 0; i < tensor1.Channels.Count; i++)
                tensor1.Channels[i] -= value;

            return endTensor;
        }        
        
        public static Tensor operator *(Tensor tensor1, double value) {
            var endTensor = new Tensor(tensor1.Channels);
            
            for (var i = 0; i < tensor1.Channels.Count; i++)
                tensor1.Channels[i] *= value;

            return endTensor;
        }
        
        public string GetInfo() {
            return $"x: {Channels[0].Body.GetLength(0)}\n" +
                   $"y: {Channels[0].Body.GetLength(1)}\n" +
                   $"depth: {Channels.Count}";
        }

        public Filter AsFilter() => new Filter(Channels);
    }
    
    public class Filter : Tensor {
        public Filter(List<Matrix> matrix) : base(matrix) {
            Bias = new List<double>();
            for (var i = 0; i < matrix.Count; i++) Bias.Add(0);
            
            Channels = matrix;
        }
        
        public List<double> Bias { get; }

        public Tensor AsTensor() => new Tensor(Channels);
    }
}