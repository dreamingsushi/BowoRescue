using UnityEngine;

public abstract class EnemyBaseState {
    public Enemy enemy;
    public Animator animator;

    public EnemyBaseState(Enemy enemy, Animator animator) {
        this.enemy = enemy;
        this.animator = animator;
    }

    public abstract void Enter();
    public abstract void Update();
    public abstract void Exit();
}
