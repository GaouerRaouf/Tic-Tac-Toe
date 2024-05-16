using System;
using System.Collections.Generic;
using UnityEngine;

public class ClickTrigger : MonoBehaviour
{
	TicTacToeAI _ai;

	[SerializeField]
	private int _myCoordX = 0;
	[SerializeField]
	private int _myCoordY = 0;

	[SerializeField]
	public bool canClick;

	public TicTacToeState _state = TicTacToeState.none;

	private void Awake()
	{
		_ai = FindObjectOfType<TicTacToeAI>();
	}

	private void Start(){

		_ai.onGameStarted.AddListener(AddReference);
		_ai.onGameStarted.AddListener(() => SetInputEndabled(true));
		_ai.onPlayerWin.AddListener((win) => SetInputEndabled(false));
	}

	private void SetInputEndabled(bool val){
		canClick = val;
	}

    private void AddReference()
    {
        _ai.RegisterTransform(_myCoordX, _myCoordY, this);
        canClick = true;

        Debug.Log("ClickTrigger registered at (" + _myCoordX + ", " + _myCoordY + ")");
    }


    private void OnMouseDown()
	{
        if (canClick && _ai != null)
        {
            _ai.PlayerSelects(_myCoordX, _myCoordY);
            SetInputEndabled(false);
        }
    }
}
