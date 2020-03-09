using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorMain { RED, GREEN, BLUE };

public class Rainbow : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    private SpriteRenderer sprRendRef;
    private Color curCol;
    private ColorMain colToChange;
    private bool shouldIncrease;

    private void Awake()
    {
        sprRendRef = this.GetComponent<SpriteRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        curCol = sprRendRef.color;
        colToChange = ColorMain.RED;
        StartCoroutine(StarPowerUpdate());
    }

    private void OnEnable()
    {
        curCol = sprRendRef.color;
        colToChange = ColorMain.RED;
        StartCoroutine(StarPowerUpdate());
    }

    private IEnumerator StarPowerUpdate()
    {
        while (true)
        {
            if (colToChange == ColorMain.RED)
            {
                curCol.r += Time.deltaTime * speed;
                curCol.g -= Time.deltaTime * speed;
                if (curCol.r >= 1)
                {
                    colToChange += 1;
                }
                yield return null;
            }
            else if (colToChange == ColorMain.GREEN)
            {
                curCol.g += Time.deltaTime * speed;
                curCol.b -= Time.deltaTime * speed;
                if (curCol.g >= 1)
                {
                    colToChange += 1;
                }
                yield return null;
            }
            else
            {
                curCol.b += Time.deltaTime * speed;
                curCol.r -= Time.deltaTime * speed;
                if (curCol.b >= 1)
                {
                    colToChange = ColorMain.RED;
                }
                yield return null;
            }
            sprRendRef.color = curCol;
            yield return null;
        }
    }
}
