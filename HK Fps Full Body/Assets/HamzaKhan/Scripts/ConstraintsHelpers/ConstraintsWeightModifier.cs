using System.Collections;
using System.Collections.Generic;
using UnityEngine.Animations.Rigging;
using UnityEngine;
using System;

public class ConstraintsWeightModifier : MonoBehaviour
{


    [Header("The Weight For All The Constraints Assigned in the List")]
    [Range(0, 1)]
    [SerializeField] private float _weight = 1.0f;

    // List to hold GameObjects
    [SerializeField] private List<GameObject> _iKConstraints = new List<GameObject>();

    // Internal list to hold the actual constraint components
    private List<IRigConstraint> _rigConstraints = new List<IRigConstraint>();
    private List<ICustomConstraints> _customConstraints = new List<ICustomConstraints>();

    void Start()
    {
        // Initialize Constraints.
        InitializeConstraints();
    }

    void Update()
    {
        // Loop through each of the rig constraint.
        for (int i = 0; i < _rigConstraints.Count; i++)
        {
            // Check if the constrant isn't null.
            if (_rigConstraints[i] != null)
            {
                // Set the weight property
                _rigConstraints[i].weight = _weight;
            }
        }

        // Loop through each of the custom constraint.
        for (int i = 0; i < _customConstraints.Count; i++)
        {
            // Check if the constrant isn't null.
            if (_customConstraints[i] != null)
            {
                // Set the weight property
                _customConstraints[i].SetWeight(_weight);
            }
        }
    }

    /// <summary>
    /// Initializes the Constraints.
    /// </summary>
    private void InitializeConstraints()
    {
        // Clear the Contraints List.
        _rigConstraints.Clear();

        // Loop through all objects.
        for (int i = 0; i < _iKConstraints.Count; i++)
        {
            if (_iKConstraints[i] != null)
            {
                var rigConstraint = _iKConstraints[i].GetComponent<IRigConstraint>();
                if (rigConstraint != null)
                {
                    _rigConstraints.Add(rigConstraint);
                }
                else if (_iKConstraints[i].GetComponent<ICustomConstraints>() != null)
                {
                    var customConstraint = _iKConstraints[i].GetComponent<ICustomConstraints>();
                    _customConstraints.Add(customConstraint);
                }
                else
                {
                    Debug.LogWarning($"GameObject {_iKConstraints[i].name} does not have a constraint component.");
                }
            }
        }
    }

    public float GetWeight()
    {
        return _weight;
    }

    public void SetWeight(float weight)
    {
        this._weight = weight;
    }

    public List<IRigConstraint> GetAffectedRigConstraints()
    {
        return _rigConstraints;
    }

    public List<ICustomConstraints> GetAffectedCustomConstraints()
    {
        return _customConstraints;
    }
}