using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.Collections.Generic;

public class INVENTORY : MonoBehaviour
{
    [Header("components")]
    [SerializeField] SESSION_DATA session_data;

    //state
    SLOT cursor_slot = SLOT.create();
    SLOT[] backpack_slots = null;
    SLOT[] belt_slots = null;
    int selected_slot_index = 0;
    float score = 0f;
    int current_tally_count = 0;
    int total_tally_count = 0;

    //events
    public event Action score_event;
    public event Action tally_event;



    void Start()
    {
        this.selected_slot().invoke_select_event();
        this.score_event?.Invoke();
        this.tally_event?.Invoke();
    }

    public SLOT get_cursor_slot()
    {
        return this.cursor_slot;
    }

    public SLOT[] get_backpack_slots()
    {
        if (this.backpack_slots == null)
        {
            this.backpack_slots = new SLOT[12];
            for (int n = 0; n < this.backpack_slots.Length; n = n + 1)
            {
                this.backpack_slots[n] = SLOT.create();
            }
        }
        return this.backpack_slots;
    }

    public SLOT[] get_belt_slots()
    {
        if (this.belt_slots == null)
        {
            this.belt_slots = new SLOT[6];
            for (int n = 0; n < this.belt_slots.Length; n = n + 1)
            {
                this.belt_slots[n] = SLOT.create();
            }
        }
        return this.belt_slots;
    }

    public float get_score()
    {
        return this.score;
    }

    public void set_score(float score)
    {
        this.score = score;
        this.score_event?.Invoke();
    }

    public int get_current_tally_count()
    {
        return this.current_tally_count;
    }

    public void set_current_tally_count(int count)
    {
        this.current_tally_count = count;
        this.tally_event?.Invoke();

        if (this.current_tally_count == this.total_tally_count)
        {
            this.load_end_screen();
        }
    }

    public void load_end_screen()
    {
        this.session_data.score = this.score;
        this.session_data.time = Time.timeSinceLevelLoad;
        SceneManager.LoadScene(GLOBAL.end_screen_scene_number);
    }
    
    public int get_total_tally_count()
    {
        return this.total_tally_count;
    }
    
    public void set_total_tally_count(int count)
    {
        this.total_tally_count = count;
        this.tally_event?.Invoke();
    }

    
    
    public SLOT selected_slot()
    {
        return this.get_belt_slots()[this.selected_slot_index];
    }

    public void set_selected_slot_index(int index)
    {
        this.selected_slot_index = ((index % belt_slots.Length) + belt_slots.Length) % belt_slots.Length;
        this.selected_slot().invoke_select_event();
    }
    
    public void increment_selected_slot_index()
    {
        this.set_selected_slot_index(this.selected_slot_index + 1);
    }
    
    public void decrement_selected_slot_index()
    {
        this.set_selected_slot_index(this.selected_slot_index - 1);
    }

    public bool merge(ITEM.TYPE type, ITEM.STATE state)
    {
        for (int n = 0; n < this.get_belt_slots().Length; n = n + 1)
        {
            if (this.get_belt_slots()[n].merge(type, state))
            {
                return true;
            }
        }
        for (int n = 0; n < this.get_backpack_slots().Length; n = n + 1)
        {
            if (this.get_backpack_slots()[n].merge(type, state))
            {
                return true;
            }
        }
        return false;
    }

    public bool allot(ITEM.TYPE type, ITEM.STATE state)
    {
        for (int n = 0; n < this.get_belt_slots().Length; n = n + 1)
        {
            if (this.get_belt_slots()[n].allot(type, state))
            {
                return true;
            }
        }
        for (int n = 0; n < this.get_backpack_slots().Length; n = n + 1)
        {
            if (this.get_backpack_slots()[n].allot(type, state))
            {
                return true;
            }
        }
        return false;
    }
}



public class SLOT
{
    //state
    public ITEM.TYPE type;
    Stack<ITEM.STATE> state_stack;

    //events
    public event Action modify_event;
    public event Action select_event;

    

    static public SLOT create()
    {
        SLOT result = new SLOT { };
        result.type = null;
        result.state_stack = new Stack<ITEM.STATE> { };
        return result;
    }



    public int count()
    {
        return this.state_stack.Count;
    }

    public bool merge(ITEM.TYPE type, ITEM.STATE state)
    {
        if (this.count() > 0 && this.type == type)
        {
            this.state_stack.Push(state);
            this.modify_event?.Invoke();
            return true;
        }
        return false;
    }

    public bool allot(ITEM.TYPE type, ITEM.STATE state)
    {
        if (this.count() == 0)
        {
            this.type = type;
            this.state_stack.Push(state);
            this.modify_event?.Invoke();
            return true;
        }
        return false;
    }

    public (ITEM.TYPE type, ITEM.STATE state) remove()
    {
        if (this.count() > 0)
        {
            ITEM.TYPE type = this.type;
            ITEM.STATE state = this.state_stack.Pop();
            if (this.count() == 0)
            {
                this.type = null;
            }
            this.modify_event?.Invoke();
            return (type, state);
        }
        return (default, default);
    }

    public (ITEM.TYPE type, ITEM.STATE state) peek()
    {
        if (this.count() > 0)
        {
            return (this.type, this.state_stack.Peek());
        }
        return (default, default);
    }

    public void exchange(SLOT other)
    {
        (this.type, other.type) = (other.type, this.type);
        (this.state_stack, other.state_stack) = (other.state_stack, this.state_stack);
        this.modify_event?.Invoke();
        other.modify_event?.Invoke();
    }

    public void invoke_select_event()
    {
        this.select_event?.Invoke();
    }
}