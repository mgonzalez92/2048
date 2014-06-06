using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace nn2048
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int POPULATION = 128;
        //16 squares * 16 possible values (up to 65536)
        const int INPUTS = 16;
        const int HIDDEN = 12;
        const int OUTPUTS = 1;
        const int SPEED1 = 256;
        const int SPEED2 = 128;
        const int SPEED3 = 32;
        const int SPEED4 = 8;
        const int LAYERNUM = 3;
        const int WIDTH = 4;
        const int STUCK = 4;
        const string netPath = @"C:\Users\Matt\Documents\Visual Studio 2012\Projects\C#\nn2048\nn2048\net.txt";

        ANN[] ann = new ANN[POPULATION];
        Game game;
        DispatcherTimer timer;
        Random random = new Random();
        int c = 0;
        int gen = 0;
        int[,] prevBoard = new int[WIDTH, WIDTH];
        int totalFitness = 0;
        int avgFitness = 0;
        int maxFitness = 0;
        int finalMaxFitness = 0;

        TextBlock Score;
        TextBlock Info;
        Button playButton;
        Button newNetButton;
        Button loadNetButton;
        Button saveNetButton;

        public MainWindow()
        {
            InitializeComponent();

            playButton = CreateButton(0, 100, "Play Game");
            newNetButton = CreateButton(0, 140, "New Network");
            loadNetButton = CreateButton(0, 180, "Load Network");
            saveNetButton = CreateButton(235, 325, "Save");
            saveNetButton.HorizontalAlignment = HorizontalAlignment.Left;
            saveNetButton.Width = 50;
            saveNetButton.Height = 20;

            playButton.Click += PlayGame;
            newNetButton.Click += NewNetwork;
            loadNetButton.Click += LoadNetwork;
            saveNetButton.Click += SaveNetworkEvent;

            rootGrid.Children.Add(playButton);
            rootGrid.Children.Add(newNetButton);
            rootGrid.Children.Add(loadNetButton);

            //Textblocks
            Score = new TextBlock();
            Score.Margin = new Thickness(5, 305, 0, 0);
            Score.Foreground = new SolidColorBrush(Colors.Black);
            Score.Text = "Score: 0";
            Score.FontSize = 16;

            Info = new TextBlock();
            Info.Margin = new Thickness(5, 325, 0, 0);
            Info.Foreground = new SolidColorBrush(Colors.Black);
            Info.Text = "Gen: 0, Type: 0";
            Info.FontSize = 16;
        }

        public Button CreateButton(int x, int y, string s)
        {
            Button button = new Button();
            button.Width = 100;
            button.Height = 25;
            button.HorizontalAlignment = HorizontalAlignment.Center;
            button.VerticalAlignment = VerticalAlignment.Top;
            button.Margin = new Thickness(x, y, 0, 0);
            button.Content = s;
            return button;
        }

        public void PlayGame(object sender, EventArgs e)
        {
            rootGrid.Children.Clear();
            rootGrid.Children.Add(Score);

            //Old way
            game = new Game(rootGrid, random);

            KeyDown += PlayerMove;
        }

        void PlayerMove(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
                game.Update(0, false);
            else if (e.Key == Key.Right)
                game.Update(1, false);
            else if (e.Key == Key.Down)
                game.Update(2, false);
            else if (e.Key == Key.Left)
                game.Update(3, false);

            if (e.Key == Key.Up || e.Key == Key.Right || e.Key == Key.Down || e.Key == Key.Left)
            {
                game.UpdateDisplay();
                Score.Text = "Score: " + game.score;
            }
        }

        public void NewNetwork(object sender, EventArgs e)
        {
            rootGrid.Children.Clear();
            rootGrid.Children.Add(Score);
            rootGrid.Children.Add(Info);
            rootGrid.Children.Add(saveNetButton);

            //Create randomized ann's
            for (int i = 0; i < POPULATION; i++)
            {
                //Create ann
                ann[i] = new ANN(INPUTS, HIDDEN, OUTPUTS);
                //For each neuron layer
                for (int j = 0; j < LAYERNUM; j++)
                {
                    //For each neuron in that layer
                    for (int k = 0; k < ann[i].neuronLayers[j].neuronNum; k++)
                    {
                        for (int l = 0; l < ann[i].neuronLayers[j].neurons[k].inputNum + 1; l++)
                        {
                            ann[i].neuronLayers[j].neurons[k].weights[l] = random.NextDouble();
                        }
                    }
                }
            }

            //Start a game
            game = new Game(rootGrid, random);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(SPEED2);
            timer.Tick += delegate { Update(); game.UpdateDisplay(); UpdateDisplay(); };
            timer.Start();

            KeyDown += AdjustSpeed;
        }

        public void LoadNetwork(object sender, EventArgs e)
        {
            rootGrid.Children.Clear();
            rootGrid.Children.Add(Score);
            rootGrid.Children.Add(Info);
            rootGrid.Children.Add(saveNetButton);

            // Create a string to read into
            int numWeights = (INPUTS + 1) * HIDDEN + (HIDDEN + 1) * OUTPUTS;
            string[] lines = new string[numWeights + 1];

            //Initialize networks
            for (int i = 0; i < ann.Length; i++)
            {
                ann[i] = new ANN(INPUTS, HIDDEN, OUTPUTS);
            }

            //Read file
            if (System.IO.File.Exists(netPath))
            {
                lines = System.IO.File.ReadAllLines(netPath);
            }
            gen = Convert.ToInt32(lines[2]);
            for (int i = 0; i < ann.Length; i++)
            {
                int ii = 3;
                for (int j = 1; j < 3; j++)
                {
                    for (int k = 0; k < ann[i].neuronLayers[j].neuronNum; k++)
                    {
                        for (int l = 0; l < ann[i].neuronLayers[j].neurons[k].inputNum + 1; l++)
                        {
                            ann[i].neuronLayers[j].neurons[k].weights[l] = Convert.ToDouble(lines[ii]);
                            ii++;
                        }
                    }
                }
            }
            GA.Mutate(ann);

            //Start a game
            game = new Game(rootGrid, random);

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(SPEED2);
            timer.Tick += delegate { Update(); game.UpdateDisplay(); UpdateDisplay(); };
            timer.Start();

            KeyDown += AdjustSpeed;
        }

        public void SaveNetworkEvent(object sender, EventArgs e)
        {
            SaveNetwork();
        }

        public void SaveNetwork()
        {
            // Create a string to write
            int numWeights = (INPUTS + 1) * HIDDEN + (HIDDEN + 1) * OUTPUTS;
            string[] lines = new string[numWeights + 3];

            //Find nn with highest fitness
            int highest = 0;
            int i = 1;
            for (i = 1; i < ann.Length; i++)
            {
                if (ann[i].fitness > ann[highest].fitness)
                    highest = i;
            }

            bool isHighest = true;
            if (System.IO.File.Exists(netPath))
            {
                string[] readLines = System.IO.File.ReadAllLines(netPath);
                if (Convert.ToInt32(readLines[1]) > ann[highest].fitness)
                {
                    //isHighest = false;
                }
            }
            if (!System.IO.File.Exists(netPath) || isHighest)
            {
                lines[0] = "strain";
                lines[1] = ann[highest].fitness.ToString();
                lines[2] = gen.ToString();

                i = 3;
                for (int j = 1; j < 3; j++)
                {
                    for (int k = 0; k < ann[highest].neuronLayers[j].neuronNum; k++)
                    {
                        for (int l = 0; l < ann[highest].neuronLayers[j].neurons[k].inputNum + 1; l++)
                        {
                            lines[i] = ann[highest].neuronLayers[j].neurons[k].weights[l].ToString();
                            i++;
                        }
                    }
                }

                // WriteAllLines creates a file, writes a collection of strings it and then closes it
                System.IO.File.WriteAllLines(netPath, lines);
            }
        }

        void AdjustSpeed(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.D1)
                timer.Interval = TimeSpan.FromMilliseconds(SPEED1);
            else if (e.Key == Key.D2)
                timer.Interval = TimeSpan.FromMilliseconds(SPEED2);
            else if (e.Key == Key.D3)
                timer.Interval = TimeSpan.FromMilliseconds(SPEED3);
            else if (e.Key == Key.D4)
                timer.Interval = TimeSpan.FromTicks(SPEED4);
            else if (e.Key == Key.D5)
            {
                timer.IsEnabled = false;
                int goal = 10;
                if (gen < 100) { goal = 100; }
                else if (gen < 500) { goal = 500; }
                else if (gen < 1000) { goal = 1000; }
                else if (gen < 5000) { goal = 5000; }
                while (gen < goal)
                {
                    Update();
                }
                timer.IsEnabled = true;
            }
        }

        void Update()
        {
            for (int y = 0; y < WIDTH; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    prevBoard[x, y] = game.board[x, y];
                }
            }

            //Check each direction to see if it is a legal move
            int prevScore = game.score;
            double[] outputs = new double[4];
            for (int i = 0; i < 4; i++)
            {
                //Move in a direction
                game.Update(i, true);

                //Check if legal
                bool legal = false;
                for (int y = 0; y < WIDTH; y++)
                {
                    for (int x = 0; x < WIDTH; x++)
                    {
                        if (prevBoard[x, y] != game.board[x, y])
                        {
                            legal = true;
                            break;
                        }
                    }
                    if (legal) { break; }
                }

                //If legal, feed to network
                if (legal)
                    outputs[i] = ComputeNetwork(ann[c]);
                else
                    outputs[i] = 0;

                //Revert game state for next check
                game.score = prevScore;
                game.board = prevBoard;
            }

            //Update game with highest output
            int highest = 0;
            for (int i = 1; i < 4; i++)
            {
                if (outputs[i] > outputs[highest])
                    highest = i;
            }
            game.Update(highest, false);

            if (game.end)
            {
                ann[c].fitness = game.score;
                if (game.score > maxFitness)
                    maxFitness = game.score;
                c++;
                game = null;
                rootGrid.Children.Clear();
                rootGrid.Children.Add(Info);
                rootGrid.Children.Add(Score);
                rootGrid.Children.Add(saveNetButton);
                game = new Game(rootGrid, random);
            }
            if (c >= POPULATION)
            {
                c = 0;
                gen++;

                //Total fitness
                totalFitness = 0;
                for (int i = 0; i < POPULATION; i++)
                {
                    totalFitness += ann[i].fitness;
                }
                avgFitness = totalFitness / POPULATION;
                finalMaxFitness = maxFitness;
                maxFitness = 0;
                Console.Write("gen: " + gen.ToString() + ", avg: " + avgFitness + "\n");
                if (gen % 100 == 0)
                {
                    SaveNetwork();
                }

                GA.Compute(ann);
            }
        }

        void UpdateDisplay()
        {
            //Update Score text
            Score.Text = "Avg: " + avgFitness + " Max: " + finalMaxFitness + " Score: " + game.score;

            //Update Info text
            Info.Text = "Gen: " + gen + " Samp: " + c;
        }

        double ComputeNetwork(ANN nn)
        {
            //Inputs
            int[] inputs = new int[INPUTS];
            for (int i = 0; i < INPUTS; i++)
            {
                //inputs[i] = game.board[i % WIDTH, i / WIDTH];
                if (game.board[i % WIDTH, i / WIDTH] == 0)
                    inputs[i] = 0;
                else
                    inputs[i] = (int)Math.Log(game.board[i % WIDTH, i / WIDTH], 2);
            }
            /*int[] inputs = new int[INPUTS];
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    if ((int)Math.Log(game.board[i % WIDTH, i / WIDTH], 2) == j + 1)
                        inputs[i * 16 + j] = 1;
                    else
                        inputs[i * 16 + j] = 0;
                }
            }*/

            //Compute values at hidden neurons
            double[] hidden = new double[HIDDEN];
            for (int i = 0; i < HIDDEN; i++)
            {
                hidden[i] = 0;
                for (int j = 0; j < INPUTS; j++)
                {
                    hidden[i] += inputs[j] * nn.neuronLayers[1].neurons[i].weights[j];
                }
                hidden[i] -= nn.neuronLayers[1].neurons[i].weights[INPUTS];
                hidden[i] = (1 / (float)(1 + Math.Pow(Math.E, -hidden[i])));
            }

            //Compute values at output neurons
            double[] outputs = new double[OUTPUTS];
            for (int i = 0; i < OUTPUTS; i++)
            {
                outputs[i] = 0;
                for (int j = 0; j < HIDDEN; j++)
                {
                    outputs[i] += hidden[j] * nn.neuronLayers[2].neurons[i].weights[j];
                }
                outputs[i] -= nn.neuronLayers[2].neurons[i].weights[HIDDEN];
                outputs[i] = (1 / (float)(1 + Math.Pow(Math.E, -outputs[i])));
            }

            return outputs[0];
        }

        void GeneticAlgorithm()
        {
            ANN[] tmp = new ANN[POPULATION];

            //Find best half
            int BEST = POPULATION / 2;
            int[] best = new int[BEST];

            for (int i = 0; i < BEST; i++)
            {
                best[i] = -1;
                for (int j = 0; j < POPULATION; j++)
                {
                    if (!best.Contains(j))
                    {
                        if (best[i] == -1)
                            best[i] = j;
                        else if (ann[j].fitness > ann[best[i]].fitness)
                            best[i] = j;
                    }
                }
            }

            int selectFitness = 0;
            for (int i = 0; i < BEST; i++)
            {
                selectFitness += ann[best[i]].fitness;
            }

            //Create new ann's
            for (int i = 0; i < POPULATION; i++)
            {
                //Create ann
                tmp[i] = new ANN(INPUTS, HIDDEN, OUTPUTS);
                
                //Pick two parents
                ANN parent1 = ann[0], parent2 = ann[1];
                int parentFitness = random.Next(0, selectFitness);
                int currentFitness = 0;
                for (int j = 0; j < BEST; j++)
                {
                    currentFitness += ann[best[j]].fitness;
                    if (parentFitness < currentFitness)
                    {
                        parent1 = ann[best[j]];
                        break;
                    }
                }
                parentFitness = random.Next(0, selectFitness);
                currentFitness = 0;
                for (int j = 0; j < BEST; j++)
                {
                    currentFitness += ann[best[j]].fitness;
                    if (parentFitness < currentFitness)
                    {
                        parent2 = ann[best[j]];
                        break;
                    }
                }

                //Pick a random division of their weights
                int numWeights = (INPUTS + 1) * HIDDEN + (HIDDEN + 1) * OUTPUTS;
                int division = 1 + random.Next(0, numWeights - 1);

                //Assign weights
                int ii = 0;
                //For each neuron layer
                for (int j = 0; j < LAYERNUM; j++)
                {
                    //For each neuron in that layer
                    for (int k = 0; k < tmp[i].neuronLayers[j].neuronNum; k++)
                    {
                        for (int l = 0; l < tmp[i].neuronLayers[j].neurons[k].inputNum + 1; l++)
                        {
                            //Parent 1
                            if (ii < division)
                                tmp[i].neuronLayers[j].neurons[k].weights[l] = parent1.neuronLayers[j].neurons[k].weights[l];
                            //Parent 2
                            else
                                tmp[i].neuronLayers[j].neurons[k].weights[l] = parent2.neuronLayers[j].neurons[k].weights[l];
                            ii++;

                            //Small mutation
                            float mutation = (random.Next(0, 2001) - 1000) / 20000f;
                            tmp[i].neuronLayers[j].neurons[k].weights[l] *= (1 + mutation);
                        }
                    }
                }
            }

            //Assign old ann's to the new population
            for (int i = 0; i < POPULATION; i++)
            {
                ann[i] = tmp[i];
            }
        }
    }
}
