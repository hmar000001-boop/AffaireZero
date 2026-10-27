extends Node3D

# AFFAIRE ZÉRO — Case 01: The Impossible Disappearance
# Self-contained vertical slice; no external assets or online services required.

const SAVE_PATH := "user://affaire_zero_save.json"
const SETTINGS_PATH := "user://settings.cfg"
const CLUES := [
	{"id":"ticket", "title_en":"The 21:10 ticket", "title_ar":"تذكرة الساعة 21:10", "desc_en":"A railway ticket stamped 21:10. The paper is damp, but the ink is perfectly sharp. Someone wanted this departure time to be noticed.", "desc_ar":"تذكرة قطار مختومة 21:10. الورق رطب لكن الحبر واضح تماماً. أراد أحدهم أن نلاحظ موعد المغادرة.", "room":"office"},
	{"id":"glass", "title_en":"Glass over dust", "title_ar":"زجاج فوق الغبار", "desc_en":"Fresh glass fragments rest on top of an undisturbed layer of dust. The window was broken after the room had been cleaned.", "desc_ar":"تستقر شظايا زجاج حديثة فوق طبقة غبار لم تُمس. كُسرت النافذة بعد تنظيف الغرفة.", "room":"hall"},
	{"id":"note", "title_en":"The three times", "title_ar":"الأوقات الثلاثة", "desc_en":"Three times are circled: 20:40, 21:10, 21:25. On the reverse: “The altered ledger leaves by the service passage tonight.”", "desc_ar":"ثلاثة أوقات محددة: 20:40 و21:10 و21:25. وعلى الوجه الآخر: «سيغادر السجل المعدّل عبر ممر الخدمة الليلة».", "room":"archive"},
	{"id":"mud", "title_en":"Wet footprints", "title_ar":"آثار أقدام مبللة", "desc_en":"The prints lead from the service entrance toward the hatch. They do not come from the street door.", "desc_ar":"تقود الآثار من مدخل الخدمة نحو الفتحة، ولا تأتي من باب الشارع.", "room":"hall"},
	{"id":"recorder", "title_en":"The voice recorder", "title_ar":"مسجل الصوت", "desc_en":"A coded exchange mentions the planned 20:40 blackout and tells someone to keep the service route clear until 21:25. The disappearance was timed, not supernatural.", "desc_ar":"يذكر حوار مشفّر انقطاعاً مخططاً عند 20:40 ويطلب إبقاء ممر الخدمة خالياً حتى 21:25. كان الاختفاء مدبّراً وليس خارقاً للطبيعة.", "room":"office"},
	{"id":"key", "title_en":"The brass key", "title_ar":"المفتاح النحاسي", "desc_en":"A maintenance key with fresh machine oil in its grooves. It has been used recently.", "desc_ar":"مفتاح صيانة عليه زيت آلي حديث داخل شقوقه. استُخدم مؤخراً.", "room":"service"},
	{"id":"log", "title_en":"Maintenance log", "title_ar":"سجل الصيانة", "desc_en":"The service hatch was opened at 21:25 with the master key. The initials beside the entry are smudged, not erased.", "desc_ar":"فُتحت فتحة الخدمة عند 21:25 بالمفتاح الرئيسي. الأحرف الأولى بجانب السجل ممسوحة وليست محذوفة.", "room":"service"},
	{"id":"fiber", "title_en":"Blue synthetic fibre", "title_ar":"ليف صناعي أزرق", "desc_en":"A blue fibre is caught on the hatch hinge. It matches the synthetic weave of a technician's work jacket.", "desc_ar":"علق ليف أزرق بمفصلة الفتحة. يتوافق مع نسيج سترة يوسف الزرقاء، كما تظهر في صورة الموظفين.", "room":"service"}
]
const SUSPECTS := [
	{"id":"mira", "en":"Mira — Archivist", "ar":"ميرا — أمينة الأرشيف", "bio_en":"She had legitimate access to the archive. Her key log places her upstairs during the critical interval.", "bio_ar":"كان لديها وصول رسمي للأرشيف. يضعها سجل المفاتيح في الطابق العلوي خلال الفترة الحاسمة."},
	{"id":"yusef", "en":"Yusef — Maintenance technician", "ar":"يوسف — فني الصيانة", "bio_en":"He knew the blackout schedule, held the master key, and wore the cobalt jacket matching the fibre. The altered-ledger route gives him a reason to cooperate in a staged disappearance.", "bio_ar":"كان يعرف موعد انقطاع الكهرباء ويحمل المفتاح الرئيسي ويرتدي السترة الزرقاء المطابقة لليف. ويمنحه مسار السجل المعدّل سبباً للمشاركة في اختفاء مدبّر."},
	{"id":"nadia", "en":"Nadia — Visitor", "ar":"نادية — زائرة", "bio_en":"Her train ticket proves she planned to leave. It does not connect her to the hatch or maintenance log.", "bio_ar":"تثبت تذكرتها أنها كانت تخطط للمغادرة، لكنها لا تربطها بفتحة الخدمة أو سجل الصيانة."}
]

var lang := "en"
var found: Array[String] = []
var opened_hatch := false
var solved := false
var hint_level := 0
var player: CharacterBody3D
var camera: Camera3D
var ambience_player: AudioStreamPlayer
var hatch_mesh: MeshInstance3D
var hatch_collision: StaticBody3D
var interactables: Array[Dictionary] = []
var held_directions := {"left":false, "right":false, "up":false, "down":false}
var gameplay_controls: Array[Control] = []
var message_label: Label
var clue_count_label: Label
var objective_label: Label
var modal: PanelContainer
var modal_content: VBoxContainer
var modal_body: Label
var top_panel: PanelContainer
var menu_panel: PanelContainer
var look_touch_id := -1
var last_interaction := 0.0
var game_started := false

