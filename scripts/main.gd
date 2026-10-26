extends Node3D

const SAVE_PATH := "user://affaire_zero_save.json"
const CLUES := [
	{"id":"ticket","title_en":"Train ticket","title_ar":"تذكرة القطار","desc_en":"A ticket stamped 21:10. The ink is fresh despite the rain.","desc_ar":"تذكرة مختومة الساعة 21:10. الحبر حديث رغم المطر.","room":"office"},
	{"id":"glass","title_en":"Broken glass","title_ar":"زجاج مكسور","desc_en":"The fragments lie on top of the dust: the window broke recently.","desc_ar":"الشظايا فوق الغبار؛ النافذة كُسرت مؤخراً.","room":"hall"},
	{"id":"note","title_en":"Encrypted note","title_ar":"ملاحظة مشفّرة","desc_en":"Three times are circled: 20:40, 21:10, 21:25.","desc_ar":"ثلاثة أوقات محددة: 20:40 و21:10 و21:25.","room":"archive"},
	{"id":"mud","title_en":"Wet footprints","title_ar":"آثار أقدام مبللة","desc_en":"The footprints lead from the service entrance, not the street door.","desc_ar":"تقود الآثار من مدخل الخدمة لا من باب الشارع.","room":"hall"},
	{"id":"recorder","title_en":"Voice recorder","title_ar":"مسجل صوت","desc_en":"A recording mentions a power cut at 20:40.","desc_ar":"يذكر التسجيل انقطاع الكهرباء عند 20:40.","room":"office"},
	{"id":"key","title_en":"Brass key","title_ar":"مفتاح نحاسي","desc_en":"A maintenance key with traces of fresh machine oil.","desc_ar":"مفتاح صيانة عليه آثار زيت آلي حديث.","room":"service"},
	{"id":"log","title_en":"Maintenance log","title_ar":"سجل الصيانة","desc_en":"The service hatch was opened at 21:25 using a master key.","desc_ar":"فُتحت فتحة الخدمة عند 21:25 بمفتاح رئيسي.","room":"service"},
	{"id":"fiber","title_en":"Blue fiber","title_ar":"ليف أزرق","desc_en":"A blue synthetic fiber caught on the hatch hinge.","desc_ar":"ليف صناعي أزرق عالق بمفصلة الفتحة.","room":"service"}
]
const SUSPECTS := [
	{"id":"mira","en":"Mira — archivist","ar":"ميرا — أمينة الأرشيف","hint_en":"Had access to the archive, but the timeline does not place her at the hatch.","hint_ar":"كان لديها وصول للأرشيف، لكن التسلسل الزمني لا يضعها عند الفتحة."},
	{"id":"yusef","en":"Yusef — technician","ar":"يوسف — الفني","hint_en":"Had the maintenance key and knew the power-cut schedule.","hint_ar":"كان يملك مفتاح الصيانة ويعرف جدول انقطاع الكهرباء."},
	{"id":"nadia","en":"Nadia — visitor","ar":"نادية — الزائرة","hint_en":"Her train ticket explains her departure, not the hidden passage.","hint_ar":"تفسر تذكرة القطار مغادرتها لا الممر المخفي."}
]
var lang := "en"
var found: Array[String] = []
var opened_hatch := false
var selected_suspect := ""
var hint_level := 0
var player: CharacterBody3D
var camera: Camera3D
var interactables: Array[Dictionary] = []
var message_label: Label
var clue_panel: PanelContainer
var clue_text: Label
var overlay: Control
var room_label: Label
var clue_count_label: Label
var move_touch := Vector2.ZERO
var touch_start := Vector2.ZERO
var look_touch_id := -1
var move_touch_id := -1
var move_touch_origin := Vector2.ZERO
var move_touch_current := Vector2.ZERO
var look_delta := Vector2.ZERO
var mobile_move_pad: Control
var last_interaction := 0.0

func _ready() -> void:
	_load_save()
	_build_world()
	_build_ui()
	_set_message(tr("intro"))

