using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NewBehaviourScript : MonoBehaviour {

	public GameObject centerTile;
	private GameObject originalTile;
	private GameObject editedTile;
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


	// Use this for initialization
	void Start () {
		gap = 0.1f;
		gapTemp = 0.0f;
		offset = centerTile.collider.bounds.size.x;
		group = "t";
		originalTile = centerTile;
		changedCubes = new HashSet<GameObject> ();
		color = Color.magenta;
		editColor = false;
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetMouseButton (0)) {
			if(editColor){
			EditColorOfCubes();
			} else {
			EditShapeOfTile(group);
			}
		}

		if (Input.GetMouseButtonDown (1)) {
			count = 0;
			//StartCoroutine("Flood", centerTile);
			GameObject parentTile = CreateTile(centerTile);
			StartCoroutine("Flood", parentTile);
		}

		if ((Input.GetAxis("Mouse ScrollWheel") > 0) && (Camera.main.orthographicSize < 50)) // forward
		{
			Camera.main.orthographicSize++;
			StartCoroutine("CheckForCameraProjectionSize", Camera.main.orthographicSize);
		}

		if ((Input.GetAxis("Mouse ScrollWheel") < 0) && (Camera.main.orthographicSize > 10)) // back
		{
			Camera.main.orthographicSize--;
		}

		if (Input.GetKey ("d")) {
			DestroyClones();
		}

		if (Input.GetKey ("y")) {
			color = Color.white;
		}

		if (Input.GetKey ("u")) {
			color = Color.magenta;
		}

		if (Input.GetKey ("i")) {
			color = Color.black;
		}

		if (Input.GetKey ("o")) {
			color = Color.blue;
		}

		if (Input.GetKey ("p")) {
			color = Color.red;
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

	//Floodfill the whole screen with clones of original tile, one by one until 40 are created, than 8 at once
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
				if(count < 40 || count % (int)(count/8) == 0){
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
		if (!hit.collider.tag.Equals ("Tile")) {
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

		//Vector3 topRightCubePosition = FindExtremeCubes (true);
		//Vector3 bottomLeftCubePosition = FindExtremeCubes (false);


		RaycastHit hit;
		Physics.Raycast(Camera.main.ScreenToWorldPoint(Input.mousePosition), Vector3.forward, out hit);
		//Debug.Log(Camera.main.ScreenToWorldPoint(Input.mousePosition));
		if (hit.collider.name.Equals ("Cube")) {
			//Debug.Log("Found a tile");
			GameObject cube = hit.collider.gameObject;
			//Debug.Log(cube.transform.position);

			switch (group) {
				case "t":
					cube.transform.position = (cube.transform.position.x>0.0f) ? new Vector3 (cube.transform.position.x - offset, cube.transform.position.y) :
						new Vector3 (cube.transform.position.x + offset, cube.transform.position.y);
				changedCubes.Add(cube);
						break;

				case "t_rot":
				if(changedCubes.Contains(cube)){
					changedCubes.Remove(cube);
					cube.transform.position = (cube.transform.position.x>0.0f) ? new Vector3 (-cube.transform.position.y, -offset + cube.transform.position.x) :
						new Vector3 (-cube.transform.position.y, +offset + cube.transform.position.x);
				} else {
					cube.transform.position = (cube.transform.position.x>0.0f) ? new Vector3 (cube.transform.position.y, offset - cube.transform.position.x) :
						new Vector3 (cube.transform.position.y, -offset - cube.transform.position.x);
					changedCubes.Add(cube);
				}
						break;

				case "t_rot_ref":
					cube.transform.position = (cube.transform.position.x>0.0f) ? new Vector3 (cube.transform.position.y, -offset + cube.transform.position.x) :
					new Vector3 (cube.transform.position.y, offset + cube.transform.position.x);
				changedCubes.Add(cube);
					break;
				case "t_ref":
					cube.transform.position = (cube.transform.position.x>0.0f) ? new Vector3 (cube.transform.position.x - offset, -cube.transform.position.y) :
						new Vector3 (cube.transform.position.x + offset, -cube.transform.position.y);
				changedCubes.Add(cube);
					break;
				default:
					Debug.Log ("No symmetry group was selected");
				break;
			}
		}
		boxColliderOfTile.size = sizeOfTileCollider;
		//return tile;
	}

	//reduce boarder cubes with gap value
	void CheckForBoarderCubes(){

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

	void DestroyClones(){
		GameObject[] gameObjects = GameObject.FindGameObjectsWithTag ("Clone");
		for (var i = 0; i < gameObjects.Length; i ++) {
			Destroy (gameObjects [i]);
		}
		gameObjects = GameObject.FindGameObjectsWithTag ("Tile");
		for (var i = 0; i < gameObjects.Length; i ++) {
			Destroy (gameObjects [i]);
		}
		centerTile = (GameObject)Instantiate(editedTile, Vector3.zero, Quaternion.identity);
	}

	GameObject CreateTile(GameObject cube){

		CheckForBoarderCubes ();

		GameObject tile = new GameObject ();
		tile.name = "SquareTile4x4";
		tile.transform.position = new Vector3 (cube.collider.bounds.size.x/2, cube.collider.bounds.size.y/2, 0);
		BoxCollider bc = tile.AddComponent ("BoxCollider") as BoxCollider;
		bc.size = new Vector3 (2*cube.collider.bounds.size.x, 2*cube.collider.bounds.size.y, 2);
		bc.tag = "Tile";

		GameObject cubeRight = (GameObject)Instantiate(cube, cube.transform.position + new Vector3(offset, 0, 0), Quaternion.identity);
		GameObject cubeTop = (GameObject)Instantiate(cube, cube.transform.position + new Vector3(0, offset, 0), Quaternion.identity);
		GameObject cubeTopRight = (GameObject)Instantiate(cube, cube.transform.position + new Vector3(offset, offset, 0), Quaternion.identity);

		cube.transform.parent = tile.transform;
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

	public void SelectColor(string str){
		switch (str) {
		case ("white"):
			color = Color.white;
			return;
		case ("black"):
			color = Color.black;
			return;
		case ("blue"):
			color = Color.blue;
			return;
		case ("magenta"):
			color = Color.magenta;
			return;
		case ("cyan"):
			color = Color.cyan;
			return;
		case("red"):
			color = Color.red;
			return;
		case("green"):
			color = Color.green;
			return;
		case ("grey"):
			color = Color.grey;
			return;
		default:
			return;;
		}
	}

	public void SelectGroup(string str){

		switch (group) {
		case ("t"):
			if(str.Equals("rot")) group = "t_rot";
			if(str.Equals("ref")) group = "t_ref";
			return;
		case("t_rot"):
			if(str.Equals("rot")) group = "t";
			if(str.Equals("ref")) group = "t_rot_ref";
			return;
		case("t_rot_ref"):
			if(str.Equals("rot")) group = "t_ref";
			if(str.Equals("ref")) group = "t_rot";
			return;
		case("t_ref"):
			if(str.Equals("rot")) group = "t_rot_ref";
			if(str.Equals("ref")) group = "t";
			return;
		default:
			return;
		}
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

	public void Edit(){

	}

	public void Draw(){

	}

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

}
