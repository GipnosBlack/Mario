namespace Mario1.GameObject.Gameobject.Creature
{
    using im_gl = Properties.Resources;// im_gl = images_global все картинки проекта

    public class Creature : GameObject
    {
        public Creature(int x, int y, string name, int height, int width, Image image, bool direction, string property, int top, int g, int spaceG, List<string> condition)
        {
            X = x;
            Y = y;
            this.name = name;
            this.height = height;
            this.width = width;
            this.direction = direction;
            this.property = property;
            this.top = top;
            this.g = g;
            this.spaceG = spaceG;
            spaceG_const = spaceG;
            for (int i = 0; i < condition.Count; i++)
            {
                if(i==0) State = ParseCondition(condition[i]);
                else State |= ParseCondition(condition[i]);
            }
            this.image = image;
            run_animation = 0;
            runIf = 0;
            speed = 5;
            we_stand = -1;
        }
        public Creature(int x, int y, string name, int height, int width, Image image, bool direction, string property, int top, int g, int spaceG, List<string> condition, int proper_height) : this( x,  y,  name,  height,  width,  image,  direction,  property,  top,  g,  spaceG, condition)
        {
            this.proper_height = proper_height;
        }
        public int top;
        public int g;
        public int spaceG;
        public int spaceG_const;
        public CreatureState State;
        public int proper_height;//сколькло будет лететь вверх coin с State(CoinUp), чтобы потом удалиться

        public bool TimerGravity = true;

        private static CreatureState ParseCondition(string cond) => cond switch
        {
            "stands" => CreatureState.Stands,
            "dead_fall" => CreatureState.DeadFall,
            "intangible" => CreatureState.Intangible,
            "attack_on_everyone" => CreatureState.AttackOnEveryone,
            "waiting_for_Mario_to_exit_to_kill" => CreatureState.WaitingForMario,
            "jump" => CreatureState.Jump,
            "doesn_t_kill" => CreatureState.DoesntKill,
            "coin_up" => CreatureState.CoinUp,
            _ => CreatureState.None
        };

        public void Delete_сreature_we_stand(int i)
        {
            if (we_stand == i) we_stand = -1;
            else if (we_stand > i) we_stand -= 1;
        }

        public void Dead_fall()
        {
            State |= CreatureState.DeadFall | CreatureState.Intangible;
            g = 1;
            TimerGravity = true;
            top = 1500;
        }

        public Rectangle DestRect()
        {
            return new Rectangle(0, 0, width, height);
        }

        public bool Animation(string metod, short triger)
        {
            Animation(metod);
            if (run_animation >= triger) return true;
            return false;
        }

        public void Animation(string metod)
        {
            run_animation++;
            switch (metod)
            {
                case "":
                    return;
                case "Walk":
                    if (name == "SMB_greenkoopatroopa")
                    {
                        if (direction)
                        {
                            if (run_animation % 10 == 0)
                            {
                                if (runIf == 0) { image = im_gl.SMB_greenkoopatroopa1; runIf = 1; }
                                else { image = im_gl.SMB_greenkoopatroopa2; runIf = 0; }
                                run_animation = 0;
                            }
                        }
                        else
                        {
                            if (run_animation % 10 == 0)
                            {
                                if (runIf == 0) { image = im_gl.SMB_greenkoopatroopa1_invert; runIf = 1; }
                                else { image = im_gl.SMB_greenkoopatroopa2_invert; runIf = 0; }
                                run_animation = 0;
                            }
                        }
                    }
                    else if (name == "SMB_greenparatrooper")
                    {
                        if (direction)
                        {
                            if (run_animation % 10 == 0)
                            {
                                if (runIf == 0) { image = im_gl.SMB_greenparatrooper1; runIf = 1; }
                                else { image = im_gl.SMB_greenparatrooper2; runIf = 0; }
                                run_animation = 0;
                            }
                        }
                        else
                        {
                            if (run_animation % 10 == 0)
                            {
                                if (runIf == 0) { image = im_gl.SMB_greenparatrooper1_invert; runIf = 1; }
                                else { image = im_gl.SMB_greenparatrooper2_invert; runIf = 0; }
                                run_animation = 0;
                            }
                        }
                    }
                    break;
                case "Dead":
                    if (name == "Image_Goomba") { image = im_gl.Goomba___Grey___Stomp; runIf = 0; }
                    if (name == "SMB_greenparatrooper" || name == "SMB_greenkoopatroopa") image = im_gl.SMB_Greenshell__1_1;
                    break;
            }
        }
    }
}