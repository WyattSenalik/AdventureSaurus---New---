using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ColorMain { RED, GREEN, BLUE };

public class Rainbow : MonoBehaviour
{
    [SerializeField] private float speed = 3f;
    [SerializeField] private GameObject partSysObj = null;
    private SpriteRenderer sprRendRef;
    private Color curCol;
    private ColorMain colToChange;
    private bool shouldIncrease;
    private GameObject myPartSys;
    private bool hasSpawned;

    private void Awake()
    {
        sprRendRef = this.GetComponent<SpriteRenderer>();
    }

    // Start is called before the first frame update
    void Start()
    {
        hasSpawned = false;
        sprRendRef.color = new Color(0, 1, 1);
        curCol = sprRendRef.color;
        colToChange = ColorMain.RED;
    }

    public void StartFlashing()
    {
        curCol = sprRendRef.color;
        colToChange = ColorMain.RED;
        StartCoroutine(StarPowerUpdate());
        if (!hasSpawned)
        {
            //Debug.Log("Hello from " + this.name);
            myPartSys = Instantiate(partSysObj);
            myPartSys.transform.SetParent(this.transform);
            hasSpawned = true;
        }
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

            ParticleSystem tempPartSys = myPartSys.GetComponent<ParticleSystem>();
            ParticleSystem.MainModule tempMain = tempPartSys.main;
            tempMain.startColor = curCol;
            myPartSys.transform.localPosition = Vector3.zero;
            yield return null;
        }
    }
}
