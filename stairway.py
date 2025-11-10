# stairway.py
import time
from gpiozero import LED
from datetime import datetime

class Light:
    """Represents a single light (LED) on the stairway."""
    def __init__(self, pin_number):
        self._led = LED(pin_number)

    def turn_on(self):
        self._led.on()

    def turn_off(self):
        self._led.off()
    
    def close(self):
        """Releases the GPIO resource."""
        self._led.close()

class Stairway:
    """Manages the entire set of stairway lights and their sequences."""
    def __init__(self, config):
        print("Initializing stairway...")
        self.config = config
        self._lights = []
        # --- Cooldown logic ---
        self._last_sequence_end_time = 0
        self._cooldown_period_s = 10 # Do not allow a new sequence for 10s after one ends
        
        for pin in config.STAIRS_PIN_NUMBERS:
            self._lights.append(Light(pin))
        print(f"{len(self._lights)} lights initialized.")

    def _get_timestamp(self):
        """Returns a formatted timestamp string."""
        return datetime.now().strftime('%Y-%m-%d %H:%M:%S')

    def _run_sequence(self, light_order):
        """Executes a lighting and extinguishing sequence."""
        # --- Check cooldown ---
        current_time = time.time()
        if (current_time - self._last_sequence_end_time) < self._cooldown_period_s:
            print(f"[{self._get_timestamp()}] Sequence trigger ignored: system is in cooldown.")
            return
        
        print(f"[{self._get_timestamp()}] Sequence started.")
        
        try:
            # Cascade on
            for light in light_order:
                light.turn_on()
                time.sleep(self.config.CASCADE_DELAY_S)

            # Pause with all lights on
            time.sleep(self.config.STAY_ON_DURATION_S)

            # Cascade off (in the reverse order of how they were turned on)
            for light in reversed(light_order):
                light.turn_off()
                time.sleep(self.config.CASCADE_DELAY_S)
        finally:
            # --- Update the end time to start the cooldown ---
            self._last_sequence_end_time = time.time()
            print(f"[{self._get_timestamp()}] Sequence finished. Cooldown started.")

    def go_up(self):
        """Sequence for someone going up the stairs."""
        print(f"[{self._get_timestamp()}] Triggering 'GoUp' sequence.")
        self._run_sequence(list(reversed(self._lights)))

    def go_down(self):
        """Sequence for someone going down the stairs."""
        print(f"[{self._get_timestamp()}] Triggering 'GoDown' sequence.")
        self._run_sequence(self._lights)

    def cleanup(self):
        """Cleans up all GPIO resources."""
        print("Cleaning up stairway resources...")
        for light in self._lights:
            light.close()