using UnityEngine;

public class ScrollNonUI : MonoBehaviour
{
    public bool freezeX;
    public bool freezeY;
    public Constraints xConstraints = new Constraints();
    public Constraints yConstraints = new Constraints();

    [System.Serializable]
    public class Constraints
    {
        public float min;
        public float max;
    }

    private Vector3 startPos;
    private Vector3 dragStartPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            dragStartPos = cam.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(0))
        {
            Vector3 currentPos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 delta = dragStartPos - currentPos;
            Vector3 newPos = startPos + delta;

            if (!freezeX)
                newPos.x = Mathf.Clamp(newPos.x, xConstraints.min, xConstraints.max);
            else
                newPos.x = startPos.x;

            if (!freezeY)
                newPos.y = Mathf.Clamp(newPos.y, yConstraints.min, yConstraints.max);
            else
                newPos.y = startPos.y;

            transform.position = new Vector3(newPos.x, newPos.y, startPos.z);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            startPos = transform.position;
        }
    }

    private Camera cam => Camera.main;
}