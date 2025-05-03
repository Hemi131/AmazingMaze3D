using System;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

namespace AmazingMaze3D
{
    public class EnemyAI
    {
        private enum State { Idle, Wander, Chase }

        private State currentState = State.Idle;
        private float stateTimer = 0f;
        private Vector3 currentDirection = Vector3.UnitX;
        private readonly Random rng = new Random();

        // Parameters
        private const float viewRange = 8f;
        private const float viewAngleThreshold = 0.8f; // ~45 degrees
        private const float idleTimeMin = 2f, idleTimeMax = 5f;
        private const float wanderSpeed = 1f;
        private const float chaseSpeed = 3f;
        private const float reachThreshold = 1f;

        Vector2 vec1;
        Vector2 vec2;
        Vector2 vec3;
        Vector2 vec4;

        public EnemyAI()
        {
            vec1 = new Vector2(-1f, -1f) * 0.5f;
            vec2 = new Vector2(1f, -1f) * 0.5f;
            vec3 = new Vector2(-1f, 1f) * 0.5f;
            vec4 = new Vector2(1f, 1f) * 0.5f;
        }

        public Vector3 GetNextMove(
            Vector3 enemyPos, Vector3 playerPos, Vector3 playerForward, float deltaTime, bool[,] wallHere)
        {
            stateTimer -= deltaTime;
            Vector3 flatEnemy = new Vector3(enemyPos.X, 0, enemyPos.Z);
            Vector3 flatPlayer = new Vector3(playerPos.X, 0, playerPos.Z);
            Vector3 toEnemy = Vector3.Normalize(flatEnemy - flatPlayer);
            float distance = Vector3.Distance(flatEnemy, flatPlayer);

            playerForward.Y = 0; // Ignore Y component for direction comparison
            playerForward.X *= -1f;
            playerForward.Z *= -1f; // Invert direction for correct comparison
            playerForward.Normalize();
            bool playerLookingAtEnemy = Vector3.Dot(playerForward, toEnemy) > viewAngleThreshold;

            // --- State Transitions ---
            switch (currentState)
            {
                case State.Idle:
                    if (stateTimer <= 0)
                    {
                        currentState = State.Wander;
                        stateTimer = 2f + (float)rng.NextDouble() * 3f;
                        PickRandomDirection();
                    }
                    else if (distance < viewRange && (playerLookingAtEnemy || rng.NextDouble() < 0.0))
                    {
                        currentState = State.Chase;
                    }
                    break;

                case State.Wander:
                    if (distance < viewRange && (playerLookingAtEnemy || rng.NextDouble() < 0.0))
                    {
                        currentState = State.Chase;
                    }
                    else if (stateTimer <= 0)
                    {
                        currentState = State.Idle;
                        stateTimer = idleTimeMin + (float)rng.NextDouble() * (idleTimeMax - idleTimeMin);
                    }
                    break;

                case State.Chase:
                    if (distance < reachThreshold)
                    {
                        currentState = State.Wander;
                        stateTimer = 2f + (float)rng.NextDouble() * 3f;
                        PickRandomDirection();
                    }
                    break;
            }

            // --- Movement Based on State ---
            Vector3 move = Vector3.Zero;

            if (currentState == State.Idle)
            {
                // Optionally rotate here
            }
            else if (currentState == State.Wander)
            {
                move = TryMove(enemyPos, currentDirection, wanderSpeed, deltaTime, wallHere);
            }
            else if (currentState == State.Chase)
            {
                Vector3 chaseDir = Vector3.Normalize(flatPlayer - flatEnemy);
                move = TryMove(enemyPos, chaseDir, chaseSpeed, deltaTime, wallHere);
            }

            return move;
        }

        private Vector3 TryMove(Vector3 from, Vector3 desiredDir, float speed, float dt, bool[,] wallHere)
        {
            Vector3 moveAttempt = from + desiredDir * speed * dt;
            if (IsWalkable(moveAttempt, wallHere))
                return desiredDir * speed * dt;

            // Try offset directions toward goal first
            float[] angles = new float[] { 15, -15, 30, -30, 45, -45 }; // degrees
            foreach (float angleDeg in angles)
            {
                float angleRad = MathHelper.DegreesToRadians(angleDeg);
                Vector3 testDir = RotateY(desiredDir, angleRad);
                Vector3 testMove = from + testDir * speed * dt;

                if (IsWalkable(testMove, wallHere))
                    return testDir * speed * dt;
            }

            return Vector3.Zero; // stuck
        }

        private Vector3 RotateY(Vector3 v, float angle)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            return new Vector3(
                v.X * cos - v.Z * sin,
                0,
                v.X * sin + v.Z * cos
            );
        }
        private void PickRandomDirection()
        {
            float angle = (float)(rng.NextDouble() * Math.PI * 2);
            currentDirection = new Vector3((float)Math.Cos(angle), 0, (float)Math.Sin(angle));
        }

        private bool IsWalkable(Vector3 pos, bool[,] wallHere)
        {
            float potX = pos.X;
            float potZ = pos.Z;

            int x1 = (int)((potX + 1f + vec1.X) / 2f);
            int z1 = (int)((potZ + 1f + vec1.Y) / 2f);
            int x2 = (int)((potX + 1f + vec2.X) / 2f);
            int z2 = (int)((potZ + 1f + vec2.Y) / 2f);
            int x3 = (int)((potX + 1f + vec3.X) / 2f);
            int z3 = (int)((potZ + 1f + vec3.Y) / 2f);
            int x4 = (int)((potX + 1f + vec4.X) / 2f);
            int z4 = (int)((potZ + 1f + vec4.Y) / 2f);

            return
                x1 >= 0 && x1 < wallHere.GetLength(0) && z1 >= 0 && z1 < wallHere.GetLength(1) && !wallHere[x1, z1] &&
                x2 >= 0 && x2 < wallHere.GetLength(0) && z2 >= 0 && z2 < wallHere.GetLength(1) && !wallHere[x2, z2] &&
                x3 >= 0 && x3 < wallHere.GetLength(0) && z3 >= 0 && z3 < wallHere.GetLength(1) && !wallHere[x3, z3] &&
                x4 >= 0 && x4 < wallHere.GetLength(0) && z4 >= 0 && z4 < wallHere.GetLength(1) && !wallHere[x4, z4];

        }
    }




}