func _ready() -> void:
	_load_save()
	_build_world()
	_build_audio()
	_build_ui()
	_show_title_screen()
	_update_hud()

func tr(key: String) -> String:
	var d := {
		"title":{"en":"AFFAIRE ZÉRO", "ar":"القضية صفر"},
		"case":{"en":"CASE FILE 01  /  THE IMPOSSIBLE DISAPPEARANCE", "ar":"ملف القضية 01 / الاختفاء المستحيل"},
		"intro":{"en":"An archive curator vanished during a blackout. No body. No forced exit. Only eight clues and a timeline that does not add up.", "ar":"اختفى أمين الأرشيف أثناء انقطاع الكهرباء. لا جثة ولا مخرج مكسور. ثمانية أدلة وتسلسل زمني لا يستقيم."},
		"start":{"en":"BEGIN INVESTIGATION", "ar":"ابدأ التحقيق"},
		"resume":{"en":"CONTINUE CASE", "ar":"تابع القضية"},
		"new":{"en":"RESTART CASE", "ar":"أعد القضية من البداية"},
		"evidence":{"en":"CASE NOTES", "ar":"ملف الأدلة"},
		"suspects":{"en":"SUSPECTS", "ar":"المشتبه بهم"},
		"deduce":{"en":"FINAL DEDUCTION", "ar":"الاستنتاج النهائي"},
		"hint":{"en":"HINT", "ar":"تلميح"},
		"inspect":{"en":"INSPECT", "ar":"افحص"},
		"close":{"en":"CLOSE", "ar":"إغلاق"},
		"objective":{"en":"OBJECTIVE: Find evidence and reconstruct the timeline.", "ar":"الهدف: اعثر على الأدلة وأعد بناء التسلسل الزمني."},
		"need":{"en":"Collect at least four clues before presenting a deduction.", "ar":"اجمع أربعة أدلة على الأقل قبل تقديم الاستنتاج."},
		"not_near":{"en":"Move closer to an evidence marker or the service hatch.", "ar":"اقترب من علامة دليل أو من فتحة الخدمة."},
		"hatch_locked":{"en":"The hatch is sealed. The recorder, departure ticket and maintenance log establish the sequence needed to open it.", "ar":"الفتحة مغلقة. يحدد المسجل وتذكرة المغادرة وسجل الصيانة التسلسل اللازم لفتحها."},
		"hatch_open":{"en":"The hatch opens. A narrow service passage runs beneath the building.", "ar":"انفتحت الفتحة. يمتد ممر خدمة ضيق تحت المبنى."},
		"solved":{"en":"CASE SOLVED\n\nThe disappearance was staged as an impossible vanishing. The note reveals that an altered ledger was being moved; the recorder places the plan behind the 20:40 blackout. The ticket and broken glass send investigators toward the street, while the 21:25 maintenance entry, master key and fibre matching Yusef’s blue jacket tie him to the hidden route. The evidence supports Yusef as the person who enabled the exit. No supernatural explanation is needed.", "ar":"حُلّت القضية\n\nدُبّر الاختفاء ليبدو مستحيلاً. تكشف الملاحظة أن سجلاً معدّلاً كان سيُنقل، ويضع المسجل الخطة خلف انقطاع الكهرباء عند 20:40. توجّه التذكرة والزجاج المحققين إلى الشارع، بينما يربط سجل 21:25 والمفتاح الرئيسي والليف المطابق لسترة يوسف الزرقاء بينه وبين الممر المخفي. تدعم الأدلة أن يوسف سهّل المغادرة؛ ولا حاجة إلى تفسير خارق للطبيعة."},
		"wrong":{"en":"That theory does not explain the maintenance log and the fibre. Revisit the service room and compare the timeline.", "ar":"لا تفسر هذه الفرضية سجل الصيانة والليف. عد إلى غرفة الخدمة وقارن التسلسل الزمني."},
		"hint1":{"en":"Hint 1/3 — Some clues are distractions. Prioritise evidence tied to a place and a precise time.", "ar":"التلميح 1/3 — بعض الأدلة للتمويه. أعط الأولوية لما يرتبط بمكان ووقت محددين."},
		"hint2":{"en":"Hint 2/3 — Compare the 20:40 blackout with the 21:25 hatch entry.", "ar":"التلميح 2/3 — قارن انقطاع 20:40 بسجل فتح الفتحة عند 21:25."},
		"hint3":{"en":"Hint 3/3 — Who knew the outage schedule, held the master key and wore blue workwear?", "ar":"التلميح 3/3 — من عرف موعد الانقطاع وحمل المفتاح الرئيسي وارتدى ملابس عمل زرقاء؟"},
		"restart_confirm":{"en":"Restart this case? Your collected evidence and conclusion will be cleared.", "ar":"هل تريد إعادة القضية؟ ستُحذف الأدلة والاستنتاج الحاليان."},
		"timeline":{"en":"TIMELINE", "ar":"التسلسل الزمني"},
		"case_solved":{"en":"CASE CLOSED", "ar":"أُغلقت القضية"}
	}
	if d.has(key): return d[key][lang]
	return key

func _build_audio() -> void:
	var stream := load("res://assets/audio/archive_ambience.wav") as AudioStreamWAV
	if stream == null: return
	stream.loop_mode = AudioStreamWAV.LOOP_FORWARD
	ambience_player = AudioStreamPlayer.new()
	ambience_player.name = "AmbientSound"
	ambience_player.stream = stream
	ambience_player.volume_db = -22.0
	add_child(ambience_player)
	ambience_player.play()

