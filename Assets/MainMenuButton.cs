using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuButton : MonoBehaviour {

	public Button mainMenuButton;
	public Button exitButton;

	// Use this for initialization
	void Start () {
		mainMenuButton.onClick.AddListener (()=>{SceneManager.LoadScene ("MainMenu");});
		exitButton.onClick.AddListener (()=>{Application.Quit();});
	}
}
