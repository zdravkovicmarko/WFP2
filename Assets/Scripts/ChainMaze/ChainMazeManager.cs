using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class ChainMazeManager : MonoBehaviour
{
    [Header("Scene References")]
    [SerializeField] private Transform gridCellsParent;
    [SerializeField] private Transform pinsParent;

    [Header("Interactor + UI Press")]
    [SerializeField] private XRBaseInteractor interactor;

    [SerializeField] private InputActionProperty uiPressAction;

    [Header("Line Settings")]
    [SerializeField] private float lineWidth = 0.02f;
    [SerializeField] private float lineYOffset = 0.01f;

    [Header("Win / End Game")]
    [SerializeField] private RoomTeleporter endGameTeleporter;
    [SerializeField] private float resetDelaySeconds = 1.6f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip correctClip;
    public Image image; 

    private bool hasWon = false;

    private readonly Dictionary<Vector2Int, int> occupiedByPair = new();
    private readonly Dictionary<int, ChainData> chainsByPair = new();
    private readonly Dictionary<int, List<PinEndpoint>> pinsByPair = new();
    private readonly Dictionary<int, List<Vector2Int>> endpointsByPair = new();
    private readonly HashSet<Vector2Int> allCells = new();

    private int activePairId = -1;
    private PinEndpoint activeStartPin = null;

    private IXRInteractor drawingInteractor = null;
    private bool isHolding = false;
    private bool prevUIPress = false;

    private ChainCellInteractable[] cellObjects;
    private XRSimpleInteractable[] cellXRInteractables;

    private void Awake()
    {
        BuildLookup();
        EnsureChainsForAllPairs();

        cellObjects = gridCellsParent.GetComponentsInChildren<ChainCellInteractable>(true);
        cellXRInteractables = gridCellsParent.GetComponentsInChildren<XRSimpleInteractable>(true);

        SetCellsEnabled(false);
    }

    private void OnEnable()
    {
        if (uiPressAction.action != null)
            uiPressAction.action.Enable();
    }

    private void OnDisable()
    {
        if (uiPressAction.action != null)
            uiPressAction.action.Disable();
    }

    private void Update()
    {
        bool pressed = uiPressAction.action != null && uiPressAction.action.IsPressed();
        bool down = pressed && !prevUIPress;
        bool up = !pressed && prevUIPress;
        prevUIPress = pressed;

        if (down) HandleUIPressDown();
        if (up) HandleUIPressUp();
    }

    private void HandleUIPressDown()
    {
        var hovered = GetFirstHovered(interactor);
        if (hovered == null) return;

        var pin = hovered.transform.GetComponentInParent<PinEndpoint>();
        if (pin == null) return;

        Debug.Log($"[ChainMaze] UI PRESS DOWN on PIN {pin.name} pair={pin.pairId}");
        OnPinHoldStart(pin, interactor);
    }

    private void HandleUIPressUp()
    {
        if (!isHolding) return;

        Debug.Log("[ChainMaze] UI PRESS UP");
        OnPinHoldEnd();
    }

    private IXRHoverInteractable GetFirstHovered(XRBaseInteractor it)
    {
        if (it == null) return null;

        var hovered = it.interactablesHovered;
        if (hovered == null || hovered.Count == 0) return null;

        return hovered[0] as IXRHoverInteractable;
    }

    private void BuildLookup()
    {
        // Cells
        allCells.Clear();
        var cells = gridCellsParent.GetComponentsInChildren<ChainCellInteractable>(true);
        foreach (var c in cells)
            allCells.Add(c.Coord);

        // Pins
        pinsByPair.Clear();
        var allPins = pinsParent.GetComponentsInChildren<PinEndpoint>(true);
        foreach (var p in allPins)
        {
            if (!pinsByPair.TryGetValue(p.pairId, out var list))
            {
                list = new List<PinEndpoint>();
                pinsByPair[p.pairId] = list;
            }
            list.Add(p);
        }

        // Endpoints
        endpointsByPair.Clear();
        foreach (var kvp in pinsByPair)
        {
            int pairId = kvp.Key;
            var pinList = kvp.Value;

            var endpoints = new List<Vector2Int>(2);
            foreach (var p in pinList)
                endpoints.Add(p.Coord);

            endpointsByPair[pairId] = endpoints;
        }

    }

    private void EnsureChainsForAllPairs()
    {
        foreach (var kvp in pinsByPair)
        {
            int pairId = kvp.Key;
            if (!chainsByPair.ContainsKey(pairId))
                chainsByPair[pairId] = new ChainData(pairId, CreateLineObject(pairId));
        }
    }


    // Pin hold logic (UI Press)
    private void OnPinHoldStart(PinEndpoint pin, IXRInteractor interactorSource)
    {
        // If already holding with another controller, ignore
        if (isHolding && drawingInteractor != interactorSource)
            return;

        isHolding = true;
        drawingInteractor = interactorSource;

        activePairId = pin.pairId;
        activeStartPin = pin;

        ResetChain(activePairId);

        SetCellsEnabled(true);

        // Snap-start to nearest cell so player sees instant feedback
        var startCell = FindNearestCell(pin.transform.position);
        if (startCell != null)
            TryStepToCell(startCell.Coord, startCell.CenterWorld);
    }

    private void OnPinHoldEnd()
    {
        isHolding = false;
        drawingInteractor = null;

        SetCellsEnabled(false);

        if (activePairId != -1 && !chainsByPair[activePairId].isComplete)
            ResetChain(activePairId);

        activePairId = -1;
        activeStartPin = null;
    }

    // Cell hover during hold
    public void OnCellHovered(Vector2Int coord, Vector3 worldCenter, IXRInteractor interactorSource)
    {
        // UI Press is the gate
        if (!isHolding) return;
        if (activePairId == -1) return;

        TryStepToCell(coord, worldCenter);
    }

    // Chain logic
    private void TryStepToCell(Vector2Int targetCell, Vector3 targetWorld)
    {
        var chain = chainsByPair[activePairId];

        if (chain.cells.Count == 0)
        {
            if (IsCellBlockedByOtherPair(targetCell, activePairId)) return;
            AddCell(chain, targetCell, targetWorld);
            CheckAutoComplete();
            return;
        }

        var from = chain.cells[^1];

        if (targetCell == from) return;
        if (!IsNeighbor(from, targetCell)) return;

        if (chain.cells.Count >= 2 && targetCell == chain.cells[^2])
        {
            RemoveLastCell(chain);
            return;
        }

        int idx = chain.cells.IndexOf(targetCell);
        if (idx >= 0)
        {
            TrimChainToIndex(chain, idx);
            return;
        }

        if (IsCellBlockedByOtherPair(targetCell, activePairId)) return;

        AddCell(chain, targetCell, targetWorld);
        CheckAutoComplete();
    }

    private bool IsNeighbor(Vector2Int a, Vector2Int b)
    {
        int dx = Mathf.Abs(a.x - b.x);
        int dy = Mathf.Abs(a.y - b.y);
        return (dx + dy) == 1;
    }

    private bool IsCellBlockedByOtherPair(Vector2Int cell, int pairId)
        => occupiedByPair.TryGetValue(cell, out var owner) && owner != pairId;

    private void AddCell(ChainData chain, Vector2Int cell, Vector3 worldPos)
    {
        chain.cells.Add(cell);
        occupiedByPair[cell] = chain.pairId;

        chain.worldPoints.Add(worldPos + Vector3.up * lineYOffset);
        UpdateLine(chain);
    }

    private void RemoveLastCell(ChainData chain)
    {
        if (chain.cells.Count == 0) return;

        var last = chain.cells[^1];
        chain.cells.RemoveAt(chain.cells.Count - 1);
        chain.worldPoints.RemoveAt(chain.worldPoints.Count - 1);

        if (occupiedByPair.TryGetValue(last, out var owner) && owner == chain.pairId)
            occupiedByPair.Remove(last);

        UpdateLine(chain);
    }

    private void TrimChainToIndex(ChainData chain, int indexInclusive)
    {
        for (int i = chain.cells.Count - 1; i > indexInclusive; i--)
        {
            var cell = chain.cells[i];
            chain.cells.RemoveAt(i);
            chain.worldPoints.RemoveAt(i);

            if (occupiedByPair.TryGetValue(cell, out var owner) && owner == chain.pairId)
                occupiedByPair.Remove(cell);
        }
        UpdateLine(chain);
    }

    private void ResetChain(int pairId)
    {
        if (!chainsByPair.TryGetValue(pairId, out var chain)) return;

        foreach (var c in chain.cells)
        {
            if (occupiedByPair.TryGetValue(c, out var owner) && owner == pairId)
                occupiedByPair.Remove(c);
        }

        chain.cells.Clear();
        chain.worldPoints.Clear();
        chain.isComplete = false;

        UpdateLine(chain);
    }

    private void CheckAutoComplete()
    {
        if (activePairId == -1) return;
        if (!chainsByPair.TryGetValue(activePairId, out var chain)) return;
        if (chain.isComplete) return;
        if (chain.cells.Count < 2) return;

        if (!endpointsByPair.TryGetValue(activePairId, out var endpoints)) return;
        if (endpoints.Count < 2) return;

        var last = chain.cells[^1];
        var start = activeStartPin != null ? activeStartPin.Coord : endpoints[0];

        Vector2Int end =
            (endpoints[0] == start) ? endpoints[1] :
            (endpoints[1] == start) ? endpoints[0] :
            endpoints[1];

        if (last == end)
        {
            chain.isComplete = true;
            PlayCorrectSound();
            Debug.Log($"[ChainMaze] Pair {activePairId} COMPLETED! ({start} -> {end})");

            isHolding = false;
            drawingInteractor = null;
            SetCellsEnabled(false);

            activePairId = -1;
            activeStartPin = null;

            if (IsWin())
                TriggerWin();
        }
    }

    private bool IsWin()
    {
        // 1) all pairs complete
        foreach (var pairId in endpointsByPair.Keys)
        {
            if (!chainsByPair.TryGetValue(pairId, out var chain) || !chain.isComplete)
                return false;
        }

        // 2) all cells filled
        foreach (var cell in allCells)
        {
            if (!occupiedByPair.ContainsKey(cell))
                return false;
        }

        return true;
    }

    // Helpers

    private void SetCellsEnabled(bool enabled)
    {
        if (cellXRInteractables == null) return;
        foreach (var xr in cellXRInteractables)
            xr.enabled = enabled;
    }

    private ChainCellInteractable FindNearestCell(Vector3 worldPos)
    {
        ChainCellInteractable best = null;
        float bestDist = float.MaxValue;

        foreach (var c in cellObjects)
        {
            float d = (c.CenterWorld - worldPos).sqrMagnitude;
            if (d < bestDist) { bestDist = d; best = c; }
        }
        return best;
    }

    public void ResetAll()
    {
        hasWon = false;

        isHolding = false;
        drawingInteractor = null;
        activePairId = -1;
        activeStartPin = null;

        occupiedByPair.Clear();

        foreach (var kvp in chainsByPair)
        {
            kvp.Value.cells.Clear();
            kvp.Value.worldPoints.Clear();
            kvp.Value.isComplete = false;

            if (kvp.Value.line != null)
                kvp.Value.line.positionCount = 0;
        }

        SetCellsEnabled(false);

        Debug.Log("[ChainMaze] ResetAll() done.");
    }

    void PlayCorrectSound()
    {
        if (audioSource != null && correctClip != null)
            audioSource.PlayOneShot(correctClip);
    }


    // Line rendering

    private LineRenderer CreateLineObject(int pairId)
    {
        var go = new GameObject($"ChainLine_{pairId}");
        go.transform.SetParent(transform, false);

        var lr = go.AddComponent<LineRenderer>();
        lr.useWorldSpace = true;
        lr.positionCount = 0;

        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.numCornerVertices = 8;
        lr.numCapVertices = 8;

        lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        lr.receiveShadows = false;
        lr.sortingOrder = 100;

        var unlitMat = Resources.Load<Material>("ChainLine_Unlit");
        if (unlitMat != null)
            lr.material = unlitMat;
        else
            Debug.LogWarning("[ChainMaze] Could not load Resources/ChainLine_Unlit.mat (line will still render, but material may be default).");

        if (pinsByPair.TryGetValue(pairId, out var pinList) && pinList.Count > 0)
        {
            Color c = pinList[0].ChainColor;
            lr.startColor = c;
            lr.endColor = c;
        }

        return lr;
    }

    private void UpdateLine(ChainData chain)
    {
        if (chain.line == null) return;

        chain.line.positionCount = chain.worldPoints.Count;
        for (int i = 0; i < chain.worldPoints.Count; i++)
            chain.line.SetPosition(i, chain.worldPoints[i]);
    }

    private System.Collections.IEnumerator DelayedReset()
    {
        yield return new WaitForSeconds(resetDelaySeconds);
        ResetAll();
    }

    private void TriggerWin()
    {
        if (hasWon) return;
        hasWon = true;

        Debug.Log("CHAIN MAZE: WIN! Triggering teleporter...");

        if (endGameTeleporter != null)
            endGameTeleporter.TeleportWithDefaultDelay();
        
        if (image != null)
            image.gameObject.SetActive(true);
        
        StartCoroutine(DelayedReset());
    }


    [Serializable]
    private class ChainData
    {
        public int pairId;
        public bool isComplete;
        public List<Vector2Int> cells = new();
        public List<Vector3> worldPoints = new();
        public LineRenderer line;

        public ChainData(int pairId, LineRenderer line)
        {
            this.pairId = pairId;
            this.line = line;
        }
    }
}