func _build_world() -> void:
	var env := WorldEnvironment.new()
	var e := Environment.new()
	e.background_mode = Environment.BG_SKY
	e.sky = Sky.new()
	var sky_mat := ProceduralSkyMaterial.new()
	sky_mat.sky_top_color = Color(0.012, 0.025, 0.055)
	sky_mat.sky_horizon_color = Color(0.18, 0.22, 0.29)
	sky_mat.ground_bottom_color = Color(0.018, 0.022, 0.03)
	sky_mat.ground_horizon_color = Color(0.10, 0.12, 0.16)
	e.sky.sky_material = sky_mat
	e.ambient_light_source = Environment.AMBIENT_SOURCE_COLOR
	e.ambient_light_color = Color(0.39, 0.43, 0.52)
	e.ambient_light_energy = 0.48
	e.tonemap_mode = Environment.TONE_MAPPER_FILMIC
	e.fog_enabled = true
	e.fog_light_color = Color(0.09, 0.11, 0.15)
	e.fog_density = 0.012
	env.environment = e
	add_child(env)
	var sun := DirectionalLight3D.new()
	sun.rotation_degrees = Vector3(-42, -25, 0)
	sun.light_color = Color(0.70, 0.76, 0.90)
	sun.light_energy = 0.35
	add_child(sun)
	_make_box("Main floor", Vector3(0,-0.22,0), Vector3(24,0.4,24), Color(0.12,0.13,0.15), true)
	# Connected rooms. Trim, door frames and lamps add depth without expensive assets.
	for x in [-8.0, 8.0]:
		_make_box("Partition", Vector3(x,1.6,-7), Vector3(0.22,3.2,8), Color(0.16,0.18,0.21), true)
		_make_box("Partition", Vector3(x,1.6,7), Vector3(0.22,3.2,8), Color(0.16,0.18,0.21), true)
	_make_box("Left exterior wall", Vector3(-11.8,1.6,0), Vector3(0.35,3.2,24), Color(0.12,0.14,0.17), true)
	_make_box("Right exterior wall", Vector3(11.8,1.6,0), Vector3(0.35,3.2,24), Color(0.12,0.14,0.17), true)
	_make_box("Rear wall", Vector3(0,1.6,-11.7), Vector3(24,3.2,0.35), Color(0.12,0.14,0.17), true)
	_make_box("Front wall left", Vector3(-7,1.6,11.7), Vector3(10,3.2,0.35), Color(0.12,0.14,0.17), true)
	_make_box("Front wall right", Vector3(7,1.6,11.7), Vector3(10,3.2,0.35), Color(0.12,0.14,0.17), true)
	_make_box("Ceiling beam", Vector3(0,3.13,0), Vector3(24,0.18,24), Color(0.085,0.095,0.11), false)
	# Archive: warm wood, stacked books, filing cabinets.
	_make_box("Archive table", Vector3(-4,0.62,-3), Vector3(2.4,0.16,1.3), Color(0.27,0.16,0.09), true)
	_make_box("Archive pedestal", Vector3(-4,0.30,-3), Vector3(0.22,0.55,0.22), Color(0.20,0.13,0.08), true)
	for i in range(5):
		_make_box("Archive book", Vector3(-5.0 + i*0.35,0.79,-3.05), Vector3(0.23,0.17,0.52), [Color(0.28,0.09,0.07),Color(0.08,0.19,0.22),Color(0.45,0.31,0.15)][i%3], false)
	_make_box("Archive cabinet", Vector3(-6.4,1.05,-5.3), Vector3(1.25,2.1,0.6), Color(0.20,0.22,0.24), true)
	for i in range(3): _make_box("Cabinet drawer", Vector3(-6.4,0.55+i*0.53,-4.98), Vector3(1.1,0.08,0.04), Color(0.48,0.42,0.32), false)
	# Office: desk, monitor, shelves, lamp.
	_make_box("Office desk", Vector3(4,0.68,-3), Vector3(2.6,0.16,1.4), Color(0.22,0.13,0.075), true)
	_make_box("Desk leg left", Vector3(3.0,0.32,-3), Vector3(0.12,0.65,1.0), Color(0.10,0.09,0.08), true)
	_make_box("Desk leg right", Vector3(5.0,0.32,-3), Vector3(0.12,0.65,1.0), Color(0.10,0.09,0.08), true)
	_make_box("Monitor", Vector3(4.25,1.05,-3.2), Vector3(0.85,0.52,0.12), Color(0.025,0.09,0.12), false)
	_make_box("Monitor screen", Vector3(4.25,1.05,-3.125), Vector3(0.72,0.39,0.012), Color(0.08,0.36,0.39), false)
	_make_box("Office shelf", Vector3(6.7,1.25,-6.8), Vector3(0.7,2.5,2.6), Color(0.17,0.12,0.085), true)
	for level in range(4):
		for book in range(3): _make_box("Office book", Vector3(6.7,0.45+level*0.48,-7.6+book*0.7), Vector3(0.48,0.33,0.1), [Color(0.14,0.24,0.29),Color(0.38,0.15,0.12),Color(0.42,0.32,0.16)][book], false)
	# Service room: metal bench, pipes and hatch.
	_make_box("Service bench", Vector3(4,0.58,5), Vector3(2.5,0.14,1.0), Color(0.22,0.24,0.26), true)
	_make_box("Pipe vertical", Vector3(6.7,1.3,4.1), Vector3(0.16,2.6,0.16), Color(0.23,0.29,0.32), true)
	_make_box("Pipe horizontal", Vector3(5.5,2.25,4.1), Vector3(2.4,0.14,0.14), Color(0.23,0.29,0.32), true)
	hatch_mesh = _make_box("Service hatch", Vector3(4,0.018,7.8), Vector3(1.7,0.06,1.5), Color(0.045,0.055,0.065), false)
	hatch_collision = _add_static_box(Vector3(4,0.018,7.8), Vector3(1.7,0.06,1.5))
	_make_box("Hatch handle", Vector3(4.5,0.075,7.8), Vector3(0.28,0.06,0.07), Color(0.50,0.37,0.18), false)
	if opened_hatch: _open_hatch_visual()
	# Windows have cold blue glass and dark frames.
	for x in [-5.4, 2.7, 5.4]:
		_make_box("Window glass", Vector3(x,1.9,-11.48), Vector3(1.35,1.3,0.025), Color(0.055,0.13,0.19), false)
		_make_box("Window sill", Vector3(x,1.18,-11.4), Vector3(1.55,0.10,0.15), Color(0.26,0.28,0.30), false)
	# Atmosphere: small warm pools of light in each room.
	_add_lamp(Vector3(-4,2.8,-3), Color(1.0,0.66,0.35), 5.0)
	_add_lamp(Vector3(4,2.8,-3), Color(0.56,0.74,1.0), 4.5)
	_add_lamp(Vector3(4,2.8,5), Color(0.62,0.82,0.78), 4.2)
	_add_evidence("ticket", Vector3(4.0,0.92,-3.0), Color(0.78,0.72,0.52))
	_add_evidence("recorder", Vector3(3.45,0.92,-2.72), Color(0.13,0.16,0.18))
	_add_evidence("glass", Vector3(-2.0,0.13,1.0), Color(0.43,0.72,0.79))
	_add_evidence("note", Vector3(-4.1,0.92,-3.0), Color(0.83,0.79,0.62))
	_add_evidence("mud", Vector3(0.0,0.13,3.0), Color(0.27,0.20,0.13))
	_add_evidence("key", Vector3(4.0,0.80,5.0), Color(0.72,0.48,0.16))
	_add_evidence("log", Vector3(4.8,0.80,5.1), Color(0.70,0.66,0.53))
	_add_evidence("fiber", Vector3(4.9,0.20,7.25), Color(0.12,0.40,0.78))
	_add_label_3d("ARCHIVES", Vector3(-4,2.72,-8.8))
	_add_label_3d("RECORDS OFFICE", Vector3(4,2.72,-8.8))
	_add_label_3d("MAINTENANCE", Vector3(4,2.72,8.8))
	player = CharacterBody3D.new()
	player.name = "Investigator"
	player.position = Vector3(0,0.15,9.3)
	var shape_node := CollisionShape3D.new()
	var capsule := CapsuleShape3D.new()
	capsule.radius = 0.33
	capsule.height = 1.65
	shape_node.shape = capsule
	shape_node.position.y = 0.82
	player.add_child(shape_node)
	camera = Camera3D.new()
	camera.position = Vector3(0,1.48,0)
	camera.rotation_degrees.x = -5
	camera.fov = 75
	camera.current = true
	player.add_child(camera)
	add_child(player)


