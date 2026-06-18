using System.Collections;
using UnityEngine;

public class GhostController : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;

    [Header("Spawn Settings")]
    public float minSpawnDistance = 15f;
    public float maxSpawnDistance = 30f;

    [Header("Approach Settings")]
    public float approachSpeed = 2f;

    [Header("Appearance Settings")]
    public int appearancesPerGame = 3;
    public float gameDuration = 300f;
    public float minTimeBetweenAppearances = 30f;
    public float ghostVisibleDuration = 20f;

    [Header("Look At Settings")]
    public float lookAtRequiredTime = 3f;
    public float lookAtAngleThreshold = 15f;

    [Header("Retreat Settings")]
    public float retreatSpeed = 6f;
    public float retreatDistance = 50f;

    [Header("Fade Settings")]
    public float fadeInDuration = 0.8f;
    public float fadeOutDuration = 0.8f;

    private float timeSinceGameStart;
    private float timeUntilNextAppearance;
    private float lookAtTimer;
    private float ghostLifetime;
    private int appearancesCount;

    private bool ghostActive;
    private bool isApproaching;
    private bool isRetreating;

    private Vector3 retreatTarget;
    private Renderer[] allRenderers;
    private Light ghostLight;

    private Material[] allMaterials;
    private Color[] originalColors;
    private float originalLightIntensity;

    private Coroutine fadeCoroutine;

    private void Start()
    {
        allRenderers = GetComponentsInChildren<Renderer>();
        ghostLight = GetComponentInChildren<Light>();

        CacheOriginalColors();

        SetAlpha(0f);
        foreach (Renderer r in allRenderers)
            r.enabled = false;
        if (ghostLight != null)
            ghostLight.enabled = false;

        ScheduleNextAppearance();
    }

    private void CacheOriginalColors()
    {
        allMaterials = new Material[allRenderers.Length];
        originalColors = new Color[allRenderers.Length];

        for (int i = 0; i < allRenderers.Length; i++)
        {
            allMaterials[i] = allRenderers[i].material;
            originalColors[i] = allMaterials[i].color;
        }

        if (ghostLight != null)
            originalLightIntensity = ghostLight.intensity;
    }

    private void Update()
    {
        timeSinceGameStart += Time.deltaTime;

        if (!ghostActive)
        {
            timeUntilNextAppearance -= Time.deltaTime;
            if (timeUntilNextAppearance <= 0 && appearancesCount < appearancesPerGame)
                SpawnGhost();
            return;
        }

        if (isRetreating)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(retreatTarget.x, transform.position.y, retreatTarget.z),
                retreatSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, retreatTarget) < 0.1f)
                HideGhost();
            return;
        }

        ghostLifetime -= Time.deltaTime;
        if (ghostLifetime <= 0)
            StartRetreat();

        if (isApproaching)
        {
            Vector3 playerPos = playerCamera.transform.position;
            Vector3 dir = (playerPos - transform.position).normalized;
            dir.y = 0;

            transform.position = Vector3.MoveTowards(transform.position,
                new Vector3(playerPos.x, transform.position.y, playerPos.z),
                approachSpeed * Time.deltaTime);

            if (dir.sqrMagnitude > 0.01f)
                transform.rotation = Quaternion.Slerp(transform.rotation,
                    Quaternion.LookRotation(dir), Time.deltaTime * 3f);
        }

        HandleLookAt();
    }

    private void SpawnGhost()
    {
        Vector3 playerPos = playerCamera.transform.position;
        Vector3 dir = Random.insideUnitSphere * Random.Range(minSpawnDistance, maxSpawnDistance);
        dir.y = 0;
        Vector3 pos = playerPos + dir;

        if (Physics.Raycast(pos + Vector3.up * 50f, Vector3.down, out RaycastHit hit, 100f))
            pos = hit.point + Vector3.up;
        else
            pos.y = 1f;

        transform.position = pos;

        Vector3 toPlayer = playerPos - pos;
        toPlayer.y = 0;
        if (toPlayer.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(toPlayer);

        ghostActive = true;
        isApproaching = true;
        isRetreating = false;
        lookAtTimer = 0f;
        ghostLifetime = ghostVisibleDuration;
        appearancesCount++;

        StartFade(1f, fadeInDuration);
    }

    private void HandleLookAt()
    {
        Vector3 dir = (transform.position - playerCamera.transform.position).normalized;
        float angle = Vector3.Angle(playerCamera.transform.forward, dir);

        if (angle < lookAtAngleThreshold)
        {
            lookAtTimer += Time.deltaTime;
            if (lookAtTimer >= lookAtRequiredTime)
                StartRetreat();
        }
        else
        {
            lookAtTimer = Mathf.Max(0, lookAtTimer - Time.deltaTime * 2f);
        }
    }

    private void StartRetreat()
    {
        isRetreating = true;
        isApproaching = false;
        Vector3 away = (transform.position - playerCamera.transform.position).normalized;
        retreatTarget = transform.position + away * retreatDistance;
        retreatTarget.y = transform.position.y;

        StartFade(0f, fadeOutDuration);
    }

    private void HideGhost()
    {
        ghostActive = false;
        isApproaching = false;
        isRetreating = false;

        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        SetAlpha(0f);
        foreach (Renderer r in allRenderers)
            r.enabled = false;
        if (ghostLight != null)
            ghostLight.enabled = false;

        ScheduleNextAppearance();
    }

    private void ScheduleNextAppearance()
    {
        if (appearancesCount >= appearancesPerGame)
        {
            timeUntilNextAppearance = float.MaxValue;
            return;
        }

        float timeLeft = gameDuration - timeSinceGameStart;
        if (timeLeft <= 0)
        {
            timeUntilNextAppearance = float.MaxValue;
            return;
        }

        float left = appearancesPerGame - appearancesCount;
        float avg = timeLeft / left;
        timeUntilNextAppearance = Random.Range(avg * 0.5f, avg * 1.5f);
    }

    private void StartFade(float targetAlpha, float duration)
    {
        if (fadeCoroutine != null)
            StopCoroutine(fadeCoroutine);

        fadeCoroutine = StartCoroutine(FadeCoroutine(targetAlpha, duration));
    }

    private IEnumerator FadeCoroutine(float targetAlpha, float duration)
    {
        bool fadingIn = targetAlpha > 0f;

        if (fadingIn)
        {
            foreach (Renderer r in allRenderers)
                r.enabled = true;
            if (ghostLight != null)
                ghostLight.enabled = true;
        }

        float currentAlpha = GetCurrentAlpha();
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float alpha = Mathf.Lerp(currentAlpha, targetAlpha, t);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(targetAlpha);

        if (!fadingIn)
        {
            foreach (Renderer r in allRenderers)
                r.enabled = false;
            if (ghostLight != null)
                ghostLight.enabled = false;
        }

        fadeCoroutine = null;
    }

    private float GetCurrentAlpha()
    {
        if (allMaterials.Length == 0)
            return 0f;
        return allMaterials[0].color.a;
    }

    private void SetAlpha(float alpha)
    {
        for (int i = 0; i < allMaterials.Length; i++)
        {
            Color c = originalColors[i];
            c.a = alpha;
            allMaterials[i].color = c;
        }

        if (ghostLight != null)
            ghostLight.intensity = originalLightIntensity * alpha;
    }
}
