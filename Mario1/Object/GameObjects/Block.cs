namespace Mario1.GameObject.Gameobject
{
    public class Block : GameObject
    {
        public Block(int x, int y, string name, int height, int width, Image image, string property)
        {
            X = x;
            Y = y;
            this.name = name;
            this.height = height;
            this.width = width;
            this.property = property;
            this.image = image;
            sluchay = null;
        }
        public Block(int x, int y, string name, int height, int width, Image image, string property, int location_transfer) : this(x, y, name, height, width, image, property)
        {
            this.location_transfer = location_transfer;
        }
        
        public int[] sluchay;//для таймера случаев, хранящий за одно старое значение spawnY_block, чтобы небыло ошибок при задевании головой блоков и они вернулись на место, если нужно
        public List<Creature.Creature> Check_сreature_we_stand(List<Creature.Creature> сreatures, int block_i)
        {
            for (int r = 0; r < сreatures.Count; r++)
            {
                сreatures[r].Delete_сreature_we_stand(block_i);
            }
            return сreatures;
        }
    }
}
