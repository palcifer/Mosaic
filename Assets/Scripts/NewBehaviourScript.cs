using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NewBehaviourScript : MonoBehaviour {

	public GameObject originalTile;
	private GameObject centerTile;
	private GameObject background;
	private HashSet<GameObject> changedCubes;

	private float offset;
	private float gap;
	private float gapTemp;
	private int count;
	private Vector3 cameraPosition;
	private float orthographicCameraSize;
	private string group;
	private Color color;
	private bool editColor;
	private bool drawing;
	private string backgroundColor;

	//canvases for editing and drowing and help
	private GameObject canvasEdit;
	private GameObject canvasDraw;
	private GameObject canvasHelp;

	//GUI objects
	private GameObject editShapeButton;
	private GameObject editColorButton;
	private GameObject rotationButton;
	private GameObject reflectionButton;
	private GameObject marginsButton;
	private Color skyBlue;

	// Use this for initialization
	void Start () {
		gap = 0.2f;
		gapTemp = 0.0f;
		offset = originalTile.collider.bounds.size.x;
		group = "t";
		centerTile = originalTile;
		changedCubes = new HashSet<GameObject> ();
		color = Color.magenta;
		editColor = false;
		drawing = false;
		backgroundColor = "";

		//Find canvases for editing and drowing
		GameObject[] canvasA = GameObject.FindGameObjectsWithTag ("Edit");
		canvasEdit = canvasA [0];
		GameObject[] canvasB = GameObject.FindGameObjectsWithTag ("Draw");
		canvasDraw = canvasB [0];
		GameObject[] canvasC = GameObject.FindGameObjectsWithTag ("Help");
		canvasHelp = canvasC [0];
		GameObject[] backgroundA = GameObject.FindGameObjectsWithTag ("Background");
		background = backgroundA [0];

		//Find GUI buttons
		GameObject[] editShapeButtons = GameObject.FindGameObjectsWithTag ("EditShape");
		editShapeButton = editShapeButtons [0];
		GameObject[] editColorButtons = GameObject.FindGameObjectsWithTag ("EditColor");
		editColorButton = editColorButtons [0];
		GameObject[] rotationButtons = GameObject.FindGameObjectsWithTag ("Rotation");
		rotationButton = rotationButtons [0];
		GameObject[] reflectionButtons = GameObject.FindGameObjectsWithTag ("Reflection");
		reflectionButton = reflectionButtons [0];
		GameObject[] marginsButtons = GameObject.FindGameObjectsWithTag ("Margins");
		marginsButton = marginsButtons [0];
		skyBlue = new Color (168, 246, 246);

		canvasEdit.SetActive (true);
		canvasDraw.SetActive (false);
		canvasHelp.SetActive (false);

		Camera.main.orthographicSize = 8;
	}
	
	// Update is called once per frame
	void Update () {
		if (!drawing) {
						if (Input.GetMouseButton (0)) {
								if (editColor) {
										EditColorOfCubes ();
								} else {
										EditShapeOfTile (group);
								}
						}
				} else {

						if ((Input.GetAxis ("Mouse ScrollWheel") > 0) && (Camera.main.orthographicSize < 50)) { // forward
								Camera.main.orthographicSize++;
								StartCoroutine ("CheckForCameraProjectionSize", Camera.main.orthographicSize);
						}

						if ((Input.GetAxis ("Mouse ScrollWheel") < 0) && (Camera.main.orthographicSize > 10)) { // back
								Camera.main.orthographicSize--;
						}
				}
	}

	//wait n seconds for not scrolling, than floodfill
	IEnumerator CheckForCameraProjectionSize(float size){
		yield return new WaitForSeconds(2);
		if (Camera.main.orthographicSize == size) {
			//Debug.Log("2 sekundu je rovnaka");
			FloodBoarders ();
		}
	}

	//Floodfill the whole screen with clones of center tile, one by one until 40 are created, than 8 at once
	IEnumerator Flood(GameObject gameObj){

		cameraPosition = new Vector3(0, 0, -10);

		//CheckForBoarderCubes ();

		Queue que = new Queue();
		que.Enqueue(gameObj);
		
		while (que.Count != 0) {
			GameObject obj = (GameObject)que.Dequeue();
			if ((Camera.main.WorldToViewportPoint(obj.transform.position).x<1.15) && (Camera.main.WorldToViewportPoint(obj.transform.position).y<1.15) && 
			    (Camera.main.WorldToViewportPoint(obj.transform.position).x>-0.15) && (Camera.main.WorldToViewportPoint(obj.transform.position).y>-0.15)) {

				count++;

				Vector3 up = obj.transform.position + Vector3.up * obj.collider.bounds.size.y ;
				Vector3 right = obj.transform.position + Vector3.right * obj.collider.bounds.size.x;
				Vector3 down = obj.transform.position + Vector3.down * obj.collider.bounds.size.y;
				Vector3 left = obj.transform.position + Vector3.left * obj.collider.bounds.size.x;

				RaycastHit hitUp;
				RaycastHit hitRight;
				RaycastHit hitDown;
				RaycastHit hitLeft;

				Physics.Raycast(up + cameraPosition, Vector3.forward, out hitUp);
				Physics.Raycast(right + cameraPosition, Vector3.forward, out hitRight);
				Physics.Raycast(down + cameraPosition, Vector3.forward, out hitDown);
				Physics.Raycast(left + cameraPosition, Vector3.forward, out hitLeft);

				//Debug.Log("Tile wasn't hit by raycast " + hitUp.collider.tag.Equals("Background"));
				if(hitUp.collider.tag.Equals("Background")){
					GameObject tileUp = (GameObject)Instantiate(obj, up, Quaternion.identity);
					tileUp.tag = "Clone";
					if(group.Equals("t_rot")) tileUp.transform.Rotate(Vector3.forward, -90f);
					que.Enqueue(tileUp);
				}

				//Debug.Log(hitRight.collider.tag.Equals("Background"));
				if(hitRight.collider.tag.Equals("Background")){
					GameObject tileRight = (GameObject)Instantiate(obj, right, Quaternion.identity);
					tileRight.tag = "Clone";
					if(group.Equals("t_rot")) tileRight.transform.Rotate(Vector3.forward, 90f);
					que.Enqueue(tileRight);
				}

				//Debug.Log(hitDown.collider.tag.Equals("Background"));
				if(hitDown.collider.tag.Equals("Background")){
					GameObject tileDown = (GameObject)Instantiate(obj, down, Quaternion.identity);
					tileDown.tag = "Clone";
					if(group.Equals("t_rot")) tileDown.transform.Rotate(Vector3.forward, -90f);
					que.Enqueue(tileDown);
				}

				//Debug.Log(hitLeft.collider.tag.Equals("Background"));
				if(hitLeft.collider.tag.Equals("Background")){
					GameObject tileLeft = (GameObject)Instantiate(obj, left, Quaternion.identity);
					tileLeft.tag = "Clone";
					if(group.Equals("t_rot")) tileLeft.transform.Rotate(Vector3.forward, 90f);
					que.Enqueue(tileLeft);
				}
				if(count < 40 || count % (int)(count/3) == 0){
				yield return null;
				}
			}
		}
	}

	void EditColorOfCubes(){

		GameObject[] tiles = GameObject.FindGameObjectsWithTag ("Tile");
		BoxCollider boxColliderOfTile = tiles [0].collider as BoxCollider;
		Vector3 sizeOfTileCollider = boxColliderOfTile.size;
		boxColliderOfTile.size = new Vector3(0, 0, 0);

		RaycastHit hit;
		Physics.Raycast (Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward, out hit);
		if (hit.collider.tag.Equals ("Cube")) {
			hit.collider.gameObject.renderer.material.color = color;
		}

		boxColliderOfTile.size = sizeOfTileCollider;
	}

	void EditShapeOfTile(string group){

		//Temporarely minimise BoxCollider of parent tile
		GameObject[] gameObjects = GameObject.FindGameObjectsWithTag ("Tile");
		BoxCollider boxColliderOfTile = gameObjects [0].collider as BoxCollider;
		Vector3 sizeOfTileCollider = boxColliderOfTile.size;
		boxColliderOfTile.size = new Vector3(0, 0, 0);

		RaycastHit hit;
		Physics.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward, out hit);
		//Debug.Log(Camera.main.ScreenToWorldPoint(Input.mousePosition));
		if (hit.collider.name.Equals ("Cube")) {
			//Debug.Log("Found a tile");
			GameObject cube = hit.collider.gameObject;
			//Debug.Log(cube.transform.position);
			MoveTile(group, cube);
		}
		boxColliderOfTile.size = sizeOfTileCollider;
		//return tile;
	}

	void MoveTile(string group, GameObject cube){
		switch (group) {
		case "t":
			if(changedCubes.Contains(cube)) {
				changedCubes.Remove(cube);
			} else {
				changedCubes.Add(cube);
			}
			cube.transform.position = (cube.transform.position.x>0.0f) ? new Vector3 (cube.transform.position.x - offset, cube.transform.position.y) :
				new Vector3 (cube.transform.position.x + offset, cube.transform.position.y);
			break;
			
		case "t_rot":
			if(changedCubes.Contains(cube)){
				changedCubes.Remove(cube);
				cube.transform.position = (cube.transform.position.y>0.0f) ? new Vector3 (offset - cube.transform.position.y, cube.transform.position.x) :
					new Vector3 (-offset -cube.transform.position.y, cube.transform.position.x);
			} else {
				cube.transform.position = (cube.transform.position.x>0.0f) ? new Vector3 (cube.transform.position.y, offset - cube.transform.position.x) :
					new Vector3 (cube.transform.position.y, -offset - cube.transform.position.x);
				changedCubes.Add(cube);
			}
			break;
			
		case "t_rot_ref":
			if(changedCubes.Contains(cube)){
				changedCubes.Remove(cube);
				cube.transform.position = (cube.transform.position.y>0.0f) ? new Vector3 (-offset + cube.transform.position.y, cube.transform.position.x) :
					new Vector3 (+offset +cube.transform.position.y, cube.transform.position.x);
			} else {
			cube.transform.position = (cube.transform.position.x>0.0f) ? new Vector3 (cube.transform.position.y, -offset + cube.transform.position.x) :
				new Vector3 (cube.transform.position.y, offset + cube.transform.position.x);
			changedCubes.Add(cube);
			}
			break;
		case "t_ref":
			if(changedCubes.Contains(cube)) {
				changedCubes.Remove(cube);
			} else {
				changedCubes.Add(cube);
			}
			cube.transform.position = (cube.transform.position.x>0.0f) ? new Vector3 (cube.transform.position.x - offset, -cube.transform.position.y) :
				new Vector3 (cube.transform.position.x + offset, -cube.transform.position.y);
			break;
		default:
			Debug.Log ("No symmetry group was selected");
			break;
		}
	}

	//reduce boarder cubes with gap value
	void CheckForBoarderCubes(float gap){
		//temporalely reduce whole tile collider
		GameObject[] tiles = GameObject.FindGameObjectsWithTag ("Tile");
		BoxCollider boxColliderOfTile = tiles [0].collider as BoxCollider;
		Vector3 sizeOfTileCollider = boxColliderOfTile.size;
		boxColliderOfTile.size = new Vector3(0, 0, 0);

		Vector3 minusZPosition = new Vector3 (0, 0, -10);

		GameObject[] gameObjects = GameObject.FindGameObjectsWithTag ("Cube");
		for (int i = 0; i < gameObjects.Length; i++) {
			GameObject obj = gameObjects[i];

			Vector3 up = obj.transform.position + Vector3.up * obj.collider.bounds.size.y  + minusZPosition;
			Vector3 right = obj.transform.position + Vector3.right * obj.collider.bounds.size.x + minusZPosition;
			Vector3 down = obj.transform.position + Vector3.down * obj.collider.bounds.size.y + minusZPosition;
			Vector3 left = obj.transform.position + Vector3.left * obj.collider.bounds.size.x + minusZPosition;
			
			RaycastHit hitUp;
			RaycastHit hitRight;
			RaycastHit hitDown;
			RaycastHit hitLeft;

			Physics.Raycast(up + cameraPosition, Vector3.forward, out hitUp);
			Physics.Raycast(right + cameraPosition, Vector3.forward, out hitRight);
			Physics.Raycast(down + cameraPosition, Vector3.forward, out hitDown);
			Physics.Raycast(left + cameraPosition, Vector3.forward, out hitLeft);

			if(hitUp.collider.tag.Equals("Background")){
				obj.transform.localScale = new Vector3(obj.transform.localScale.x, obj.transform.localScale.y - gap, 1);
				obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y - gap/2, obj.transform.position.z);
			}

			if(hitRight.collider.tag.Equals("Background")){
				obj.transform.localScale = new Vector3(obj.transform.localScale.x - gap, obj.transform.localScale.y, 1);
				obj.transform.position = new Vector3(obj.transform.position.x - gap/2, obj.transform.position.y, obj.transform.position.z);
			}

			if(hitDown.collider.tag.Equals("Background")){
				obj.transform.localScale = new Vector3(obj.transform.localScale.x, obj.transform.localScale.y - gap, 1);
				obj.transform.position = new Vector3(obj.transform.position.x, obj.transform.position.y + gap/2, obj.transform.position.z);
			}

			if(hitLeft.collider.tag.Equals("Background")){
				obj.transform.localScale = new Vector3(obj.transform.localScale.x - gap, obj.transform.localScale.y, 1);
				obj.transform.position = new Vector3(obj.transform.position.x + gap/2, obj.transform.position.y, obj.transform.position.z);
			}
		}
		boxColliderOfTile.size = sizeOfTileCollider;
	}

	//Floodfill uncovered boarders after zooming in back
	void FloodBoarders(){
		Vector3 step = new Vector3 (0.2f, 0, 0);
		Vector3 pos = new Vector3 (Camera.main.ViewportToWorldPoint(new Vector3(1.0f, 0.5f)).x, 0, -10);
		RaycastHit hit;
		Physics.Raycast (pos, Vector3.forward, out hit);

		while (!hit.collider.tag.Equals("Clone")) {
			pos = pos - step;
			//Debug.Log(pos);
			Physics.Raycast (pos, Vector3.forward, out hit);
		}

		//Debug.Log("Tile found " + pos + hit.collider.gameObject.name);
		StartCoroutine("Flood", hit.collider.gameObject);
	}

	void DestroyClones(){
		GameObject[] gameObjects = GameObject.FindGameObjectsWithTag ("Clone");
		for (var i = 0; i < gameObjects.Length; i ++) {
			Destroy (gameObjects [i]);
		}
		gameObjects = GameObject.FindGameObjectsWithTag ("Tile4x4");
		for (var i = 0; i < gameObjects.Length; i ++) {
			Destroy (gameObjects [i]);
		}
	}

	GameObject CreateTile(GameObject cube){

		CheckForBoarderCubes (gap);

		GameObject tile = new GameObject ();
		tile.name = "SquareTile4x4";
		tile.transform.position = new Vector3 (cube.collider.bounds.size.x/2, cube.collider.bounds.size.y/2, 0);
		BoxCollider bc = tile.AddComponent ("BoxCollider") as BoxCollider;
		bc.size = new Vector3 (2*cube.collider.bounds.size.x, 2*cube.collider.bounds.size.y, 2);
		bc.tag = "Tile4x4";

		GameObject cubeOrigin = (GameObject)Instantiate(cube, cube.transform.position, Quaternion.identity);
		GameObject cubeRight = (GameObject)Instantiate(cube, cube.transform.position + new Vector3(offset, 0, 0), Quaternion.identity);
		GameObject cubeTop = (GameObject)Instantiate(cube, cube.transform.position + new Vector3(0, offset, 0), Quaternion.identity);
		GameObject cubeTopRight = (GameObject)Instantiate(cube, cube.transform.position + new Vector3(offset, offset, 0), Quaternion.identity);

		cubeOrigin.transform.parent = tile.transform;
		cubeRight.transform.parent = tile.transform;
		cubeTop.transform.parent = tile.transform;
		cubeTopRight.transform.parent = tile.transform;

		if (group.Equals ("t_rot")) {
			cubeRight.transform.Rotate(Vector3.forward, 90);
			cubeTop.transform.Rotate(Vector3.forward, -90);
			cubeTopRight.transform.Rotate(Vector3.forward, 180);
		}

		if (group.Equals ("t_rot_ref")) {
			cubeRight.transform.Rotate(Vector3.forward, 90);
			Vector3 temp = cubeRight.transform.localScale;
			temp.y = cubeRight.transform.localScale.y * -1f;
			cubeRight.transform.localScale = temp;

			cubeTop.transform.Rotate(Vector3.forward, 90);
			temp = cubeTopRight.transform.localScale;
			temp.y = cubeTopRight.transform.localScale.y * -1f;
			cubeTop.transform.localScale = temp;
		}

		if (group.Equals ("t_ref")) {
			Vector3 temp = cubeRight.transform.localScale;
			temp.y = cubeRight.transform.localScale.y * -1f;
			cubeRight.transform.localScale = temp;

			temp = cubeTopRight.transform.localScale;
			temp.y = cubeTopRight.transform.localScale.y * -1f;
			cubeTopRight.transform.localScale = temp;
		}
		centerTile = cube;
		return tile;
	}

	void ReordinateTheCubes(bool restore){
		GameObject[] temp = new GameObject[changedCubes.Count];
		changedCubes.CopyTo (temp);
		changedCubes.Clear();
		for (int i = 0; i < temp.Length; i++) {
			GameObject cube = temp[i];
			MoveTile(group, cube);
		}
	}

	void RestoreCubes(){
		GameObject[] temp = new GameObject[changedCubes.Count];
		HashSet<GameObject> tempHash = new HashSet<GameObject> (changedCubes);
		changedCubes.CopyTo (temp);
		for (int i = 0; i < temp.Length; i++) {
			GameObject cube = temp[i];
			MoveTile(group, cube);
		}
		changedCubes = tempHash;
	}

	//GUI methods
	public void SelectColor(string str){
				if (str.Equals (backgroundColor)) {
						background.renderer.material.color = color;
				}
		editColor = true;
		switch (str) {
		case ("white"):
			color = Color.white;
			break;
		case ("black"):
			color = Color.black;
			break;
		case ("blue"):
			color = Color.blue;
			break;
		case ("magenta"):
			color = Color.magenta;
			break;
		case ("cyan"):
			color = Color.cyan;
			break;
		case("red"):
			color = Color.red;
			break;
		case("green"):
			color = Color.green;
			break;
		case ("grey"):
			color = Color.grey;
			Debug.Log("tralala");
			break;
		}
	backgroundColor = str;
	}

	public void SelectGroup(string str){
		RestoreCubes ();
		switch (group) {
		case ("t"):
			if(str.Equals("rot")) group = "t_rot";
			if(str.Equals("ref")) group = "t_ref";
			break;
		case("t_rot"):
			if(str.Equals("rot")) group = "t";
			if(str.Equals("ref")) group = "t_rot_ref";
			break;
		case("t_rot_ref"):
			if(str.Equals("rot")) group = "t_ref";
			if(str.Equals("ref")) group = "t_rot";
			break;
		case("t_ref"):
			if(str.Equals("rot")) group = "t_rot_ref";
			if(str.Equals("ref")) group = "t";
			break;
		default:
			break;
		}
		ReordinateTheCubes (false);
	}

	public void EnableOffset(){
		float temp = gap;
		gap = gapTemp;
		gapTemp = temp;
	}

	public void EnableEditColor(){
		editColor = true;
	}

	public void EnableEditShape(){
		editColor = false;
	}

	public void ResetAll(){
		Application.LoadLevel ("first");
	}

	public void Edit(){
		DestroyClones ();

		canvasEdit.SetActive (true);
		canvasDraw.SetActive (false);

		drawing = false;
		Camera.main.orthographicSize = 8;

		Invoke ("EnlargeCubes", 0.000f);
	}

	void EnlargeCubes(){
		CheckForBoarderCubes (-gap);
		}

	public void Draw(){
		canvasEdit.SetActive (false);
		canvasDraw.SetActive (true);

		drawing = true;

		Camera.main.orthographicSize = 30;

		count = 0;
		GameObject parentTile = CreateTile(centerTile);
		StartCoroutine("Flood", parentTile);
	}

	public void MakeScreenshot(){
		canvasDraw.SetActive (false);
		Application.CaptureScreenshot ("../../../screenshot" + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".png");
		Invoke ("MkScrnsht", 0.0001f);
	}

	void MkScrnsht(){
		canvasDraw.SetActive (true);
	}

	public void ActivateHelp(bool bo){
		canvasHelp.SetActive (bo);
		}

	public Color GetSelectedColor(){
		return color;
	}

	public bool IsRotationOn(){
		if (group.Equals ("t_rot") || group.Equals ("t_rot_ref")) {
			return true;
				} else {
			return false;
				}
	}

	public bool IsReflectionOn(){
		if (group.Equals ("t_ref") || group.Equals ("t_rot_ref")) {
			return true;
		} else {
			return false;
		}
	}

	public bool IsOffsetOn(){
		if (offset == 0.0f) {
			return false;
				} else {
			return true;
				}
	}

	public bool IsEditModeOn(){
		if (drawing) {
			return false;
		} else {
			return true;
		}
	}

	/*
	void CreateTile(){
			float i = -3.5f;
			float j = -3.5f;
			while(i<3.6f){
				while(j<3.6f){
					GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
					cube.renderer.material.color = Color.cyan;
					cube.transform.position = new Vector3(i, j, 0);
					j=j+1f;
				}
				j=-3.5f;
				i = i+1f;
			}
	}


	Vector3 FindExtremeCubes(bool last){
		GameObject[] cubes = GameObject.FindGameObjectsWithTag ("Cube");
		if (last) {
			Debug.Log(cubes[0].transform.position);
			return cubes[0].transform.position;
		} else {
			Debug.Log(cubes[cubes.Length-1].transform.position);
			return cubes[cubes.Length-1].transform.position;
		}
	}

	*/
}
