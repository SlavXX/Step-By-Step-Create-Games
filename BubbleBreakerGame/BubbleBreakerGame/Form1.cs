using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text;
using System.Windows.Forms;

namespace BubbleBreakerGame
{
    public partial class frmBubbleBreaker : Form
    {
        enum Colors
        {
            None,
            Red,
            Green,
            Yellow,
            Blue,
            Purple
        };

        const int NUM_BUBBLES = 5;
        const int BUBBLE_SIZE = 40;
        Colors[,] colors;
        Random rand;
        int score;
        bool[,] isSelected;
        int numOfSelectedBubbles;
        Scores scores;

        public frmBubbleBreaker()
        {
            InitializeComponent();
            rand = new Random();
            numOfSelectedBubbles = 0;
            score = 0;
            colors = new Colors[NUM_BUBBLES, NUM_BUBBLES]; //holds colors for the bubbles on the game board
            isSelected = new bool[NUM_BUBBLES, NUM_BUBBLES]; //holds bubbles user selected with the mouse click
            lblInfo.BackColor = Color.White;
        }

        private void frmBubbleBreaker_Load(object sender, EventArgs e)
        {
            init();
        }

        private void init()
        {
            SetClientSizeCore(NUM_BUBBLES * BUBBLE_SIZE, NUM_BUBBLES * BUBBLE_SIZE);
            FormBorderStyle = FormBorderStyle.FixedSingle; //disables form resizing
            MaximizeBox = false; //disables maximizing the form
            BackColor = Color.Black;
            DoubleBuffered = true; //prevents screen flickering

            txtName.Visible = false;
            btnName.Visible = false;
            btnName.Text = "Enter Your Name";

            txtName.Width = ClientSize.Width < 100 ? ClientSize.Width : ClientSize.Width / 2;
            btnName.Width = ClientSize.Width < 100 ? ClientSize.Width : ClientSize.Width / 2;
            txtName.Location = new Point((ClientSize.Width - txtName.Width) / 2, txtName.Height);
            btnName.Location = new Point((ClientSize.Width - btnName.Width) / 2, btnName.Height + 20);
            Start();
        }

        private void GameOver()
        {
            scores = new Scores(score, NUM_BUBBLES);
            StringBuilder sb = new StringBuilder();
            sb.Append("***GAME OVER***");
            sb.Append("\n");
            sb.Append("***TOP 3 SCORES***\n");
            sb.Append(scores.GetTopThreeScores() + "\n");
            sb.Append("\n");
            sb.Append(scores.GetScoreMessage());

            MessageBox.Show(sb.ToString());
            txtName.Visible = true;
            btnName.Visible = true;
        }

        //populate the array with bubbles of random colors
        private void Start()
        {
            for (int row = 0; row < NUM_BUBBLES; row++)
            {
                for (int col = 0; col < NUM_BUBBLES; col++)
                {
                    colors[row, col] = (Colors) rand.Next(1, 6);
                }
            }

            this.Text = score + " points";
        }