func _make_box(label: String, pos: Vector3, size: Vector3, color: Color, collision: bool) -> MeshInstance3D:
	var mesh := MeshInstance3D.new()
	mesh.name = label
	var box := BoxMesh.new()
	box.size = size
	mesh.mesh = box
	var mat := StandardMaterial3D.new()
	mat.albedo_color = color
	mat.roughness = 0.78
	mat.metallic = 0.05 if color.r < 0.25 else 0.0
	mesh.material_override = mat
	mesh.position = pos
	add_child(mesh)
	if collision: _add_static_box(pos, size)
	return mesh

func _add_static_box(pos: Vector3, size: Vector3) -> StaticBody3D:
	var body := StaticBody3D.new()
	body.position = pos
	var collider := CollisionShape3D.new()
	var shape := BoxShape3D.new()
	shape.size = size
	collider.shape = shape
	body.add_child(collider)
	add_child(body)
	return body

func _add_lamp(pos: Vector3, tint: Color, strength: float) -> void:
	var housing := _make_box("Ceiling lamp", pos, Vector3(0.65,0.08,0.42), Color(0.22,0.23,0.24), false)
	var light := OmniLight3D.new()
	light.position = pos + Vector3(0,-0.15,0)
	light.light_color = tint
	light.light_energy = strength
	light.omni_range = 7.0
	light.omni_attenuation = 1.4
	add_child(light)