func tr(key: String) -> String:
	var dict := {
		"title":{"en":"AFFAIRE ZÉRO","ar":"القضية صفر"},
		"subtitle":{"en":"CASE 01  /  THE IMPOSSIBLE DISAPPEARANCE","ar":"القضية 01 / الاختفاء المستحيل"},
		"intro":{"en":"An archive curator vanished during a blackout. Explore the building, inspect evidence and reconstruct the timeline.","ar":"اختفى أمين أرشيف أثناء انقطاع الكهرباء. استكشف المبنى وافحص الأدلة وأعد بناء التسلسل الزمني."},
		"move":{"en":"MOVE  •  WASD / left pad     LOOK  •  drag right side","ar":"الحركة • لوحة اليسار     النظر • اسحب الجهة اليمنى"},
		"evidence":{"en":"EVIDENCE","ar":"الأدلة"},
		"suspects":{"en":"SUSPECTS","ar":"المشتبه بهم"},
		"deduce":{"en":"MAKE DEDUCTION","ar":"استنتج الحل"},
		"hint":{"en":"OPTIONAL HINT","ar":"تلميح اختياري"},
		"interact":{"en":"Inspect","ar":"افحص"},
		"hatch_locked":{"en":"The hatch is locked. Find the three times in the evidence.","ar":"الفتحة مقفلة. اعثر على الأوقات الثلاثة في الأدلة."},
		"hatch_open":{"en":"The sequence is correct. The hatch opens onto the service passage.","ar":"التسلسل صحيح. انفتحت الفتحة على ممر الخدمة."},
		"need_clues":{"en":"You need at least four clues before making a deduction.","ar":"تحتاج إلى أربعة أدلة على الأقل قبل الاستنتاج."},
		"solved":{"en":"CASE SOLVED — The evidence points to Yusef. The blackout at 20:40 hid his route; the ticket and broken glass were staged distractions. The 21:25 maintenance log and blue fiber link him to the hatch.","ar":"حُلّت القضية — تشير الأدلة إلى يوسف. أخفى انقطاع الكهرباء عند 20:40 طريقه؛ وكانت التذكرة والزجاج المكسور تمويهاً. يربطه سجل الصيانة عند 21:25 والليف الأزرق بالفتحة."},
		"wrong":{"en":"That conclusion does not fit all the evidence. Review the timeline and try again.","ar":"لا يتوافق هذا الاستنتاج مع جميع الأدلة. راجع التسلسل الزمني وحاول مجدداً."},
		"hint1":{"en":"Hint 1: distinguish staged evidence from evidence tied to a specific place and time.","ar":"التلميح 1: ميّز بين الأدلة المصطنعة والأدلة المرتبطة بمكان ووقت محددين."},
		"hint2":{"en":"Hint 2: compare the 20:40 blackout with the 21:25 maintenance log.","ar":"التلميح 2: قارن انقطاع الكهرباء عند 20:40 بسجل الصيانة عند 21:25."},
		"hint3":{"en":"Hint 3: who had the master key, knew the outage schedule and could access the service hatch?","ar":"التلميح 3: من امتلك المفتاح الرئيسي وعرف موعد الانقطاع واستطاع الوصول إلى فتحة الخدمة؟"}
	}
	if dict.has(key): return dict[key][lang]
	return key

