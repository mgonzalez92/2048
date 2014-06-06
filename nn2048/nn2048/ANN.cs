using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nn2048
{
    class ANN
    {
        public int fitness = 0;
        public const int LAYERNUM = 3;
        public NeuronLayer[] neuronLayers = new NeuronLayer[LAYERNUM];

        public ANN(int inputs, int hidden, int outputs)
        {
            //Input layer
            neuronLayers[0] = CreateNeuronLayer(inputs, 0);
            //Hidden layer
            neuronLayers[1] = CreateNeuronLayer(hidden, inputs);
            //Output layer
            neuronLayers[2] = CreateNeuronLayer(outputs, hidden);
        }

        NeuronLayer CreateNeuronLayer(int neuronNum, int inputNum)
        {
            NeuronLayer neuronLayer = new NeuronLayer();
            neuronLayer.neuronNum = neuronNum;
            neuronLayer.neurons = new Neuron[neuronNum];
            for (int i = 0; i < neuronNum; i++)
            {
                neuronLayer.neurons[i] = CreateNeuron(inputNum);
            }
            return neuronLayer;
        }

        Neuron CreateNeuron(int inputNum)
        {
            Neuron neuron = new Neuron();
            neuron.inputNum = inputNum;
            neuron.weights = new double[inputNum + 1]; //+1 for bias
            return neuron;
        }
    }

    struct Neuron
    {
        public int inputNum;
        public double[] weights;
    }

    struct NeuronLayer
    {
        public int neuronNum;
        public Neuron[] neurons;
    }
}