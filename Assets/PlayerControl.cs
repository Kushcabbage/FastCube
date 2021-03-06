﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using XInputDotNetPure;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class PlayerControl : MonoBehaviour {

    bool playerIndexSet = false;
    PlayerIndex playerIndex;
    GamePadState state;
    GamePadState prevState;
    public float TurnSpeed;
    public float MoveSpeed;
    bool Canjump;
    public float jumpPower;
    int i = 0;
    public bool ignoreSpin = false;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;
    public float lastJump;
    public bool living;
    public int Score;
    
    public bool canSpin;
    Rigidbody rb;
    public Transform camera;
    public float spincooldowntime;
    public bool isattacking;
    public float attackSpinTime;
    public bool win;
    public bool paused;
    public String course;
    public List<string> completedStages;


    public AudioSource audioSource;
    public AudioClip Jumpsound;
    public AudioClip Pickupsound;
    public AudioClip Spinsound;
    public AudioClip Deathsound;

    GameObject Wintext;
    Text ScoreText;
    GameObject pausemenu;
    Renderer playerRenderer;
    // Use this for initialization
    void Start () {
        rb = GetComponent<Rigidbody>();
        living = true;
        canSpin = true;
        Score = 0;
        win = false;
        Wintext = GameObject.Find("WinText");
        Wintext.SetActive(false);
        ScoreText = GameObject.Find("ScoreText").GetComponent<Text>();
        pausemenu = GameObject.Find("PauseMenu");
        audioSource = GetComponent<AudioSource>();
        playerRenderer = GetComponent<Renderer>();
        completedStages = new List<string>();
        course = "";
        
    }

    // Update is called once per frame
    void Update()
    {

            // Find a PlayerIndex, for a single player game
            // Will find the first controller that is connected ans use it
            if (!playerIndexSet || !prevState.IsConnected)
            {
                for (int i = 0; i < 4; ++i)
                {
                    PlayerIndex testPlayerIndex = (PlayerIndex)i;
                    GamePadState testState = GamePad.GetState(testPlayerIndex);
                    if (testState.IsConnected)
                    {
                        Debug.Log(string.Format("GamePad found {0}", testPlayerIndex));
                        playerIndex = testPlayerIndex;
                        playerIndexSet = true;
                    }
                }
            }

            prevState = state;
            state = GamePad.GetState(playerIndex);
         if (!win)                                              
        {
            ScoreText.text = Score.ToString();

            if (!paused)
            {
                Time.timeScale = 1f;
                pausemenu.SetActive(false);
                // Detect if a button was pressed this frame
                if (prevState.Buttons.A == ButtonState.Released && state.Buttons.A == ButtonState.Pressed)
                {
                    Jump();
                }

                if (prevState.Buttons.X == ButtonState.Released && state.Buttons.X == ButtonState.Pressed && canSpin)
                {
                    audioSource.clip = Spinsound;
                    audioSource.Play();
                    StartCoroutine(attacking(attackSpinTime));
                    canSpin = false;
                    StartCoroutine(Rotate(attackSpinTime, new Vector3(0, 1, 0)));
                    StartCoroutine(spinCooldown(spincooldowntime));

                }

                if (prevState.Buttons.Start == ButtonState.Released && state.Buttons.Start == ButtonState.Pressed)
                {

                    paused = true;
                }

                if (rb.velocity.y < 0)
                {
                    rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;


                }
                else if (rb.velocity.y > 0 && state.Buttons.A == ButtonState.Released)
                {
                    rb.velocity += Vector3.up * Physics.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
                }

                if (!living)
                {
                    audioSource.clip = Deathsound;
                    audioSource.Play();
                    SceneManager.LoadScene(SceneManager.GetActiveScene().name);
                    
                }



                //transform.position += transform.forward * Time.deltaTime * state.ThumbSticks.Left.Y*MoveSpeed;
                Vector3 camF = camera.forward;
                Vector3 camR = camera.right;
                camF.y = 0;
                camR.y = 0;
                camF = camF.normalized;
                camR = camR.normalized;
                transform.position += (camF * state.ThumbSticks.Left.Y + camR * state.ThumbSticks.Left.X) * Time.deltaTime * MoveSpeed;

            }
            else {              //not won and paused
                Time.timeScale = 0;
                if (prevState.Buttons.Start == ButtonState.Released && state.Buttons.Start == ButtonState.Pressed)
                {

                    paused = false;
                }

                pausemenu.SetActive(true);



            }

        }
        else {
            ///YOU WIN!
            Wintext.SetActive(true);
            
            Time.timeScale = 0.5f;

            Vector3 camF = camera.forward;
            Vector3 camR = camera.right;
            camF.y = 0;
            camR.y = 0;
            camF = camF.normalized;
            camR = camR.normalized;
            transform.position += (camF * state.ThumbSticks.Left.Y + camR * state.ThumbSticks.Left.X) * Time.deltaTime * MoveSpeed;

            if (prevState.Buttons.Start == ButtonState.Released && state.Buttons.Start == ButtonState.Pressed)
            {

                LoadnextLevel();
            }

        }
       
         
    }

    public int GetPlayerScore() {
        return Score;
        }

    void LoadnextLevel() {

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);


    }


    public void stageCompleteFeeback() {

        //flash score
        StartCoroutine(ScoretoOrange(0.3f));

        Score = 0;
        

    }


        void OnCollisionEnter(Collision collision)
    {
       

        if (collision.gameObject.tag == "standable") {
            Canjump = true;
            
        }

        if (collision.gameObject.tag == "death") {
            
                living = false;
         
            
        }

        if (collision.gameObject.tag == "enemy")
        {
            if (!isattacking)
            {
                living = false;
            }
            else
            {
                Destroy(collision.gameObject);
                Score++;
            }

        }


      
        

    }

    void OnCollisionExit(Collision collision) {
        if (collision.gameObject.tag == "standable") {
            Canjump = false;
        }

    }

    public void Pickup(GameObject item) {

        Destroy(item.gameObject);
        Score++;
        audioSource.clip = Pickupsound;
        audioSource.Play();


    }

    void Jump() {

        if (Canjump)
        {
            
            GetComponent<Rigidbody>().AddForce(new Vector3(0, jumpPower, 0), ForceMode.Impulse);
            audioSource.clip = Jumpsound;
            audioSource.Play();
        }
        
        

    }

    IEnumerator ScoretoOrange(float duration) {
        float t = 0.0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            ScoreText.color = Color.HSVToRGB(0.09f, 1f, t / duration);
            yield return null;
        }
        ScoreText.color = Color.HSVToRGB(0.3f, 1f, 0f);
    }

    IEnumerator Rotate(float duration,Vector3 dir)
    {
        ignoreSpin = true;
        Quaternion startRot = transform.rotation;
        float t = 0.0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            transform.rotation = startRot * Quaternion.AngleAxis(t / duration * 360f, dir); //or transform.right if you want it to be locally based
            yield return null;
        }
        transform.rotation = Quaternion.identity;
        ignoreSpin = false;
    }


    IEnumerator attacking(float duration)
    {
        isattacking = true;
        yield return new WaitForSecondsRealtime(duration);
        isattacking = false;
    }

    IEnumerator spinCooldown(float duration)
    {
        float t = 0f;                       
        while (t < duration) {
            t += Time.deltaTime;
            playerRenderer.material.color = Color.HSVToRGB(0.1f, t / duration, 0.95f);
            //Debug.Log(t / duration);
            yield return null;              //makes sure there is only one while iteration per frame
        }
        //playerRenderer.material.color = Color.HSVToRGB(33/360,94,95);
        canSpin = true;
    }

}
