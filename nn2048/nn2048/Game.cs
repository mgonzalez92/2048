using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace nn2048
{
    class Game
    {
        const int WIDTH = 4;
        const int BOARDIMGSIZE = 296;
        const int PIECEIMGSIZE = 64;

        public int[,] board = new int[WIDTH, WIDTH];
        Random random;
        Image Board = new Image();
        Image[] Pieces = new Image[WIDTH * WIDTH];
        TextBlock[] Texts = new TextBlock[WIDTH * WIDTH];
        BitmapImage big;
        BitmapImage s2, s4, s8, s16, s32, s64, s128, s256, s512,
            s1024, s2048, s4096, s8192, s16384, s32768, gray;
        Grid grid;
        public int score = 0;
        public bool end = false;

        public Game(Grid rootGrid, Random rand)
        {
            grid = rootGrid;
            random = rand;

            Load();

	        //Initialize board to be empty
	        for (int y = 0; y < WIDTH; y++)
	        {
		        for (int x = 0; x < WIDTH; x++)
		        {
			        board[x, y] = 0;
		        }
	        }

	        //Set two random pieces
	        SetPiece();
	        SetPiece();

            UpdateDisplay();
        }

        void Load()
        {
            big = new BitmapImage(new Uri("grid.png", UriKind.RelativeOrAbsolute));
            s2 = new BitmapImage(new Uri("s2.png", UriKind.RelativeOrAbsolute));
            s4 = new BitmapImage(new Uri("s4.png", UriKind.RelativeOrAbsolute));
            s8 = new BitmapImage(new Uri("s8.png", UriKind.RelativeOrAbsolute));
            s16 = new BitmapImage(new Uri("s16.png", UriKind.RelativeOrAbsolute));
            s32 = new BitmapImage(new Uri("s32.png", UriKind.RelativeOrAbsolute));
            s64 = new BitmapImage(new Uri("s64.png", UriKind.RelativeOrAbsolute));
            s128 = new BitmapImage(new Uri("s128.png", UriKind.RelativeOrAbsolute));
            s256 = new BitmapImage(new Uri("s256.png", UriKind.RelativeOrAbsolute));
            s512 = new BitmapImage(new Uri("s512.png", UriKind.RelativeOrAbsolute));
            s1024 = new BitmapImage(new Uri("s1024.png", UriKind.RelativeOrAbsolute));
            s2048 = new BitmapImage(new Uri("s2048.png", UriKind.RelativeOrAbsolute));
            s4096 = new BitmapImage(new Uri("s4096.png", UriKind.RelativeOrAbsolute));
            s8192 = new BitmapImage(new Uri("s8192.png", UriKind.RelativeOrAbsolute));
            s16384 = new BitmapImage(new Uri("s16384.png", UriKind.RelativeOrAbsolute));
            s32768 = new BitmapImage(new Uri("s32768.png", UriKind.RelativeOrAbsolute));
            gray = new BitmapImage(new Uri("s65536.png", UriKind.RelativeOrAbsolute));
            Board = CreateImage(big, BOARDIMGSIZE, 0, 0);
            grid.Children.Add(Board);
            for (int i = 0; i < WIDTH * WIDTH; i++)
            {
                Pieces[i] = CreateImage(gray, PIECEIMGSIZE, 8 + (8 + PIECEIMGSIZE) * (i % 4), 8 + (8 + PIECEIMGSIZE) * (i / 4));
                grid.Children.Add(Pieces[i]);
            }
            for (int i = 0; i < WIDTH * WIDTH; i++)
            {
                Texts[i] = CreateText(12 + (8 + PIECEIMGSIZE) * (i % 4), 12 + (8 + PIECEIMGSIZE) * (i / 4));
                grid.Children.Add(Texts[i]);
            }
        }

        Image CreateImage(BitmapImage source, int size, int x, int y)
        {
            Image image = new Image();
            image.Source = source;
            image.Width = size;
            image.Height = size;
            image.Margin = new System.Windows.Thickness(x, y, 0, 0);
            image.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            image.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            return image;
        }

        TextBlock CreateText(int x, int y)
        {
            TextBlock textBlock = new TextBlock();
            textBlock.Text = "";
            textBlock.Foreground = new SolidColorBrush(Colors.White);
            textBlock.FontSize = 24;
            textBlock.Margin = new System.Windows.Thickness(x, y, 0, 0);
            textBlock.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            textBlock.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            return textBlock;
        }

        public void Update(int direction, bool check)
        {
	        //Rotate the board!!
            #region Rotate
	        //Right
	        if (direction == 1)
		        board = RotateBoard();
	        //Down
	        else if (direction == 2)
	        {
		        board = RotateBoard();
		        board = RotateBoard();
	        }
	        //Left
	        else if (direction == 3)
	        {
		        board = RotateBoard();
		        board = RotateBoard();
		        board = RotateBoard();
            }
            #endregion

            //Now shift the pieces up
	        int[,] newBoard = board;
	        newBoard = MoveBoard(newBoard, check);
	        board = newBoard;

            //Rotate back!!
            #region RotateBack
            //Left
	        if (direction == 3)
		        board = RotateBoard();
	        //Down
	        else if (direction == 2)
	        {
		        board = RotateBoard();
		        board = RotateBoard();
	        }
	        //Right
	        else if (direction == 1)
	        {
		        board = RotateBoard();
		        board = RotateBoard();
		        board = RotateBoard();
            }
            #endregion

            //Check for gameover
            end = CheckForEnd();

            //Update Display
            //UpdateDisplay();
        }

        bool CheckForEnd()
        {
            //Is board full
            bool full = true;
            for (int y = 0; y < WIDTH; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    if (board[x, y] == 0)
                    {
                        full = false;
                        break;
                    }
                }
                if (!full) { break; }
            }

            //If nowhere to move
            bool canMove = true;
            if (full)
            {
                canMove = false;
                for (int y = 0; y < WIDTH; y++)
                {
                    for (int x = 0; x < WIDTH; x++)
                    {
                        //Check up
                        if (y != 0 && board[x, y - 1] == board[x, y])
                        {
                                canMove = true;
                                break;
                        }
                        //Check right
                        if (x != WIDTH - 1 && board[x + 1, y] == board[x, y])
                        {
                                canMove = true;
                                break;
                        }
                        //Check down
                        if (y != WIDTH - 1 && board[x, y + 1] == board[x, y])
                        {
                            canMove = true;
                            break;
                        }
                        //Check left
                        if (x != 0 && board[x - 1, y] == board[x, y])
                        {
                            canMove = true;
                            break;
                        }
                    }
                    if (canMove) { break; }
                }
            }

            if (canMove)
                return false;
            else
                return true;
        }

        public void UpdateDisplay()
        {
            for (int y = 0; y < WIDTH; y++)
            {
                for (int x = 0; x < WIDTH; x++)
                {
                    if (board[x, y] == 0)
                    {
                        Texts[x + y * WIDTH].Text = "";
                        Pieces[x + y * WIDTH].Visibility = Visibility.Collapsed;
                    }
                    else
                    {
                        Texts[x + y * WIDTH].Text = board[x, y].ToString();
                        Pieces[x + y * WIDTH].Visibility = Visibility.Visible;
                        switch (board[x, y])
                        {
                            case 2:
                                Pieces[x + y * WIDTH].Source = s2;
                                break;
                            case 4:
                                Pieces[x + y * WIDTH].Source = s4;
                                break;
                            case 8:
                                Pieces[x + y * WIDTH].Source = s8;
                                break;
                            case 16:
                                Pieces[x + y * WIDTH].Source = s16;
                                break;
                            case 32:
                                Pieces[x + y * WIDTH].Source = s32;
                                break;
                            case 64:
                                Pieces[x + y * WIDTH].Source = s64;
                                break;
                            case 128:
                                Pieces[x + y * WIDTH].Source = s128;
                                break;
                            case 256:
                                Pieces[x + y * WIDTH].Source = s256;
                                break;
                            case 512:
                                Pieces[x + y * WIDTH].Source = s512;
                                break;
                            case 1024:
                                Pieces[x + y * WIDTH].Source = s1024;
                                break;
                            case 2048:
                                Pieces[x + y * WIDTH].Source = s2048;
                                break;
                            case 4096:
                                Pieces[x + y * WIDTH].Source = s4096;
                                break;
                            case 8192:
                                Pieces[x + y * WIDTH].Source = s8192;
                                break;
                            case 16384:
                                Pieces[x + y * WIDTH].Source = s16384;
                                break;
                            case 32768:
                                Pieces[x + y * WIDTH].Source = s32768;
                                break;
                            default:
                                Pieces[x + y * WIDTH].Source = gray;
                                break;
                        }
                    }
                }
            }
        }

        int[,] MoveBoard(int[,] newBoard, bool check)
        {
            bool moved = false;

	        //Combine equal values
	        for (int x = 0; x < WIDTH; x++)
	        {
		        for (int y = 0; y < WIDTH; y++)
		        {
			        if (newBoard[x, y] != 0)
			        {
				        for (int y0 = y + 1; y0 < WIDTH; y0++)
				        {
					        if (newBoard[x, y0] != 0)
					        {
						        if (newBoard[x, y0] == newBoard[x, y])
						        {
							        newBoard[x, y] *= 2;
							        newBoard[x, y0] = 0;
                                    moved = true;
                                    score += newBoard[x, y];
						        }
						        break;
					        }
				        }
			        }
		        }
	        }

	        //Shift the pieces
	        for (int x = 0; x < WIDTH; x++)
	        {
		        for (int y = 1; y < WIDTH; y++)
		        {
			        if (newBoard[x, y] != 0)
			        {
				        for (int y0 = y - 1; y0 >= -1; y0--)
				        {
					        if (y0 == -1 || newBoard[x, y0] != 0)
					        {
                                if (y0 != y - 1)
                                {
						            newBoard[x, y0+1] = newBoard[x, y];
						            newBoard[x, y] = 0;
                                    moved = true;
                                }
						        break;
					        }
				        }
			        }
		        }
	        }

            if (moved && !check)
                SetPiece();

	        return newBoard;
        }

        int[,] RotateBoard()
        {
	        int[,] newBoard = new int[WIDTH, WIDTH];

	        for (int y = 0; y < WIDTH; y++)
	        {
		        for (int x = 0; x < WIDTH; x++)
		        {
			        newBoard[x, y] = board[WIDTH-y-1, x];
		        }
	        }

	        return newBoard;
        }

        void SetPiece()
        {
	        int i = 0;
	
	        //Count number of blank spaces
	        for (int y = 0; y < WIDTH; y++)
	        {
		        for (int x = 0; x < WIDTH; x++)
		        {
			        if (board[x, y] == 0) { i++; }
		        }
	        }

	        //Random place for new piece
	        int j = random.Next(0, i);
	        int c = 0;
	
	        for (i = 0; i < WIDTH*WIDTH; i++)
	        {
		        if (board[i%WIDTH, i/WIDTH] == 0)
		        {
			        if (c == j) { break; }
			        c++;
		        }
	        }

	        //Place piece in new location
	        j = random.Next(0, 10);
	        if (j == 0)
            {
                board[i % WIDTH, i / WIDTH] = 4;
                Texts[i].Text = "4";
            }
	        else
            {
                board[i % WIDTH, i / WIDTH] = 2;
                Texts[i].Text = "2";
            }
        }
    }
}
