﻿using FotNET.NETWORK.LAYERS.CONVOLUTION.SCRIPTS;
using FotNET.NETWORK.LAYERS.CONVOLUTION.SCRIPTS.PADDING;
using FotNET.NETWORK.LAYERS.CONVOLUTION.SCRIPTS.PADDING.SAME;
using FotNET.NETWORK.MATH.Initialization;
using FotNET.NETWORK.MATH.OBJECTS;

namespace FotNET.NETWORK.LAYERS.CONVOLUTION {
    public class ConvolutionLayer : ILayer {
        /// <summary> Layer that perform tensor convolution by filters and biases. </summary>
        /// <param name="filters"> Count of filters on layer. </param>
        /// <param name="filterWeight"> Weight of filters on layer. </param>
        /// <param name="filterHeight"> Height of filters on layer. </param>
        /// <param name="filterDepth"> Depth of filters on layer. </param>
        /// <param name="weightsInitialization"> Type of weights initialization of filters on layer. </param>
        /// <param name="stride"> Stride of convolution. </param>
        /// <param name="padding"> Padding type. </param>
        public ConvolutionLayer(int filters, int filterWeight, int filterHeight, int filterDepth, 
            IWeightsInitialization weightsInitialization, int stride, Padding padding) {
            _backPropagate = true;
            _padding       = padding;
            Filters        = new Filter[filters];
            
            for (var j = 0; j < filters; j++) {
                Filters[j] = new Filter(new List<Matrix>()) {
                    Bias = .001d
                };

                for (var i = 0; i < filterDepth; i++)
                    Filters[j].Channels.Add(new Matrix(
                        new double[filterWeight, filterHeight]));
            }

            foreach (var filter in Filters)
                for (var i = 0; i < filter.Channels.Count; i++)
                    filter.Channels[i] = weightsInitialization.Initialize(filter.Channels[i]);

            _stride = stride;
            Input   = new Tensor(new Matrix(0, 0));
        }

        /// <summary> Layer that perform tensor convolution by filters and biases. </summary>
        /// <param name="filtersPath"> Path to .txt file of filters. </param>
        /// <param name="filterDepth"> Depth of filters on layer. </param>
        /// <param name="stride"> Stride of convolution. </param>
        /// <param name="padding"> Padding type. </param>
        public ConvolutionLayer(string filtersPath, int filterDepth, int stride, Padding padding) {
            var filters = File.ReadAllText(filtersPath).Split("/", StringSplitOptions.RemoveEmptyEntries);
            
            _backPropagate = false;
            _padding       = padding;
            Filters        = new Filter[filters.Length];
            
            for (var j = 0; j < filters.Length; j++) {
                Filters[j] = new Filter(new List<Matrix>()) {
                    Bias = .001d
                };

                for (var i = 0; i < filterDepth; i++)
                    Filters[j].Channels.Add(new Matrix(filters[j]));
            }

            _stride = stride;
            Input   = new Tensor(new Matrix(0, 0));
        }
        
        private readonly int _stride;
        private readonly bool _backPropagate;
        
        private readonly Padding _padding;

        private Filter[] Filters { get; }
        private Tensor Input { get; set; }

        private static Filter[] FlipFilters(Filter[] filters) {
            for (var i = 0; i < filters.Length; i++)
                filters[i] = filters[i].Flip().AsFilter();

            return filters;
        }

        private static Filter[] GetFiltersWithoutBiases(Filter[] filters) {
            for (var i = 0; i < filters.Length; i++)
                filters[i] = new Filter(filters[i].Channels);

            return filters;
        }

        public Tensor GetValues() => Input;

        public Tensor GetNextLayer(Tensor tensor) {
            Input = new Tensor(new List<Matrix>(_padding.GetPadding(new Tensor(tensor.Channels)).Channels));
            return Convolution.GetConvolution(_padding.GetPadding(new Tensor(tensor.Channels)), Filters, _stride);
        }

        public Tensor BackPropagate(Tensor error, double learningRate, bool backPropagate) {
            var inputTensor = Input;
            var extendedInput = inputTensor.GetSameChannels(error);

            var originalFilters = new Filter[Filters.Length];
            for (var i = 0; i < Filters.Length; i++)
                originalFilters[i] = new Filter(new List<Matrix>(Filters[i].Channels));
            
            for (var i = 0; i < originalFilters.Length; i++)
                originalFilters[i] = originalFilters[i].GetSameChannels(error).AsFilter();

            if (_backPropagate && backPropagate)
                Parallel.For(0, Filters.Length, filter => {
                    for (var channel = 0; channel < Filters[filter].Channels.Count; channel++) 
                        Filters[filter].Channels[channel] -= Convolution.GetConvolution(
                            extendedInput.Channels[filter],error.Channels[filter],
                            _stride, Filters[filter].Bias) * learningRate;
                    
                    Filters[filter].Bias -= error.Channels[filter].Sum() * learningRate;
                });

            return Convolution.GetConvolution(new SamePadding(originalFilters[0]).GetPadding(error), 
                FlipFilters(GetFiltersWithoutBiases(originalFilters)), _stride);
        }

        public string GetData() {
            var temp = "";
            
            foreach (var filter in Filters) {
                temp = filter.Channels.Aggregate(temp, (current, channel) => current + channel.GetValues());
                temp += filter.Bias + " ";
            }
            
            return temp;
        }

        public string LoadData(string data) {
            var position = 0;
            var dataNumbers = data.Split(" ",  StringSplitOptions.RemoveEmptyEntries);

            foreach (var filter in Filters) {
                foreach (var channel in filter.Channels)
                    for (var x = 0; x < channel.Rows; x++)
                        for (var y = 0; y < channel.Columns; y++)
                            channel.Body[x, y] = double.Parse(dataNumbers[position++]);

                filter.Bias = double.Parse(dataNumbers[position++]);
            }

            return string.Join(" ", dataNumbers.Skip(position).Select(p => p.ToString()).ToArray());
        }
    }
}