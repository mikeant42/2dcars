extends TileMap

# Empty/invisible tile marked as completely walkable. The ID of the tile should correspond
# to the order in which it was created in the tileset editor.
export(int) var _nav_tile_id := 1

func _ready() -> void:
	# Find the bounds of the tilemap (there is no 'size' property available)
	var bounds_min := Vector2.ZERO
	var bounds_max := Vector2.ZERO
	for pos in get_used_cells():
		if pos.x < bounds_min.x:
			bounds_min.x = int(pos.x)
		elif pos.x > bounds_max.x:
			bounds_max.x = int(pos.x)
		if pos.y < bounds_min.y:
			bounds_min.y = int(pos.y)
		elif pos.y > bounds_max.y:
			bounds_max.y = int(pos.y)
	
	# Replace all empty tiles with the provided navigation tile
	print(bounds_max)
	print(bounds_min)
	for x in range(bounds_min.x, bounds_max.x):
		for y in range(bounds_min.y, bounds_max.y):
			if get_cell(x, y) == -1:
				set_cell(x, y, _nav_tile_id)

	# Force the navigation mesh to update immediately
	update_dirty_quadrants()