func _build_world() -> void:
	var env := WorldEnvironment.new()
	var environment := Environment.new()
	environment.background_mode = Environment.BG_SKY
	environment.sky = Sky.new()
	environment.sky.sky_material = ProceduralSkyMaterial.new()
	environment.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	environment.ambient_light_color = Color(0.25, 0.29, 0.36)
	environment.ambient_light_energy = 0.55
	env.environment = environment
	add_child(env)
	var light := DirectionalLight3D.new()
	light.rotation_degrees = Vector3(-48, -32, 0)
	light.light_energy = 0.8
	add_child(light)
	_make_box("Floor", Vector3(0,-0.2,0), Vector3(24,0.4,24), Color(0.12,0.14,0.16))
	_add_static_box(Vector3(0,-0.22,0), Vector3(24,0.4,24))
	# Four connected rooms, rendered with simple low-cost geometry.
	for x in [-8.0, 0.0, 8.0]:
		_make_box("Partition", Vector3(x,1.6,-7), Vector3(0.25,3.2,8), Color(0.22,0.24,0.27))
		_make_box("Partition", Vector3(x,1.6,7), Vector3(0.25,3.2,8), Color(0.22,0.24,0.27))
	_make_box("BackWall", Vector3(0,1.6,-11.5), Vector3(24,3.2,0.4), Color(0.18,0.20,0.23))
	_make_box("FrontWallL", Vector3(-7,1.6,11.5), Vector3(10,3.2,0.4), Color(0.18,0.20,0.23))
	_make_box("FrontWallR", Vector3(7,1.6,11.5), Vector3(10,3.2,0.4), Color(0.18,0.20,0.23))
	_make_box("Ceiling beam", Vector3(0,3.1,0), Vector3(24,0.18,24), Color(0.15,0.16,0.18))
	# Rooms are marked by furniture, lights and evidence objects.
	_make_box("Archive table", Vector3(-4,0.65,-3), Vector3(2.2,0.15,1.2), Color(0.30,0.19,0.11))
	_make_box("Office desk", Vector3(4,0.7,-3), Vector3(2.4,0.18,1.3), Color(0.28,0.18,0.12))
	_make_box("Service bench", Vector3(4,0.6,5), Vector3(2.2,0.15,1), Color(0.25,0.27,0.28))
	_make_box("Hatch", Vector3(4,0.025,7.8), Vector3(1.6,0.08,1.4), Color(0.07,0.08,0.09), "hatch")
	_add_evidence("ticket", Vector3(4,0.92,-3))
	_add_evidence("recorder", Vector3(3.5,0.92,-2.7))
	_add_evidence("glass", Vector3(-2,0.12,1))
	_add_evidence("note", Vector3(-4,0.92,-3))
	_add_evidence("mud", Vector3(0,0.15,3))
	_add_evidence("key", Vector3(4,0.85,5))
	_add_evidence("log", Vector3(5,0.85,5.2))
	_add_evidence("fiber", Vector3(5,0.3,7.3))
	_add_label_3d("ARCHIVES", Vector3(-4,2.7,-8.8))
	_add_label_3d("OFFICE", Vector3(4,2.7,-8.8))
	_add_label_3d("SERVICE", Vector3(4,2.7,8.8))
	player = CharacterBody3D.new()
	player.name = "Player"
	player.position = Vector3(0,0.1,9.5)
	var collider := CollisionShape3D.new()
	var shape := CapsuleShape3D.new()
	shape.radius = 0.35
	shape.height = 1.6
	collider.shape = shape
	collider.position.y = 0.8
	player.add_child(collider)
	camera = Camera3D.new()
	camera.position = Vector3(0,1.45,0)
	camera.rotation_degrees.x = -8
	camera.current = true
	player.add_child(camera)
	add_child(player)
	# Minimal walls collision so the player stays in the building.
	for spec in [[Vector3(0,1.5,-11.3),Vector3(24,3,0.4)],[Vector3(-11.8,1.5,0),Vector3(0.4,3,24)],[Vector3(11.8,1.5,0),Vector3(0.4,3,24)],[Vector3(0,1.5,11.4),Vector3(8,3,0.4)]]:
		_add_static_box(spec[0],spec[1])

func _make_box(label: String, pos: Vector3, size: Vector3, color: Color, evidence_id: String = "") -> MeshInstance3D:
	var mesh := MeshInstance3D.new()
	mesh.name = label
	var box := BoxMesh.new()
	box.size = size
	mesh.mesh = box
	var mat := StandardMaterial3D.new()
	mat.albedo_color = color
	mat.roughness = 0.85
	mesh.material_override = mat
	mesh.position = pos
	add_child(mesh)
	return mesh

func _add_static_box(pos: Vector3, size: Vector3) -> void:
	var body := StaticBody3D.new()
	body.position = pos
	var collision := CollisionShape3D.new()
	var shape := BoxShape3D.new()
	shape.size = size
	collision.shape = shape
	body.add_child(collision)
	add_child(body)

func _add_evidence(id: String, pos: Vector3) -> void:
	var m := _make_box("Evidence_"+id, pos, Vector3(0.48,0.12,0.36), Color(0.73,0.56,0.26) if not found.has(id) else Color(0.25,0.68,0.48))
	interactables.append({"id":id,"node":m,"pos":pos})
	if id == "fiber": m.scale = Vector3(0.55,0.3,0.25)

