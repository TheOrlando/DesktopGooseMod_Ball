using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Reflection;
using System.IO;
// 1. Added the "GooseModdingAPI" project as a reference.
// 2. Compile this.
// 3. Create a folder with this DLL in the root, and *no GooseModdingAPI DLL*
using GooseShared;
using SamEngine;
using System.Windows.Forms;

namespace Ball
{
    public class ModEntryPoint : IMod
    {

        Point position;
        Vector2 velocity;
        float speed, deceleration, lastKickTime, lastAnimateTime, animationGap;
        Image[] images;
        int currentImage;

        // Gets called automatically, passes in a class that contains pointers to
        // useful functions we need to interface with the goose.
        void IMod.Init()
        {
            position = new Point(300, 300);
            velocity = new Vector2(0, 0);
            lastKickTime = Time.time;
            speed = 0;
            deceleration = 0.25f;
            images = new Image[3];
            currentImage = 0;
            lastAnimateTime = Time.time;
            animationGap = 0;

            string assemblyFolder = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            string PicFileName = Path.Combine(assemblyFolder, "ball.png");
            images[0] = Image.FromFile(Path.Combine(assemblyFolder, "ball.png"));
            images[1] = Image.FromFile(Path.Combine(assemblyFolder, "ball2.png"));
            images[2] = Image.FromFile(Path.Combine(assemblyFolder, "ball3.png"));

            // Subscribe to whatever events we want
            InjectionPoints.PostTickEvent += PostTick;
            InjectionPoints.PreRenderEvent += PreRender;
        }

        private void PreRender(GooseEntity goose, Graphics g)
        {
            g.DrawImage(images[currentImage], position);
        }

        public void PostTick(GooseEntity goose)
        {
            if(speed > 0)
            {
                animationGap = 0.25f / speed;
                if (Time.time - lastAnimateTime > animationGap)
                {
                    animate();
                    lastAnimateTime = Time.time;
                }

                velocity.x = Vector2.Normalize(velocity).x * speed;
                velocity.y = Vector2.Normalize(velocity).y * speed;

                position.X += (int)velocity.x;
                if(position.X < 0)
                {
                    position.X = 0;
                    velocity.x *= -1;
                }
                if(position.X + 40 > Screen.PrimaryScreen.WorkingArea.Width)
                {
                    position.X = Screen.PrimaryScreen.WorkingArea.Width - 40;
                    velocity.x *= -1;
                }

                position.Y += (int)velocity.y;
                if (position.Y < 0)
                {
                    position.Y = 0;
                    velocity.y *= -1;
                }
                if (position.Y + 40 > Screen.PrimaryScreen.WorkingArea.Height)
                {
                    position.Y = Screen.PrimaryScreen.WorkingArea.Height - 40;
                    velocity.y *= -1;
                }

                speed -= deceleration;

                if (speed <= 0)
                    currentImage = 0;
            }

            if(Input.mouseX > position.X && Input.mouseX < position.X + 40)
                if (Input.mouseY > position.Y && Input.mouseY < position.Y + 40)
                {
                    speed = 20;
                    Vector2 temp = new Vector2(position.X + 20 - Input.mouseX, position.Y + 20 - Input.mouseY);
                    velocity.x = Vector2.Normalize(temp).x * speed;
                    velocity.y = Vector2.Normalize(temp).y * speed;
                    lastKickTime = Time.time;

                    API.Goose.setCurrentTaskByID(goose, "ChargeTenSeconds");
                }

            if (goose.currentTask == API.TaskDatabase.getTaskIndexByID("ChargeTenSeconds"))
            { 
                goose.targetPos = new Vector2 (position.X + 20, position.Y + 20);

                if(API.Goose.isGooseAtTarget(goose, 40) && Time.time - lastKickTime > 1f)
                {
                    speed = 20;
                    Vector2 temp = new Vector2(position.X + 20 - goose.position.x, position.Y + 20 - goose.position.y);
                    velocity.x = Vector2.Normalize(temp).x * speed;
                    velocity.y = Vector2.Normalize(temp).y * speed;
                    lastKickTime = Time.time;

                    API.Goose.playHonckSound();
                    API.Goose.setSpeed(goose, GooseEntity.SpeedTiers.Walk);
                    API.Goose.setTaskRoaming(goose);
                }
            }
        }

        public void animate()
        {
            if (velocity.x < 0)
            {
                currentImage++;
            }
            else
            {
                currentImage--;
            }

            if (currentImage > 2)
            {
                currentImage = 0;
            }
            else if(currentImage < 0)
            {
                currentImage = 2;
            }
        }
    }
}