func _add_evidence(id: String, pos: Vector3, tint: Color) -> void:
	var dims := Vector3(0.42,0.06,0.30)
	match id:
		"ticket": dims = Vector3(0.56,0.025,0.28)
		"glass": dims = Vector3(0.30,0.025,0.22)
		"note": dims = Vector3(0.48,0.025,0.34)
		"mud": dims = Vector3(0.22,0.025,0.12)
		"recorder": dims = Vector3(0.36,0.13,0.23)
		"key": dims = Vector3(0.34,0.045,0.08)
		"log": dims = Vector3(0.46,0.09,0.34)
		"fiber": dims = Vector3(0.58,0.025,0.045)
	var item := _make_box("Evidence_"+id, pos, dims, tint, false)
	var accent := StandardMaterial3D.new()
	accent.albedo_color = Color(0.15,0.76,0.69) if found.has(id) else Color(1.0,0.69,0.26)
	accent.emission_enabled = true
	accent.emission = accent.albedo_color
	accent.emission_energy_multiplier = 0.30
	item.material_override = accent
	interactables.append({"id":id,"node":item,"pos":pos})
	# Small geometric details make evidence distinguishable at a glance on a phone screen.
	match id:
		"ticket":
			_make_box("Ticket red stamp", pos + Vector3(-0.12,0.022,0.0), Vector3(0.09,0.006,0.20), Color(0.55,0.10,0.08), false)
			_make_box("Ticket print line", pos + Vector3(0.13,0.022,-0.055), Vector3(0.13,0.006,0.018), Color(0.19,0.18,0.15), false)
		"glass":
			_make_box("Glass shard A", pos + Vector3(-0.22,0.015,0.12), Vector3(0.22,0.018,0.045), Color(0.55,0.79,0.84), false).rotation.y = 0.42
			_make_box("Glass shard B", pos + Vector3(0.18,0.018,-0.08), Vector3(0.19,0.016,0.035), Color(0.64,0.83,0.87), false).rotation.y = -0.56
			_make_box("Glass shard C", pos + Vector3(0.06,0.02,0.18), Vector3(0.14,0.016,0.035), Color(0.42,0.68,0.75), false).rotation.y = 0.85
		"note":
			for line_i in range(3): _make_box("Note writing", pos + Vector3(0.0,0.02,-0.08+line_i*0.075), Vector3(0.27 if line_i != 1 else 0.19,0.006,0.012), Color(0.42,0.16,0.10), false)
		"mud":
			_make_box("Footprint two", pos + Vector3(0.23,0.006,-0.10), Vector3(0.15,0.012,0.08), Color(0.21,0.15,0.10), false).rotation.y = -0.3
			_make_box("Footprint three", pos + Vector3(-0.22,0.007,-0.22), Vector3(0.14,0.012,0.075), Color(0.18,0.13,0.09), false).rotation.y = 0.25
		"recorder":
			_make_box("Recorder red indicator", pos + Vector3(0.10,0.075,-0.02), Vector3(0.045,0.012,0.04), Color(0.87,0.12,0.08), false)
			_make_box("Recorder button", pos + Vector3(-0.07,0.075,-0.02), Vector3(0.055,0.012,0.055), Color(0.55,0.56,0.54), false)
		"key":
			_make_box("Key teeth", pos + Vector3(0.14,0.026,0.0), Vector3(0.07,0.012,0.11), Color(0.82,0.61,0.25), false)
			_make_box("Key shaft", pos + Vector3(-0.06,0.026,0.0), Vector3(0.25,0.012,0.035), Color(0.82,0.61,0.25), false)
		"log":
			_make_box("Log spine", pos + Vector3(-0.20,0.05,0), Vector3(0.035,0.012,0.33), Color(0.40,0.12,0.09), false)
			for line_i in range(3): _make_box("Log entry line", pos + Vector3(0.035,0.052,-0.09+line_i*0.08), Vector3(0.20,0.006,0.012), Color(0.25,0.23,0.20), false)
		"fiber":
			_make_box("Fiber strand", pos + Vector3(0.0,0.018,0.01), Vector3(0.48,0.012,0.018), Color(0.15,0.47,0.95), false).rotation.y = 0.08

func _add_label_3d(value: String, pos: Vector3) -> void:
	var label := Label3D.new()
	label.text = value
	label.position = pos
	label.font_size = 38
	label.modulate = Color(0.71,0.79,0.84)
	label.billboard = BaseMaterial3D.BILLBOARD_ENABLED
	add_child(label)

