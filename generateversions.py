import json
import os

FILENAME = "versions.json"

def load_data():
    if os.path.exists(FILENAME):
        with open(FILENAME, "r") as f:
            try:
                return json.load(f)
            except json.JSONDecodeError:
                return []
    return []

def save_data(data):
    with open(FILENAME, "w") as f:
        json.dump(data, f, indent=4)

def main():
    data = load_data()

    print("Enter version info. Type 'exit' at any time to quit.\n")

    while True:
        version = input("Version number: ").strip()
        if version.lower() == "exit":
            break

        manifest = input("Manifest ID: ").strip()
        if manifest.lower() == "exit":
            break

        entry = {"version": version, "manifestID": manifest}
        data.append(entry)

        save_data(data)
        print(f"✅ Saved: {entry}\n")

    print("Exiting. All data saved to versions.json.")

if __name__ == "__main__":
    main()
