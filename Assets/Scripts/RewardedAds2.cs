using System.Collections;
using UnityEngine;
using UnityEngine.Advertisements;
using UnityEngine.UI;

public class RewardedAds2 : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    [SerializeField] string _androidAdUnitId = "Rewarded_Android";
    string _adUnitId;

    [SerializeField] Button _rewardedAdButton;

    private void Awake()
    {
        _adUnitId = _androidAdUnitId;
    }

    public void LoadAd()
    {
        if (!Advertisement.isInitialized)
        {
            Debug.LogWarning("Tried to load rewarded ad before Unity ads was initialized.");
            StartCoroutine(WaitAndLoad(3f));
            return;
        }

        Debug.Log("Loading rewarded ad.");
        Advertisement.Load(_adUnitId, this);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {
        Debug.Log("Rewarded ad loaded!");

        if(placementId.Equals(_adUnitId)) {
            if (_rewardedAdButton != null)
            {
                _rewardedAdButton.interactable = true;
            }
        }
    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Debug.LogWarning($"Failed to load rewarded ad: {error} - {message}");
        StartCoroutine(WaitAndLoad(5f));
    }

    public IEnumerator WaitAndLoad(float delay)
    {
        yield return new WaitForSeconds(delay);
        LoadAd();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Debug.LogWarning($"Failed to show rewarded ad: {error} - {message}");
        StartCoroutine(WaitAndLoad(5f));
    }

    public void OnUnityAdsShowStart(string placementId)
    {
        Time.timeScale = 0f;
    }

    public void OnUnityAdsShowClick(string placementId)
    {
        Debug.Log("User clicked on rewarded ad");
    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Time.timeScale = 1f;

        if (placementId.Equals(_adUnitId) && showCompletionState == UnityAdsShowCompletionState.COMPLETED) 
        {
            Debug.Log("Rewarded ad completed!");
            
            ReduceMovesBy5();
                
            if (_rewardedAdButton != null)
            {
                _rewardedAdButton.interactable = false;
            }
            StartCoroutine(WaitAndLoad(10f));
        }
        else
        {
            Debug.Log("Rewarded ad not completed properly");
            if (_rewardedAdButton != null)
            {
                _rewardedAdButton.interactable = true;
            }
        }
    }

    private void ReduceMovesBy5()
    {
        Debug.Log("Attempting to reduce moves by 5...");
        
        bool success = false;
        
        // Method 1: Try to find any component that might track moves
        MonoBehaviour[] allComponents = FindObjectsOfType<MonoBehaviour>();
        foreach (MonoBehaviour component in allComponents)
        {
            if (TryReduceMovesInComponent(component))
            {
                success = true;
                break;
            }
        }

        // Method 2: Try UI Text components that might display move count
        if (!success)
        {
            success = TryReduceMovesInUIText();
        }

        if (success)
        {
            Debug.Log("Successfully reduced moves by 5!");
        }
        else
        {
            Debug.LogError("Could not find move counter system to reduce moves!");
            // Don't create fallback - just log the error
        }
    }

    private bool TryReduceMovesInComponent(MonoBehaviour component)
    {
        if (component == null) return false;

        var componentType = component.GetType();
        string componentName = componentType.Name;

        // Skip if it's an ad-related component to avoid infinite loops
        if (componentName.Contains("Ad") || componentName.Contains("Advertisement")) 
            return false;

        Debug.Log($"Checking component: {componentName}");

        // Try public method ReduceMoves
        var reduceMovesMethod = componentType.GetMethod("ReduceMoves");
        if (reduceMovesMethod != null)
        {
            reduceMovesMethod.Invoke(component, new object[] { 5 });
            Debug.Log($"Reduced moves via ReduceMoves method on {componentName}");
            return true;
        }

        // Try public method AddMoves with negative value
        var addMovesMethod = componentType.GetMethod("AddMoves");
        if (addMovesMethod != null)
        {
            addMovesMethod.Invoke(component, new object[] { -5 });
            Debug.Log($"Reduced moves via AddMoves method on {componentName}");
            return true;
        }

        // Try SetMoves method - we need to get current moves first
        var setMovesMethod = componentType.GetMethod("SetMoves");
        if (setMovesMethod != null)
        {
            int currentMoves = GetCurrentMovesFromComponent(component);
            if (currentMoves >= 0) // If we found valid moves
            {
                setMovesMethod.Invoke(component, new object[] { Mathf.Max(0, currentMoves - 5) });
                Debug.Log($"Reduced moves via SetMoves method on {componentName} from {currentMoves} to {currentMoves - 5}");
                return true;
            }
        }

        // Try various field names
        string[] possibleFieldNames = { "moveCount", "moves", "currentMoves", "totalMoves", "moveCounter", "m_Moves", "_moves" };
        foreach (string fieldName in possibleFieldNames)
        {
            var field = componentType.GetField(fieldName, 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null && field.FieldType == typeof(int))
            {
                int currentMoves = (int)field.GetValue(component);
                field.SetValue(component, Mathf.Max(0, currentMoves - 5));
                Debug.Log($"Reduced moves via {fieldName} field on {componentName} from {currentMoves} to {currentMoves - 5}");
                return true;
            }
        }

        // Try various property names
        string[] possiblePropertyNames = { "Moves", "MoveCount", "CurrentMoves", "TotalMoves" };
        foreach (string propertyName in possiblePropertyNames)
        {
            var property = componentType.GetProperty(propertyName);
            if (property != null && property.PropertyType == typeof(int) && property.CanWrite)
            {
                int currentMoves = (int)property.GetValue(component);
                property.SetValue(component, Mathf.Max(0, currentMoves - 5));
                Debug.Log($"Reduced moves via {propertyName} property on {componentName} from {currentMoves} to {currentMoves - 5}");
                return true;
            }
        }

        return false;
    }

    private int GetCurrentMovesFromComponent(MonoBehaviour component)
    {
        if (component == null) return -1;

        var componentType = component.GetType();

        // Try GetMoves method
        var getMovesMethod = componentType.GetMethod("GetMoves");
        if (getMovesMethod != null && getMovesMethod.ReturnType == typeof(int))
        {
            return (int)getMovesMethod.Invoke(component, null);
        }

        // Try various field names
        string[] possibleFieldNames = { "moveCount", "moves", "currentMoves", "totalMoves" };
        foreach (string fieldName in possibleFieldNames)
        {
            var field = componentType.GetField(fieldName, 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic | 
                System.Reflection.BindingFlags.Instance);
            
            if (field != null && field.FieldType == typeof(int))
            {
                return (int)field.GetValue(component);
            }
        }

        // Try various property names
        string[] possiblePropertyNames = { "Moves", "MoveCount", "CurrentMoves", "TotalMoves" };
        foreach (string propertyName in possiblePropertyNames)
        {
            var property = componentType.GetProperty(propertyName);
            if (property != null && property.PropertyType == typeof(int) && property.CanRead)
            {
                return (int)property.GetValue(component);
            }
        }

        return -1; // Couldn't find moves
    }

    private bool TryReduceMovesInUIText()
    {
        Text[] allTexts = FindObjectsOfType<Text>();
        foreach (Text text in allTexts)
        {
            if (text.text.ToLower().Contains("move") && text.text.Contains(":"))
            {
                Debug.Log($"Found potential moves text: {text.text}");
                
                // Try to extract number after "Moves: " pattern
                string textLower = text.text.ToLower();
                int colonIndex = textLower.IndexOf(':');
                if (colonIndex >= 0)
                {
                    string afterColon = text.text.Substring(colonIndex + 1).Trim();
                    if (int.TryParse(afterColon, out int currentMoves))
                    {
                        int newMoves = Mathf.Max(0, currentMoves - 5);
                        text.text = text.text.Replace(currentMoves.ToString(), newMoves.ToString());
                        Debug.Log($"Reduced moves in UI text from {currentMoves} to {newMoves}");
                        return true;
                    }
                }
                
                // Try to find any number in the text
                var match = System.Text.RegularExpressions.Regex.Match(text.text, @"\d+");
                if (match.Success && int.TryParse(match.Value, out int moves))
                {
                    int newMoves = Mathf.Max(0, moves - 5);
                    text.text = text.text.Replace(moves.ToString(), newMoves.ToString());
                    Debug.Log($"Reduced moves in UI text from {moves} to {newMoves}");
                    return true;
                }
            }
        }
        return false;
    }

    public void SetButton(Button button)
    {
        if (button == null)
        {
            return;
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(ShowAd);
        _rewardedAdButton = button;
        _rewardedAdButton.interactable = false;
    }

    public void ShowAd()
    {
        Debug.Log("ShowAd called - button clicked");
        if (_rewardedAdButton != null)
        {
            _rewardedAdButton.interactable = false;
        }
        Advertisement.Show(_adUnitId, this);
    }

    public void TestReduceMoves()
    {
        Debug.Log("TEST: Manually reducing moves by 5");
        ReduceMovesBy5();
    }
}