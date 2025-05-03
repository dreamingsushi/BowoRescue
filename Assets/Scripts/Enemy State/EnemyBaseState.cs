using UnityEngine;

public abstract class EnemyBaseState {
    protected EnemyStateManager enemy;
    public EnemyBaseState(EnemyStateManager enemy) {
        this.enemy = enemy;
    }

    public abstract void EnterState();
    public abstract void UpdateState();
    public abstract void ExitState();
}
