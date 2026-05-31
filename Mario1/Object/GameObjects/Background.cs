namespace Mario1.GameObject.Gameobject
{
    public class Background : GameObject
    {
        public Background(int x, int y, string name, Image image)
        {
            X = x;
            Y = y;
            this.name = name;
            this.image = image;
        }
        public Background(int x, int y, string name, Image image, int location_transfer) : this(x, y, name, image)
        {
            this.location_transfer = location_transfer;

        }
    }
}