func _add_label_3d(text_value: String, pos: Vector3) -> void:
	var label := Label3D.new()
	label.text = text_value
	label.position = pos
	label.font_size = 42
	label.modulate = Color(0.65,0.75,0.82)
	label.billboard = BaseMaterial3D.BILLBOARD_ENABLED
	add_child(label)

func _build_ui() -> void:
	var ui := CanvasLayer.new()
	ui.name = "UI"
	add_child(ui)
	overlay = Control.new()
	overlay.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	overlay.mouse_filter = Control.MOUSE_FILTER_PASS
	ui.add_child(overlay)
	var top := PanelContainer.new()
	top.anchor_left = 0.02; top.anchor_right = 0.98; top.anchor_top = 0.02; top.anchor_bottom = 0.14
	top.add_theme_stylebox_override("panel", _panel_style(Color(0.035,0.055,0.075,0.92)))
	overlay.add_child(top)
	var top_row := HBoxContainer.new()
	top_row.add_theme_constant_override("separation", 14)
	top.add_child(top_row)
	var title := Label.new(); title.text = tr("title"); title.add_theme_font_size_override("font_size", 25); title.size_flags_horizontal = Control.SIZE_EXPAND_FILL; top_row.add_child(title)
	var subtitle := Label.new(); subtitle.text = tr("subtitle"); subtitle.add_theme_font_size_override("font_size", 13); subtitle.size_flags_horizontal = Control.SIZE_EXPAND_FILL; top_row.add_child(subtitle)
	var lang_button := Button.new(); lang_button.text = "عربي / EN"; lang_button.pressed.connect(_toggle_language); top_row.add_child(lang_button)
	clue_count_label = Label.new(); clue_count_label.anchor_left=0.03; clue_count_label.anchor_right=0.45; clue_count_label.anchor_top=0.16; clue_count_label.anchor_bottom=0.21; overlay.add_child(clue_count_label)
	message_label = Label.new(); message_label.anchor_left=0.03; message_label.anchor_right=0.70; message_label.anchor_top=0.76; message_label.anchor_bottom=0.91; message_label.autowrap_mode=TextServer.AUTOWRAP_WORD_SMART; message_label.add_theme_font_size_override("font_size",18); overlay.add_child(message_label)
	var buttons := VBoxContainer.new(); buttons.anchor_left=0.76; buttons.anchor_right=0.98; buttons.anchor_top=0.18; buttons.anchor_bottom=0.72; buttons.add_theme_constant_override("separation",8); overlay.add_child(buttons)
	_add_button(buttons,tr("evidence"),_show_evidence)
	_add_button(buttons,tr("suspects"),_show_suspects)
	_add_button(buttons,tr("deduce"),_show_deduction)
	_add_button(buttons,tr("hint"),_show_hint)
	var interact := Button.new(); interact.text=tr("interact"); interact.anchor_left=0.76; interact.anchor_right=0.98; interact.anchor_top=0.84; interact.anchor_bottom=0.94; interact.pressed.connect(_interact); overlay.add_child(interact)
	clue_panel = PanelContainer.new(); clue_panel.anchor_left=0.14; clue_panel.anchor_right=0.86; clue_panel.anchor_top=0.22; clue_panel.anchor_bottom=0.75; clue_panel.visible=false; clue_panel.add_theme_stylebox_override("panel",_panel_style(Color(0.025,0.035,0.05,0.97))); overlay.add_child(clue_panel)
	var inner := VBoxContainer.new(); inner.add_theme_constant_override("separation",10); clue_panel.add_child(inner)
	var close := Button.new(); close.text="✕"; close.size_flags_horizontal=Control.SIZE_SHRINK_END; close.pressed.connect(func(): clue_panel.visible=false); inner.add_child(close)
	clue_text = Label.new(); clue_text.autowrap_mode=TextServer.AUTOWRAP_WORD_SMART; clue_text.size_flags_vertical=Control.SIZE_EXPAND_FILL; clue_text.add_theme_font_size_override("font_size",18); inner.add_child(clue_text)
	var pad := Button.new(); pad.text="◀  ▲  ▼  ▶"; pad.anchor_left=0.03; pad.anchor_right=0.24; pad.anchor_top=0.82; pad.anchor_bottom=0.96; pad.add_theme_font_size_override("font_size",18); pad.pressed.connect(func(): _set_message(tr("move"))); overlay.add_child(pad)
	_update_count()

