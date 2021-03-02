using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct Neuron
{
    public float p_prev;
    public int in_start; // where do the input neuron ids start in the in_neurons_buffer (-1 for no inputs)
    public int spiked;
    public float last_spike_time;

    public Neuron(float p_prev, int in_start, int spiked, float last_spike_time)
    {
        this.p_prev = p_prev;
        this.in_start = in_start;
        this.spiked = spiked;
        this.last_spike_time = last_spike_time;
    }
};

public struct Synapse
{
    public int in_neuron_buffer_index; // this is also the index used by the weights buffer
    public float time;

    public Synapse(int in_neuron_buffer_index, float time)
    {
        this.in_neuron_buffer_index = in_neuron_buffer_index;
        this.time = time;
    }
};

public class Manager : MonoBehaviour
{
    private const int group_size = 64;

    private int num_neurons;
    private int[][] in_neurons;
    private Neuron[] neurons;

    private int sim_time;

    // neuron properties
    public float p_min;
    public float p_thresh;
    public float p_rest;
    public float p_refract;
    public float leak;

    // learning properties
    public float no_stdp_window;
    public float a_minus;
    public float a_plus;
    public float tau_minus;
    public float tau_plus;
    public float learning_rate;
    public float weight_min;
    public float weight_max;

    // defines how a neuron indexes into synapses buffer
    public int synapses_to_keep;

    public Neuron[] neuron_buffer;

    private ComputeBuffer _neuron_buffer;
    private ComputeBuffer _synapse_write_index_buffer;
    private ComputeBuffer _synapse_buffer;
    private ComputeBuffer _in_neuron_buffer;
    private ComputeBuffer _weight_buffer;

    public ComputeShader _compute_shader;

    private int kernel;

    // Start is called before the first frame update
    void Start()
    {
        // initialize buffer data

        List<List<int>> in_neurons_for_neurons = CSVReader.Read("simple_network");

        int num_neurons = in_neurons_for_neurons.Count;
        Debug.Log(num_neurons);

        neuron_buffer = new Neuron[num_neurons];
        int[] synapse_write_index_buffer = new int[num_neurons];
        Synapse[] synapse_buffer = new Synapse[synapses_to_keep * num_neurons];

        int neuron_count = 0;
        int in_neuron_count = 0;
        List<int> in_neuron_buffer_list = new List<int>();
        List<float> weight_buffer_list = new List<float>();
        foreach(List<int> neuron_in_neurons in in_neurons_for_neurons)
        {
            neuron_buffer[neuron_count] = new Neuron(p_rest, in_neuron_count, 0, -20); // initial last_spike_time before the start of simulation and outside learning window
            synapse_write_index_buffer[neuron_count] = 0;

            for (int i = 0; i < synapses_to_keep; i++)
            {
                synapse_buffer[neuron_count * i] = new Synapse(0, 0); // initialize all synapses to neuron 0, at time 0, these will be ignored

            }

            foreach(int in_neuron_index in neuron_in_neurons)
            {
                in_neuron_buffer_list.Add(in_neuron_index);
                weight_buffer_list.Add(1);
                in_neuron_count += 1;
            }
            neuron_count += 1;
        }

        int[] in_neuron_buffer = in_neuron_buffer_list.ToArray();
        float[] weight_buffer = weight_buffer_list.ToArray();

        // put data in shader

        _compute_shader = Resources.Load<ComputeShader>("ComputeShader");

        _compute_shader.SetFloat("p_in", 0f);
        _compute_shader.SetFloat("p_min", p_min);
        _compute_shader.SetFloat("p_thresh", p_thresh);
        _compute_shader.SetFloat("p_rest", p_rest);
        _compute_shader.SetFloat("p_refract", p_refract);
        _compute_shader.SetFloat("leak", leak);

        _compute_shader.SetFloat("no_stdp_window", no_stdp_window);
        _compute_shader.SetFloat("a_minus", a_minus);
        _compute_shader.SetFloat("a_plus", a_plus);
        _compute_shader.SetFloat("tau_minus", tau_minus);
        _compute_shader.SetFloat("tau_plus", tau_plus);
        _compute_shader.SetFloat("learning_rate", learning_rate);
        _compute_shader.SetFloat("weight_min", weight_min);
        _compute_shader.SetFloat("weight_max", weight_max);

        _compute_shader.SetInt("synapses_to_keep", synapses_to_keep);

        _neuron_buffer = new ComputeBuffer(neuron_buffer.Length, sizeof(float) + sizeof(int) + sizeof(int) + sizeof(float));
        _synapse_write_index_buffer = new ComputeBuffer(synapse_write_index_buffer.Length, sizeof(int));
        _synapse_buffer = new ComputeBuffer(synapse_buffer.Length, sizeof(int) + sizeof(float));
        _in_neuron_buffer = new ComputeBuffer(in_neuron_buffer.Length, sizeof(int));
        _weight_buffer = new ComputeBuffer(weight_buffer.Length, sizeof(float));

        _neuron_buffer.SetData(neuron_buffer);
        _synapse_write_index_buffer.SetData(synapse_write_index_buffer);
        _synapse_buffer.SetData(synapse_buffer);
        _in_neuron_buffer.SetData(in_neuron_buffer);
        _weight_buffer.SetData(weight_buffer);

        kernel = _compute_shader.FindKernel("calc");

        _compute_shader.SetBuffer(kernel, "neuron_buffer", _neuron_buffer);
        _compute_shader.SetBuffer(kernel, "synapse_write_index_buffer", _synapse_write_index_buffer);
        _compute_shader.SetBuffer(kernel, "synapse_buffer", _synapse_buffer);
        _compute_shader.SetBuffer(kernel, "in_neuron_buffer", _in_neuron_buffer);
        _compute_shader.SetBuffer(kernel, "weight_buffer", _weight_buffer);

        sim_time = 0;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.frameCount % 60 == 0)
        {
            _compute_shader.SetFloat("sim_time", sim_time);
            _neuron_buffer.SetData(neuron_buffer);
            _compute_shader.Dispatch(kernel, 64, 1, 1);
            _neuron_buffer.GetData(neuron_buffer);
            for(int i = 0; i < neuron_buffer.Length; i++)
            {
                GameObject node = GameObject.Find(i.ToString());
                node.GetComponent<Node>().spiked = neuron_buffer[i].spiked;
            }

            sim_time += 1;
        }
    }
}
