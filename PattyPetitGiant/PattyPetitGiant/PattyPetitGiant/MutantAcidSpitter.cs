using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Spine;

namespace PattyPetitGiant
{
    class MutantAcidSpitter : Enemy
    {
        private enum SpitterState
        {
            Search,
            Fire,
            Reset
        }

        private bool death;

        public MutantAcidSpitter(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            dimensions = new Vector2(48.0f, 48.0f);

            enemy_damage = 4;
            enemy_life = 20;

            death = false;
        }

        public override void update(GameTime currentTime)
        {
            
        }

        public override void draw(Spine.SkeletonRenderer sb)
        {
            //base.draw(sb);
        }

        public override void knockBack(Vector2 direction, float magnitude, int damage, Entity attacker)
        {
            //base.knockBack(direction, magnitude, damage, attacker);
        }

        public override void spinerender(SkeletonRenderer renderer)
        {
            //base.spinerender(renderer);
        }
    }
}
