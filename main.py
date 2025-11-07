import time

def main():
    """Fonction principale du programme."""
    print("Démarrage du script 'StairwayToLights'...")
    
    count = 0
    while True:
        print(f"Le script est en cours d'exécution. Itération #{count}")
        count += 1
        time.sleep(5) # Attendre 5 secondes

if __name__ == "__main__":
    # Ce bloc ne s'exécute que si le script est lancé directement
    # et non importé comme un module.
    main()