        //paint event. Paints bubbles, and outlines for selected bubbles
        private void Form_Paint(object sender, PaintEventArgs e)
        {
            for (int row = 0; row < NUM_BUBBLES; row++)
            {
                for(int col = 0; col < NUM_BUBBLES; col++)
                {
                    Color bubbleColor = Color.Empty;
                    //x axis on the game board corresponds with row in array; y axis correspond with column in array
                    var xPos = col;
                    var yPos = row;
                    var isBubble = true;

                    switch (colors[row, col])
                    {
                        case Colors.Red:
                            bubbleColor = Color.Red;
                            break;
                        case Colors.Yellow:
                            bubbleColor = Color.Yellow;
                            break;
                        case Colors.Green:
                            bubbleColor = Color.Green;
                            break;
                        case Colors.Blue:
                            bubbleColor = Color.Blue;
                            break;
                        case Colors.Purple:
                            bubbleColor = Color.Purple;
                            break;
                        default:
                            e.Graphics.FillRectangle(Brushes.Black, xPos * BUBBLE_SIZE, 
                                yPos * BUBBLE_SIZE, BUBBLE_SIZE, BUBBLE_SIZE);
                            isBubble = false;
                            break;
                    }

                    if (isBubble) //make sure we are only drawing bubble if it has valid color
                    {
                        e.Graphics.FillEllipse(
                        new LinearGradientBrush(
                        new Point(row * BUBBLE_SIZE, col * BUBBLE_SIZE),
                        new Point(row * BUBBLE_SIZE + BUBBLE_SIZE, col * BUBBLE_SIZE + BUBBLE_SIZE),
                        Color.White, bubbleColor),
                        xPos * BUBBLE_SIZE, //x
                        yPos * BUBBLE_SIZE, //y
                        BUBBLE_SIZE, BUBBLE_SIZE);//width and height are the same for circle

                        if (isSelected[row, col])
                        {
                            //left outline
                            if (col > 0 && colors[row, col] != colors[row, col - 1])
                                e.Graphics.DrawLine(Pens.White, xPos * BUBBLE_SIZE, yPos * BUBBLE_SIZE,
                                    xPos * BUBBLE_SIZE, yPos * BUBBLE_SIZE + BUBBLE_SIZE);

                            //right outline
                            if (col < NUM_BUBBLES - 1 && colors[row, col] != colors[row, col + 1])
                                e.Graphics.DrawLine(Pens.White, xPos * BUBBLE_SIZE + BUBBLE_SIZE, yPos * BUBBLE_SIZE,
                                    xPos * BUBBLE_SIZE + BUBBLE_SIZE, yPos * BUBBLE_SIZE + BUBBLE_SIZE);

                            //top outline
                            if (row > 0 && colors[row, col] != colors[row - 1, col])
                                e.Graphics.DrawLine(Pens.White, xPos * BUBBLE_SIZE, yPos * BUBBLE_SIZE,
                                    xPos * BUBBLE_SIZE + BUBBLE_SIZE, yPos * BUBBLE_SIZE);

                            //bottom outline
                            if (row < NUM_BUBBLES - 1 && colors[row, col] != colors[row + 1, col])
                                e.Graphics.DrawLine(Pens.White, xPos * BUBBLE_SIZE, yPos * BUBBLE_SIZE + BUBBLE_SIZE,
                                    xPos * BUBBLE_SIZE + BUBBLE_SIZE, yPos * BUBBLE_SIZE + BUBBLE_SIZE);
                        }
                    }                    
                }
            }
        }

        //mouse click event. If no bubbles are selected, the click selects bubbles. 
        //If bubbles are selected, the click removes them from game board
        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            //coordinates of the bubble that was clicked
            var x = Convert.ToInt32 (e.X / BUBBLE_SIZE);
            var y = Convert.ToInt32 (e.Y / BUBBLE_SIZE);

            //row and column of clicked bubble in the array
            var row = y;
            var col = x;

            if (isSelected[row,col] && numOfSelectedBubbles > 1)
            {
                score += Convert.ToInt32(lblInfo.Text);
                this.Text = score + " points";
                RemoveBubbles();
                ClearSelected();
                MoveBubblesDown();
                MoveBubblesRight();

                if (!HasMoreMoves())
                {
                    GameOver();
                }
            }
            else
            {
                ClearSelected();

                if (colors[row, col] > Colors.None)
                {
                    HighlightNeighbors(row, col);
                    this.Invalidate();
                    Application.DoEvents();

                    if (numOfSelectedBubbles > 1)
                    {
                        SetLabel(numOfSelectedBubbles, x, y);
                    }
                }
            }
        }

        //remove bubbles by giving removed bubbles none color
        private void RemoveBubbles()
        {
            for (int row = 0; row < NUM_BUBBLES; row++)
            {
                for (int col = 0; col < NUM_BUBBLES; col++)
                {
                    if (isSelected[row, col])
                        colors[row, col] = Colors.None;
                }
            }

            this.Invalidate();
            Application.DoEvents();
        }

