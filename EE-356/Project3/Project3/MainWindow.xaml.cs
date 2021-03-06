﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace Project3
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //sets the global variables
        private static int rowNum = 18;
        private static int colNum = 10;
        private static Slot[,] OldGenarray = new Slot[rowNum, colNum];
        private static Slot[,] NewGenarray = new Slot[rowNum, colNum];
        System.Windows.Threading.DispatcherTimer tmr1;
        private int sandSpeed = 100;
        private bool forever = false;
        private int sandSize = 25;
        private static Mutex m = new Mutex();

        public MainWindow()
        {
            InitializeComponent();
            //thread
            Thread mSand = new Thread(moveSand);
            //make the initial generation    
            makeInitialGen();

            StringBuilder lblOutput = new StringBuilder();            
            for (int i = 0; i < rowNum; i++)
            {
                for (int j = 0; j < colNum; j++)
                {
                    lblOutput.Append(OldGenarray[i, j] + " ");
                }
                lblOutput.Append("\n");
            }
            lblHourglass.Content = lblOutput;
            //paint the hour glass
            paintHourGlass();                                 
        }                 
        //starts the time
        private void btnTimer_Click(object sender, RoutedEventArgs e)
        {        

            tmr1 = new System.Windows.Threading.DispatcherTimer();
            tmr1.Tick += new EventHandler(DoWorkMethod);
            tmr1.Interval = new System.TimeSpan(0, 0, 0, 0, sandSpeed); //1 sec
            tmr1.Start();
        }
        //ends the timer
        private void btnTimerEnd_Click(object sender, RoutedEventArgs e)
        {
            tmr1.Stop();
        }
        //does the sand move on the current Gen 0
        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            //move the sand
            moveSand();
            m.WaitOne();
            //paints the Gen 1 CURRENT
            StringBuilder Output = new StringBuilder();
            for (int i = 0; i < rowNum; i++)
            {
                for (int j = 0; j < colNum; j++)
                {                    
                    Output.Append(NewGenarray[i, j] + " ");
                }
                Output.Append("\n");
            }
            m.ReleaseMutex(); // release the mutex
            lblHourglass.Content = Output;

            //paint the hour glass
            paintHourGlass();            

            changeGeneration();

            //does the forver loop as it resets the sand
            if (forever)
            {
                if (NewGenarray[10, 6].isEmpty())
                {
                    if (!NewGenarray[11, 7].isEmpty())
                    {
                        if (!NewGenarray[12, 8].isEmpty())
                        {
                            btnReset.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
                        }
                    }
                }
            }                      

        }
        //reset the generations
        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            //makes the inital generation
            makeInitialGen();

            StringBuilder Output = new StringBuilder();

            for (int i = 0; i < rowNum; i++)
            {
                for (int j = 0; j < colNum; j++)
                {
                    Output.Append(OldGenarray[i, j] + " ");
                }
                Output.Append("\n");
            }

            lblHourglass.Content = Output;
            //paint the hour glass
            paintHourGlass();            
        }
        //starts the forver loop, calls the timer in loop
        private void btnFrvStart_Click(object sender, RoutedEventArgs e)
        {
            forever = true;
        }
        //ends the timer
        private void btnFrvEnd_Click(object sender, RoutedEventArgs e)
        {
            forever = false;
        }
        //cnages the sand number or speed
        private void btnSand_Click(object sender, RoutedEventArgs e)
        {
            sandSize = Convert.ToInt32(txtbxSandSize.Text);
            sandSpeed = Convert.ToInt32(txtbxTimeiter.Text);

            tmr1.Stop();
            tmr1.Start();

        }
        //does the thread work of using move on current Gen 0
        private void DoWorkMethod(object sender, EventArgs e)
        {
            btnStart.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));

        }

        //make an intitial gen
        public void makeInitialGen()
        {
            //Setting up the initial grid
            for (int i = 0; i < rowNum; i++)
            {
                for (int j = 0; j < colNum; j++)
                {
                    OldGenarray[i, j] = new Slot();
                    NewGenarray[i, j] = new Slot();
                }
            }

            firstGenMaker();
        }
        //make the first gen
        private void firstGenMaker()
        {
            //setup the edge
            for (int j = 0; j < colNum; j++)
            {
                OldGenarray[0, j].setEdgeTrue();
                OldGenarray[rowNum - 1, j].setEdgeTrue();
            }

            for (int i = 0; i < rowNum; i++)
            {
                OldGenarray[i, 0].setEdgeTrue();
                OldGenarray[i, colNum - 1].setEdgeTrue();
            }

            //Static slot for hourglass
            createStaticslots();
            resetSand();
        }
        //reset the sand after one whole iterationis done
        private void resetSand()
        {
            //Filling up the upper side of glass
            for (int i = 1; i < 8; i++)
            {
                for (int j = 1; j < colNum; j++)
                {
                    if (OldGenarray[i, j].isEmpty() && !OldGenarray[i, j].isStatic() && !OldGenarray[i, j].isEdge())
                    {
                        OldGenarray[i, j].setEmptyFalse();
                    }
                }
            }

            //paint the hour glass
            paintHourGlass();            
        }
        //create the static slot like edges and the wall
        private void createStaticslots()
        {
            OldGenarray[6, 1].setStatic();
            OldGenarray[7, 1].setStatic();
            OldGenarray[8, 1].setStatic();
            OldGenarray[9, 1].setStatic();
            OldGenarray[10, 1].setStatic();
            OldGenarray[11, 1].setStatic();

            OldGenarray[7, 2].setStatic();
            OldGenarray[8, 2].setStatic();
            OldGenarray[9, 2].setStatic();
            OldGenarray[10, 2].setStatic();

            OldGenarray[8, 3].setStatic();
            OldGenarray[9, 3].setStatic();

            OldGenarray[8, 6].setStatic();
            OldGenarray[9, 6].setStatic();

            OldGenarray[7, 7].setStatic();
            OldGenarray[8, 7].setStatic();
            OldGenarray[9, 7].setStatic();
            OldGenarray[10, 7].setStatic();

            OldGenarray[6, 8].setStatic();
            OldGenarray[7, 8].setStatic();
            OldGenarray[8, 8].setStatic();
            OldGenarray[9, 8].setStatic();
            OldGenarray[10, 8].setStatic();
            OldGenarray[11, 8].setStatic();
        }
        //main program that moves the sand
        //goes through each row and colum and follows the 
        //algorithms as discussed inclass
        private void moveSand()
        {
            m.WaitOne();
            for (int i = rowNum - 1; i >= 0; i--)//int j = colNum - 1; j >= 0; j--
            {
                for (int j = colNum - 1; j >= 0; j--)//int i = rowNum - 1; i >= 0; i--
                {
                    if (!OldGenarray[i, j].isEmpty() && OldGenarray[i, j].isMovable())
                    {
                        if (OldGenarray[i + 1, j].isEmpty() && OldGenarray[i + 1, j].isMovable())
                        {
                            NewGenarray[i + 1, j].setEmptyFalse();
                            NewGenarray[i, j].setEmptyTrue();

                            OldGenarray[i + 1, j].setEmptyFalse();
                            OldGenarray[i, j].setEmptyTrue();
                        }
                        else if (OldGenarray[i + 1, j - 1].isEmpty() && OldGenarray[i + 1, j - 1].isMovable())
                        {
                            NewGenarray[i + 1, j - 1].setEmptyFalse();
                            NewGenarray[i, j].setEmptyTrue();

                            OldGenarray[i + 1, j - 1].setEmptyFalse();
                            OldGenarray[i, j].setEmptyTrue();
                        }
                        else if (OldGenarray[i + 1, j + 1].isEmpty() && OldGenarray[i + 1, j + 1].isMovable())
                        {
                            NewGenarray[i + 1, j + 1].setEmptyFalse();
                            NewGenarray[i, j].setEmptyTrue();

                            OldGenarray[i + 1, j + 1].setEmptyFalse();
                            OldGenarray[i, j].setEmptyTrue();
                        }
                        else
                        {
                            NewGenarray[i, j].setSlot(OldGenarray[i, j]);
                        }
                    }
                    else
                    {
                        NewGenarray[i, j].setSlot(OldGenarray[i, j]);
                    }
                }
            }
            m.ReleaseMutex();            
        }
        //cnages the array so that gen 1-> gen 0
        private void changeGeneration()
        {
            for (int i = 0; i < rowNum; i++)
            {
                for (int j = 0; j < colNum; j++)
                {
                    OldGenarray[i, j].setSlot(NewGenarray[i, j]);
                }
            }
        }
        //paints the hour glass, after each iteration is called
        public void paintHourGlass()
        {
            Pen[] penArray = new Pen[3];
            penArray[0] = new Pen(Brushes.Black, 1);
            penArray[1] = new Pen(Brushes.Blue, 1);
            penArray[2] = new Pen(Brushes.Red, 1);

            Brush[] brushArray = new Brush[3];
            brushArray[0] = new SolidColorBrush(Color.FromArgb(255, 0, 0, 0));
            brushArray[1] = new SolidColorBrush(Color.FromArgb(255, 255, 223, 0));
            brushArray[2] = new SolidColorBrush(Color.FromArgb(255, 255, 0, 0));

            DrawingVisual vis = new DrawingVisual();
            DrawingContext dc = vis.RenderOpen();

            int gapx = 2*sandSize,gapy= 2 *sandSize;
            Brush b = new SolidColorBrush();
            Pen p = new Pen();
            for (int i = 0; i < rowNum; i++)
            {
                for (int j = 0; j < colNum; j++)
                {
                    if (OldGenarray[i,j].isEdge())
                    {
                       p = penArray[0];
                       b = brushArray[0];
                       dc.DrawEllipse(b, p, new Point(gapx, gapy), sandSize, sandSize);
                    }
                    else if (OldGenarray[i, j].isStatic())
                    {
                        p = penArray[1];
                        b = brushArray[1];
                        dc.DrawEllipse(b, p, new Point(gapx, gapy), sandSize, sandSize);
                    }
                    else if (OldGenarray[i, j].isMovable() && !OldGenarray[i,j].isEmpty())
                    {
                        p = penArray[2];
                        b = brushArray[2];
                        
                        if(i >= 9)
                        {
                            b = new SolidColorBrush(Color.FromArgb(255, 255, 255, 0));
                        }
                        dc.DrawEllipse(b, p, new Point(gapx, gapy), sandSize, sandSize);
                    }
                                       
                    gapx += 2 * sandSize;
                }

                gapy += 2 * sandSize;
                gapx = 2 * sandSize;
            }


            dc.Close();
            RenderTargetBitmap bmp = new RenderTargetBitmap(1000, 1000, 96, 96, PixelFormats.Pbgra32);
            bmp.Render(vis);
            imgPlot.Source = bmp;

        }
        
    }

    //A new class is added in the terms of Slot
    //the class has the attributes of empty, static or edge
    public class Slot
    {
        // Auto-implemented readonly property:
        public bool Empty { get; set; }
        public bool Static { get; set; }
        public bool Edge { get; set; }

        public Slot()
        {
            Empty = true;
            Static = false;
            Edge = false;
        }

        public void setEmptyFalse()
        {
            Empty = false;
        }

        public void setEmptyTrue()
        {            
           Empty = true;
        }

        public Boolean getEmpty()
        {
            return Empty;
        }

        public bool isEmpty()
        {
            return Empty;
        }

        public bool isStatic()
        {
            return Static;
        }

        public void setStatic()
        {
            Static = true;
            Empty = false;
        }

        public Boolean getStatic()
        {
            return Static;
        }

        public void setEdgeTrue()
        {
            Edge = true;
            Empty = false;
        }

        public bool isEdge()
        {
            return Edge;
        }

        public Boolean getEdge()
        {
            return Edge;
        }

        public override string ToString()
        {
            if (Edge)
                return "!";
            else if (Static)
                return "X";
            else if (Empty)
                return ".";
            else if (!Static && !Edge)
                return "O";
            else
                return "";
        }

        internal void setSlot(Slot slot)
        {
            Static = slot.getStatic();
            Empty = slot.getEmpty();
            Edge = slot.getEdge();
        }

        public Boolean isMovable()
        {
            if (Edge)
                return false;
            if (Static)
                return false;
            else
                return true;
        }
    }    

}