func _build_ui() -> void:
	var layer := CanvasLayer.new()
	layer.name = "HUD"
	add_child(layer)
	var root := Control.new()
	root.set_anchors_and_offsets_preset(Control.PRESET_FULL_RECT)
	root.mouse_filter = Control.MOUSE_FILTER_PASS
	root.layout_direction = Control.LAYOUT_DIRECTION_RTL if lang == "ar" else Control.LAYOUT_DIRECTION_LTR
	layer.add_child(root)
	top_panel = PanelContainer.new()
	top_panel.anchor_left = 0.02; top_panel.anchor_right = 0.98
	top_panel.anchor_top = 0.02; top_panel.anchor_bottom = 0.115
	top_panel.add_theme_stylebox_override("panel", _panel_style(Color(0.025,0.035,0.052,0.91)))
	root.add_child(top_panel)
	gameplay_controls.append(top_panel)
	var top_row := HBoxContainer.new()
	top_row.add_theme_constant_override("separation", 16)
	top_panel.add_child(top_row)
	var title := Label.new()
	title.text = tr("title")
	title.add_theme_font_size_override("font_size", 24)
	title.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	top_row.add_child(title)
	clue_count_label = Label.new()
	clue_count_label.add_theme_font_size_override("font_size", 15)
	top_row.add_child(clue_count_label)
	var lang_button := Button.new()
	lang_button.text = "عربي / EN"
	lang_button.pressed.connect(_toggle_language)
	top_row.add_child(lang_button)
	var settings := Button.new()
	settings.text = "Ⅱ"
	settings.tooltip_text = "Pause / Menu"
	settings.pressed.connect(_show_pause)
	top_row.add_child(settings)
	objective_label = Label.new()
	objective_label.anchor_left = 0.03; objective_label.anchor_right = 0.66
	objective_label.anchor_top = 0.14; objective_label.anchor_bottom = 0.21
	objective_label.add_theme_font_size_override("font_size", 16)
	objective_label.add_theme_color_override("font_color", Color(0.93,0.83,0.61))
	root.add_child(objective_label)
	gameplay_controls.append(objective_label)
	message_label = Label.new()
	message_label.anchor_left = 0.25; message_label.anchor_right = 0.72
	message_label.anchor_top = 0.77; message_label.anchor_bottom = 0.95
	message_label.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	message_label.add_theme_font_size_override("font_size", 17)
	message_label.add_theme_color_override("font_color", Color(0.94,0.94,0.91))
	root.add_child(message_label)
	gameplay_controls.append(message_label)
	var actions := VBoxContainer.new()
	actions.anchor_left = 0.77; actions.anchor_right = 0.98
	actions.anchor_top = 0.17; actions.anchor_bottom = 0.62
	actions.add_theme_constant_override("separation", 7)
	root.add_child(actions)
	gameplay_controls.append(actions)
	_add_button(actions, tr("evidence"), _show_evidence)
	_add_button(actions, tr("suspects"), _show_suspects)
	_add_button(actions, tr("timeline"), _show_timeline)
	_add_button(actions, tr("deduce"), _show_deduction)
	_add_button(actions, tr("hint"), _show_hint)
	var inspect := Button.new()
	inspect.text = tr("inspect")
	inspect.anchor_left = 0.77; inspect.anchor_right = 0.98
	inspect.anchor_top = 0.84; inspect.anchor_bottom = 0.95
	inspect.custom_minimum_size = Vector2(0,58)
	inspect.add_theme_font_size_override("font_size", 20)
	inspect.pressed.connect(_interact)
	root.add_child(inspect)
	gameplay_controls.append(inspect)
	# Press-and-hold directional pad for touch. Camera look uses unhandled right-side drags.
	var pad := GridContainer.new()
	pad.columns = 3
	pad.anchor_left = 0.025; pad.anchor_right = 0.235
	pad.anchor_top = 0.72; pad.anchor_bottom = 0.98
	pad.add_theme_constant_override("h_separation", 4)
	pad.add_theme_constant_override("v_separation", 4)
	root.add_child(pad)
	gameplay_controls.append(pad)
	_add_pad_spacer(pad)
	_add_direction_button(pad,"up","▲")
	_add_pad_spacer(pad)
	_add_direction_button(pad,"left","◀")
	_add_direction_button(pad,"down","▼")
	_add_direction_button(pad,"right","▶")
	modal = PanelContainer.new()
	modal.anchor_left = 0.18; modal.anchor_right = 0.82
	modal.anchor_top = 0.19; modal.anchor_bottom = 0.79
	modal.visible = false
	modal.add_theme_stylebox_override("panel", _panel_style(Color(0.018,0.027,0.043,0.98)))
	root.add_child(modal)
	modal_content = VBoxContainer.new()
	modal_content.add_theme_constant_override("separation", 12)
	modal.add_child(modal_content)
	var modal_header := HBoxContainer.new()
	modal_content.add_child(modal_header)
	var modal_title := Label.new()
	modal_title.name = "ModalTitle"
	modal_title.text = tr("evidence")
	modal_title.add_theme_font_size_override("font_size", 23)
	modal_title.size_flags_horizontal = Control.SIZE_EXPAND_FILL
	modal_header.add_child(modal_title)
	var close := Button.new()
	close.text = "✕"
	close.pressed.connect(func(): modal.visible = false)
	modal_header.add_child(close)
	modal_body = Label.new()
	modal_body.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	modal_body.size_flags_vertical = Control.SIZE_EXPAND_FILL
	modal_body.add_theme_font_size_override("font_size", 17)
	modal_content.add_child(modal_body)
	menu_panel = PanelContainer.new()
	menu_panel.anchor_left = 0.18; menu_panel.anchor_right = 0.82
	menu_panel.anchor_top = 0.17; menu_panel.anchor_bottom = 0.84
	menu_panel.visible = false
	menu_panel.add_theme_stylebox_override("panel", _panel_style(Color(0.018,0.027,0.043,0.98)))
	root.add_child(menu_panel)
	_update_hud()

func _panel_style(color: Color) -> StyleBoxFlat:
	var s := StyleBoxFlat.new()
	s.bg_color = color
	s.border_color = Color(0.48,0.39,0.23,0.9)
	s.set_border_width_all(1)
	s.set_corner_radius_all(12)
	s.content_margin_left = 16; s.content_margin_right = 16
	s.content_margin_top = 12; s.content_margin_bottom = 12
	return s

func _add_button(parent: Control, text_value: String, callback: Callable) -> void:
	var b := Button.new()
	b.text = text_value
	b.custom_minimum_size = Vector2(0,46)
	b.add_theme_font_size_override("font_size", 15)
	b.pressed.connect(callback)
	parent.add_child(b)

func _add_pad_spacer(parent: GridContainer) -> void:
	var spacer := Control.new()
	spacer.custom_minimum_size = Vector2(46,42)
	parent.add_child(spacer)

func _add_direction_button(parent: GridContainer, direction: String, caption: String) -> void:
	var b := Button.new()
	b.text = caption
	b.custom_minimum_size = Vector2(48,44)
	b.add_theme_font_size_override("font_size", 19)
	b.button_down.connect(func(): held_directions[direction] = true)
	b.button_up.connect(func(): held_directions[direction] = false)
	parent.add_child(b)

func _begin_game() -> void:
	menu_panel.visible = false
	game_started = true
	for control in gameplay_controls: control.visible = true
	_set_message(tr("intro"))

func _show_title_screen() -> void:
	for control in gameplay_controls: control.visible = false
	menu_panel.visible = true
	var box := VBoxContainer.new()
	box.add_theme_constant_override("separation", 14)
	menu_panel.add_child(box)
	var header := Label.new()
	header.text = tr("title")
	header.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	header.add_theme_font_size_override("font_size", 36)
	box.add_child(header)
	var sub := Label.new()
	sub.text = tr("case")
	sub.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	sub.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	sub.add_theme_color_override("font_color", Color(0.88,0.68,0.36))
	box.add_child(sub)
	var intro := Label.new()
	intro.text = tr("intro")
	intro.autowrap_mode = TextServer.AUTOWRAP_WORD_SMART
	intro.vertical_alignment = VERTICAL_ALIGNMENT_CENTER
	intro.size_flags_vertical = Control.SIZE_EXPAND_FILL
	box.add_child(intro)
	var begin := Button.new()
	begin.text = tr("resume") if not found.is_empty() else tr("start")
	begin.custom_minimum_size = Vector2(0,54)
	begin.pressed.connect(_begin_game)
	box.add_child(begin)
	var language := Button.new()
	language.text = "العربية / English"
	language.pressed.connect(_toggle_language)
	box.add_child(language)
	var restart := Button.new()
	restart.text = tr("new")
	restart.pressed.connect(_confirm_restart)
	box.add_child(restart)
	var foot := Label.new()
	foot.text = "OFFLINE INVESTIGATION  •  العربية / ENGLISH"
	foot.horizontal_alignment = HORIZONTAL_ALIGNMENT_CENTER
	foot.add_theme_font_size_override("font_size", 11)
	box.add_child(foot)
	_set_message(tr("intro"))

