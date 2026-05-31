namespace Mario1.GameObject
{
    public abstract class GameObject
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string name { get; set; }
        public Image image { get; set; }
        public int height { get; set; }
        public int width { get; set; }
        public bool direction { get; set; } //true - направо, false - налево
        public string property { get; set; }
        public int we_stand { get; set; }
        public int location_transfer { get; set; }
        public short speed { get; set; }
        public short run_animation { get; set; }//для картинок бега (прибавляеться каждый цикл таймера и обнуляеться при остановке)
        public short runIf { get; set; }//для картинок бега (что-то типо флажков только не 2 варианта, а больше)(0-первая картинка бега 1-вторая картинка и т.д.)
    }
}
