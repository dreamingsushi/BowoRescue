using UnityEngine;

public class Spell : MonoBehaviour
{
    public float spellDuration = 2f;

    void Start()
    {
        Destroy(gameObject, spellDuration);
    }
}