func _show_pause() -> void:
	if modal.visible:
		modal.visible = false
		return
	menu_panel.visible = true
	for child in menu_panel.get_children(): child.queue_free()
	var box := VBoxContainer.new()
	box.add_theme_constant_override("separation", 12)
	menu_panel.add_child(box)
	var h := Label.new(); h.text = tr("title")+" — MENU"; h.add_theme_font_size_override("font_size",26); box.add_child(h)
	var resume := Button.new(); resume.text = tr("resume"); resume.pressed.connect(func(): menu_panel.visible=false); box.add_child(resume)
	var language := Button.new(); language.text = "العربية / English"; language.pressed.connect(_toggle_language); box.add_child(language)
	var notebook := Button.new(); notebook.text = tr("evidence"); notebook.pressed.connect(func(): menu_panel.visible=false; _show_evidence()); box.add_child(notebook)
	var close_case := Button.new(); close_case.text = tr("new"); close_case.pressed.connect(_confirm_restart); box.add_child(close_case)

func _confirm_restart() -> void:
	for child in menu_panel.get_children(): child.queue_free()
	var box := VBoxContainer.new(); menu_panel.add_child(box)
	var msg := Label.new(); msg.text=tr("restart_confirm"); msg.autowrap_mode=TextServer.AUTOWRAP_WORD_SMART; box.add_child(msg)
	var yes := Button.new(); yes.text="RESET CASE"; yes.pressed.connect(_restart_case); box.add_child(yes)
	var no := Button.new(); no.text=tr("close"); no.pressed.connect(func(): menu_panel.visible=false); box.add_child(no)
	menu_panel.visible=true

func _restart_case() -> void:
	found.clear(); opened_hatch=false; solved=false; hint_level=0
	_save(); get_tree().reload_current_scene()

func _toggle_language() -> void:
	lang = "ar" if lang == "en" else "en"
	var cfg := ConfigFile.new(); cfg.set_value("settings","language",lang); cfg.save(SETTINGS_PATH)
	get_tree().reload_current_scene()

func _set_message(value: String) -> void:
	if is_instance_valid(message_label): message_label.text=value

func _update_hud() -> void:
	if is_instance_valid(clue_count_label):
		clue_count_label.text = ("الأدلة " if lang=="ar" else "EVIDENCE ")+str(found.size())+" / "+str(CLUES.size())
	if is_instance_valid(objective_label):
		objective_label.text = tr("case_solved") if solved else tr("objective")

func _nearest_evidence() -> Dictionary:
	var nearest: Dictionary = {}
	var best := 2.8
	for item in interactables:
		if item.id == "fiber" and not opened_hatch: continue
		var dist: float = player.global_position.distance_to(item.pos)
		if dist < best: best=dist; nearest=item
	return nearest

func _interact() -> void:
	if not game_started or menu_panel.visible or modal.visible: return
	if Time.get_ticks_msec()/1000.0-last_interaction < 0.22: return
	last_interaction=Time.get_ticks_msec()/1000.0
	var nearest := _nearest_evidence()
	if not nearest.is_empty():
		var id: String = nearest.id
		var clue := _get_clue(id)
		if not found.has(id):
			found.append(id); _save(); _update_hud()
			var obj: MeshInstance3D = nearest.node
			var mat := StandardMaterial3D.new(); mat.albedo_color=Color(0.12,0.47,0.41); mat.emission_enabled=true; mat.emission=Color(0.04,0.15,0.12); mat.emission_energy_multiplier=0.4; obj.material_override=mat
			_set_message((clue.title_ar if lang=="ar" else clue.title_en)+"\n"+(clue.desc_ar if lang=="ar" else clue.desc_en))
		else:
			_set_message((clue.title_ar if lang=="ar" else clue.title_en)+"\n"+(clue.desc_ar if lang=="ar" else clue.desc_en))
		return
	if player.global_position.distance_to(Vector3(4,0,7.8)) < 3.5:
		_try_hatch()
	else:
		_set_message(tr("not_near"))

func _get_clue(id: String) -> Dictionary:
	for c in CLUES:
		if c.id == id: return c
	return {}

func _open_hatch_visual() -> void:
	if is_instance_valid(hatch_mesh):
		hatch_mesh.rotation_degrees.x = 62.0
		hatch_mesh.position.y = 0.38
	if is_instance_valid(hatch_collision): hatch_collision.queue_free()

func _try_hatch() -> void:
	if opened_hatch:
		_set_message(tr("hatch_open")); return
	if found.has("recorder") and found.has("ticket") and found.has("log"):
		opened_hatch=true; _open_hatch_visual(); _save(); _set_message(tr("hatch_open")); _update_hud()
		for item in interactables:
			if item.id=="fiber":
				var mat:=StandardMaterial3D.new(); mat.albedo_color=Color(0.12,0.40,0.78); mat.emission_enabled=true; mat.emission=Color(0.04,0.10,0.25); item.node.material_override=mat
	else:
		_set_message(tr("hatch_locked"))

func _open_modal(title: String, body: String) -> void:
	# Preserve the header (title + close button) and the body; remove old suspect buttons.
	var children := modal_content.get_children()
	for i in range(2, children.size()):
		var child: Node = children[i]
		modal_content.remove_child(child)
		child.queue_free()
	var title_label := modal_content.get_child(0).get_node_or_null("ModalTitle") as Label
	if is_instance_valid(title_label): title_label.text = title
	modal_body.text = body
	modal.visible = true

