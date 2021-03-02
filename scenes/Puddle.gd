tool
extends Sprite


# Declare member variables here. Examples:
# var a = 2
# var b = "text"


# Called when the node enters the scene tree for the first time.
func _ready():
	pass # Replace with function body.


func _on_Puddle_item_rect_changed():
	material.set_shader_param("scale", scale)
