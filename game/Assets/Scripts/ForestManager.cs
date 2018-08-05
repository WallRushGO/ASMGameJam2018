using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

using UnityEngine;

using Elements;

public class ForestManager : MonoBehaviour {

    private ForestElement[,] _map;
    
    private Dictionary<string, FireElement> _fires;
    private Dictionary<string, WaterElement> _waterTargets;

    public static ForestManager instance = null; 

    public int MaxFires;

    public int FireDecay;

    public float FireSpreadSeconds;

    public int Padding;

    public float GridSize;

    public int Wiggle;
    
    public GridElement GridElement;

    [Range(0,100)]
    public int SpreadOdds;

    [Range(0,100)]
    public int ForestDensity;

    public int Width;
    public int Height;

    private System.Random _random;

    private float _fireCalcTrigger = 0;

    void Awake() {
        _map = new ForestElement[Width,Height];
        _fires = new Dictionary<string, FireElement>(2);
        _waterTargets = new Dictionary<string, WaterElement>();
        _random = new System.Random(Time.time.GetHashCode());

        //Check if instance already exists
        if (instance == null){
            
            //if not, set instance to this
            instance = this;
        }
        //If instance already exists and it's not this:
        else if (instance != this) {
            
            //Then destroy this. This enforces our singleton pattern, meaning there can only ever be one instance of a GameManager.
            Destroy(gameObject);
        }

        GenerateForrest();

        var centerX = (int) Math.Round( Width / 2.0f, 0);
        var centerY = (int) Math.Round( Height / 2.0f, 0);
        AddFire(centerX, centerY);
    }

    void Start() {
    }

    void Update() {
        _fireCalcTrigger += Time.deltaTime;

        if(_fireCalcTrigger > FireSpreadSeconds){
            _fireCalcTrigger = 0.0f;

            foreach(var fire in _fires){
                CalculateFire(fire.Key);
            }
        }

        foreach(var water in _waterTargets){
            if(water.Value.Update()){
                DecayFire(water.Value.X, water.Value.Y);
            }
        }
    }
    ForestElementType RandomForestElement()
    {
        if(_random.Next(0, 100) < ForestDensity){
            return ForestElementType.TREE;
        }
        else{
            return ForestElementType.EMPTY;
        }
    }

    void GenerateForrest(){
        var modelWidth = 12.0f;
        var modelHeight = 12.0f;
        var centerX = (int) Math.Round(modelWidth / 2, 0);
        var centerY = (int) Math.Round(modelHeight / 2, 0);

        for(var x = 0; x < Width; x++){
            for(var y = 0; y < Height; y++){
                var element = new ForestElement();
                var gap = (x == 0 && y == 0 ) ? 0 : Padding;

                element.Healt = 1.0f;
                element.Type = RandomForestElement();

                _map[x, y] = element;

                if(element.Type == ForestElementType.TREE){                    
                    // Wigle
                    var wiggleX = _random.Next(-Wiggle, Wiggle);
                    var wiggleY = _random.Next(-Wiggle, Wiggle);

                    Vector3 pos = new Vector3(x * centerX + gap + wiggleX, 0, y * centerY + gap + wiggleY );
      
                    var newTree = Instantiate(GridElement, pos, Quaternion.identity);
                    newTree.GridPosition = new Vector2Int(x, y);
                }
            }
        }
    }

    void DecayFire(int x, int y)
    {
        string targetFire = "fire-" + CreateKey(x, y);
        if(_fires.ContainsKey(targetFire))
        {
            var fire = _fires[targetFire];
            fire.Healt -= 0.1f;

            if(fire.Healt < 0.1f){
                fire.Healt = 0.0f;
                Debug.Log("Fire " + targetFire + " has decayed");
            }            
        }
    }

    public float GetHealt(int x, int y)
    {
        if(_map != null){
            return _map[x, y].Healt;
        }
        else{
            return 1.0f;
        }
    }

    void AddFire(int x, int y, float intensity = 1.0f)
    {
        var keyName = "fire-" + CreateKey(x, y);
        if( !_fires.ContainsKey(keyName) )
        {
            _fires.Add( keyName, new FireElement{
                X = x,
                Y = y,
                Healt = intensity
            });
        }
        else{
            Debug.LogWarning(String.Format("Fire already exists at position {0}, {1}", x, y));
        }
    }

    void AddWater(int x, int y)
    {
        var keyName = "water-" + CreateKey(x, y);
        if( !_waterTargets.ContainsKey(keyName) )
        {
            _waterTargets.Add(keyName, new WaterElement(x, y, 1));
        }
        else{
            Debug.LogWarning(String.Format("Water exists at {0}, {1}", x, y));
        }
    }

