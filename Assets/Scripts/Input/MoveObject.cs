using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Mathematics;
public class MoveObject : MonoBehaviour
{

    private Vector3 m_previousMousePos;
    private Vector3 m_currentMousPos;

    private Camera m_cam;
    private List<Transform> m_selectedObjects;

    /// <summary>
    /// This class handles all input for selecting & moving objects,
    /// handles click selecting, alt selecting multiple objects, drag selecting multiple objects and moving selected objects, it also handles hovering over objects
    /// </summary>
    void Start()
    {
        m_cam = Camera.main;
        m_selectedObjects = new List<Transform>();

    }

    void Update()
    {
        m_currentMousPos = m_cam.ScreenToWorldPoint(Input.mousePosition);

        //mouse is pressed down
        if (Input.GetMouseButtonDown(0))
        {
            //check if we clicked something
            RayCast();
            //we did not hit something do drag select
        }
        if (Input.GetMouseButton(0))
        {
            //move selected objects
            MoveSelectedObjects();
            //else we are drag selecting
        }

        m_previousMousePos = m_currentMousPos;
        if (Input.GetMouseButtonUp(0))
        {
            m_currentMousPos = Vector3.zero;

        }

    }
    private void MoveSelectedObjects()
    {
        //iterate selected objects
        foreach (Transform t in m_selectedObjects)
        {
            //check if previous mouse position was not 0
            if (m_previousMousePos.magnitude > 0.001f)
            {
                //get dir and calculate new mouse position
                Vector3 dir = m_currentMousPos - m_previousMousePos;
                Vector3 newPos = t.transform.position + dir;
                t.transform.position = newPos;
            }
        }
    }
    private bool RayCast()
    {
        var hit = Physics2D.Raycast(m_currentMousPos, Vector2.zero);
        if (!hit)
        {
            UnSelect();
            return false;
        }
        if (!hit.transform.CompareTag("Interactable")) return false;

        // do highlight object, for hovering


        if (m_selectedObjects.Contains(hit.transform)) return true;

        Debug.Log("adding & clearing");
        UnSelect();
        m_selectedObjects.Add(hit.transform);


        return true;
    }

    public void SelectObject(Transform t)
    {
        m_selectedObjects.Add(t);
    }
    private void UnSelect()
    {
        m_selectedObjects.Clear();
    }
}