func _panel_style(color: Color) -> StyleBoxFlat:
	var s := StyleBoxFlat.new(); s.bg_color=color; s.border_color=Color(0.30,0.48,0.60,0.8); s.set_border_width_all(1); s.set_corner_radius_all(8); s.content_margin_left=12; s.content_margin_right=12; s.content_margin_top=8; s.content_margin_bottom=8; return s

func _add_button(parent: Control, text_value: String, callback: Callable) -> void:
	var b := Button.new(); b.text=text_value; b.custom_minimum_size=Vector2(0,44); b.pressed.connect(callback); parent.add_child(b)

func _toggle_language() -> void:
	lang = "ar" if lang == "en" else "en"
	var cfg := ConfigFile.new()
	cfg.set_value("settings", "language", lang)
	cfg.save("user://settings.cfg")
	get_tree().reload_current_scene()

func _set_message(value: String) -> void:
	if is_instance_valid(message_label): message_label.text=value

func _update_count() -> void:
	if is_instance_valid(clue_count_label): clue_count_label.text = ("الأدلة: " if lang=="ar" else "EVIDENCE FOUND: ")+str(found.size())+" / "+str(CLUES.size())

func _interact() -> void:
	if Time.get_ticks_msec()/1000.0 - last_interaction < 0.2: return
	last_interaction=Time.get_ticks_msec()/1000.0
	var nearest: Dictionary = {}
	var best := 3.3
	for item in interactables:
		var d: float = player.global_position.distance_to(item.pos)
		if d < best:
			best=d; nearest=item
	if nearest.is_empty():
		if player.global_position.distance_to(Vector3(4,0,7.8)) < 3.5: _try_hatch()
		else: _set_message(tr("intro"))
		return
	var id: String = nearest.id
	if id == "fiber" and not opened_hatch:
		_set_message(tr("hatch_locked")); return
	var clue: Dictionary = _get_clue(id)
	if not found.has(id):
		found.append(id); _save(); _update_count()
		var n: Node3D = nearest.node
		if is_instance_valid(n):
			var mat := StandardMaterial3D.new(); mat.albedo_color=Color(0.25,0.68,0.48); n.material_override=mat
	_set_message((clue.title_ar if lang=="ar" else clue.title_en)+"\n"+(clue.desc_ar if lang=="ar" else clue.desc_en))

func _get_clue(id: String) -> Dictionary:
	for c in CLUES:
		if c.id==id: return c
	return {}

func _try_hatch() -> void:
	if opened_hatch:
		_set_message(tr("hatch_open")); return
	if found.has("recorder") and found.has("ticket") and found.has("log"):
		opened_hatch=true; _save(); _set_message(tr("hatch_open"))
		for item in interactables:
			if item.id=="fiber":
				var mat := StandardMaterial3D.new(); mat.albedo_color=Color(0.2,0.8,0.7); item.node.material_override=mat
	else: _set_message(tr("hatch_locked"))

func _show_evidence() -> void:
	var lines: Array[String]=[]
	for c in CLUES:
		if found.has(c.id): lines.append("• "+(c.title_ar if lang=="ar" else c.title_en)+" — "+(c.desc_ar if lang=="ar" else c.desc_en))
	if lines.is_empty(): lines.append("لا توجد أدلة بعد." if lang=="ar" else "No evidence collected yet.")
	clue_text.text=("الأدلة المكتشفة\n\n" if lang=="ar" else "COLLECTED EVIDENCE\n\n")+"\n\n".join(lines)
	clue_panel.visible=true

func _show_suspects() -> void:
	var lines: Array[String]=[]
	for s in SUSPECTS:
		lines.append((s.ar if lang=="ar" else s.en)+"\n"+(s.hint_ar if lang=="ar" else s.hint_en))
	clue_text.text=("المشتبه بهم\n\n" if lang=="ar" else "SUSPECTS\n\n")+"\n\n".join(lines)
	clue_panel.visible=true

