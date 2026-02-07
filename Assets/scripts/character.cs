using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class CHARACTER : MonoBehaviour
{
	//constants
	const int default_layer = 0;
	const int outline_layer = 6;
	const int preview_layer = 7;

	[Header("configuration")]
	[SerializeField] float sensitivity = 8f;
	[SerializeField] float reach = 4f;
    [SerializeField] float speed = 750;
    [SerializeField] float airborne_speed = 250;
    [SerializeField] float max_horizontal_velocity_magnitude = 4f;
    [SerializeField] float counter_strength = 0.25f;
    [SerializeField] float max_slope = 45f;
	[SerializeField] float grounded_timeout = 0.125f;
	[SerializeField] Material preview_material;
    [SerializeField] AudioClip collect_sound;
    [SerializeField] AudioClip place_sound;
    [SerializeField] AudioClip correct_sound;
    [SerializeField] AudioClip incorrect_sound;
    [SerializeField] float diminish_multiplier = 1f / 2f;

    InputActionMap character_actions;
    InputAction move_action;
    InputAction look_action;
	InputAction drag_action;
	InputAction collect_action;
	InputAction place_action;
    InputAction scroll_belt_action;
    InputAction open_backpack_action;
    InputActionMap backpack_actions;
    InputAction close_backpack_action;

	[Header("components")]
	[SerializeField] Rigidbody body;
	[SerializeField] ConfigurableJoint drag_joint;
	[SerializeField] LineRenderer drag_line;
	[SerializeField] AudioSource audio_source;
    [SerializeField] INVENTORY inventory;
    [SerializeField] GameObject backpack_slots_object;
    [SerializeField] GameObject cursor_slot_object;

    //state
    Vector2 move_input = default;
    float pitch = 0f;
	float grounded_timer = 0f;
	bool is_grounded = true;
    RaycastHit hit = default;
    float drag_distance = 0f;
	GameObject outlined_object = null;
	GameObject preview_object = null;
	GameObject preview_prefab = null;


	
    void Awake()
    {
        this.character_actions = InputSystem.actions.FindActionMap("character");
        this.move_action = this.character_actions.FindAction("move");
        this.look_action = this.character_actions.FindAction("look");
		this.drag_action = this.character_actions.FindAction("drag");
		this.collect_action = this.character_actions.FindAction("collect");
		this.place_action = this.character_actions.FindAction("place");
		this.scroll_belt_action = this.character_actions.FindAction("scroll_belt");
        this.open_backpack_action = this.character_actions.FindAction("open_backpack");
        this.backpack_actions = InputSystem.actions.FindActionMap("backpack");
        this.close_backpack_action = this.backpack_actions.FindAction("close_backpack");

        this.body.freezeRotation = true;
		this.drag_joint.connectedBody = null;
		this.drag_line.enabled = false;
    }
    
    void OnEnable()
    {
        this.drag_action.started += this.on_drag_start;
        this.drag_action.canceled += this.on_drag_end;
        this.collect_action.performed += this.on_collect;
        this.place_action.performed += this.on_place;
        this.scroll_belt_action.performed += this.on_scroll_belt;
        this.open_backpack_action.performed += this.on_open_backpack;
        this.close_backpack_action.performed += this.on_close_backpack;
        this.character_actions.Enable();
        this.backpack_actions.Disable();
    }
    
    void OnDisable()
    {
        this.drag_action.started -= this.on_drag_start;
        this.drag_action.canceled -= this.on_drag_end;
        this.collect_action.performed -= this.on_collect;
        this.place_action.performed -= this.on_place;
        this.scroll_belt_action.performed -= this.on_scroll_belt;
        this.open_backpack_action.performed -= this.on_open_backpack;
        this.close_backpack_action.performed -= this.on_close_backpack;
        this.character_actions.Disable();
        this.backpack_actions.Disable();
    }

	void Start()
    {
		Cursor.lockState = CursorLockMode.Locked;
    }
    
    void on_scroll_belt(InputAction.CallbackContext context)
    {
        if (context.ReadValue<Vector2>().y > 0)
        {
            this.inventory.decrement_selected_slot_index();
        }
        else if (context.ReadValue<Vector2>().y < 0)
        {
            this.inventory.increment_selected_slot_index();
        }
    }
    
    void Update()
	{
        this.move_input = this.move_action.ReadValue<Vector2>();
        this.look();
        
		if (this.drag_joint.connectedBody != null)
		{
			this.drag();
		}
        
        if (Physics.Raycast(Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f)), out this.hit, this.reach, ~(1 << preview_layer)))
        {
            (ITEM.TYPE item_type, ITEM.STATE _) = this.inventory.selected_slot().peek();
            if (item_type != null)
            {
                if (this.hit.transform.TryGetComponent(out SOCKET socket))
                {
                    this.reset_outline();
                    this.set_preview(socket.transform.position, socket.transform.rotation, item_type.prefab);
                }
                else if (this.hit.transform.root.TryGetComponent(out BIN _))
                {
                    this.set_outline(this.hit.transform.root.gameObject);
                    this.reset_preview();
                }
                else if (this.hit.transform.root.TryGetComponent(out ITEM _))
                {
                    this.set_outline(this.hit.transform.root.gameObject);
                    this.set_preview(this.hit.point, this.camera_facing_rotation(this.hit), item_type.prefab);
                }
                else
                {
                    this.reset_outline();
                    this.set_preview(this.hit.point, this.camera_facing_rotation(this.hit), item_type.prefab);
                }
            }
            else
            {
                this.reset_preview();
                if (this.hit.transform.root.TryGetComponent(out ITEM _))
                {
                    this.set_outline(this.hit.transform.root.gameObject);
                }
                else
                {
                    this.reset_outline();
                }
            }
        }
        else
        {
            this.reset_outline();
            this.reset_preview();
        }
    }
    
	void look()
    {
        float pitch_delta = this.look_action.ReadValue<Vector2>().y * Time.deltaTime * this.sensitivity;
        float yaw_delta = this.look_action.ReadValue<Vector2>().x * Time.deltaTime * this.sensitivity;

        this.pitch = this.pitch - pitch_delta;
        this.pitch = Mathf.Clamp(this.pitch, -90f, 90f);

        Camera.main.transform.localRotation = Quaternion.Euler(this.pitch, 0, 0);
        this.transform.Rotate(0, yaw_delta, 0);
    }
    
    void on_open_backpack(InputAction.CallbackContext context)
    {
        this.character_actions.Disable();
        this.backpack_actions.Enable();
        Cursor.lockState = CursorLockMode.None;
        this.backpack_slots_object.SetActive(true);
        this.cursor_slot_object.SetActive(true);
    }
    
    void on_close_backpack(InputAction.CallbackContext context)
    {
        this.character_actions.Enable();
        this.backpack_actions.Disable();
        Cursor.lockState = CursorLockMode.Locked;
        this.backpack_slots_object.SetActive(false);
        this.cursor_slot_object.SetActive(false);
    }
    
    void on_drag_start(InputAction.CallbackContext context)
    {
        if (this.hit.transform == null) { return; }
        if (this.hit.rigidbody != null)
        {
            this.attach(this.hit);
        }
    }

    void on_drag_end(InputAction.CallbackContext context)
    {
		if (this.drag_joint.connectedBody != null)
		{
			this.detach();
		}
    }
    
    void on_collect(InputAction.CallbackContext context)
    {
        if (this.hit.transform == null) { return; }
        if (this.hit.transform.root.TryGetComponent(out ITEM item))
        {
            if (this.inventory.merge(item.type, item.state) ||
                this.inventory.selected_slot().allot(item.type, item.state) ||
                this.inventory.allot(item.type, item.state))
            {
               	if (this.drag_joint.connectedBody == this.hit.rigidbody)
               	{
              		this.detach();
               	}
               	Destroy(this.hit.transform.root.gameObject);
                this.audio_source.clip = this.collect_sound;
                this.audio_source.Play();
            }
        }
    }
    
	Quaternion camera_facing_rotation(RaycastHit hit)
	{
        //Vector3 camera_direction = Camera.main.transform.position - hit.point;
        //camera_direction.y = 0f;
        Vector3 camera_direction = Camera.main.transform.position - Vector3.up * 1.125f - hit.point;
        Vector3 projection = Vector3.ProjectOnPlane(camera_direction, hit.normal);
		return projection != Vector3.zero ? Quaternion.LookRotation(projection, hit.normal) : Quaternion.FromToRotation(Vector3.up, hit.normal);
	}

    void on_place(InputAction.CallbackContext context)
    {
        if (this.hit.transform == null) { return; }
        if (this.inventory.selected_slot().count() == 0) { return; }
        if (this.hit.transform.TryGetComponent(out SOCKET socket))
        {
            (ITEM.TYPE item_type, ITEM.STATE item_state) = this.inventory.selected_slot().peek();
            if (item_type.target_id == socket.id)
            {
                this.inventory.set_score(this.inventory.get_score() + item_type.score * item_state.multiplier);
                this.inventory.set_current_tally_count(this.inventory.get_current_tally_count() + 1);
                this.audio_source.clip = this.correct_sound;
                this.audio_source.Play();
                GameObject item_object = Instantiate(item_type.prefab, socket.transform.position, socket.transform.rotation, socket.transform);
                _ = this.inventory.selected_slot().remove();
                Destroy(socket);
            }
            else
            {
                item_state.multiplier = item_state.multiplier * this.diminish_multiplier;
                this.audio_source.clip = this.incorrect_sound;
                this.audio_source.Play();
            }
        }
        else if (this.hit.transform.root.TryGetComponent(out BIN bin))
        {
            (ITEM.TYPE item_type, ITEM.STATE item_state) = this.inventory.selected_slot().peek();
            if (item_type.target_id == bin.id)
            {
                this.inventory.set_score(this.inventory.get_score() + item_type.score * item_state.multiplier);
                this.inventory.set_current_tally_count(this.inventory.get_current_tally_count() + 1);
                this.audio_source.clip = this.correct_sound;
                this.audio_source.Play();
                _ = this.inventory.selected_slot().remove();
            }
            else
            {
                item_state.multiplier = item_state.multiplier * this.diminish_multiplier;
                this.audio_source.clip = this.incorrect_sound;
                this.audio_source.Play();
            }
        }
        else
        {
            (ITEM.TYPE item_type, ITEM.STATE item_state) = this.inventory.selected_slot().remove();
            GameObject item_object = Instantiate(item_type.prefab, this.hit.point, this.camera_facing_rotation(this.hit));
            ITEM item = item_object.AddComponent<ITEM>();
            item.type = item_type;
            item.state = item_state;
            item_object.AddComponent<Rigidbody>().mass = item_type.mass;
            this.audio_source.clip = this.place_sound;
            this.audio_source.Play();
        }
    }
    
	void attach(RaycastHit hit)
    {
        this.drag_distance = Vector3.Distance(Camera.main.transform.position, hit.point);
        this.drag_joint.transform.position = hit.point;
		this.drag_joint.anchor = Vector3.zero;
		this.drag_joint.connectedBody = hit.rigidbody;
		this.drag_joint.connectedAnchor = hit.rigidbody.transform.InverseTransformPoint(hit.point);
		this.drag_line.enabled = true;
	}

	void drag()
    {
        this.drag_joint.transform.position = Camera.main.transform.position + (Camera.main.transform.forward * this.drag_distance);
        this.drag_line.SetPosition(0, this.drag_joint.transform.TransformPoint(this.drag_joint.anchor));
		this.drag_line.SetPosition(1, this.drag_joint.connectedBody.transform.TransformPoint(this.drag_joint.connectedAnchor));
	}

	void detach()
	{
		this.drag_joint.connectedBody = null;
		this.drag_line.enabled = false;
	}

	void set_layer_recursively(GameObject game_object, int layer)
	{
        game_object.layer = layer;
    	foreach (Transform child_transform in game_object.transform)
    	{
        	this.set_layer_recursively(child_transform.gameObject, layer);
    	}
	}

	void set_preview(Vector3 position, Quaternion rotation, GameObject prefab)
    {
        if (this.preview_object == null || this.preview_prefab != prefab)
        {
            this.reset_preview();
			this.preview_object = Instantiate(prefab, position, rotation);
			this.preview_prefab = prefab;
            foreach (Collider collider in this.preview_object.GetComponentsInChildren<Collider>())
            {
                collider.isTrigger = true;
			}
            foreach (Rigidbody body in this.preview_object.GetComponentsInChildren<Rigidbody>())
			{
                body.isKinematic = true;
			}
            foreach (var renderer in this.preview_object.GetComponentsInChildren<Renderer>())
            {
				//i loooooooove c#
				/*
    			for (int n = 0; n < rend.materials.Length; n = n + 1)
    			{
        			rend.materials[n] = preview_material;
   	 			}
				*/
				Material[] preview_materials = new Material[renderer.sharedMaterials.Length];
    			for (int n = 0; n < preview_materials.Length; n = n + 1)
    			{
    			    preview_materials[n] = preview_material;
    			}
				renderer.materials = preview_materials;
            	renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
			this.set_layer_recursively(this.preview_object, preview_layer);
        }
		this.preview_object.transform.position = position;
		this.preview_object.transform.rotation = rotation;
    }
    
    void reset_preview()
    {
        if (this.preview_object == null) { return; }
        Destroy(this.preview_object);
        this.preview_object = null;
		this.preview_prefab = null;
    }

	void set_outline(GameObject game_object)
	{
		if (game_object != this.outlined_object)
		{
			this.reset_outline();
			this.set_layer_recursively(game_object, outline_layer);
			this.outlined_object = game_object;
		}
	}

	void reset_outline()
	{
		if (this.outlined_object == null) { return; }
		this.set_layer_recursively(this.outlined_object, default_layer);
        this.outlined_object = null;
	}

	
	
	void FixedUpdate()
    {
		if (this.grounded_timer > 0f)
    	{
        	this.grounded_timer -= Time.fixedDeltaTime;
    	}
    	else
    	{
        	this.is_grounded = false;
    	}

        this.move();
        if (this.is_grounded)
        {
            this.counter_move();
        }
        this.limit_horizontal_velocity();
    }

    void move()
    {
        float z = this.move_input.y;
        float x = this.move_input.x;
        this.body.AddForce((this.transform.forward * z + this.transform.right * x) * (this.is_grounded ? this.speed : this.airborne_speed));
    }

    void counter_move()
    {
		float yaw = this.transform.eulerAngles.y;
        float velocity_yaw = Mathf.Atan2(this.body.linearVelocity.x, this.body.linearVelocity.z) * Mathf.Rad2Deg;
        float u = Mathf.DeltaAngle(yaw, velocity_yaw);
        float v = 90f - u;
        Vector2 counter_velocity = new Vector2(Mathf.Cos(v * Mathf.Deg2Rad), Mathf.Cos(u * Mathf.Deg2Rad)) * this.body.linearVelocity.magnitude;

        float z = this.move_input.y;
        float x = this.move_input.x;

        if (Mathf.Abs(counter_velocity.x) > 0.5f && Mathf.Abs(x * this.speed) < 0.05f
            || counter_velocity.x < -0.5f && x * this.speed > 0
            || counter_velocity.x > 0.5f && x * this.speed < 0)
        {
            this.body.AddForce(this.transform.right * -counter_velocity.x * this.counter_strength * this.speed);
        }
        if (Mathf.Abs(counter_velocity.y) > 0.5f && Mathf.Abs(z * this.speed) < 0.05f
            || counter_velocity.y < -0.5f && z * this.speed > 0
            || counter_velocity.y > 0.5f && z * this.speed < 0)
        {
            this.body.AddForce(this.transform.forward * -counter_velocity.y * this.counter_strength * this.speed);
        }
    }

	void limit_horizontal_velocity()
    {
        Vector3 horizontal_velocity = new Vector3(this.body.linearVelocity.x, 0f, this.body.linearVelocity.z);
        if (horizontal_velocity.magnitude > this.max_horizontal_velocity_magnitude)
        {
            Vector3 limited_velocity = horizontal_velocity.normalized * this.max_horizontal_velocity_magnitude;
            this.body.linearVelocity = new Vector3(limited_velocity.x, this.body.linearVelocity.y, limited_velocity.z);
        }
    }

	void OnCollisionStay(Collision other)
	{
    	foreach (ContactPoint contact in other.contacts)
    	{
        	if (Vector3.Angle(Vector3.up, contact.normal) < this.max_slope)
        	{
            	this.is_grounded = true;
            	this.grounded_timer = this.grounded_timeout;
            	return;
        	}
    	}
	}
}
