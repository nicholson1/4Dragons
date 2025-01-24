using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary> Creates a probability distribution for random values with varying probability weights. </summary>
/// <typeparam name="T">The result that gets output from the chance calculation. </typeparam>
public class Chances<T>
{
    private List<float> _weights;
    private List<T> _outcomes;
    private List<float> _thresholds;

    private float _totalWeight = 0;

    public int Count => _outcomes.Count;

    public Chances() : this(3) { }
    public Chances(int capacity)
    {
        _weights = new List<float>(capacity);
        _thresholds = new List<float>(capacity);
        _outcomes = new List<T>(capacity);
    }

    //=================== Main Usage ======================================

    public void AddOutcome(T outcome, float weight = 1)
    {
        if (weight < 0)
            throw new Exception("Chance weight cannot be negative");

        if (weight == 0)
            return;

        _weights.Add(weight);
        _outcomes.Add(outcome);
        _thresholds.Add(_totalWeight + weight);

        _totalWeight += weight;
    }

    public T GetRandomOutcome()
    {
        if (Count == 0)
            return default;

        float value = Random.Range(0, _totalWeight);

        for (int i = 0; i < _thresholds.Count - 1; i++)
            if (value <= _thresholds[i])
                return _outcomes[i];

        return _outcomes[^1];
    }

    public List<T> GetAllOutcomes()
    {
        return _outcomes;
    }

    //=================== Utility ======================================

    public void RemoveOutcome(T outcome)
    {
        int index = _outcomes.IndexOf(outcome);
        if (index < 0)
            return;
        RemoveOutcome(index);
    }

    public void RemoveOutcome(int index) => ModifyOutcomeWeight(index, 0);

    public void ModifyOutcomeWeight(T outcome, float newWeight) => ModifyOutcomeWeight(_outcomes.IndexOf(outcome), newWeight);

    public void ModifyOutcomeWeight(int index, float newWeight)
    {
        if (index < 0 || index >= _outcomes.Count)
            throw new Exception("Outcome not found: index out of bounds.");

        if (newWeight < 0)
            throw new Exception("Chance weight must be above 0");

        _totalWeight -= _weights[index];

        for (int i = index + 1; i < _thresholds.Count; i++)
            _thresholds[i] -= _weights[index];
        _thresholds.RemoveAt(index);

        _weights.RemoveAt(index);

        T outcome = _outcomes[index];
        _outcomes.RemoveAt(index);

        if (newWeight > 0)
            AddOutcome(outcome, newWeight);
    }

    public float GetChancePercentage(T outcome)
    {
        return GetChancePercentage(_outcomes.IndexOf(outcome));
    }
    public float GetChancePercentage(int index)
    {
        if (index < 0 || index >= _outcomes.Count)
            throw new Exception("Outcome not found: index out of bounds.");

        return _weights[index] / _totalWeight;
    }

    public void Clear()
    {
        _weights.Clear();
        _outcomes.Clear();
        _thresholds.Clear();
    }
}