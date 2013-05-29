using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace PattyPetitGiant
{
    class ToughPrisonerEnemy : Enemy
    {
        private EnemyComponents component;
        public ToughPrisonerEnemy(LevelState parentWorld, float initial_x, float initial_y)
        {
            position = new Vector2(initial_x, initial_y);
            dimensions = new Vector2(48.0f, 48.0f);
            velocity = Vector2.Zero;

            disable_movement = false;
            disable_movement_time = 0.0f;
            knockback_magnitude = 6.0f;
            enemy_damage = 20;
            enemy_life = 10;
            player_found = false;
            change_direction_time = 0.0f;
            range_distance = 300.0f;

            state = EnemyState.Idle;
            enemy_type = EnemyType.Alien;
            component = new IdleSearch();

            this.parentWorld = parentWorld;
        }
    }
}
