using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Rigidbody rb;
    enum moveState {Walking, Swimming, Airborne }
    moveState _moveState = moveState.Walking;
    const float speed = 2.5f;
    float _speed = speed;
    GameObject character;
    float camAngle = 0;
    List<Relic> backpack;
    float funds = 0;
    float health = 100;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        character = transform.Find("Character").gameObject;
        backpack = new List<Relic>();
    }

    private void Update()
    {
        
        if (_moveState == moveState.Swimming)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                rise(1);
            }
            brake();
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Space) && grounded())
            {
                rise(0);
            }
        }

        if(Input.GetMouseButtonDown(1))
        {
            Debug.Log("Attempting pickup");
            pickUp();
        }

        if(Input.GetMouseButtonDown(0))
        {
            Interact();
        }

        if (Input.GetKey(KeyCode.LeftShift))
        {
            _speed = Mathf.Lerp(_speed, speed * 5f, 1 * Time.deltaTime);
        }
        else if(_speed!=speed)
        {
            _speed = Mathf.Lerp(_speed, speed, 2 * Time.deltaTime);
        }
    }

    void FixedUpdate()
    {
        move();
        rotate();
        camStick();
    }

    void move()//only move parent player object
    {        
        const float gravForce = 1; //strength of gravity

        float horz = Input.GetAxis("Horizontal");
        float fwd = Input.GetAxis("Vertical");
        float vert = rb.velocity.y;

        if (_moveState != moveState.Swimming)
        {
            //gravity calculations
            if (grounded()) //character on ground
            {
                 horz = Input.GetAxis("Horizontal");
                 fwd = Input.GetAxis("Vertical");
                if (vert < 0) //dont prevent jumping
                {
                    vert = 0;
                }
                if (_moveState != moveState.Walking)
                {
                    _moveState = moveState.Walking;
                    character.transform.rotation = Quaternion.Euler(0, character.transform.rotation.eulerAngles.y, 0);
                }

            }
            else //character in air
            {
                if (vert < 0)
                {
                    vert -= gravForce * 2;
                }
                else
                {
                    vert -= gravForce;
                }
                vert = Mathf.Clamp(vert, -30, 50);
                if (_moveState != moveState.Airborne)
                {
                    _moveState = moveState.Airborne;
                }
            }
        }

        Vector3 newDir = new Vector3(horz, 0, fwd);
        newDir = newDir.normalized * _speed;
        //dont normalize gravity or jumpForce
        newDir.y = vert;
        //rotate new dir based on camera parent (camRig) when grounded or airborne
        if (_moveState != moveState.Swimming)
        {
            newDir = character.transform.rotation * newDir;
        }
        else
        {
            newDir = character.transform.rotation * newDir;
        }
        
        if(_moveState == moveState.Swimming)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, newDir, 1 * Time.deltaTime);
        }
        else
        {
            rb.velocity = newDir;
        }        
    }

    void rise(int pass)
    {
        const float jumpForce = 20; //strength of characters jump

        if (pass == 0) //character jumps
        {            
            rb.AddForce(Vector3.up * jumpForce,ForceMode.VelocityChange);            
        }

        if (pass == 1) //character swims upwards
        {
            rb.AddForce(Camera.main.transform.up * _speed);
        }

        if(pass == 2) //coming out of water
        {
            rb.AddForce(Vector3.up * (jumpForce*0.75f), ForceMode.VelocityChange);
        }
    }

    void rotate()//only rotate child character object
    {
        const float mod = 1; //sensitivity
        float x = Input.GetAxis("Mouse X") * mod;
        float y = -Input.GetAxis("Mouse Y") * mod;
        if(_moveState != moveState.Swimming)
        {   //dont rotate character vertically if they aren't swimming
            y = 0;
        }

        Quaternion rot = character.transform.rotation;
        rot.eulerAngles += new Vector3(y,x,0);
        character.transform.rotation = rot;
    }


    bool grounded()
    {
        Collider col = GetComponentInChildren<Collider>();
        Ray ray = new Ray(character.transform.position, -Vector3.up);
        LayerMask lm = LayerMask.GetMask("Default");
        Debug.DrawRay(ray.origin, ray.direction * col.bounds.extents.y * 1.2f, Color.red);
        if(Physics.Raycast(ray,col.bounds.extents.y*1.2f,lm))
        {
            return true;
        }
        return false;        
    }

    Vector3 groundPoint()
    {
        return new Vector3(0, 0, 0);
    }

    void camStick()//camera stick shoots from character not parent
    {
        float camSpeed = 0.2f + (0.4f * rb.velocity.magnitude);
        float rotSpeed;
        float angle = camAngle;
        float dist = 3.75f; //distance for raycast
        Vector3 offset = new Vector3(0.68f, 0.85f, 0);
        offset = character.transform.rotation * offset; //make offset local to character
        Quaternion newRot;
        if (_moveState != moveState.Swimming)
        {
            const float mod = 2; //sensitivity
                                   
            Vector3 dir = -Vector3.forward;
            
            float mouse = -Input.GetAxis("Mouse Y") * mod; //get inverted mouse movement and multiply by sensitivity
            if (_moveState != moveState.Swimming)
            {
                angle = Mathf.Clamp(angle + mouse, -50, 50);
            }
            angle += mouse;

            Quaternion rot = character.transform.rotation;
            rot.eulerAngles += new Vector3(angle, 0, 0);
            dir = rot * dir;
            Ray ray = new Ray(character.transform.position + offset, dir);
            RaycastHit hit;
            LayerMask lm = LayerMask.GetMask("Default");

            Debug.DrawRay(ray.origin, ray.direction * dist, Color.green);
            Physics.Raycast(ray, out hit, dist, lm);
            if (hit.transform != null)
            {
                dist = Vector3.Distance(hit.point, ray.origin) * 0.9f;
            }

            //move camera
            Vector3 newPos = character.transform.position + offset + (dir * dist);
            Camera.main.transform.parent.position = Vector3.Lerp(Camera.main.transform.parent.position, newPos, camSpeed);
            newRot = Quaternion.LookRotation(character.transform.Find("Crosshair").position - Camera.main.transform.position);
            //Camera.main.transform.LookAt(character.transform.Find("Crosshair"));
        }
        else
        {
            Vector3 newPos = character.transform.position + offset + (-character.transform.forward * dist);
            Camera.main.transform.parent.position = Vector3.Lerp(Camera.main.transform.parent.position, newPos, camSpeed);
            //Camera.main.transform.rotation = character.transform.rotation;
            newRot = character.transform.rotation;
        }
        rotSpeed = 0.01f * Quaternion.Angle(Camera.main.transform.rotation, newRot);
        Camera.main.transform.rotation = Quaternion.Lerp(Camera.main.transform.rotation, newRot, rotSpeed);
        camAngle = angle;
    }

    void Interact() //interact with things in the world
    {
        float dist = Vector3.Distance(character.transform.Find("Crosshair").position, Camera.main.transform.position) * 3f;
        LayerMask lm = LayerMask.GetMask("Default");
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, dist, lm))
        {
            if (hit.transform.GetComponent<Interactable>())
            {
                Debug.Log("hit item!");
                hit.transform.GetComponent<Interactable>().Interact();
            }
        }
    }

    void pickUp() //pickup relic
    {
        float dist = Vector3.Distance(character.transform.Find("Crosshair").position, Camera.main.transform.position) * 3f;
        LayerMask lm = LayerMask.GetMask("Pickup");
        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * dist, Color.blue);
        if(Physics.Raycast(ray, out hit, dist, lm))
        {
            if(hit.transform.GetComponent<Relic>())
            {
                Debug.Log("hit item!");
                addBackpack(hit.transform.GetComponent<Relic>());
            }
        }
    }

    #region backpack

    void addBackpack(Relic r)
    {
        backpack.Add(r);
        refreshBackpack();
    }

    public void removeBackpack(Relic r)
    {
        if (backpack.Contains(r))
        {
            backpack.Remove(r);
            Destroy(r.gameObject);
            refreshBackpack();
        }
    }

    //TODO: place backpack item()

    Relic getRelic(Relic r)
    {
        foreach(Relic rel in backpack)
        {
            if(rel == r)
            {
                return rel;
            }
        }
        return null;
    }

    public List<Relic> getBackpack()
    {
        return backpack;
    }

    void refreshBackpack()
    {
        GameObject bp = transform.Find("Character/BackPack").gameObject;
        foreach(Relic r in backpack)
        {
            r.transform.GetComponent<Collider>().enabled = false;
            r.transform.GetComponent<Rigidbody>().isKinematic = true;
            r.transform.rotation = character.transform.rotation;
            r.transform.position = bp.transform.position - (character.transform.forward * r.GetComponent<Collider>().bounds.extents.z);
            r.transform.parent = bp.transform;
        }
    }
    #endregion

    void brake() //call upon entering water to slow player down
    {
        if (rb.velocity.magnitude > 2)
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, (1 + (0.4f * rb.velocity.magnitude)) * Time.deltaTime);
        }
    }

    public float getFunds()
    {
        return funds;
    }

    public void setFunds(float amount)
    {
        funds = amount;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer == 4)
        {
            _moveState = moveState.Swimming;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        //character.transform.rotation = Quaternion.Euler(0, character.transform.rotation.eulerAngles.y, 0);
        
        if(grounded())
        {
            _moveState = moveState.Walking;
        }
        else
        {
            _moveState = moveState.Airborne;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            rise(2);
        }
    }
}
