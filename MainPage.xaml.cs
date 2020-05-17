﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace GameOfLifeCS
{
    class cell
    {
        public delegate void stateChagedHandler(cell sender);
        public event stateChagedHandler stateChaged;
        public Windows.UI.Xaml.Shapes.Rectangle body;
        public int state;
        public int nextState;
        public cell(int size)
        {
            this.body = new Windows.UI.Xaml.Shapes.Rectangle();
            this.body.Width = size; this.body.Height = size;
            Random rState = new Random();
            this.state = rState.Next(2);
            SolidColorBrush newBrush = new SolidColorBrush();
            if (this.state == 0)
                newBrush.Color = Windows.UI.Colors.Black;
            else
                newBrush.Color = Windows.UI.Colors.White;
            this.body.Fill = newBrush;
        }
        public void setState(int newState)
        {
            this.nextState = newState;
        }
        public void update()
        {
            this.state = this.nextState;
            this.stateChaged(this);
        }
    }
    public sealed partial class MainPage : Page
    {
        int size;
        TimeSpan tick;
        List<List<cell>> grid;
        DispatcherTimer innerClock;
        bool isAnimated;
        public MainPage()
        {
            this.InitializeComponent(); 
            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            this.innerClock = new DispatcherTimer();
            this.innerClock.Tick += new EventHandler<object>(tickTock);
            this.tick = new TimeSpan(0,0,0,0,1);
            this.innerClock.Interval = this.tick;
            this.grid = new List<List<cell>>();
            this.size = 25;
            this.isAnimated = false;
            ScaleTransform scale = new ScaleTransform();
            this.Board.RenderTransform = scale;
            //Window.Current.SizeChanged += new WindowSizeChangedEventHandler(resize);
            //this.Board.Width = ((Frame)Window.Current.Content).ActualWidth;
            //this.Board.Height = ((Frame)Window.Current.Content).ActualHeight;
        }
        private void resize(Object sender, Windows.UI.Core.WindowSizeChangedEventArgs args)
        {
            if (this.isAnimated)
            {
                this.innerClock.Stop();
                double a, b, c, d;
                a = ((Frame)Window.Current.Content).ActualHeight;
                b = this.Board.ActualHeight;// * ((ScaleTransform)this.Board.RenderTransform).ScaleY;
                c = ((Frame)Window.Current.Content).ActualWidth;
                d = this.Board.ActualWidth;// * ((ScaleTransform)this.Board.RenderTransform).ScaleX;
                double yFactor = a / b;
                double xFactor = c / d;
                ((ScaleTransform)this.Board.RenderTransform).ScaleX = xFactor;//((Frame)Window.Current.Content).ActualWidth / this.Board.ActualWidth;
                ((ScaleTransform)this.Board.RenderTransform).ScaleY = yFactor;//((Frame)Window.Current.Content).ActualHeight / this.Board.ActualHeight;
                this.innerClock.Start();
            }
            else
            {
                this.Board.Width= ((Frame)Window.Current.Content).ActualWidth;
                this.Board.Height = ((Frame)Window.Current.Content).ActualHeight - 45;
            }
        }
        private void tickTock(Object sender,Object e)
        {
            update();

        }
        private void TimeSpan_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            TimeSpan newTs = new TimeSpan(0, 0, 0, 0, (int)this.TimeSpan.Value);
            this.tick = newTs;
            if(this.innerClock!=null)
                this.innerClock.Interval = this.tick;
        }

        private void Size_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            this.size = (int)this.Size.Value;
        }

        private void BeginPlay_Click(object sender, RoutedEventArgs e)
        {
            if (!this.isAnimated)
            {
                this.isAnimated = true;
                initGrid();
                show();
                this.innerClock.Start();
            }
            else
            {
                this.isAnimated = false;
                this.innerClock.Stop();
                this.grid.Clear();
                this.Board.Children.Clear();
            }
        }
        private void initGrid()
        {
            int wCount, hCount;
            wCount = (int)(this.Board.ActualWidth+this.size) / size;
            hCount = (int)(this.Board.ActualHeight+this.size) / size;
            for(int i = 0; i < hCount; i++)
            {
                List<cell> holder = new List<cell>();
                for (var j = 0; j < wCount; j++)
                {
                    cell newCell = new cell(size);
                    newCell.stateChaged += new cell.stateChagedHandler(changeState);
                    holder.Add(newCell);
                }
                this.grid.Add(holder);
            }
        }
        private void changeState(cell sender)
        {
            SolidColorBrush newBrush = new SolidColorBrush();
            if(sender.state == 0)
                newBrush.Color = Windows.UI.Colors.DimGray;
            else
                newBrush.Color = Windows.UI.Colors.White;
            sender.body.Fill = newBrush;
        }
        private void show()
        {
            this.Board.Children.Clear();
            for(int i = 0; i < this.grid.Count; i++)
            {
                for(int j = 0; j < this.grid[i].Count; j++)
                {
                    this.Board.Children.Add(this.grid[i][j].body);
                    Canvas.SetLeft(this.grid[i][j].body, j * this.size);
                    Canvas.SetTop(this.grid[i][j].body, i * this.size);
                }
            }
        }
        private void update()
        {
            for (int i = 0; i < this.grid.Count; i++)
            {
                for (int j = 0; j < this.grid[i].Count; j++)
                {
                    int sum = checkNeighbors(j, i);
                    if (sum == 3 && this.grid[i][j].state == 0)
                        this.grid[i][j].nextState = 1;
                    else if (sum < 3 && sum > 2)
                        this.grid[i][j].nextState = 1;
                    else if (sum < 2 || sum > 3)
                        this.grid[i][j].nextState = 0;
                    else
                        this.grid[i][j].nextState = this.grid[i][j].state;
                }
            }
            updateView();
        }
        private int checkNeighbors(int x,int y)
        {
            int Height_C = this.grid.Count; 
            int WidthC_C = this.grid[0].Count;
            int sum = 0;
            for(int i = -1; i < 2; i++)
            {
                for(int j = -1; j < 2; j++)
                {
                    int locX, locY;
                    locX = (x + j + WidthC_C) % WidthC_C;
                    locY = (y + i + Height_C) % Height_C;
                    sum += this.grid[locY][locX].state;
                }
            }
            sum-= this.grid[y][x].state;
            return sum;
        }
        private void updateView()
        {
            for (int i = 0; i < this.grid.Count; i++)
            {
                for (int j = 0; j < this.grid[i].Count; j++)
                {
                    this.grid[i][j].update();
                }
            }
        }

        private void AppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Exit();
        }
    }
}