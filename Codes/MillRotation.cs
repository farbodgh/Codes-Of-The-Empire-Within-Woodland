using UnityEngine;

public class MillRotation : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        //If there are peasants working then will rotate - change later
        transform.Rotate(0.0f, 0.0f, Time.deltaTime * 45.0f);
    }
    

}
