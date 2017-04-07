using UnityEngine;
using System.Collections;
using System;
using System.Text;

public class SphereCommands : MonoBehaviour
{
    float m_progress = 0f;      //  進捗 [0, 1)
    int m_ix = 0;               //  現データインデックス
    Vector3[] m_data = {new Vector3(-4f, 0.5f, -4f),
                        new Vector3( 4f, 0.5f, -4f),
                        new Vector3( 4f, 0.5f,  4f),
                        new Vector3(-4f, 0.5f,  4f),
                        new Vector3(-4f, 0.5f, -4f)};

    public AudioClip goodmorning;
    public AudioClip yoiichinichi;
    public AudioClip sutekinaichinichi;
    public AudioClip ganbatte;
    private AudioClip[] goodmorning_next_voices;

    public AudioClip otsukaresama1;
    public AudioClip otsukaresama2;
    public AudioClip mataashita;
    public AudioClip gokigenyou;

    public AudioClip nani;

    public AudioClip gmail1;
    public AudioClip gmail2;
    public AudioClip gmail3;
    public AudioClip gmail4;

    public AudioClip schedule;

    public AudioClip idobata;

    public AudioClip sasuga;
    public AudioClip deathma;
    public AudioClip shinchoku;
    public AudioClip shihen;
    public AudioClip gokatsuyaku;
    public AudioClip sobaniiru;

    AudioSource audioSource;

    Vector3 originalPosition;

    Animator animator;

    System.Random random = new System.Random();

    // Use this for initialization
    void Start()
    {
        goodmorning_next_voices = new AudioClip[]{ yoiichinichi, sutekinaichinichi, ganbatte };

        audioSource = gameObject.GetComponent<AudioSource>();

        // Grab the original local position of the sphere when the app starts.
        originalPosition = this.transform.localPosition;

        animator = GetComponent<Animator>();

        StartCoroutine(PullEvents(PrintEvent));
        
        // StartCoroutine(LateStart(5F));

        // transform.position = m_data[m_ix];
    }

    IEnumerator PullEvents(Action<string> callback)
    {
        while (true)
        {
            Debug.Log("Try to connect Google");
            StartCoroutine(EventsPuller(callback));
            yield return new WaitForSeconds(60F);
        }
    }


    IEnumerator LateStart(float time)
    {
        yield return new WaitForSeconds(time);

        Debug.Log("Start Linda move!");

        var headPosition = Camera.main.transform.position;
        var gazeDirection = Camera.main.transform.forward;

        RaycastHit hitInfo;

        if (Physics.Raycast(headPosition, gazeDirection, out hitInfo))
        {
            this.transform.position = hitInfo.point;
        }

    }

    // Called by GazeGestureManager when the user performs a Select gesture
    void OnSelect()
    {
        Debug.Log("OnSelect called!!");

        animator.SetTrigger("greeting");
        TextMesh textMesh = (TextMesh)GameObject.Find("MessageText").GetComponent(typeof(TextMesh));
        textMesh.text = "";

        PlayGoodmorningSound();

        StartCoroutine(Idobata("IN"));
    }

    void OnSelect2()
    {
        Debug.Log("OnSelect2 called!!");

        animator.SetTrigger("greeting");
        TextMesh textMesh = (TextMesh)GameObject.Find("MessageText").GetComponent(typeof(TextMesh));
        textMesh.text = "";

        PlayOtsukareSound();

        StartCoroutine(Idobata("OUT"));
    }

    void OnSelect3()
    {
        Debug.Log("OnSelect3 called!!");

        AudioClip[] tapped_sounds = { nani, sasuga, deathma, shinchoku, shihen, gokatsuyaku, sobaniiru, ganbatte };

        audioSource.clip = tapped_sounds[random.Next(8)];
        audioSource.Play();
    }

    // Called by SpeechManager when the user says the "Drop sphere" command
    void OnDrop()
    {
        // Just do the same logic as a Select gesture.
        OnSelect();
    }
    
