tool
extends Sprite


func _ready():
	connect("item_rect_changed", self, "_on_item_rect_changed")

func _process(delta: float):
	yield(VisualServer, "frame_post_draw")
	var viewport = get_node("../ViewportContainer/Viewport")
	var pos = get_node("../Car").position
	material.set_shader_param("player_pos", pos)
	var img = viewport.get_texture().get_data()
	#img.flip_y()
	var tex = ImageTexture.new()
	tex.create_from_image(img)

	material.set_shader_param("target", tex)

	update_zoom(get_viewport_transform().x.y)


func update_zoom(value: float):
	material.set_shader_param("zoom_y", value)


func _on_item_rect_changed():
	material.set_shader_param("scale_y", scale.y)
