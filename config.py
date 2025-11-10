# config.py
"""
Central configuration file for the StairwayToLights application.
"""

# --- Hardware Configuration ---

# GPIO pin mapping for each step.
# The list is ordered from the TOP of the stairs to the BOTTOM.
# The first pin in the list (4) corresponds to the top step.
STAIRS_PIN_NUMBERS = [4, 5, 6, 17, 13, 19, 26, 22, 16, 20, 21, 18, 25]

# Pins for the trigger sensors (adjust to your hardware).
# Assuming the "up" sensor is at the bottom of the stairs and the "down" sensor is at the top.
DOWN_SENSOR_PIN = 23  # This sensor triggers the go_down() sequence.
UP_SENSOR_PIN = 24    # This sensor triggers the go_up() sequence.


# --- Sequence Configuration ---

# Delay in seconds between lighting/turning off each LED.
CASCADE_DELAY_S = 0.300  # 300ms

# Duration in seconds that the lights remain fully lit.
STAY_ON_DURATION_S = 5.0 # 5000ms