    // Called by SpeechManager when the user says the "Drop sphere" command
    void OnDrop2()
    {
        // Just do the same logic as a Select gesture.
        OnSelect2();
    }

    void _Update()
    {
    }

    private void PrintEvent(string json)
    {
        LindaEvent lindaEvent = JsonUtility.FromJson<LindaEvent>(json);

        Debug.Log(lindaEvent.event_type);
        Debug.Log(lindaEvent.text);

        string messageTag = "";

        if (lindaEvent.event_type == "mail")
        {
            PlayMailSound();

            messageTag = "【メール受信】";
        }
        else if (lindaEvent.event_type == "schedule")
        {
            PlayScheduleSound();

            messageTag = "【予定】";
        }
        else if (lindaEvent.event_type == "idobata")
        {
            PlayIdobataSound();

            messageTag = "【Idobata】";
        }
        else {

            return;
        }

        animator.SetTrigger("message");

        TextMesh textMesh = (TextMesh)GameObject.Find("MessageText").GetComponent(typeof(TextMesh));
        textMesh.text = FormatMessage(messageTag, lindaEvent.text);

        StartCoroutine(RemoveMessage(textMesh));
    }

    private void PlayMailSound() {
        AudioClip[] mail_voices = { gmail1, gmail2, gmail3, gmail4 };

        audioSource.clip = mail_voices[random.Next(4)];
        audioSource.Play();
    }

    private void PlayScheduleSound() {
        audioSource.clip = schedule;
        audioSource.Play();
    }

    private void PlayIdobataSound() {
        audioSource.clip = idobata;
        audioSource.Play();
    }

    private void PlayGoodmorningSound() {
        audioSource.clip = goodmorning;
        audioSource.Play();

        StartCoroutine(DelayedSound(goodmorning_next_voices[random.Next(3)], 2.5F));
    }

    private void PlayOtsukareSound() {
        AudioClip[] otsukare_voices = { otsukaresama1, otsukaresama2 };
        audioSource.clip = otsukare_voices[random.Next(2)];
        audioSource.Play();

        AudioClip[] otsukare_next_voices = { mataashita, gokigenyou };

        StartCoroutine(DelayedSound(otsukare_next_voices[random.Next(2)], 2.5F));
    }

    IEnumerator DelayedSound(AudioClip clip, float seconds) {
        yield return new WaitForSeconds(seconds);

        audioSource.clip = clip;

        audioSource.Play();
    }

    private string FormatMessage(string messageTag, string message) {
        StringBuilder sb = new StringBuilder();
        sb.Append(messageTag).Append("\r\n").Append(message);

        return sb.ToString();
    }

    IEnumerator RemoveMessage(TextMesh textMesh) {
        yield return new WaitForSeconds(20F);

        textMesh.text = "";
    }

    IEnumerator EventsPuller(Action<string> callback) {
        Debug.Log("Requesting start...");
        WWW request = new WWW("gas-url");

        yield return request;

        if (!string.IsNullOrEmpty(request.error)) {
            Debug.Log(request.error);
        } else {
            Debug.Log(request.responseHeaders["STATUS"]);
            string event_info = request.text;
            if (!string.IsNullOrEmpty(event_info)) {
                callback(request.text);
            }
            //if (request.responseHeaders.ContainsKey("STATUS") &&
            //    request.responseHeaders["STATUS"] == "200") {
            //    Debug.Log("Success!!!!");
            //}
        }
    }

    IEnumerator Idobata(string s)
    {
        WWWForm form = new WWWForm();
        form.AddField("source", "@smagla " + s);
        form.AddField("format", "html");
        WWW www = new WWW("idobata-hook-url", form);
        yield return www;
        if (www.error == null)
        {
            Debug.Log(www.text);
        }
    }

}

[Serializable]
public class LindaEvent
{
    public string event_type;
    public string text;
}
