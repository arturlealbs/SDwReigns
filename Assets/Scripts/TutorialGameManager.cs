using UnityEngine;

public class TutorialGameManager : MonoBehaviour
{
    public SpriteRenderer cardRenderer;
    public GameObject cardPrefab;
    public CardController cardController;
    public float movingSpeed = 1f;
    public float margin = 2f;
    public float sideTrigger;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetMouseButton(0) && cardController.isHovering)
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            cardPrefab.transform.position = mousePosition;
        }
        else
        {
            cardPrefab.transform.position = Vector2.MoveTowards(cardPrefab.transform.position, new Vector2(0,0), movingSpeed);
        }

        if (cardPrefab.transform.position.x > margin)
        {
            cardRenderer.color = Color.green;
            if (!Input.GetMouseButton(0) && cardPrefab.transform.position.x > sideTrigger)
            {

            }
        }
        else if (cardPrefab.transform.position.x < -margin)
        {
            cardRenderer.color = Color.red;
            if (!Input.GetMouseButton(0) && cardPrefab.transform.position.x > sideTrigger)
            {
            }
        }
        else
        {
            cardRenderer.color = Color.white;
        }
    }
}
