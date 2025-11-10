# main.py
import sys
from signal import pause
import config
from stairway import Stairway
from sensor_controller import SensorController

def main():
    """The main entry point for the application."""
    print("--- Starting StairwayToLights Application ---")
    
    stairway_manager = None
    sensor_manager = None

    try:
        stairway_manager = Stairway(config)
        sensor_manager = SensorController(stairway_manager, config)
        print("\nApplication is running. Waiting for sensor triggers...")
        pause()

    except KeyboardInterrupt:
        print("\nShutdown signal received (Ctrl+C).")
    
    except Exception as e:
        print(f"A critical error occurred: {e}", file=sys.stderr)
        return 1
    
    finally:
        print("\n--- Shutting Down Application ---")
        if sensor_manager:
            sensor_manager.cleanup()
        if stairway_manager:
            stairway_manager.cleanup()
        print("GPIO resources have been released. Goodbye.")
        return 0

if __name__ == "__main__":
    sys.exit(main())