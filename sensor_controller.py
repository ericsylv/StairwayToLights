# sensor_controller.py
from gpiozero import Button

class SensorController:
    """Manages the sensors that trigger the stairway sequences."""
    def __init__(self, stairway, config):
        print("Initializing sensors...")
        
        # Added bounce_time to ignore rapid duplicate triggers (contact bounce).
        # A value of 1 second is a good starting point.
        self.up_sensor = Button(config.UP_SENSOR_PIN, pull_up=True, bounce_time=1)
        self.down_sensor = Button(config.DOWN_SENSOR_PIN, pull_up=True, bounce_time=1)

        self.up_sensor.when_pressed = stairway.go_up
        self.down_sensor.when_pressed = stairway.go_down
        
        print(f"Sensor for 'GoUp' sequence is active on GPIO {config.UP_SENSOR_PIN}.")
        print(f"Sensor for 'GoDown' sequence is active on GPIO {config.DOWN_SENSOR_PIN}.")

    def cleanup(self):
        """Cleans up and releases the GPIO resources used by the sensors."""
        print("Cleaning up sensor resources...")
        self.up_sensor.close()
        self.down_sensor.close()