using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

[CreateAssetMenu(fileName = "inventory", menuName = "inventory")]
public class INVENTORY : ScriptableObject
{
    //constants
    //logically linked to `belt` object's children count
    const int belt_slots_count = 6;
    //logically linked to `backpack` object's children count
    const int backpack_slots_count = 12;

    //state
    public SLOT cursor_slot = new SLOT {};
    public SLOT[] backpack_slots = new SLOT[backpack_slots_count];
    public SLOT[] belt_slots = new SLOT[belt_slots_count];
    int selected_index = 0;
    
    float score = 0f;
    int sorted_count = 0;
    int total_count = 0;

    //events
    public event Action scored;



    void OnEnable()
    {
        this.cursor_slot = SLOT.create();
        for (int n = 0; n < this.backpack_slots.Length; n = n + 1)
        {
            this.backpack_slots[n] = SLOT.create();
        }
        for (int n = 0; n < this.belt_slots.Length; n = n + 1)
        {
            this.belt_slots[n] = SLOT.create();
        }
        this.selected_index = 0;
        this.score = 0f;
        this.sorted_count = 0;
        this.total_count = 0;
    }

    
    
    public float get_score()
    {
        return this.score;
    }

    public void increase_score(float score)
    {
        this.score = this.score + score;
        this.sorted_count = this.sorted_count + 1;
        this.scored.Invoke();

        if (this.sorted_count == this.total_count)
        {
            //win game
            StreamWriter writer = new StreamWriter(Path.Combine(Application.persistentDataPath, MAIN_MENU.leaderboard_path), true);
            using (writer)
            {
                writer.WriteLine($"{PLAYER_INFO.name},{this.score},{Time.timeSinceLevelLoad}");
            }
        }
    }

    public int get_sorted_count()
    {
        return this.sorted_count;
    }
    
    public int get_total_count()
    {
        return this.total_count;
    }
    
    public void set_total_count(int total_count)
    {
        this.total_count = total_count;
        this.scored.Invoke();
    }

    
    
    public SLOT selected_slot()
    {
        return this.belt_slots[this.selected_index];
    }

    public void set_selected_index(int index)
    {
        this.selected_index = ((index % belt_slots_count) + belt_slots_count) % belt_slots_count;
        this.selected_slot().trigger_selected();
    }
    
    public void increment_selected_index()
    {
        this.set_selected_index(this.selected_index + 1);
    }
    
    public void decrement_selected_index()
    {
        this.set_selected_index(this.selected_index - 1);
    }

    public bool merge(ITEM.TYPE type, ITEM.STATE state)
    {
        for (int n = 0; n < this.belt_slots.Length; n = n + 1)
        {
            if (this.belt_slots[n].merge(type, state))
            {
                return true;
            }
        }
        for (int n = 0; n < this.backpack_slots.Length; n = n + 1)
        {
            if (this.backpack_slots[n].merge(type, state))
            {
                return true;
            }
        }
        return false;
    }

    public bool allot(ITEM.TYPE type, ITEM.STATE state)
    {
        for (int n = 0; n < this.belt_slots.Length; n = n + 1)
        {
            if (this.belt_slots[n].allot(type, state))
            {
                return true;
            }
        }
        for (int n = 0; n < this.backpack_slots.Length; n = n + 1)
        {
            if (this.backpack_slots[n].allot(type, state))
            {
                return true;
            }
        }
        return false;
    }



    public class SLOT
    {
        //state
        ITEM.TYPE type;
        Stack<ITEM.STATE> state_stack;

        //events
        public event Action modified;
        public event Action selected;


        static public SLOT create()
        {
            SLOT result = new SLOT {};
            result.type = null;
            result.state_stack = new Stack<ITEM.STATE> {};
            return result;
        }



        public int count()
        {
            return this.state_stack.Count;
        }

        public ITEM.TYPE get_type()
        {
            return this.type;
        }



        public bool merge(ITEM.TYPE type, ITEM.STATE state)
        {
            if (this.count() > 0 && this.get_type() == type)
            {
                this.state_stack.Push(state);
                this.modified.Invoke();
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
                this.modified.Invoke();
                return true;
            }
            return false;
        }

        public (ITEM.TYPE type, ITEM.STATE state) remove()
        {
            if (this.count() > 0)
            {
                ITEM.TYPE type = this.get_type();
                ITEM.STATE state = this.state_stack.Pop();
                if (this.count() == 0)
                {
                    this.type = null;
                }
                this.modified.Invoke();
                return (type, state);
            }
            return (default, default);
        }

        public (ITEM.TYPE type, ITEM.STATE state) peek()
        {
            if (this.count() > 0)
            {
                return (this.get_type(), this.state_stack.Peek());
            }
            return (default, default);
        }

        public void exchange(SLOT other)
        {
            (this.type, other.type) = (other.type, this.type);
            (this.state_stack, other.state_stack) = (other.state_stack, this.state_stack);
            this.modified.Invoke();
            other.modified.Invoke();
        }

        public void trigger_selected()
        {
            this.selected.Invoke();
        }
    }
}