func _show_hint() -> void:
	hint_level=min(hint_level+1,3)
	_set_message(tr("hint"+str(hint_level)))

func _show_deduction() -> void:
	if found.size()<4:
		_set_message(tr("need_clues")); return
	clue_text.text=("اختر استنتاجك :\n\n" if lang=="ar" else "CHOOSE YOUR CONCLUSION:\n\n")
	clue_panel.visible=true
	var inner: VBoxContainer=clue_panel.get_child(0)
	for child in inner.get_children():
		if child is Button and child.text!="✕": child.queue_free()
	for s in SUSPECTS:
		var b:=Button.new(); b.text=s.ar if lang=="ar" else s.en; b.pressed.connect(func(): _choose_suspect(s.id)); inner.add_child(b)

func _choose_suspect(id: String) -> void:
	selected_suspect=id
	if id=="yusef" and found.has("key") and found.has("log") and found.has("recorder") and found.has("fiber"):
		_set_message(tr("solved")); clue_text.text=tr("solved"); _save()
	else:
		_set_message(tr("wrong")); clue_text.text=tr("wrong")

func _input(event: InputEvent) -> void:
	if event is InputEventScreenTouch:
		if event.pressed:
			touch_start=event.position
			if event.position.x > get_viewport().get_visible_rect().size.x*0.42: look_touch_id=event.index
			else:
				move_touch_id=event.index; move_touch_origin=event.position; move_touch_current=event.position
		else:
			if look_touch_id==event.index: look_touch_id=-1
			if move_touch_id==event.index: move_touch_id=-1; move_touch_current=move_touch_origin
	elif event is InputEventScreenDrag:
		if event.index==look_touch_id:
			player.rotate_y(-event.relative.x*0.004)
			camera.rotate_x(-event.relative.y*0.003)
			camera.rotation.x=clamp(camera.rotation.x,-1.1,0.7)
		elif event.index==move_touch_id:
			move_touch_current=event.position

func _physics_process(delta: float) -> void:
	if not is_instance_valid(player): return
	var dir:=Vector2.ZERO
	if Input.is_key_pressed(KEY_W) or Input.is_key_pressed(KEY_UP): dir.y-=1
	if Input.is_key_pressed(KEY_S) or Input.is_key_pressed(KEY_DOWN): dir.y+=1
	if Input.is_key_pressed(KEY_A) or Input.is_key_pressed(KEY_LEFT): dir.x-=1
	if Input.is_key_pressed(KEY_D) or Input.is_key_pressed(KEY_RIGHT): dir.x+=1
	# Touch movement: virtual joystick on the left side.
	if move_touch_id >= 0:
		var delta_touch := move_touch_current - move_touch_origin
		dir.x=clamp(delta_touch.x/65.0,-1,1); dir.y=clamp(delta_touch.y/65.0,-1,1)
	var basis:=player.global_transform.basis
	var move:Vector3=(basis.x*dir.x + basis.z*dir.y); move.y=0
	if move.length()>1: move=move.normalized()
	player.velocity.x=move.x*3.2; player.velocity.z=move.z*3.2
	if not player.is_on_floor(): player.velocity.y-=9.8*delta
	else: player.velocity.y=0
	player.move_and_slide()

func _save() -> void:
	var f:=FileAccess.open(SAVE_PATH,FileAccess.WRITE)
	if f: f.store_string(JSON.stringify({"found":found,"opened_hatch":opened_hatch,"hint_level":hint_level}))

func _load_save() -> void:
	var cfg := ConfigFile.new()
	if cfg.load("user://settings.cfg") == OK:
		lang = str(cfg.get_value("settings", "language", "en"))
	if not FileAccess.file_exists(SAVE_PATH): return
	var f:=FileAccess.open(SAVE_PATH,FileAccess.READ)
	if not f: return
	var data=JSON.parse_string(f.get_as_text())
	if data is Dictionary:
		for id in data.get("found",[]):
			if id in ["ticket","glass","note","mud","recorder","key","log","fiber"] and not found.has(id): found.append(id)
		opened_hatch=bool(data.get("opened_hatch",false)); hint_level=int(data.get("hint_level",0))
