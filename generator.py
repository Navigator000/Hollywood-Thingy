# this program generates the data because scripting each part in the original program is an absolute pain in the.. arch (get it?)
total_seconds = 40
frames_per_second = 10
total_steps = total_seconds * frames_per_second

for step in range(total_steps + 1):
    # finds how far we are into the program through fractional time second (like 0.1, 0.2 etc. etc.)
    t = step / frames_per_second

    
    computed_x = t * 2.2
    computed_y = (t * t) / 22.0  # this makes that curve. it barely works but.. eh
    
    computed_alt = t * 18
    computed_vel = (t * t * 5) + (t * 120)

    computed_log = "VEHICLE TRACKING TRAJECTORY CONTINUOUS... [NOMINAL]"
    if step == 0:   computed_log = "LIFTOFF! Main engine ignition confirmed."
    if step == 20:  computed_log = "Tower clearance verified. Roll program start."
    if step == 100: computed_log = "ENTERING MAX Q. High structural dynamic drag stress."
    if step == 250: computed_log = "SOLID ROCKET BOOSTER BURNOUT. Releasing structural locks."
    if step == 400: computed_log = "ORBITAL INJECTION NOMINAL. STABLE SATELLITE ORBIT ACHIEVED." #fake logs at scripted intervals

    # for coordinates. its cool innit. definitely not copied from a solution. no, surely not
    packet = {
        "Time": int(t),
        "X": int(computed_x),
        "Y": int(computed_y),
        "Alt": int(computed_alt),
        "Vel": int(computed_vel),
        "Log": computed_log
    }

    streaming_history.append(packet)

    try:
        with open(json_path, "w") as f:
            json.dump(streaming_history, f) #opens the json and reads it to get the data
    except Exception:
        pass

    # this sleeps every tenth of a second before making a new frame
    time.sleep(1.0 / frames_per_second)
