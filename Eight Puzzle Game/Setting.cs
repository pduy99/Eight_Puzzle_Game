using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace Eight_Puzzle_Game
{
    public enum Mode
    {
       Random_Image = 0,
       Select_Image = 1
    }

    public enum Difficulty
    {
        Easy,
        Medium,
        Hard
    }

    public class Setting: INotifyPropertyChanged
    {
        public Mode mode { get; set; }
        public Difficulty difficulty { get; set; }
        public int timer { get; set; } // in second
        public string ImagePath { get; set; }
       
        //this contructor use for save/load game function
        public Setting(string imagepath, int time, Difficulty difficulty, Mode mode)
        {
            this.ImagePath = imagepath;
            this.timer = time;
            this.difficulty = difficulty;
            this.mode = mode;
        }

        public Setting()
        {

        }

        public void setTimer(Difficulty difficulty)
        {
            if (difficulty == Difficulty.Easy)
            {
                timer = 480; // 8 minutes
            }
            else if (difficulty == Difficulty.Medium)
            {
                timer = 300; //5 minutes
            }
            else
            {
                timer = 180; //3 minutes
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
