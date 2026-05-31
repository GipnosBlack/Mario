namespace Mario1.GameObject.Gameobject
{
    using im_gl = Properties.Resources;// im_gl = images_global все картинки проекта
    public class Mario : GameObject
    {
        public Mario(int x, int y, short height, short width)
        {
            X = x;
            Y = y;
            this.height = height;
            this.width = width;
            image = im_gl.Mario;
            direction = true;
            we_stand = -1;
            nam = new List<int>();
            run_animation = 0;
            runIf = 0;
            speed = 1;
            top = 3000;
        }

        public bool TimerRight = false;
        public bool TimerLeft = false;
        public bool TimerSliding = false;
        public bool TimerSpace = false;
        public bool TimerGravity = true;
        public bool stopForm1_KeyDown = false;
        public bool deadPadeniye = false;
        public int coin = 0;
        public int coin_max = 30;

        public List<int> nam;
        /*
        для сравненя двух блоков, которые бьёт головой марио
        (коллекция, для оптимизации от багов,
        так как марио может в быстро прыгнуть и задеть другой блок
        пока предедущий не опустился обратно или не разрушился и т.д.)
        */

        
        public string mode = "ordinary";
        public bool braking = false;//срабатывает 1 раз
        public bool braking2 = false;//работает пока не затормозит
        public int max_speed = 10;
        public bool acceleration = false;
        public int top = 3000;//высота пола, на котором должен стоять марио
        public sbyte spaceG = 20;//замедление во время прыжка
        public bool spaceG_bool = false;//для неполных прыжков
        public int spaceG_max = 15;
        public int g = 1;//ускорение свободного падения
        public bool sits = false;//true - сидит, false - стоит (работает только для больших марио)
        public int intangible = 0;//для неосязаемого марио
        public int pause_atack_fire_bar;
        private Image images_previous;

        public void Defines_the_image(string image)
        {
            if (deadPadeniye & image != "Dead") return;
            if (image == "Walk")
            {
                if (direction)
                {
                    if (mode == "ordinary")
                    {
                        if (run_animation % (20 - speed) == 0)
                        {
                            if (runIf == 0)
                            {
                                this.image = im_gl.Mario___Walk1;
                                runIf = 1;
                            }
                            else if (runIf == 1)
                            {
                                this.image = im_gl.Mario___Walk2;
                                runIf = 2;
                            }
                            else
                            {
                                this.image = im_gl.Mario___Walk3;
                                runIf = 0;
                            }
                        }
                    }
                    else if (mode == "big ordinary")
                    {
                        if (run_animation % 10 == 0)
                        {
                            if (runIf == 0)
                            {
                                this.image = im_gl.Super_Mario___Walk1;
                                runIf = 1;
                            }
                            else if (runIf == 1)
                            {
                                this.image = im_gl.Super_Mario___Walk2;
                                runIf = 2;
                            }
                            else
                            {
                                this.image = im_gl.Super_Mario___Walk3;
                                runIf = 0;
                            }
                        }
                    }
                    else if (mode == "big shooter")
                    {
                        if (run_animation % 10 == 0)
                        {
                            if (runIf == 0)
                            {
                                this.image = im_gl.Fiery_Mario___Walk1;
                                runIf = 1;
                            }
                            else if (runIf == 1)
                            {
                                this.image = im_gl.Fiery_Mario___Walk2;
                                runIf = 2;
                            }
                            else
                            {
                                this.image = im_gl.Fiery_Mario___Walk3;
                                runIf = 0;
                            }
                        }
                    }
                }
                else
                {
                    if (mode == "ordinary")
                    {
                        if (run_animation % 10 == 0)
                        {
                            if (runIf == 0)
                            {
                                this.image = im_gl.Mario___Walk1_invert;
                                runIf = 1;
                            }
                            else if (runIf == 1)
                            {
                                this.image = im_gl.Mario___Walk2_invert;
                                runIf = 2;
                            }
                            else
                            {
                                this.image = im_gl.Mario___Walk3_invert;
                                runIf = 0;
                            }
                        }
                    }
                    else if (mode == "big ordinary")
                    {
                        if (run_animation % 10 == 0)
                        {
                            if (runIf == 0)
                            {
                                this.image = im_gl.Super_Mario___Walk1_invert;
                                runIf = 1;
                            }
                            else if (runIf == 1)
                            {
                                this.image = im_gl.Super_Mario___Walk2_invert;
                                runIf = 2;
                            }
                            else
                            {
                                this.image = im_gl.Super_Mario___Walk3_invert;
                                runIf = 0;
                            }
                        }
                    }
                    else if (mode == "big shooter")
                    {
                        if (run_animation % 10 == 0)
                        {
                            if (runIf == 0)
                            {
                                this.image = im_gl.Fiery_Mario___Walk1_invert;
                                runIf = 1;
                            }
                            else if (runIf == 1)
                            {
                                this.image = im_gl.Fiery_Mario___Walk2_invert;
                                runIf = 2;
                            }
                            else
                            {
                                this.image = im_gl.Fiery_Mario___Walk3_invert;
                                runIf = 0;
                            }
                        }
                    }
                }
            }
            else if (image == "Skid")
            {
                if (direction == true) 
                {
                    if (mode == "ordinary") { this.image = im_gl.Mario___Skid; }
                    else if (mode == "big ordinary") { this.image = im_gl.Super_Mario___Skid; }
                    else if (mode == "big shooter") { this.image = im_gl.Fiery_Mario___Skid; }
                }
                else 
                {
                    if (mode == "ordinary") { this.image = im_gl.Mario___Skid_invert; }
                    else if (mode == "big ordinary") { this.image = im_gl.Super_Mario___Skid_invert; }
                    else if (mode == "big shooter") { this.image = im_gl.Fiery_Mario___Skid_invert; }
                }
            }
            else if (image == "Mario")
            {
                if (direction == true) { this.image = im_gl.Mario; }
                else { this.image = im_gl.Mario_invert; }
            }
            else if (image == "Super Mario")
            {
                if (direction == true) { this.image = im_gl.Super_Mario; }
                else { this.image = im_gl.Super_Mario_invert; }
            }
            else if (image == "Fiery Mario")
            {
                if (direction == true) { this.image = im_gl.Fiery_Mario; }
                else { this.image = im_gl.Fiery_Mario_invert; }
            }
            else if (image == "Mario/Super Mario/Fiery Mario")
            {
                if (direction == true)
                {
                    if (mode == "ordinary") { this.image = im_gl.Mario; }
                    else if (mode == "big ordinary") { this.image = im_gl.Super_Mario; }
                    else if (mode == "big shooter") { this.image = im_gl.Fiery_Mario; }
                }
                else
                {
                    if (mode == "ordinary") { this.image = im_gl.Mario_invert; }
                    else if (mode == "big ordinary") { this.image = im_gl.Super_Mario_invert; }
                    else if (mode == "big shooter") { this.image = im_gl.Fiery_Mario_invert; }
                }

            }
            else if (image == "Jump")
            {
                if (direction == true)
                {
                    if (mode == "ordinary") { this.image = im_gl.Mario___Jump; }
                    else if (mode == "big ordinary") { this.image = im_gl.Super_Mario___Jump; }
                    else if (mode == "big shooter") { this.image = im_gl.Fiery_Mario___Jump; }
                }
                else
                {
                    if (mode == "ordinary") { this.image = im_gl.Mario___Jump_invert; }
                    else if (mode == "big ordinary") { this.image = im_gl.Super_Mario___Jump_invert; }
                    else if (mode == "big shooter") { this.image = im_gl.Fiery_Mario___Jump_invert; }
                }
            }
            else if (image == "Duck")
            {
                if (direction == true)
                {
                    if (mode == "big ordinary") { this.image = im_gl.Super_Mario___Duck; }
                    else if (mode == "big shooter") { this.image = im_gl.Fiery_Mario___Duck; }
                }
                else
                {
                    if (mode == "big ordinary") { this.image = im_gl.Super_Mario___Duck_invert; }
                    else if (mode == "big shooter") { this.image = im_gl.Fiery_Mario___Duck_invert; }
                }
            }
            else if (image == "Dead")
            {
                this.image = im_gl.Mario___Dead__1_;
            }
        }

        public void Intangible_Mario()
        {

            if (mode == "intangible ordinary")
            {
                if (intangible < 150)
                {
                    intangible += 1;
                    if (intangible % 10 == 0)
                    {
                        if (image is not null) { images_previous = image;  image = null; }
                        else { image = images_previous; }
                    }
                }
                else
                {
                    image = images_previous;
                    mode = "ordinary";
                    intangible = 0;
                }
            }
        }

        public void Mario_Dead()
        {
            Defines_the_image("Dead");
            deadPadeniye = true;
            stopForm1_KeyDown = true;
            TimerRight = false;
            TimerLeft = false;
            TimerSpace = false;
            top = 3000;
            g = -20;
            TimerGravity = true;
        }
    }
}