        //remove selected bubbles from the array
        private void ClearSelected()
        {
            for (int row = 0; row < NUM_BUBBLES; row++)
            {
                for (int col = 0; col < NUM_BUBBLES; col++)
                {
                    isSelected[row, col] = false;
                }
            }

            numOfSelectedBubbles = 0;
            lblInfo.Visible = false;
        }

        //check if the game is over. The game is NOT over if there are at least two neighboring bubbles of the same color
        private bool HasMoreMoves()
        {
            for (int row = 0; row < NUM_BUBBLES; row++)
            {
                for (int col = 0; col < NUM_BUBBLES; col++)
                {
                    if (colors[row, col] > Colors.None)
                    {
                        if (col < NUM_BUBBLES - 1 && colors[row, col] == colors[row, col + 1])
                            return true;

                        if (row < NUM_BUBBLES - 1 && colors[row, col] == colors[row + 1, col])
                            return true;
                    }
                }
            }

            return false;
        }

        //set the label for the score of the selected bubbles
        private void SetLabel(int numOfBubles, int x, int y)
        {
            var value = numOfBubles * (numOfBubles - 1);
            lblInfo.Text = value.ToString();

            lblInfo.Left = x * BUBBLE_SIZE + BUBBLE_SIZE;
            lblInfo.Top = y * BUBBLE_SIZE + BUBBLE_SIZE;

            //if the label would be too close to the edge, move ot inward a bit
            if (lblInfo.Left > this.ClientSize.Width / 2)
                lblInfo.Left -= BUBBLE_SIZE;

            if (lblInfo.Top > this.ClientSize.Height / 2)
                lblInfo.Top -= BUBBLE_SIZE;

            lblInfo.Visible = true;
        }

        //moves bubbles down on the spots where the removed bubbles used to be
        private void MoveBubblesDown()
        {
            for (int col = 0; col < NUM_BUBBLES; col++)
            {
                var noneColorBubblePosition = NUM_BUBBLES - 1;
                var foundNoneColor = false;

                for (int row = NUM_BUBBLES - 1; row >= 0; row--)
                {
                    if (colors[row, col] == Colors.None) //find the position of the first removed bubble
                        foundNoneColor = true;

                    if (colors[row, col] != Colors.None && !foundNoneColor)
                        noneColorBubblePosition--;

                    if (colors[row,col] != Colors.None && foundNoneColor)
                    {
                        colors[noneColorBubblePosition, col] = colors[row, col];
                        noneColorBubblePosition--;
                    }                    
                }

                //remove the leftover bubbles on the top
                for (int r = noneColorBubblePosition; r >= 0; r--)
                {
                    colors[r, col] = Colors.None;
                }
            }

            this.Invalidate();
            Application.DoEvents();
        }

        //after bubbles move down, move bubbles on the left to fill any gaps created from removed bubbles
        private void MoveBubblesRight()
        {
            for (int row = 0; row < NUM_BUBBLES; row++)
            {
                var noneColorBubblePosition = NUM_BUBBLES - 1;
                var foundNoneColor = false;

                for (int col = NUM_BUBBLES - 1; col >= 0; col--)
                {
                    if (colors[row, col] == Colors.None) //find the position of the first removed bubble
                        foundNoneColor = true;

                    if (colors[row, col] != Colors.None && !foundNoneColor)
                        noneColorBubblePosition--;

                    //start moving bubbles to the right on positions of the removed bubbles
                    if (colors[row, col] != Colors.None && foundNoneColor)
                    {
                        colors[row, noneColorBubblePosition] = colors[row, col];
                        noneColorBubblePosition--;
                    }
                }

                //remove the leftover bubbles on the left
                for (int c = noneColorBubblePosition; c >= 0; c--)
                {
                    colors[row, c] = Colors.None;
                }
            }

            this.Invalidate();
            Application.DoEvents();
            GenerateBubbles();
        }

        private void GenerateBubbles()
        {
            if (colors[NUM_BUBBLES -1, 0] == Colors.None)
            {
                for (int row = NUM_BUBBLES - 1; row >= 0; row--)
                {
                    colors[row, 0] = (Colors) rand.Next(1, 6);
                }

                this.Invalidate();
                Application.DoEvents();
                MoveBubblesRight();
            }
        }

