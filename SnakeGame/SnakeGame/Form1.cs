using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SnakeGame
{
    public partial class frmSnake : Form
    {
        Random rand;
        enum GameBoardFields
        {
            Free,
            Snake,
            Bonus
        };

        enum Directions
        {
            Up,
            Down,
            Left,
            Right
        };

        struct SnakeCoordinates
        {
            public int x;
            public int y;
        }

        GameBoardFields[,] gameBoardField; //array of game board fields
        SnakeCoordinates[] snakeXY; //array of snake coordinates for body and head (head is always index [0]
        int snakeLength; //after the snake eats bonus, another piece of body is added and length is increased by 1
        Directions direction; //direction where the head is facing
        Graphics g;

        public frmSnake()
        {
            InitializeComponent();
            gameBoardField = new GameBoardFields[11, 11]; //playable game board size is 10x10 (index 0 to 9). The other 2 indexes are for the wall
            snakeXY = new SnakeCoordinates[100]; //index 0 to 99 (10x10 is playable game board size)
            rand = new Random();
        }

        private void frmSnake_Load(object sender, EventArgs e)
        {
            picGameBoard.Image = new Bitmap(420, 420); //35 pixels * 12 fields (10 fields of game board + 2 fields for the wall)
            g = Graphics.FromImage(picGameBoard.Image);
            g.Clear(Color.GreenYellow);

            for (int i = 1; i <= 10; i++)
            {
                //top and bottom walls
                g.DrawImage(imgList.Images[6], i * 35, 0);//moving 35 pixels to the right
                g.DrawImage(imgList.Images[6], i * 35, 385); //moving 35 pixels to the right, but on the bottom of the game board (starting in index 1; so it's 35*11 = 385)
            }

            for (int i = 0; i <= 11; i++)
            {
                //left and right walls
                g.DrawImage(imgList.Images[6], 0, i * 35); //moving 35 pixels down
                g.DrawImage(imgList.Images[6], 385, i * 35); //moving 35 pixels down, but starting on the right side of the game board (starting from index 1; so it's 35 * 11 = 385
            }

            //initial snake body and head
            snakeXY[0].x = 5; //head
            snakeXY[0].y = 5;
            snakeXY[1].x = 5;//first body part
            snakeXY[1].y = 6;
            snakeXY[2].x = 5;//second body part
            snakeXY[2].y = 7;

            g.DrawImage(imgList.Images[5], 5 * 35, 5 * 35); //head
            g.DrawImage(imgList.Images[4], 5 * 35, 6 * 35); //first body part
            g.DrawImage(imgList.Images[4], 5 * 35, 7 * 35); //second body part

            gameBoardField[5, 5] = GameBoardFields.Snake; //head
            gameBoardField[5, 6] = GameBoardFields.Snake; //first body part
            gameBoardField[5, 7] = GameBoardFields.Snake; //second body part

            direction = Directions.Up;
            snakeLength = 3;

            for (int i = 0; i < 4; i++)
            {
                Bonus();
            }
        }

        private void Bonus()
        {
            int x, y;
            var imgIndex = rand.Next(0, 4);

            //if bonus is randomly generated on a snake field or another bonus field, then keep looping and generating new coordinates
            do
            {
                x = rand.Next(1, 10);
                y = rand.Next(1, 10);
            }
            while (gameBoardField[x, y] != GameBoardFields.Free);

            gameBoardField[x, y] = GameBoardFields.Bonus; //set the field to bonus
            g.DrawImage(imgList.Images[imgIndex], x * 35, y * 35); //draw the randomly generated bonus picture in the randomly generated coordinates
        }

        private void frmSnake_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    direction = Directions.Up;
                    break;
                case Keys.Down:
                    direction = Directions.Down;
                    break;
                case Keys.Left:
                    direction = Directions.Left;
                    break;
                case Keys.Right:
                    direction = Directions.Right;
                    break;
            }
        }

        //private void DirectionDown()
        //{
        //    g.DrawImage(imgList.Images[5], snakeXY[0].x * 35, snakeXY[0].y * 35);

        //    Bitmap RotateImage(Bitmap b, float angle)
        //    {
        //        //create a new empty bitmap to hold rotated image
        //        Bitmap returnBitmap = new Bitmap(b.Width, b.Height);
        //        //make a graphics object from the empty bitmap
        //        using (Graphics g = Graphics.FromImage(returnBitmap))
        //        {
        //            //move rotation point to center of image
        //            g.TranslateTransform((float)b.Width / 2, (float)b.Height / 2);
        //            //rotate
        //            g.RotateTransform(angle);
        //            //move image back
        //            g.TranslateTransform(-(float)b.Width / 2, -(float)b.Height / 2);
        //            //draw passed in image onto graphics object
        //            g.DrawImage(b, new Point(0, 0));
        //        }
        //        return returnBitmap;
        //    }
        //}

   

        private void GameOver()
        {
            timer.Enabled = false;
            MessageBox.Show("GAME OVER");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            //delete the end of the snake
            g.FillRectangle(Brushes.GreenYellow, snakeXY[snakeLength - 1].x * 35,
                snakeXY[snakeLength - 1].y * 35, 35, 35);
            //set the game board filed of the last body part to be free
            gameBoardField[snakeXY[snakeLength - 1].x, snakeXY[snakeLength - 1].y] = GameBoardFields.Free;

            //move snake field on the position of previous one
            for (int i = snakeLength; i >= 1; i--)
            {
                snakeXY[i].x = snakeXY[i - 1].x;
                snakeXY[i].y = snakeXY[i - 1].y;
            }

            g.DrawImage(imgList.Images[4], snakeXY[0].x * 35, snakeXY[0].y * 35);



            //change direction of the head
            switch (direction)
            {
                case Directions.Up:
                    snakeXY[0].y = snakeXY[0].y - 1; 
                    break;
                case Directions.Down:
                    snakeXY[0].y = snakeXY[0].y + 1;
                    break;
                case Directions.Left:
                    snakeXY[0].x = snakeXY[0].x - 1;
                    break;
                case Directions.Right:
                    snakeXY[0].x = snakeXY[0].x + 1;
                    break;
            }

            //check if snake hit the wall
            if (snakeXY[0].x < 1 || snakeXY[0].x > 10 || snakeXY[0].y < 1 || snakeXY[0].y > 10)
            {
                GameOver();
                picGameBoard.Refresh();
                return;
            }

            //check if snake hit its body (if the head coordinates corespond with game board field coordinates of Snake, the snake hit its body)
            if (gameBoardField[snakeXY[0].x, snakeXY[0].y] == GameBoardFields.Snake)
            {
                GameOver();
                picGameBoard.Refresh();
                return;
            }

            //check if snake ate the bonus
            if (gameBoardField[snakeXY[0].x, snakeXY[0].y] == GameBoardFields.Bonus)
            {
                //add another body part after the last body part of the snake
                g.DrawImage(imgList.Images[4], snakeXY[snakeLength].x * 35, snakeXY[snakeLength].y * 35);
                //set the field of the newly added body part to be of Snake
                gameBoardField[snakeXY[snakeLength].x, snakeXY[snakeLength].y] = GameBoardFields.Snake; snakeLength++;

                //we only want to generate bonus if there is still a room for it on the board.
                //we don't want to draw bonus on top of a snake. If we didn't do this if statement and are game board would be filled with
                //snake body, the Bonus() method would go into infinite loop because in that method, we loop until we find a valid Free field
                if (snakeLength < 96)
                    Bonus();

                this.Text = "Snake - score: " + snakeLength;
            }

            //draw the head
            g.DrawImage(imgList.Images[5], snakeXY[0].x * 35, snakeXY[0].y * 35);
            //change the game board field of new head to be of Snake
            gameBoardField[snakeXY[0].x, snakeXY[0].y] = GameBoardFields.Snake;

            //refresh the whole game board
            picGameBoard.Refresh();
        }
    }
}