func _show_evidence() -> void:
	var lines: Array[String]=[]
	for c in CLUES:
		if found.has(c.id): lines.append("◆ "+(c.title_ar if lang=="ar" else c.title_en)+"\n"+(c.desc_ar if lang=="ar" else c.desc_en))
	if lines.is_empty(): lines.append("لم تُجمع أدلة بعد." if lang=="ar" else "No evidence collected yet. Search nearby objects marked by a warm amber glow.")
	_open_modal(tr("evidence"),"\n\n".join(PackedStringArray(lines)))

func _show_suspects() -> void:
	var lines: Array[String]=[]
	for s in SUSPECTS: lines.append((s.ar if lang=="ar" else s.en)+"\n"+(s.bio_ar if lang=="ar" else s.bio_en))
	_open_modal(tr("suspects"),"\n\n".join(PackedStringArray(lines)))

func _show_timeline() -> void:
	var body := "20:40  —  Planned power cut\n21:10  —  Train ticket stamped\n21:25  —  Service hatch opened\n\n"
	body += ("اجمع الأدلة التي تدعم كل وقت. لا تفترض أن كل دليل يخبر الحقيقة كاملة." if lang=="ar" else "Compare each time with the physical clues. Not every clue tells the whole truth.")
	_open_modal(tr("timeline"),body)

func _show_hint() -> void:
	hint_level=min(hint_level+1,3); _save(); _set_message(tr("hint"+str(hint_level)))

func _show_deduction() -> void:
	if found.size()<4:
		_set_message(tr("need")); return
	_open_modal(tr("deduce"),("اختر المشتبه الذي يتطابق وصوله وتسلسله الزمني وآثاره المادية مع الأدلة." if lang=="ar" else "Select the suspect whose access, timeline and physical trace match the evidence."))
	for s in SUSPECTS:
		var b:=Button.new(); b.text=s.ar if lang=="ar" else s.en
		b.pressed.connect(_choose_suspect.bind(s.id))
		modal_content.add_child(b)

func _choose_suspect(id: String) -> void:
	if id=="yusef" and found.has("key") and found.has("log") and found.has("recorder") and found.has("fiber"):
		solved=true; _save(); _update_hud(); modal_body.text=tr("solved"); _set_message(tr("solved"))
	else:
		modal_body.text=tr("wrong"); _set_message(tr("wrong"))

func _unhandled_input(event: InputEvent) -> void:
	if not game_started or menu_panel.visible or modal.visible: return
	if event is InputEventMouseMotion and Input.is_mouse_button_pressed(MOUSE_BUTTON_RIGHT):
		player.rotate_y(-event.relative.x*0.004)
		camera.rotate_x(-event.relative.y*0.003)
		camera.rotation.x=clamp(camera.rotation.x,-1.05,0.65)
	elif event is InputEventScreenTouch:
		if event.pressed and event.position.x > get_viewport().get_visible_rect().size.x*0.48:
			look_touch_id=event.index
		elif not event.pressed and look_touch_id==event.index:
			look_touch_id=-1
	elif event is InputEventScreenDrag and event.index==look_touch_id:
		player.rotate_y(-event.relative.x*0.004)
		camera.rotate_x(-event.relative.y*0.003)
		camera.rotation.x=clamp(camera.rotation.x,-1.05,0.65)

func _physics_process(delta: float) -> void:
	if not is_instance_valid(player) or not game_started or menu_panel.visible or modal.visible: return
	var dir := Vector2.ZERO
	if Input.is_key_pressed(KEY_W) or Input.is_key_pressed(KEY_UP): dir.y-=1
	if Input.is_key_pressed(KEY_S) or Input.is_key_pressed(KEY_DOWN): dir.y+=1
	if Input.is_key_pressed(KEY_A) or Input.is_key_pressed(KEY_LEFT): dir.x-=1
	if Input.is_key_pressed(KEY_D) or Input.is_key_pressed(KEY_RIGHT): dir.x+=1
	if held_directions.up: dir.y-=1
	if held_directions.down: dir.y+=1
	if held_directions.left: dir.x-=1
	if held_directions.right: dir.x+=1
	if dir.length()>1: dir=dir.normalized()
	var move := player.global_transform.basis.x*dir.x + player.global_transform.basis.z*dir.y
	move.y=0
	if move.length()>1: move=move.normalized()
	player.velocity.x=move.x*3.0; player.velocity.z=move.z*3.0
	if not player.is_on_floor(): player.velocity.y-=9.8*delta
	else: player.velocity.y=0
	player.move_and_slide()

func _save() -> void:
	var f:=FileAccess.open(SAVE_PATH,FileAccess.WRITE)
	if f: f.store_string(JSON.stringify({"found":found,"opened_hatch":opened_hatch,"hint_level":hint_level,"solved":solved}))

func _load_save() -> void:
	var cfg:=ConfigFile.new()
	if cfg.load(SETTINGS_PATH)==OK: lang=str(cfg.get_value("settings","language","en"))
	if not FileAccess.file_exists(SAVE_PATH): return
	var f:=FileAccess.open(SAVE_PATH,FileAccess.READ)
	if not f: return
	var data=JSON.parse_string(f.get_as_text())
	if data is Dictionary:
		for id in data.get("found",[]):
			if id in ["ticket","glass","note","mud","recorder","key","log","fiber"] and not found.has(id): found.append(id)
		opened_hatch=bool(data.get("opened_hatch",false))
		hint_level=clampi(int(data.get("hint_level",0)),0,3)
		solved=bool(data.get("solved",false))