    string CreateKey(int x, int y){
        return x.ToString() + "," + y.ToString();
    }

    Dictionary<Vector2Int, ForestElement> GetSpreadLocations(int x, int y){
        var targets = new Dictionary<Vector2Int, ForestElement>(9);

        // Left Top
        if(x - 1 > -1 && y - 1 > -1 && _map[x - 1, y - 1].Type == ForestElementType.TREE){
            targets.Add(new Vector2Int(x - 1, y - 1), _map[x - 1, y - 1]);
        }

        // Middle Top
        if(y - 1 > -1 && _map[x, y - 1].Type == ForestElementType.TREE){
            targets.Add(new Vector2Int(x, y - 1), _map[x, y - 1]);
        }

        // Right Top
        if(x + 1 < Width && y - 1 > -1 && _map[x + 1, y - 1].Type == ForestElementType.TREE){
            targets.Add(new Vector2Int(x + 1, y - 1), _map[x + 1, y - 1]);
        }

        // Left Middle
        if(x - 1 > -1 && _map[x - 1, y].Type == ForestElementType.TREE){
            targets.Add(new Vector2Int(x - 1, y), _map[x - 1, y]);
        }

        // Right Middle
        if(x + 1 < Width && _map[x + 1, y].Type == ForestElementType.TREE){
            targets.Add(new Vector2Int(x + 1, y), _map[x + 1, y]);
        }

        // Left Bottom
        if(x - 1 > -1 && y + 1 < Height && _map[x - 1, y + 1].Type == ForestElementType.TREE){
            targets.Add(new Vector2Int(x - 1, y + 1), _map[x - 1, y + 1]);
        }

        // Middle Bottom
        if(y + 1 < Height && _map[x, y + 1].Type == ForestElementType.TREE){
            targets.Add(new Vector2Int(x, y + 1), _map[x, y + 1]);
        }

        // Right Bottom
        if(x + 1 < Width && y + 1 < Height && _map[x + 1, y + 1].Type == ForestElementType.TREE){
            targets.Add(new Vector2Int(x + 1, y + 1), _map[x + 1, y + 1]);
        }

        return targets;
    }

    void CalculateFire(string fireName)
    {
        var fire = _fires[fireName];

        // Has the tree burned down?
        if(_map[fire.X, fire.Y].Healt <= 0.1f){
            _map[fire.X, fire.Y].Healt = 0.0f;
            _map[fire.X, fire.Y].Type = ForestElementType.BURNEDTREE;
            
            Debug.Log("Tree burned " + fireName);
        }

        if(_fires.Count != Width * Height){
            // Spread fire
            var targetTrees = GetSpreadLocations(fire.X, fire.Y);

            foreach(var tree in targetTrees){
                if(_random.Next(0,100) < SpreadOdds){
                    AddFire(tree.Key.x, tree.Key.y);
                    Debug.Log("Fire spreads " + fireName + " to " + tree.Key.x + ", " + tree.Key.y);
                }
            }
        }

        // Decay fire or burn tree
        if(_map[fire.X, fire.Y].Healt == 0.0f){
            DecayFire(fire.X, fire.Y);
        }
        else if(_map[fire.X, fire.Y].Healt > 0.1f){
            _map[fire.X, fire.Y].Healt -= 0.1f;
        }
        else if(_map[fire.X, fire.Y].Healt <= 0.1f){
            _map[fire.X, fire.Y].Healt = 0.0f;
        }
        /*
        if(fire.Healt == 0.0f){
            _fires.Remove(fireName);
            Debug.Log("Removed fire " + fireName);
        }
        */
    }
    void OnDrawGizmos() {
        if (_map != null) {                
            var modelWidth = 12.0f;
            var modelHeight = 12.0f;
            var centerX = (int) Math.Round(modelWidth / 2, 0);
            var centerY = (int) Math.Round(modelHeight / 2, 0);

            var startX = (int) Math.Round(modelWidth / 4, 0);
            var startY = (int) Math.Round(modelWidth / 4, 0);
            
            for (int x = 0; x < Width; x ++) {
                for (int y = 0; y < Height; y ++) {
                    
                    var gap = (x == 0 && y == 0 ) ? 0 : Padding;
                    Gizmos.color = Color.cyan;              
                    Gizmos.DrawWireCube( new Vector3(x * centerX + startX + gap, 5.0f, y * centerY - startY + gap), new Vector3(modelWidth, 10.0f, modelHeight));
                }
            }
        }
    }
}