        //select neighboring bubbles of the same color
        private void HighlightNeighbors(int row, int col)
        {
            //this is recursive solution. You can comment it out and uncomment the loop solution if you prefer loops over recursion
            isSelected[row, col] = true;
            numOfSelectedBubbles++;

            //move up
            if (row > 0 && colors[row, col] == colors[row - 1, col] &&
                !isSelected[row - 1, col])
            {
                HighlightNeighbors(row - 1, col);
            }

            //move down
            if (row < NUM_BUBBLES - 1 && colors[row, col] == colors[row + 1, col] &&
                !isSelected[row + 1, col])
            {
                HighlightNeighbors(row + 1, col);
            }

            //move left
            if (col > 0 && colors[row, col] == colors[row, col - 1] &&
                !isSelected[row, col - 1])
            {
                HighlightNeighbors(row, col - 1);
            }

            //move right
            if (col < NUM_BUBBLES - 1 && colors[row, col] == colors[row, col + 1] &&
                !isSelected[row, col + 1])
            {
                HighlightNeighbors(row, col + 1);
            }

            /* LOOP SOLUTION
            isSelected[row, col] = true;
            numOfSelectedBubbles++;
            int[,] positionTracking = new int[NUM_BUBBLES, NUM_BUBBLES];
            var positionCounter = 1;
            positionTracking[row, col] = positionCounter;
            var rowIndex = row;
            var colIndex = col;

            while(positionCounter > 0)
            {
                //move up
                if (rowIndex > 0 && colors[rowIndex,colIndex] == colors[rowIndex - 1, colIndex] &&
                    !isSelected[rowIndex - 1, colIndex])
                {
                    isSelected[rowIndex - 1, colIndex] = true;
                    numOfSelectedBubbles++;
                    positionCounter++;
                    positionTracking[rowIndex - 1, colIndex] = positionCounter;
                    rowIndex--;
                }
                //move down
                else if (rowIndex < NUM_BUBBLES -1 && colors[rowIndex, colIndex] == colors[rowIndex + 1, colIndex] &&
                    !isSelected[rowIndex + 1, colIndex])
                {
                    isSelected[rowIndex + 1, colIndex] = true;
                    numOfSelectedBubbles++;
                    positionCounter++;
                    positionTracking[rowIndex + 1, colIndex] = positionCounter;
                    rowIndex++;
                }
                //move left
                else if (colIndex > 0 && colors[rowIndex, colIndex] == colors[rowIndex, colIndex - 1] &&
                    !isSelected[rowIndex, colIndex - 1])
                {
                    isSelected[rowIndex, colIndex - 1] = true;
                    numOfSelectedBubbles++;
                    positionCounter++;
                    positionTracking[rowIndex, colIndex - 1] = positionCounter;
                    colIndex--;
                }
                //move right
                else if (colIndex < NUM_BUBBLES - 1 && colors[rowIndex, colIndex] == colors[rowIndex, colIndex + 1] &&
                    !isSelected[rowIndex, colIndex + 1])
                {
                    isSelected[rowIndex, colIndex + 1] = true;
                    numOfSelectedBubbles++;
                    positionCounter++;
                    positionTracking[rowIndex, colIndex + 1] = positionCounter;
                    colIndex++;
                }
                else
                {
                    positionCounter--;
                    for (int r = 0; r < NUM_BUBBLES; r++)
                    {
                        for (int c = 0; c < NUM_BUBBLES; c++)
                        {
                            if (positionTracking[r, c] == positionCounter + 1)
                                positionTracking[r, c] = 0;

                            if (positionTracking[r,c] == positionCounter)
                            {
                                rowIndex = r;
                                colIndex = c;
                            }
                        }
                    }
                }
                
            }*/
        }

        private void btnName_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter a valid name");
                return;
            }

            scores.WriteScore(txtName.Text);
            numOfSelectedBubbles = 0;
            txtName.Text = "";
            score = 0;
            init();
        }
    }
}
